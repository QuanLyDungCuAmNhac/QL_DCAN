using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using SHOP_DCAN.Models;
namespace SHOP_DCAN.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        QL_DCAN2Entities db = new QL_DCAN2Entities();
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterView model)
        {
            if (ModelState.IsValid)
            {
                if (db.KhachHangs.Any(kh => kh.Email == model.Email))
                {
                    ModelState.AddModelError("", "Email đã tồn tại.");
                    return View(model);
                }

                KhachHang khachHang = new KhachHang()
                {
                   
                    TenKH = model.TenKH,
                    SDT = model.SDT,
                    Email = model.Email,
                    Username = model.Username,
                    Password = model.Password // Hash mật khẩu trước khi lưu
                };

               db.KhachHangs.Add(khachHang);
               db.SaveChanges();
            //Crypto.HashPassword(
               return RedirectToAction("Login");
            }
            return View(model);
        }

        // GET: Account/Login
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginView model)
        {
            //KhachHang kh = (KhachHang)Session["KH"];
            if (ModelState.IsValid)
            {
                var khachHang = db.KhachHangs.SingleOrDefault(c => c.Email == model.Email && c.Password == model.Password);

                if (khachHang != null)
                {
                    // Lưu thông tin khách hàng vào session
                    Session["KH"] = khachHang;

                    return RedirectToAction("Index", "Home");
                }
                else ModelState.AddModelError("", "Email hoặc mật khẩu không chính xác.");
            }
            return View(model);
        }

        // GET: Account/Logout
        [HttpGet]
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index","Home");
        }
    }
}