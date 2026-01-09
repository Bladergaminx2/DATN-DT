using DATN_DT.Data;
using DATN_DT.Form;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace DATN_DT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly string _jwtKey = "your-secret-key-123456789"; // nên lưu trong appsettings.json

        public AuthController(MyDbContext context)
        {
            _context = context;
        }

        // ========== Đăng nhập khách hàng ==========
        [HttpPost("login-customer")]
        public async Task<IActionResult> LoginCustomer([FromBody] LoginForm model)
        {
            var kh = await _context.KhachHangs.FirstOrDefaultAsync(x => x.EmailKhachHang == model.EmailKhachHang);
            if (kh == null || !VerifyPassword(model.Password, kh.Password))
                return BadRequest(new { Success = false, Message = "Sai tài khoản hoặc mật khẩu" });

            if (kh.TrangThaiKhachHang == 0)
                return BadRequest(new { Success = false, Message = "Tài khoản bị khóa" });

            var token = GenerateJwtToken(kh.EmailKhachHang, "KhachHang");

            return Ok(new
            {
                Success = true,
                Message = "Đăng nhập thành công",
                Token = token,
                Role = "KhachHang",
                Data = new
                {
                    kh.HoTenKhachHang,
                    kh.EmailKhachHang,
                    kh.SdtKhachHang
                }
            });
        }

        // ========== Đăng nhập nhân viên ==========
        [HttpPost("login-staff")]
        public async Task<IActionResult> LoginStaff([FromBody] LoginNhanVienForm model)
        {
            // Cho phép đăng nhập bằng TenTaiKhoanNV hoặc EmailNhanVien
            var loginInput = model.TenTaiKhoanNV?.Trim() ?? string.Empty;
            Models.NhanVien? nv = null;

            // Kiểm tra xem input có phải là email không
            if (!string.IsNullOrEmpty(loginInput) && loginInput.Contains("@"))
            {
                // Tìm theo EmailNhanVien
                nv = await _context.NhanViens.FirstOrDefaultAsync(x => x.EmailNhanVien == loginInput);
            }
            else
            {
                // Tìm theo TenTaiKhoanNV
                nv = await _context.NhanViens.FirstOrDefaultAsync(x => x.TenTaiKhoanNV == loginInput);
            }

            // Nếu không tìm thấy theo cách trên, thử tìm theo cả hai
            if (nv == null)
            {
                nv = await _context.NhanViens.FirstOrDefaultAsync(x => 
                    x.TenTaiKhoanNV == loginInput || x.EmailNhanVien == loginInput);
            }

            if (nv == null || !VerifyPassword(model.Password, nv.Password))
                return BadRequest(new { Success = false, Message = "Sai tài khoản hoặc mật khẩu" });

            if (nv.TrangThaiNV == 0)
                return BadRequest(new { Success = false, Message = "Tài khoản bị khóa" });

            var token = GenerateJwtToken(nv.TenTaiKhoanNV, "NhanVien");

            return Ok(new
            {
                Success = true,
                Message = "Đăng nhập thành công",
                Token = token,
                Role = "NhanVien",
                Data = new
                {
                    nv.HoTenNhanVien,
                    nv.EmailNhanVien,
                    nv.IdChucVu
                }
            });
        }

        // ========== Sinh JWT Token ==========
        private string GenerateJwtToken(string username, string role)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role)
            };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(3),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // ========== Hàm mã hóa / kiểm tra mật khẩu ==========
        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        private bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }
    }
}
