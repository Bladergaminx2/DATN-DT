

using System.ComponentModel.DataAnnotations;
namespace DATN_DT.Form
{
    public class LoginForm
    {
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string EmailKhachHang { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterModel
    {
        [Required(ErrorMessage = "Họ tên không được để trống")]
        public string HoTenKhachHang { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string EmailKhachHang { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Xác nhận mật khẩu không được để trống")]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string SdtKhachHang { get; set; } = string.Empty;

        [Required(ErrorMessage = "Địa chỉ không được để trống")]
        public string DiaChiKhachHang { get; set; } = string.Empty;
    }

    public class LoginNhanVienModel
    {
        [Required(ErrorMessage = "Tên tài khoản không được để trống")]
        public string TenTaiKhoanNV { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        public string Password { get; set; } = string.Empty;
    }
}
