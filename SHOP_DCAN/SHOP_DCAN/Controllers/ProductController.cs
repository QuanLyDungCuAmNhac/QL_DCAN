using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using SHOP_DCAN.Models;
namespace SHOP_DCAN.Controllers
{
    public class ProductController : Controller
    {
        // GET: Product
        QL_DCAN2Entities db = new QL_DCAN2Entities();
        private readonly CloudinaryService _cloudinaryService;
        public ActionResult Index(string query, int? page)
        {
           
            var products = string.IsNullOrEmpty(query) ? db.SanPhams.OrderByDescending(x => x.MaSP) :
                                                    db.SanPhams.Where(sp => sp.TenSP.Contains(query))
                                                               .OrderByDescending(x => x.MaSP);

            var pageSize = 6;
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
        private double ComputeCosineSimilarity(SanPham product1, SanPham product2)
        {
            var vector1 = new List<double> { (int)product1.MaLoai, (int)product1.MaTH, (double)product1.DonGia };
            var vector2 = new List<double> { (int)product2.MaLoai, (int)product2.MaTH, (double)product2.DonGia };

            double dotProduct = 0.0;
            double magnitude1 = 0.0;
            double magnitude2 = 0.0;

            for (int i = 0; i < vector1.Count; i++)
            {
                dotProduct += vector1[i] * vector2[i];
                magnitude1 += Math.Pow(vector1[i], 2);
                magnitude2 += Math.Pow(vector2[i], 2);
            }

            magnitude1 = Math.Sqrt(magnitude1);
            magnitude2 = Math.Sqrt(magnitude2);

            if (magnitude1 == 0 || magnitude2 == 0)
            {
                return 0;
            }

            return dotProduct / (magnitude1 * magnitude2);
        }
        private List<SanPham> GetUserLikedProducts(int userId)
        {
            var purchasedProducts = db.HoaDons.Where(hd => hd.MaKH == userId)
                                              .SelectMany(hd => hd.ChiTietHoaDons)
                                              .Select(ct => ct.SanPham)
                                              .Distinct()
                                              .ToList();

            var userLikedProducts = purchasedProducts.ToList();

            return userLikedProducts;
        }
        //private List<SanPham> GetItemBasedRecommendations(List<SanPham> likedProducts, int topN = 10)
        //{
        //    var allProducts = db.SanPhams.ToList();
        //    var similarityScores = new Dictionary<SanPham, double>();

        //    foreach (var likedProduct in likedProducts)
        //    {
        //        foreach (var product in allProducts)
        //        {
        //            if (!likedProducts.Contains(product))
        //            {
        //                double similarity = ComputeCosineSimilarity(likedProduct, product);
        //                if (similarityScores.ContainsKey(product))
        //                {
        //                    similarityScores[product] += similarity;
        //                }
        //                else
        //                {
        //                    similarityScores[product] = similarity;
        //                }
        //            }
        //        }
        //    }

        //    var recommendedProducts = similarityScores.OrderByDescending(x => x.Value)
        //                                               .Take(topN)
        //                                               .Select(x => x.Key)
        //                                               .ToList();

        //    return recommendedProducts;
        //}
        //public ActionResult ItemBasedRecommendations()
        //{
        //    var khachHang = Session["KH"] as KhachHang;
        //    if (khachHang == null)
        //    {
        //        return PartialView("_NoRecommendations"); // Trả về view hiển thị không có gợi ý
        //    }

        //    int userId = khachHang.MaKH;
        //    var likedProducts = GetUserLikedProducts(userId);
        //    var recommendedProducts = GetItemBasedRecommendations(likedProducts);

        //    return PartialView("ItemBasedRecommendations", recommendedProducts); // Trả về partial view với sản phẩm gợi ý
        //}
        private List<SanPham> GetItemBasedRecommendations(List<SanPham> likedProducts, int k = 5, int topN = 10)
        {
            var allProducts = db.SanPhams.ToList();

            var kmeans = new KMeans(k, allProducts);

            var likedClusters = new HashSet<Cluster>();
            foreach (var likedProduct in likedProducts)
            {
                foreach (var cluster in kmeans.Clusters)
                {
                    if (cluster.Products.Contains(likedProduct))
                    {
                        likedClusters.Add(cluster);
                        break;
                    }
                }
            }
            var recommendedProducts = new List<SanPham>();
            foreach (var cluster in likedClusters)
            {
                recommendedProducts.AddRange(cluster.Products.Where(p => !likedProducts.Contains(p)));
            }
            return recommendedProducts.Distinct().Take(topN).ToList();
        }

        public ActionResult ItemBasedRecommendations()
        {
            var khachHang = Session["KH"] as KhachHang;
            if (khachHang == null)
            {
                return PartialView("_NoRecommendations"); 
            }

            int userId = khachHang.MaKH;
            var likedProducts = GetUserLikedProducts(userId);
            var recommendedProducts = GetItemBasedRecommendations(likedProducts);

            return PartialView("ItemBasedRecommendations", recommendedProducts);
        }
        public ActionResult LichSuDonHang()
        {
            var khachHang = Session["KH"] as KhachHang;
            if (khachHang == null)
            {
                // Xử lý khi khách hàng chưa đăng nhập (ví dụ: chuyển hướng đến trang đăng nhập)
                return RedirectToAction("_NoOrder");
            }

            int userId = khachHang.MaKH;
            var orders = db.HoaDons.Where(hd => hd.MaKH == userId).ToList();

            return View(orders);
        }
        public ActionResult _NoOrder()
        {
            return View();
        }
        public ActionResult ChiTietDonHang(int id)
        {
            var khachHang = Session["KH"] as KhachHang;
            if (khachHang == null)
            {
                // Xử lý khi khách hàng chưa đăng nhập (ví dụ: chuyển hướng đến trang đăng nhập)
                return RedirectToAction("Login", "Account");
            }

            var order = db.HoaDons.SingleOrDefault(hd => hd.MaHD == id && hd.MaKH == khachHang.MaKH);
            if (order == null)
            {
                return HttpNotFound();
            }

            var orderDetails = db.ChiTietHoaDons.Where(ct => ct.MaHD == id).ToList();
            ViewBag.Order = order;
            return View(orderDetails);
        }
        private List<SanPham> GetContentBasedRecommendations(List<SanPham> likedProducts)
        {
            var recommendedProducts = new List<SanPham>();

            foreach (var product in likedProducts)
            {
                // Lấy các sản phẩm có cùng loại hoặc cùng thương hiệu, nhưng khác sản phẩm hiện tại
                var similarProducts = db.SanPhams.Where(sp => (sp.MaLoai == product.MaLoai || sp.MaTH == product.MaTH) && sp.MaSP != product.MaSP)
                                                 .ToList();

                recommendedProducts.AddRange(similarProducts);
            }

            // Loại bỏ các sản phẩm trùng lặp
            recommendedProducts = recommendedProducts.Distinct().ToList();

            return recommendedProducts;
        }
        public ActionResult Partial_RecommendedProducts()
        {
            var khachHang = Session["KH"] as KhachHang;
            if (khachHang == null)
            {
                // Xử lý khi khách hàng chưa đăng nhập (ví dụ: chuyển hướng đến trang đăng nhập)
                return PartialView("_NoRecommendations");
            }

            int userId = khachHang.MaKH;
            var likedProducts = GetUserLikedProducts(userId);
            var recommendedProducts = GetContentBasedRecommendations(likedProducts);

            return PartialView(recommendedProducts);
        }




    }
}