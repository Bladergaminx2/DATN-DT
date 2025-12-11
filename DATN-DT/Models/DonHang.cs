namespace DATN_DT.Models
{
    using DATN_DT.Models;

    public class DonHang
    {
        public int IdDonHang { get; set; }
        public int? IdKhachHang { get; set; }
        public string HoTenNguoiNhan { get; set; } = string.Empty;
        public string SdtNguoiNhan { get; set; } = string.Empty;
        public string? MaDon { get; set; }
        public DateTime? NgayDat { get; set; }
        public string? DiaChiGiaoHang { get; set; }
        public string? TrangThaiHoaDon { get; set; }
        public string? GhiChu { get; set; }
        public string? PhuongThucThanhToan { get; set; }
        public int? TrangThaiDH { get; set; }



        // Navigation properties
        public KhachHang? KhachHang { get; set; }
        public ICollection<DonHangChiTiet>? DonHangChiTiets { get; set; }
    }
}
