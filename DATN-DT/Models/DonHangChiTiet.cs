namespace DATN_DT.Models
{
    using DATN_DT.Models;

    public class DonHangChiTiet
    {
        public int IdDonHangChiTiet { get; set; }
        public int? IdDonHang { get; set; }
        public int? IdModelSanPham { get; set; }
        public decimal? GiaKhuyenMai { get; set; } // Giá sản phẩm sau khi áp dụng khuyến mãi
        public int? SoLuong { get; set; }
        public decimal? DonGia { get; set; }
        public decimal? ThanhTien { get; set; }



        // Navigation properties
        public DonHang? DonHang { get; set; }
        public ModelSanPham? ModelSanPham { get; set; }
    }
}
