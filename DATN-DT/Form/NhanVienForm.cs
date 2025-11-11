namespace DATN_DT.Form
{
    public class NhanVienForm
    {
        public string? TenTaiKhoanNV { get; set; }
        public string? Password { get; set; }
        public string? HoTenNhanVien { get; set; }
        public int? IdChucVu { get; set; }
        public string? SdtNhanVien { get; set; }
        public string? EmailNhanVien { get; set; }
        public string? DiaChiNV { get; set; }
        public DateTime? NgayVaoLam { get; set; }
        public string? TrangThaiNV { get; set; }
    }

    public class LoginNhanVienForm
    {
        public string? TenTaiKhoanNV { get; set; }
        public string? Password { get; set; }
    }

    public class RegisterNhanVienForm
    {
        public string? TenTaiKhoanNV { get; set; }
        public string? Password { get; set; }
        public string? HoTenNhanVien { get; set; }
        public int? IdChucVu { get; set; }
        public string? SdtNhanVien { get; set; }
        public string? EmailNhanVien { get; set; }
        public string? DiaChiNV { get; set; }
        public DateTime? NgayVaoLam { get; set; }
    }
}
