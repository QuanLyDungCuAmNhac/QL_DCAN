using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using SHOP_DCAN.Models;
namespace SHOP_DCAN.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        QL_DCAN2Entities db = new QL_DCAN2Entities();
        public ActionResult Index()
        {
            if (Request.QueryString["code"] != null)
            {
                GoogleCallback(Request.QueryString["code"].ToString());
            }

            //if (Session[Constants.USER_SESSION] == null)
            //    return RedirectToAction("Index", "Home");
            var item = db.SanPhams.ToList();
            return View(item);
           
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult LienHe()
        {
            return View();
        }
        //public void GoogleCallback(string code)
        //{
        //    string poststring = "grant_type=authorization_code&code=" + code + "&client_id=" + clientid + "&client_secret=" + clientsecret + "&redirect_uri=" + redirection_url + "";
        //    var request = (HttpWebRequest)WebRequest.Create(url);
        //    request.ContentType = "application/x-www-form-urlencoded";
        //    request.Method = "POST";
        //    UTF8Encoding utfenc = new UTF8Encoding();
        //    byte[] bytes = utfenc.GetBytes(poststring);
        //    Stream outputstream = null;
        //    try
        //    {
        //        request.ContentLength = bytes.Length;
        //        outputstream = request.GetRequestStream();
        //        outputstream.Write(bytes, 0, bytes.Length);
        //    }
        //    catch
        //    { }
        //    var response = (HttpWebResponse)request.GetResponse();
        //    var streamReader = new StreamReader(response.GetResponseStream());
        //    string responseFromServer = streamReader.ReadToEnd();
        //    JavaScriptSerializer js = new JavaScriptSerializer();
        //    Tokenclass obj = js.Deserialize<Tokenclass>(responseFromServer);

        //    GetLogin(obj.access_token);
        //}

        ////your client id  
        //string clientid = ConfigurationManager.AppSettings["GgAppId"];
        ////your client secret  
        //string clientsecret = ConfigurationManager.AppSettings["GgAppSecret"];
        //string redirection_url = ConfigurationManager.AppSettings["redirectUri"];
        //string url = "https://accounts.google.com/o/oauth2/token";

        //public ActionResult GetLogin(string token)
        //{
        //    string url3 = "https://www.googleapis.com/oauth2/v1/userinfo?alt=json&access_token=" + token + "";
        //    WebRequest request2 = WebRequest.Create(url3);
        //    request2.Credentials = CredentialCache.DefaultCredentials;
        //    WebResponse response2 = request2.GetResponse();
        //    Stream dataStream = response2.GetResponseStream();
        //    StreamReader reader = new StreamReader(dataStream);
        //    string responseFromServer2 = reader.ReadToEnd();
        //    reader.Close();
        //    response2.Close();
        //    JavaScriptSerializer js2 = new JavaScriptSerializer();

        //    GoogleUserInfo userinfo = js2.Deserialize<GoogleUserInfo>(responseFromServer2);
        //    var resultUser = db.KhachHangs.FirstOrDefault(u => u.Email == userinfo.email);

        //    if (resultUser != null)
        //    {
        //        // Người dùng đã tồn tại, đăng nhập
        //        //var userSession = new LoginView
        //        //{
        //        //    Email = resultUser.Email
        //        //    // Không lưu mật khẩu trong session vì lý do bảo mật
        //        //};
        //        ////Session[Constants.USER_SESSION] = userSession;
        //        Session["KH"] = resultUser;
        //    }
        //    else
        //    {
        //        // Người dùng chưa tồn tại, thêm vào cơ sở dữ liệu
        //        var newUser = new KhachHang
        //        {
        //            Email = userinfo.email,
        //            TenKH = userinfo.name, // Lưu tên người dùng từ Google
        //            //Password = HashPassword(GenerateRandomPassword())
        //            // Thêm các thông tin khác nếu có
        //        };
        //        db.KhachHangs.Add(newUser);
        //        db.SaveChanges();

        //        //// Đăng nhập người dùng mới
        //        //var userSession = new LoginView
        //        //{
        //        //    Email = newUser.Email
        //        //    // Không lưu mật khẩu trong session vì lý do bảo mật
        //        //};
        //        //Session[Constants.USER_SESSION] = userSession;
        //        Session["KH"] = newUser;
        //    }

        //    //ViewBag.UserSessionKey = Constants.USER_SESSION;
        //    return View();
        //}
        public void GoogleCallback(string code)
        {
            string poststring = "grant_type=authorization_code&code=" + code + "&client_id=" + clientid + "&client_secret=" + clientsecret + "&redirect_uri=" + redirection_url + "";
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.ContentType = "application/x-www-form-urlencoded";
            request.Method = "POST";
            UTF8Encoding utfenc = new UTF8Encoding();
            byte[] bytes = utfenc.GetBytes(poststring);
            Stream outputstream = null;
            try
            {
                request.ContentLength = bytes.Length;
                outputstream = request.GetRequestStream();
                outputstream.Write(bytes, 0, bytes.Length);
            }
            catch
            { }
            var response = (HttpWebResponse)request.GetResponse();
            var streamReader = new StreamReader(response.GetResponseStream());
            string responseFromServer = streamReader.ReadToEnd();
            JavaScriptSerializer js = new JavaScriptSerializer();
            Tokenclass obj = js.Deserialize<Tokenclass>(responseFromServer);

            GetLogin(obj.access_token);
        }

        // client id của bạn
        string clientid = ConfigurationManager.AppSettings["GgAppId"];
        // client secret của bạn
        string clientsecret = ConfigurationManager.AppSettings["GgAppSecret"];
        string redirection_url = ConfigurationManager.AppSettings["redirectUri"];
        string url = "https://accounts.google.com/o/oauth2/token";

        public ActionResult GetLogin(string token)
        {
            string url3 = "https://www.googleapis.com/oauth2/v1/userinfo?alt=json&access_token=" + token + "";
            WebRequest request2 = WebRequest.Create(url3);
            request2.Credentials = CredentialCache.DefaultCredentials;
            WebResponse response2 = request2.GetResponse();
            Stream dataStream = response2.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer2 = reader.ReadToEnd();
            reader.Close();
            response2.Close();
            JavaScriptSerializer js2 = new JavaScriptSerializer();

            GoogleUserInfo userinfo = js2.Deserialize<GoogleUserInfo>(responseFromServer2);

            // Phần xử lý kết quả trả về từ Google
            var resultUser = db.KhachHangs.FirstOrDefault(u => u.Email == userinfo.email);

            if (resultUser != null)
            {
                // Người dùng đã tồn tại, đăng nhập
                Session["KH"] = resultUser;
            }
            else
            {
                // Người dùng chưa tồn tại, thêm vào cơ sở dữ liệu
                var newUser = new KhachHang
                {
                    Email = userinfo.email,
                    TenKH = userinfo.id // Lưu tên người dùng từ Google
                                          // Thêm các thông tin khác nếu có
                };
                db.KhachHangs.Add(newUser);
                db.SaveChanges();

                // Đăng nhập người dùng mới
                Session["KH"] = newUser;
            }

            // Debugging: Kiểm tra giá trị session
            //var sessionKhachHang = Session["KH"] as KhachHang;
            //System.Diagnostics.Debug.WriteLine("Session KH: " + sessionKhachHang?.Email);

            return RedirectToAction("Index", "Home");
        }

       
    }
}