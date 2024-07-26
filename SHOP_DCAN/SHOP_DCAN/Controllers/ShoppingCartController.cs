using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SHOP_DCAN.Models;
namespace SHOP_DCAN.Controllers
{
    public class ShoppingCartController : Controller
    {
        // GET: ShoppingCart
        QL_DCAN2Entities db = new QL_DCAN2Entities();
        public ActionResult Index()
        {
            ShoppingCart cart = (ShoppingCart)Session["Cart"];
            if (cart != null)
            {
                return View(cart.Items);
            }
            return View();
        }
       
        public ActionResult Partial_Item_ThanhToan()
        {
            ShoppingCart cart = (ShoppingCart)Session["Cart"];
            if (cart != null && cart.Items.Any())
            {
                return PartialView(cart.Items);
            }
            return PartialView();
        }
        public ActionResult Partial_CheckOut()
        {
            KhachHang customerInfo = (KhachHang)Session["KH"];

            //// Nếu không có thông tin khách hàng trong session, chuyển hướng đến trang đăng nhập
            if (customerInfo == null)
            {
                return RedirectToAction("Login", "Account");
            }           
            var checkoutViewModel = new CheckoutView
            {
                MaKH = customerInfo.MaKH,
                TenKH = customerInfo.TenKH,
                SDT = customerInfo.SDT,
                Email = customerInfo.Email.Trim(),              

            };
            //System.Diagnostics.Debug.WriteLine($"Email: {checkoutViewModel.Email}");
            //return View(checkoutViewModel);
            return PartialView(checkoutViewModel);
        }
     

        
        public ActionResult Partial_Item_Cart()
        {
            ShoppingCart cart = (ShoppingCart)Session["Cart"];
            if (cart != null && cart.Items.Any())
            {
                return PartialView(cart.Items);
            }
            return PartialView();
        }
        public ActionResult ShowCount()
        {
            ShoppingCart cart = (ShoppingCart)Session["Cart"];
            if (cart != null)
            {
                return Json(new { Count = cart.Items.Count }, JsonRequestBehavior.AllowGet);

            }
            return Json(new { Count = 0 }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult AddToCart(int id, int quantity)
        {
            var code = new { Success = false, msg = "", code = -1, Count = 0 };
            var db = new QL_DCAN2Entities();
            var checkProduct = db.SanPhams.FirstOrDefault(x => x.MaSP == id);
            if (checkProduct != null)
            {
                ShoppingCart cart = (ShoppingCart)Session["Cart"];
                if (cart == null)
                {
                    cart = new ShoppingCart();
                }

                ShoppingCartItem item = new ShoppingCartItem
                {
                    ProductId = checkProduct.MaSP,
                    ProductName = checkProduct.TenSP,
                    CateName = checkProduct.LoaiSP.TenLoai,
                    Quantity = quantity
                };
                item.ProductImg = checkProduct.HinhAnh;
                item.Price = (decimal)checkProduct.DonGia;
                item.TotalPrice = item.Quantity * item.Price;
                cart.AddToCart(item, quantity);
                Session["cart"] = cart;
                code = new { Success = true, msg = "Thêm Sản Phẩm vào Giỏ Hàng Thành Công", code = 1, Count = cart.Items.Count() };
            }
            return Json(code);
        }
        [HttpPost]
        public ActionResult Delete(int id)
        {
            var code = new { Success = false, msg = "", code = -1, Count = 0 };
            ShoppingCart cart = (ShoppingCart)Session["Cart"];
            if (cart != null)
            {
                var checkProduct = cart.Items.FirstOrDefault(x => x.ProductId == id);
                if (checkProduct != null)
                {
                    cart.Remove(id);
                    code = new { Success = true, msg = "", code = 1, Count = cart.Items.Count };

                }

            }
            return Json(code);
        }
        [HttpPost]
        public ActionResult DeleteAll()
        {
            ShoppingCart cart = (ShoppingCart)Session["Cart"];
            if (cart != null)
            {
                cart.ClearCart();
                return Json(new { Success = true });
            }
            return Json(new { Success = false });
        }
        [HttpPost]
        public ActionResult Update(int id, int quantity)
        {
            ShoppingCart cart = (ShoppingCart)Session["Cart"];
            if (cart != null)
            {
                cart.UpdateQuantity(id, quantity);
                return Json(new { Success = true });
            }
            return Json(new { Success = false });
        }
        [HttpGet]
        public ActionResult CheckOut()
        {
            ShoppingCart cart = (ShoppingCart)Session["Cart"];
            if (cart != null && cart.Items.Any())
            {
                return View(cart.Items);
            }

            return CheckOut();

        }
        public ActionResult CheckOutSuccess()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CheckOut(CheckoutView req)
        {

            if (ModelState.IsValid)
            {
                string fullAddress = $"{req.DiaChiGiaoHang}, {req.TenXa}, {req.TenHuyen}, {req.TenTinh}";
                ShoppingCart cart = Session["Cart"] as ShoppingCart;
                KhachHang customer = Session["KH"] as KhachHang;
                // Kiểm tra nếu giỏ hàng có tồn tại
                if (cart != null && customer != null)
                {
                    // Tạo đối tượng hóa đơn
                    HoaDon hoaDon = new HoaDon
                    {
                        MaKH = customer.MaKH,
                        DiaChiGiaoHang = fullAddress,
                       TongTien = cart.Items.Sum(x => x.Price * x.Quantity),
                       // Tính tổng tiền
                      // TongTien = 1,
                        HinhThucThanhToan = req.HinhThucThanhToan,
                        GhiChu = req.GhiChu,
                        NgayDat = DateTime.Now,
                        TinhTrang = "Chờ xác nhận",
                        MaNV = null,


                    };

                    db.HoaDons.Add(hoaDon);
                    db.SaveChanges();

                    // Lấy MaHD của hóa đơn vừa tạo
                    int maHD = hoaDon.MaHD;

                    // Thêm chi tiết hóa đơn từ giỏ hàng
                    foreach (var item in cart.Items)
                    {
                        ChiTietHoaDon chiTietHoaDon = new ChiTietHoaDon
                        {
                            MaHD = maHD,
                            MaSP = item.ProductId,
                            SoLuong = item.Quantity,
                            DonGia = item.Price,
                        };
                        db.ChiTietHoaDons.Add(chiTietHoaDon);
                    }

                    db.SaveChanges();
                    cart.ClearCart();
                    return RedirectToAction("CheckOutSuccess"); // Chuyển hướng tới trang thành công sau khi đặt hàng
                }
            }
            return View(req);
        }
        [HttpPost]
        public ActionResult ProceedToCheckout()
        {
            // Kiểm tra xem người dùng đã đăng nhập hay chưa
            if (Session["KH"] == null)
            {
                ViewBag.Message = "Hãy đăng nhập để đặt hàng.";
                // Nếu chưa đăng nhập, chuyển hướng đến trang login
                return RedirectToAction("Login", "Account");
            }
            else
            {
                // Nếu đã đăng nhập, chuyển hướng đến trang checkout
                return RedirectToAction("CheckOut");
            }
        }
    }
}