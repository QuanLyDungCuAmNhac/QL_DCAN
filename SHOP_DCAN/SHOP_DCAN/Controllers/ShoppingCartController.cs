using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SHOP_DCAN.Models;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text;
using System.Configuration;
using System.Security.Cryptography;
using System.Net.Http;

namespace SHOP_DCAN.Controllers
{
    public class ShoppingCartController : Controller
    {
        private static readonly HttpClient client = new HttpClient();
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
        public ActionResult CheckOutSuccess(string partnerCode, string accessKey, string requestId, string amount, string orderId, string orderInfo, string orderType, string transId, string message, string localMessage, string responseTime, string errorCode, string payType, string extraData, string signature)
        {
            System.Diagnostics.Debug.WriteLine($"partnerCode: {partnerCode}");
            System.Diagnostics.Debug.WriteLine($"accessKey: {accessKey}");
            System.Diagnostics.Debug.WriteLine($"requestId: {requestId}");
            System.Diagnostics.Debug.WriteLine($"amount: {amount}");
            System.Diagnostics.Debug.WriteLine($"orderId: {orderId}");
            System.Diagnostics.Debug.WriteLine($"orderInfo: {orderInfo}");
            System.Diagnostics.Debug.WriteLine($"orderType: {orderType}");
            System.Diagnostics.Debug.WriteLine($"transId: {transId}");
            System.Diagnostics.Debug.WriteLine($"message: {message}");
            System.Diagnostics.Debug.WriteLine($"localMessage: {localMessage}");
            System.Diagnostics.Debug.WriteLine($"responseTime: {responseTime}");
            System.Diagnostics.Debug.WriteLine($"errorCode: {errorCode}");
            System.Diagnostics.Debug.WriteLine($"payType: {payType}");
            System.Diagnostics.Debug.WriteLine($"extraData: {extraData}");
            System.Diagnostics.Debug.WriteLine($"signature: {signature}");

            // Chuỗi dữ liệu gốc để kiểm tra chữ ký
            string rawHash = $"partnerCode={partnerCode}&accessKey={accessKey}&requestId={requestId}&amount={amount}&orderId={orderId}&orderInfo={orderInfo}&orderType={orderType}&transId={transId}&message={message}&localMessage={localMessage}&responseTime={responseTime}&errorCode={errorCode}&payType={payType}&extraData={extraData}";

            // Khóa bí mật của bạn (đảm bảo khóa này giống với khóa bí mật bạn đã sử dụng để tạo chữ ký)
            string secretKey = "at67qH6mk8w5Y1nAyMoYKMWACiEi2bsa";

            // Tạo chữ ký từ chuỗi dữ liệu gốc
            string generatedSignature = GenerateSignature(rawHash, secretKey);
            System.Diagnostics.Debug.WriteLine($"generatedSignature: {generatedSignature}");

            // So sánh chữ ký tạo ra với chữ ký MoMo gửi lại
            if (generatedSignature.Equals(signature))
            {
                if (errorCode == "0")
                {
                    // Thanh toán thành công
                    ViewBag.Message = "Thanh toán thành công!";
                    // Ghi nhật ký thanh toán thành công
                    System.Diagnostics.Debug.WriteLine("Thanh toán thành công!");
                }
                else
                {
                    // Xử lý lỗi thanh toán
                    ViewBag.Message = $"Thanh toán thất bại! Mã lỗi: {errorCode}, Thông báo: {message}";
                    // Ghi nhật ký lỗi thanh toán
                    System.Diagnostics.Debug.WriteLine($"Thanh toán thất bại! Mã lỗi: {errorCode}, Thông báo: {message}");
                }
            }
            else
            {
                // Xử lý lỗi xác thực chữ ký
                ViewBag.Message = "Chữ ký không hợp lệ!";
                // Ghi nhật ký lỗi chữ ký
                System.Diagnostics.Debug.WriteLine("Chữ ký không hợp lệ!");
            }

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


        //[HttpPost]
        //public ActionResult ProceedToCheckout()
        //{
        //    // Kiểm tra xem người dùng đã đăng nhập hay chưa
        //    if (Session["KH"] == null)
        //    {
        //        ViewBag.Message = "Hãy đăng nhập để đặt hàng.";
        //        // Nếu chưa đăng nhập, chuyển hướng đến trang login
        //        return RedirectToAction("Login", "Account");
        //    }
        //    else
        //    {
        //        // Nếu đã đăng nhập, chuyển hướng đến trang checkout
        //        return RedirectToAction("CheckOut");
        //    }
        //}
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
                // Lấy thông tin khách hàng từ session
                var khachHang = Session["KH"] as KhachHang;

                // Kiểm tra xem thông tin họ tên và số điện thoại có null không
                if (string.IsNullOrEmpty(khachHang.TenKH) || string.IsNullOrEmpty(khachHang.SDT))
                {
                    ViewBag.Message = "Vui lòng cập nhật thông tin cá nhân trước khi đặt hàng.";
                    // Chuyển hướng đến trang cập nhật thông tin
                    return RedirectToAction("LichSuDonHang", "Product");
                }

                // Nếu đã đăng nhập và có đủ thông tin, chuyển hướng đến trang checkout
                return RedirectToAction("CheckOut");
            }
        }
        public static async Task<string> ExecPostRequest(string url, string jsonData)
        {
            // Tạo nội dung cho yêu cầu POST
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            // Cấu hình timeout
            client.Timeout = TimeSpan.FromSeconds(5);

            // Gửi yêu cầu POST
            HttpResponseMessage response = await client.PostAsync(url, content);

            // Đảm bảo yêu cầu thành công
            response.EnsureSuccessStatusCode();

            // Đọc nội dung phản hồi
            return await response.Content.ReadAsStringAsync();
        }
        public async Task<ActionResult> MomoPayment(CheckoutView req)
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
                    // Định dạng tổng tiền hóa đơn thành chuỗi không có phần thập phân
                    string amount = hoaDon.TongTien.ToString();

                    string endpoint = "https://test-payment.momo.vn/gw_payment/transactionProcessor";
                    string partnerCode = "MOMOBKUN20180529";
                    string accessKey = "klm05TvNBzhg7h7j";
                    string secretKey = "at67qH6mk8w5Y1nAyMoYKMWACiEi2bsa";
                    string orderInfo = "Thanh toán qua MoMo";
                    string orderId = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
                    string returnUrl = "https://localhost:44342/ShoppingCart/CheckOutSuccess"; // Thay đổi thành URL thực tế
                    string notifyUrl = "https://localhost:44342/ShoppingCart/CheckOutSuccess"; // Thay đổi thành URL thực tế
                    string bankCode = "SML";
                    string requestId = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
                    string requestType = "payWithMoMoATM";
                    string extraData = "";

                    string rawHash = $"partnerCode={partnerCode}&accessKey={accessKey}&requestId={requestId}&bankCode={bankCode}&amount={amount}&orderId={orderId}&orderInfo={orderInfo}&returnUrl={returnUrl}&notifyUrl={notifyUrl}&extraData={extraData}&requestType={requestType}";

                    string signature = GenerateSignature(rawHash, secretKey);

                    var data = new Dictionary<string, string>
            {
                { "partnerCode", partnerCode },
                { "accessKey", accessKey },
                { "requestId", requestId },
                { "amount", amount },
                { "orderId", orderId },
                { "orderInfo", orderInfo },
                { "returnUrl", returnUrl },
                { "bankCode", bankCode },
                { "notifyUrl", notifyUrl },
                { "extraData", extraData },
                { "requestType", requestType },
                { "signature", signature }
            };

                    // Gửi yêu cầu thanh toán đến MoMo
                    var jsonResponse = await ExecPostRequest(endpoint, data);
                    var jsonResult = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonResponse);

                    // Chuyển hướng đến URL thanh toán
                    return Redirect(jsonResult["payUrl"]);
                }
            }

            return View(req); // Trả về view nếu ModelState không hợp lệ hoặc giỏ hàng/khách hàng không tồn tại
        }


        private async Task<string> ExecPostRequest(string url, Dictionary<string, string> data)
        {
            var jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        private string GenerateSignature(string rawHash, string secretKey)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
            {
                var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawHash));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

    }
    }