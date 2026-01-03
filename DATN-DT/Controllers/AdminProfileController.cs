using DATN_DT.Data;
using DATN_DT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace DATN_DT.Controllers
{
    public class AdminProfileController : Controller
    {
        private readonly MyDbContext _context;

        public AdminProfileController(MyDbContext context)
        {
            _context = context;
        }

        // GET: Hiển thị trang profile admin
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // GET: API - Lấy thông tin admin hiện tại
        [HttpGet]
        [Route("AdminProfile/GetProfileData")]
        public async Task<IActionResult> GetProfileData()
        {
            try
            {
                var nhanVienId = GetCurrentNhanVienId();
                if (!nhanVienId.HasValue)
                {
                    return Unauthorized(new { message = "Không tìm thấy thông tin người dùng" });
                }

                var nhanVien = await _context.NhanViens
                    .Include(nv => nv.ChucVu)
                    .FirstOrDefaultAsync(nv => nv.IdNhanVien == nhanVienId.Value);

                if (nhanVien == null)
                {
                    return NotFound(new { message = "Không tìm thấy thông tin nhân viên" });
                }

                return Ok(new
                {
                    idNhanVien = nhanVien.IdNhanVien,
                    tenTaiKhoanNV = nhanVien.TenTaiKhoanNV,
                    hoTenNhanVien = nhanVien.HoTenNhanVien,
                    sdtNhanVien = nhanVien.SdtNhanVien,
                    emailNhanVien = nhanVien.EmailNhanVien,
                    diaChiNV = nhanVien.DiaChiNV,
                    ngayVaoLam = nhanVien.NgayVaoLam,
                    trangThaiNV = nhanVien.TrangThaiNV == 1 ? "Hoạt động" : "Không hoạt động",
                    tenChucVu = nhanVien.ChucVu?.TenChucVu ?? "N/A"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        // POST: API - Cập nhật thông tin admin
        [HttpPost]
        [Route("AdminProfile/UpdateProfileData")]
        public async Task<IActionResult> UpdateProfileData([FromBody] UpdateAdminProfileModel model)
        {
            try
            {
                var nhanVienId = GetCurrentNhanVienId();
                if (!nhanVienId.HasValue)
                {
                    return Unauthorized(new { message = "Không tìm thấy thông tin người dùng" });
                }

                var nhanVien = await _context.NhanViens
                    .FirstOrDefaultAsync(nv => nv.IdNhanVien == nhanVienId.Value);

                if (nhanVien == null)
                {
                    return NotFound(new { message = "Không tìm thấy thông tin nhân viên" });
                }

                // Validation
                if (string.IsNullOrWhiteSpace(model.HoTenNhanVien))
                {
                    return BadRequest(new { message = "Họ và tên không được để trống" });
                }

                if (string.IsNullOrWhiteSpace(model.SdtNhanVien))
                {
                    return BadRequest(new { message = "Số điện thoại không được để trống" });
                }

                // Cập nhật thông tin
                nhanVien.HoTenNhanVien = model.HoTenNhanVien.Trim();
                nhanVien.SdtNhanVien = model.SdtNhanVien.Trim();
                nhanVien.EmailNhanVien = string.IsNullOrWhiteSpace(model.EmailNhanVien) ? null : model.EmailNhanVien.Trim();
                nhanVien.DiaChiNV = string.IsNullOrWhiteSpace(model.DiaChiNV) ? null : model.DiaChiNV.Trim();

                await _context.SaveChangesAsync();

                return Ok(new { message = "Cập nhật thông tin thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        // POST: API - Đổi mật khẩu
        [HttpPost]
        [Route("AdminProfile/ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
        {
            try
            {
                var nhanVienId = GetCurrentNhanVienId();
                if (!nhanVienId.HasValue)
                {
                    return Unauthorized(new { message = "Không tìm thấy thông tin người dùng" });
                }

                var nhanVien = await _context.NhanViens
                    .FirstOrDefaultAsync(nv => nv.IdNhanVien == nhanVienId.Value);

                if (nhanVien == null)
                {
                    return NotFound(new { message = "Không tìm thấy thông tin nhân viên" });
                }

                // Kiểm tra mật khẩu hiện tại
                if (!VerifyPassword(model.CurrentPassword, nhanVien.Password))
                {
                    return BadRequest(new { message = "Mật khẩu hiện tại không đúng" });
                }

                // Validation mật khẩu mới
                if (string.IsNullOrWhiteSpace(model.NewPassword))
                {
                    return BadRequest(new { message = "Mật khẩu mới không được để trống" });
                }

                if (model.NewPassword.Length < 6)
                {
                    return BadRequest(new { message = "Mật khẩu mới phải có ít nhất 6 ký tự" });
                }

                if (model.NewPassword != model.ConfirmPassword)
                {
                    return BadRequest(new { message = "Mật khẩu xác nhận không khớp" });
                }

                // Cập nhật mật khẩu
                nhanVien.Password = HashPassword(model.NewPassword);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Đổi mật khẩu thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        // Helper: Lấy IdNhanVien từ JWT token
        private int? GetCurrentNhanVienId()
        {
            try
            {
                var token = Request.Cookies["jwt"];
                if (string.IsNullOrEmpty(token))
                {
                    System.Diagnostics.Debug.WriteLine("JWT token is null or empty");
                    return null;
                }

                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadJwtToken(token);
                
                // Debug: Log tất cả claims
                System.Diagnostics.Debug.WriteLine("JWT Claims:");
                foreach (var claim in jsonToken.Claims)
                {
                    System.Diagnostics.Debug.WriteLine($"  {claim.Type} = {claim.Value}");
                }
                
                // Thử lấy IdNhanVien từ claim
                var nhanVienIdClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "IdNhanVien");
                if (nhanVienIdClaim != null && int.TryParse(nhanVienIdClaim.Value, out int nhanVienId))
                {
                    System.Diagnostics.Debug.WriteLine($"Found IdNhanVien from claim: {nhanVienId}");
                    return nhanVienId;
                }

                // Fallback: Lấy từ TenTaiKhoanNV (username)
                var usernameClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name || c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name");
                if (usernameClaim != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Found username from claim: {usernameClaim.Value}");
                    var nhanVien = _context.NhanViens
                        .FirstOrDefault(nv => nv.TenTaiKhoanNV == usernameClaim.Value);
                    if (nhanVien != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Found NhanVien by username: {nhanVien.IdNhanVien}");
                        return nhanVien.IdNhanVien;
                    }
                }
                
                System.Diagnostics.Debug.WriteLine("Could not find IdNhanVien from token");
            }
            catch (Exception ex)
            {
                // Log error
                System.Diagnostics.Debug.WriteLine($"Error getting NhanVienId: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
            return null;
        }

        // Helper: Mã hóa mật khẩu (giống LoginController)
        private string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        // Helper: Kiểm tra mật khẩu
        private bool VerifyPassword(string password, string hashedPassword)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput == hashedPassword;
        }

        // Model classes
        public class UpdateAdminProfileModel
        {
            public string HoTenNhanVien { get; set; } = string.Empty;
            public string SdtNhanVien { get; set; } = string.Empty;
            public string? EmailNhanVien { get; set; }
            public string? DiaChiNV { get; set; }
        }

        public class ChangePasswordModel
        {
            public string CurrentPassword { get; set; } = string.Empty;
            public string NewPassword { get; set; } = string.Empty;
            public string ConfirmPassword { get; set; } = string.Empty;
        }
    }
}

