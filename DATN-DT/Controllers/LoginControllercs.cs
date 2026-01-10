using DATN_DT.Data;
using DATN_DT.Form;
using DATN_DT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace DATN_DT.Controllers
{
    [AllowAnonymous]
    public class LoginController : Controller
    {
        private readonly MyDbContext _con;
        private readonly string _jwtKey;
        public LoginController(MyDbContext context, IConfiguration config)
        {
            _con = context;
            _jwtKey = config["Jwt:Key"];
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpGet]
        public IActionResult NhanVien()
        {
            return View();
        }

        // ======= Đăng ký =======
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterForm model)
        {
            try
            {
                // Kiểm tra model validation
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { Success = false, Message = "Dữ liệu không hợp lệ." });
                }

                // Kiểm tra email đã tồn tại - SỬA LỖI Ở ĐÂY
                var existingUser = await _con.KhachHangs
                    .FirstOrDefaultAsync(u => u.EmailKhachHang == model.EmailKhachHang);

                if (existingUser != null)
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
                    DiemTichLuy = 0,
                    TrangThaiKhachHang = 1
                };

                _con.KhachHangs.Add(user);
                await _con.SaveChangesAsync();

                return Ok(new { Success = true, Message = "Đăng ký thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = "Lỗi server: " + ex.Message });
            }
        }

        // ======= Đăng nhập =======
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginForm model)
        {
            try
            {
                var kh = await _con.KhachHangs.FirstOrDefaultAsync(x => x.EmailKhachHang == model.EmailKhachHang);
                if (kh == null || !VerifyPassword(model.Password, kh.Password))
                    return BadRequest(new { Success = false, Message = "Sai tài khoản hoặc mật khẩu" });

                if (kh.TrangThaiKhachHang == 0)
                    return BadRequest(new { Success = false, Message = "Tài khoản bị khóa" });

                var token = GenerateJwtToken(kh.EmailKhachHang, "KhachHang", kh.IdKhachHang);

                // Lưu token vào cookie HttpOnly
                Response.Cookies.Append("jwt", token, new CookieOptions
                {
                    HttpOnly = true,  // Không truy cập bằng JS
                    Secure = true,    // HTTPS mới gửi cookie
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.Now.AddHours(3)
                });

                // Trả về dữ liệu user và role để client hiển thị
                return Ok(new
                {
                    Success = true,
                    Message = "Đăng nhập thành công",
                    Role = "KHACHHANG",
                    Data = new
                    {
                        kh.HoTenKhachHang,
                        kh.EmailKhachHang,
                        kh.SdtKhachHang
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = "Lỗi server: " + ex.Message });
            }
        }

        // ========== ĐĂNG KÝ NHÂN VIÊN ==========
        [HttpPost]
        public async Task<IActionResult> RegisterNhanVien([FromBody] RegisterNhanVienForm model)
        {
            try
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
                    TrangThaiNV = 1
                };

                _con.NhanViens.Add(newNhanVien);
                await _con.SaveChangesAsync();

                return Ok(new { Success = true, Message = "Đăng ký nhân viên thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = "Lỗi server: " + ex.Message });
            }
        }

        // ========== ĐĂNG NHẬP NHÂN VIÊN ==========
        [HttpPost]
        public async Task<IActionResult> LoginNhanVien([FromBody] LoginNhanVienForm model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.TenTaiKhoanNV) || string.IsNullOrEmpty(model.Password))
                {
                    return BadRequest(new { Success = false, Message = "Tài khoản và mật khẩu không được để trống" });
                }

                var nv = await _con.NhanViens
                    .Include(nv => nv.ChucVu)
                    .FirstOrDefaultAsync(x => x.TenTaiKhoanNV == model.TenTaiKhoanNV);
                
                if (nv == null || !VerifyPassword(model.Password, nv.Password))
                    return BadRequest(new { Success = false, Message = "Sai tài khoản hoặc mật khẩu" });

                // TrangThaiNV: 0 = Đang làm việc, 1 = Đã nghỉ, 2 = Nghỉ phép
                // Kiểm tra trạng thái nhân viên (trừ ADMIN - ADMIN luôn được phép đăng nhập)
                var isAdmin = nv.ChucVu != null && 
                             (nv.ChucVu.TenChucVuVietHoa == "ADMIN" || nv.ChucVu.TenChucVu == "Admin");
                
                // Chỉ cho phép đăng nhập nếu TrangThaiNV = 0 (Đang làm việc) hoặc là ADMIN
                if (!isAdmin && nv.TrangThaiNV != 0)
                {
                    string message = nv.TrangThaiNV == 1 ? "Tài khoản đã nghỉ việc" : "Tài khoản đang nghỉ phép";
                    return BadRequest(new { Success = false, Message = message });
                }

                // Lấy role từ bảng ChucVu
                string role;
                if (nv.ChucVu != null && !string.IsNullOrEmpty(nv.ChucVu.TenChucVuVietHoa))
                {
                    role = nv.ChucVu.TenChucVuVietHoa;
                }
                else
                {
                    // Fallback: lấy từ database nếu chưa load
                    var cv = await _con.ChucVus.FirstOrDefaultAsync(c => c.IdChucVu == nv.IdChucVu);
                    if (cv != null && !string.IsNullOrEmpty(cv.TenChucVuVietHoa))
                    {
                        role = cv.TenChucVuVietHoa;
                    }
                    else
                    {
                        // Nếu không có chức vụ, mặc định là NHANVIEN
                        role = "NHANVIEN";
                    }
                }

                // Đảm bảo role là chữ in hoa
                role = role.ToUpper().Trim();

                // Tạo JWT token
                var token = GenerateJwtToken(nv.TenTaiKhoanNV, role, nv.IdNhanVien);

                // Lưu token vào cookie HttpOnly
                Response.Cookies.Append("jwt", token, new CookieOptions
                {
                    HttpOnly = true,  // Không truy cập bằng JS
                    Secure = false,   // Cho phép HTTP trong development (đổi thành true khi deploy production)
                    SameSite = SameSiteMode.Lax, // Lax thay vì Strict để tránh vấn đề redirect
                    Expires = DateTimeOffset.Now.AddHours(3),
                    Path = "/" // Đảm bảo cookie có sẵn cho tất cả routes
                });

                return Ok(new
                {
                    Success = true,
                    Message = "Đăng nhập thành công",
                    Role = role,
                    Data = new
                    {
                        nv.HoTenNhanVien,
                        nv.EmailNhanVien,
                        nv.IdChucVu
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = "Lỗi server: " + ex.Message });
            }
        }


        [HttpPost]
        public IActionResult Logout()
        {
            // Xoá cookie JWT
            Response.Cookies.Delete("jwt");

            // Có thể redirect về trang login hoặc trang công khai
            return Json(new
            {
                success = true,
                message = "Đăng xuất thành công",
                redirectUrl = Url.Action("Index", "Login")
            });
        }


        // ========== Sinh JWT Token ==========
        private string GenerateJwtToken(string username, string role, int idkhachhang)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role)   // luôn là chữ in hoa
            };

            // Thêm claim ID dựa trên role
            var roleUpper = role?.ToUpper() ?? "";
            if (roleUpper == "KHACHHANG" || roleUpper.Contains("KHACHHANG"))
            {
                // Nếu là khách hàng, chỉ thêm IdKhachHang
                claims.Add(new Claim("IdKhachHang", idkhachhang.ToString()));
            }
            else
            {
                // Nếu là nhân viên/admin (bất kỳ role nào khác), thêm IdNhanVien
                claims.Add(new Claim("IdNhanVien", idkhachhang.ToString()));
            }

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
