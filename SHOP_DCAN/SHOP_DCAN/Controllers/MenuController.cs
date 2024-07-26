using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SHOP_DCAN.Controllers
{
    public class MenuController : Controller
    {
        // GET: Menu
        QL_DCAN2Entities db = new QL_DCAN2Entities();
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult MenuArrivals()
        {
            var items = db.LoaiSPs.ToList();
            return PartialView("MenuArrivals", items);
        }
    }
}