namespace DATN_DT.Models
{
    public class HoaDon
    {
        public int IdHoaDon { get; set; }
        public int? IdKhachHang { get; set; }
        public string? HoTenNguoiNhan { get; set; }
        public string? SdtKhachHang { get; set; }
        public int? IdNhanVien { get; set; }
        public string? TrangThaiHoaDon { get; set; }
        public decimal? TongTien { get; set; }
        public DateTime? NgayLapHoaDon { get; set; }
        public string? PhuongThucThanhToan { get; set; }



        // Navigation properties
        public KhachHang? KhachHang { get; set; }
        public NhanVien? NhanVien { get; set; }

        public ICollection<HoaDonChiTiet>? HoaDonChiTiets { get; set; }
    }
}
