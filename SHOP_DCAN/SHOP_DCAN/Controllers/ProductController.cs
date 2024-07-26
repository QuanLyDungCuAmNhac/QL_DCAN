using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
namespace SHOP_DCAN.Controllers
{
    public class ProductController : Controller
    {
        // GET: Product
        QL_DCAN2Entities db = new QL_DCAN2Entities();
        public ActionResult Index(string query, int? page)
        {
            //IEnumerable<SanPham> items = db.SanPhams.OrderByDescending(x => x.MaSP);
            //var pageSize = 5;
            //if (page == null)
            //{
            //    page = 1;
            //}
            //var pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            //items = items.ToPagedList(pageIndex, pageSize);
            ////List<SANPHAM> list = db.SANPHAMs.ToList();
            //return View(items);
            var products = string.IsNullOrEmpty(query) ? db.SanPhams.OrderByDescending(x => x.MaSP) :
                                                    db.SanPhams.Where(sp => sp.TenSP.Contains(query))
                                                               .OrderByDescending(x => x.MaSP);

            var pageSize = 5;
            var pageIndex = page ?? 1;

            ViewBag.Query = query; // Giữ lại từ khóa tìm kiếm để hiển thị trong form

            return View(products.ToPagedList(pageIndex, pageSize));
        } 
        public ActionResult ProductDetail(int id)
        {
            SanPham item = db.SanPhams.Single(x => x.MaSP == id);
            return View(item);
        }
        public ActionResult SapXepTheoTen()
        {
            var item = db.SanPhams.OrderBy(p => p.TenSP).ToList();
            return View(item);
        }
        public ActionResult Product_Loai(int id)
        {
            var item = db.SanPhams.ToList();
            if (id > 0)
            {
                item = item.Where(x => x.MaLoai == id).ToList();
            }
            var cate = db.LoaiSPs.Find(id);
            if (cate != null)
            {
                ViewBag.CateName = cate.TenLoai;
            }
            ViewBag.CateId = id;
            return View(item);
        }
        public ActionResult Product_ThuongHieu(int id)
        {
            var item = db.SanPhams.ToList();
            if (id > 0)
            {
                item = item.Where(x => x.MaTH == id).ToList();
            }
            var th = db.ThuongHieux.Find(id);
            if (th != null)
            {
                ViewBag.TenTH = th.TenTH;
            }
            ViewBag.MaTH = id;
            return View(item);
        }
        public ActionResult Partial_ItemByCateId()
        {
            var items = db.SanPhams.Where(x => x.TrangThai == 1).ToList();
            return PartialView(items);
        }
        public ActionResult MenuLoai()
        {
            var items = db.LoaiSPs.OrderBy(x => x.MaLoai).ToList();
            return PartialView("MenuLoai", items);
        }
        public ActionResult MenuThuongHieu()
        {
            var items = db.ThuongHieux.OrderBy(x => x.MaTH).ToList();
            return PartialView("MenuThuongHieu", items);
        }
    }
}