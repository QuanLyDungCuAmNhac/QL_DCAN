using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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

                // Băm mật khẩu trước khi lưu
                string hashedPassword = Crypto.HashPassword(model.Password);

                KhachHang khachHang = new KhachHang()
                {
                    TenKH = model.TenKH,
                    SDT = model.SDT,
                    Email = model.Email,
                    Username = model.Username,
                    Password = hashedPassword // Lưu mật khẩu đã băm
                };

                db.KhachHangs.Add(khachHang);
                db.SaveChanges();

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
        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginView model)
        {
            if (ModelState.IsValid)
            {
                var khachHang = db.KhachHangs.SingleOrDefault(c => c.Email == model.Email);

                if (khachHang != null && Crypto.VerifyHashedPassword(khachHang.Password, model.Password))
                {
                    // Lưu thông tin khách hàng vào session
                    Session["KH"] = khachHang;

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Email hoặc mật khẩu không chính xác.");
                }
            }
            return View(model);
        }


        // GET: Account/Logout
        [HttpGet]
        public ActionResult Logout()
        {
            Session["KH"] = null;
            Session[Constants.USER_SESSION] = null;

            // Chuyển hướng đến trang chủ hoặc trang đăng nhập
            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        // POST: Account/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = db.KhachHangs.SingleOrDefault(k => k.Email == model.Email);
                if (user != null)
                {
                    // Generate a password reset link
                    string resetLink = Url.Action("ResetPassword", "Account", new { email = model.Email }, protocol: Request.Url.Scheme);
                    string subject = "Khôi phục mật khẩu";
                    string content = $"Nhấp vào liên kết sau để khôi phục mật khẩu của bạn: <a href='{resetLink}'>Khôi phục mật khẩu</a>";

                    // Send the email
                    bool emailSent =  Common.SendMail("SHOP_DCAN", subject, content, model.Email);

                    if (emailSent)
                    {
                        return RedirectToAction("ForgotPasswordConfirmation");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Không thể gửi email khôi phục mật khẩu.");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Email không tồn tại.");
                }
            }
            return View(model);
        }

        // GET: Account/ResetPassword
        [HttpGet]
        public ActionResult ResetPassword(string email)
        {
            // Validate that email is provided
            if (string.IsNullOrEmpty(email))
            {
                return HttpNotFound();
            }
            var model = new ResetPasswordViewModel { Email = email };
            return View(model);
        }

        // POST: Account/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = db.KhachHangs.SingleOrDefault(k => k.Email == model.Email);
                if (user != null)
                {
                    user.Password = Crypto.HashPassword(model.NewPassword);
                    db.SaveChanges();

                    return RedirectToAction("ResetPasswordConfirmation");
                }
                else
                {
                    ModelState.AddModelError("", "Email không tồn tại.");
                }
            }
            return View(model);
        }

        // GET: Account/ForgotPasswordConfirmation
        [HttpGet]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        // GET: Account/ResetPasswordConfirmation
        [HttpGet]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }
        string clientid = ConfigurationManager.AppSettings["GgAppId"];
        string redirection_url = ConfigurationManager.AppSettings["redirectUri"];
        public void LoginGoogle()
        {
            string urls = "https://accounts.google.com/o/oauth2/v2/auth?scope=email&include_granted_scopes=true&redirect_uri=" + redirection_url + "&response_type=code&client_id=" + clientid + "";
            Response.Redirect(urls);
        }
      
    }
 
}