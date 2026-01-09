namespace DATN_DT.Models
{
    public class BaoHanh
    {
        public int IdBaoHanh { get; set; }
        public int? IdImei { get; set; }
        public int? IdKhachHang { get; set; }
        public int? IdNhanVien { get; set; }
        public DateTime? NgayNhan { get; set; }
        public DateTime? NgayTra { get; set; }
        public string? TrangThai { get; set; }
        public string? MoTaLoi { get; set; }
        public string? XuLy { get; set; }
        public decimal? ChiPhiPhatSinh { get; set; }
        public string? LoaiBaoHanh { get; set; } // "Mới mua", "Sửa chữa", "Đổi máy"



        // Navigation properties
        public Imei? Imei { get; set; }
        public KhachHang? KhachHang { get; set; }
        public NhanVien? NhanVien { get; set; }
    }
}
