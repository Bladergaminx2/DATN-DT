namespace DATN_DT.Models
{
    using DATN_DT.Models;

    public class GioHangChiTiet
    {
        public int IdGioHangChiTiet { get; set; }
        public int? IdGioHang { get; set; }
        public int? IdModelSanPham { get; set; }
        public int? SoLuong { get; set; }
        public decimal? DonGia { get; set; }
        public decimal? ThanhTien { get; set; }



        // Navigation properties
        public GioHang? GioHang { get; set; }
        public ModelSanPham? ModelSanPham { get; set; }
    }
}
