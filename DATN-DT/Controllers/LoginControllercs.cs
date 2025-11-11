using DATN_DT.Data;
using DATN_DT.Form;
using DATN_DT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace DATN_DT.Controllers
{
    public class LoginController : Controller
    {
        private readonly MyDbContext _con;
        public LoginController(MyDbContext context)
        {
            _con = context;
        }

        // ======= Đăng ký =======
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterForm model)
        {
            if (await _con.KhachHangs.AnyAsync(u => u.EmailKhachHang == model.EmailKhachHang))
            {
                return BadRequest(new { Success = false, Message = "Email đã được đăng ký." });
            }

            var passwordHash = HashPassword(model.Password!);

            var user = new KhachHang
            {
                HoTenKhachHang = model.HoTenKhachHang,
                EmailKhachHang = model.EmailKhachHang,
                Password = passwordHash,
                SdtKhachHang = model.SdtKhachHang,
                DiaChiKhachHang = model.DiaChiKhachHang,
                DiemTichLuy = 0,
                TrangThaiKhachHang = 0
            };

            _con.KhachHangs.Add(user);
            await _con.SaveChangesAsync();

            return Ok(new { Success = true, Message = "Đăng ký thành công!" });
        }

        // ======= Đăng nhập =======
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginForm model)
        {
            if (model == null)
            {
                return BadRequest(new { Success = false, Message = "Dữ liệu không hợp lệ." });
            }
            var user = await _con.KhachHangs
                .FirstOrDefaultAsync(u => u.EmailKhachHang == model.EmailKhachHang);

            if (user == null || !VerifyPassword(model.Password!, user.Password!))
            {
                return BadRequest(new { Success = false, Message = "Email hoặc mật khẩu không chính xác." });
            }

            if (user.TrangThaiKhachHang == 1)
            {
                return BadRequest(new { Success = false, Message = "Tài khoản đã bị khóa." });
            }

            return Ok(new
            {
                Success = true,
                Message = "Đăng nhập thành công!",
                Data = new
                {
                    user.HoTenKhachHang,
                    user.EmailKhachHang,
                    user.SdtKhachHang,
                    user.DiaChiKhachHang,
                    user.DiemTichLuy
                }
            });
        }

        // ========== ĐĂNG KÝ NHÂN VIÊN ==========
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterNhanVienForm model)
        {
            if (string.IsNullOrEmpty(model.TenTaiKhoanNV) || string.IsNullOrEmpty(model.Password))
            {
                return BadRequest(new { Success = false, Message = "Tên tài khoản và mật khẩu không được để trống." });
            }

            var existed = await _con.NhanViens
                .FirstOrDefaultAsync(nv => nv.TenTaiKhoanNV == model.TenTaiKhoanNV);

            if (existed != null)
            {
                return BadRequest(new { Success = false, Message = "Tên tài khoản đã tồn tại." });
            }

            var newNhanVien = new NhanVien
            {
                TenTaiKhoanNV = model.TenTaiKhoanNV,
                Password = HashPassword(model.Password),
                HoTenNhanVien = model.HoTenNhanVien,
                IdChucVu = model.IdChucVu,
                SdtNhanVien = model.SdtNhanVien,
                EmailNhanVien = model.EmailNhanVien,
                DiaChiNV = model.DiaChiNV,
                NgayVaoLam = model.NgayVaoLam ?? DateTime.Now,
                TrangThaiNV = 1 // Mặc định trạng thái là "Đang làm việc"
            };

            _con.NhanViens.Add(newNhanVien);
            await _con.SaveChangesAsync();

            return Ok(new { Success = true, Message = "Đăng ký nhân viên thành công." });
        }

        // ========== ĐĂNG NHẬP NHÂN VIÊN ==========
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginNhanVienForm model)
        {
            if (string.IsNullOrEmpty(model.TenTaiKhoanNV) || string.IsNullOrEmpty(model.Password))
            {
                return BadRequest(new { Success = false, Message = "Tên tài khoản hoặc mật khẩu không được để trống." });
            }

            var nv = await _con.NhanViens
                .FirstOrDefaultAsync(u => u.TenTaiKhoanNV == model.TenTaiKhoanNV);

            if (nv == null || !VerifyPassword(model.Password, nv.Password))
            {
                return BadRequest(new { Success = false, Message = "Tên tài khoản hoặc mật khẩu không đúng." });
            }

            if (nv.TrangThaiNV == 0)
            {
                return BadRequest(new { Success = false, Message = "Tài khoản đã bị khóa." });
            }

            return Ok(new
            {
                Success = true,
                Message = "Đăng nhập thành công.",
                Data = new
                {
                    nv.IdNhanVien,
                    nv.HoTenNhanVien,
                    nv.EmailNhanVien,
                    nv.SdtNhanVien,
                    nv.IdChucVu,
                    nv.TrangThaiNV
                }
            });
        }


        // ======= Hàm băm mật khẩu =======
        private string HashPassword(string password)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }

        // ======= Hàm kiểm tra mật khẩu =======
        private bool VerifyPassword(string password, string storedHash)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput == storedHash;
        }
    }
}
