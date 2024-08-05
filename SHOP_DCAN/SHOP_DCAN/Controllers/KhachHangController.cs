using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SHOP_DCAN.Models;
namespace SHOP_DCAN.Controllers
{
    public class KhachHangController : Controller
    {
        // GET: KhachHang
        QL_DCAN2Entities db = new QL_DCAN2Entities();
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Edit()
        {
            KhachHang customer = Session["KH"] as KhachHang;
            int customerId = customer.MaKH;
           
            if (customer == null)
            {
                return HttpNotFound();
            }

            var model = new EditKhachHang
            {
                MaKH = customer.MaKH,
                TenKH = customer.TenKH,
                SDT = customer.SDT
                // Thêm các thuộc tính khác nếu cần
            };
            return View(model);
        }

        // POST: Customer/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditKhachHang model)
        {
            if (ModelState.IsValid)
            {
                var customer = db.KhachHangs.Find(model.MaKH);
                if (customer == null)
                {
                    return HttpNotFound();
                }

                customer.TenKH = model.TenKH;
                customer.SDT = model.SDT;
                // Cập nhật các thuộc tính khác nếu cần

                db.SaveChanges();
                Session["KH"] = customer;
                return RedirectToAction("LichSuDonHang","Product");
            }

            return View(model);
        }

    }
}