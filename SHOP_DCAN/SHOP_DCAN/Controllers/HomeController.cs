using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SHOP_DCAN.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        QL_DCAN2Entities db = new QL_DCAN2Entities();
        public ActionResult Index()
        {
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
    }
}