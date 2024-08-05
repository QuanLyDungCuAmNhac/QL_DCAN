using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SHOP_DCAN.Models
{
    public class EditKhachHang
    {
        public int MaKH { get; set; } // Thêm MaKH để xác định khách hàng
        public string TenKH { get; set; }
        public string SDT { get; set; }
    }
}