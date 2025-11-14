namespace DATN_DT.Form
{
    public class KhachHangForm
    {
        public string? HoTenKhachHang { get; set; }
        public string? Password { get; set; }
        public string? SdtKhachHang { get; set; }
        public string? EmailKhachHang { get; set; }
        public string? DiaChiKhachHang { get; set; }
        public int? DiemTichLuy { get; set; }
        public int? TrangThaiKhachHang { get; set; }
    }
    public class LoginForm
    {
        public string EmailKhachHang { get; set; } = string .Empty;
        public string Password { get; set; } = string.Empty;
    }
    public class RegisterForm
    {
        public string? HoTenKhachHang { get; set; }
        public string? Password { get; set; }
        public string? SdtKhachHang { get; set; }
        public string? EmailKhachHang { get; set; }
        public string? DiaChiKhachHang { get; set; }
    }
}
