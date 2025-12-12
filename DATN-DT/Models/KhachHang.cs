namespace DATN_DT.Models
{
    public class KhachHang
    {
        public int IdKhachHang { get; set; }
        public string? HoTenKhachHang { get; set; }
        public string? Password { get; set; }
        public string? SdtKhachHang { get; set; }
        public string? EmailKhachHang { get; set; }
        public int? DiemTichLuy { get; set; }
        public string? DefaultImage { get; set; }
        public int TrangThaiKhachHang { get; set; }
        public virtual ICollection<DiaChi> Diachi { get; set; }

    }
}
