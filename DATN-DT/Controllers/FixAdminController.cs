using DATN_DT.CustomAttribute;
using DATN_DT.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DATN_DT.Controllers
{
    [AllowAnonymous] // Cho phép truy cập không cần đăng nhập để fix admin
    public class FixAdminController : Controller
    {
        [HttpGet("FixNhanVien")]
        public IActionResult FixNhanVienPage()
        {
            return View("FixNhanVien");
        }
    
        private readonly MyDbContext _context;

        public FixAdminController(MyDbContext context)
        {
            _context = context;
        }

        [HttpGet("FixAdmin")]
        public async Task<IActionResult> FixAdmin()
        {
            try
            {
                // 1. Tìm hoặc tạo ChucVu ADMIN
                var roleAdmin = await _context.ChucVus
                    .FirstOrDefaultAsync(r => r.TenChucVuVietHoa == "ADMIN");

                if (roleAdmin == null)
                {
                    roleAdmin = new Models.ChucVu
                    {
                        TenChucVu = "Admin",
                        TenChucVuVietHoa = "ADMIN"
                    };
                    _context.ChucVus.Add(roleAdmin);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    // Đảm bảo TenChucVuVietHoa là "ADMIN" (tất cả chữ hoa)
                    if (roleAdmin.TenChucVuVietHoa?.ToUpper() != "ADMIN")
                    {
                        roleAdmin.TenChucVuVietHoa = "ADMIN";
                        _context.ChucVus.Update(roleAdmin);
                        await _context.SaveChangesAsync();
                    }
                }

                // 2. Tìm admin account
                var admin = await _context.NhanViens
                    .Include(nv => nv.ChucVu)
                    .FirstOrDefaultAsync(nv => nv.TenTaiKhoanNV == "admin");

                if (admin == null)
                {
                    return Json(new { 
                        success = false, 
                        message = "Không tìm thấy tài khoản admin. Vui lòng tạo tài khoản admin trước." 
                    });
                }

                // 3. Cập nhật admin
                var oldRole = admin.ChucVu?.TenChucVuVietHoa ?? "null";
                admin.IdChucVu = roleAdmin.IdChucVu;
                admin.TrangThaiNV = 0; // Đang làm việc

                _context.NhanViens.Update(admin);
                await _context.SaveChangesAsync();

                // 4. Reload để lấy thông tin mới
                await _context.Entry(admin).Reference(nv => nv.ChucVu).LoadAsync();

                return Json(new 
                { 
                    success = true, 
                    message = "Đã cập nhật role admin thành công!",
                    oldRole = oldRole,
                    newRole = admin.ChucVu?.TenChucVuVietHoa,
                    adminId = admin.IdNhanVien,
                    adminName = admin.TenTaiKhoanNV,
                    note = "Vui lòng đăng xuất và đăng nhập lại để có token mới với role ADMIN"
                });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false, 
                    message = "Lỗi: " + ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        [HttpGet("CheckAdmin")]
        public async Task<IActionResult> CheckAdmin()
        {
            try
            {
                var admin = await _context.NhanViens
                    .Include(nv => nv.ChucVu)
                    .FirstOrDefaultAsync(nv => nv.TenTaiKhoanNV == "admin");

                if (admin == null)
                {
                    return Json(new { 
                        exists = false,
                        message = "Không tìm thấy tài khoản admin" 
                    });
                }

                // Kiểm tra role trong JWT token hiện tại (nếu có)
                string? tokenRole = null;
                try
                {
                    var token = Request.Cookies["jwt"];
                    if (!string.IsNullOrEmpty(token))
                    {
                        var jwtHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                        var jwtToken = jwtHandler.ReadJwtToken(token);
                        tokenRole = jwtToken.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;
                    }
                }
                catch { }

                return Json(new 
                { 
                    exists = true,
                    idNhanVien = admin.IdNhanVien,
                    tenTaiKhoan = admin.TenTaiKhoanNV,
                    hoTen = admin.HoTenNhanVien,
                    idChucVu = admin.IdChucVu,
                    tenChucVu = admin.ChucVu?.TenChucVu,
                    tenChucVuVietHoa = admin.ChucVu?.TenChucVuVietHoa,
                    trangThaiNV = admin.TrangThaiNV,
                    isAdmin = admin.ChucVu != null && (admin.ChucVu.TenChucVuVietHoa?.ToUpper() == "ADMIN" || admin.ChucVu.TenChucVu == "Admin"),
                    tokenRole = tokenRole, // Role trong JWT token hiện tại
                    tokenRoleUpper = tokenRole?.ToUpper(),
                    needRelogin = tokenRole?.ToUpper() != "ADMIN" && (admin.ChucVu?.TenChucVuVietHoa?.ToUpper() == "ADMIN" || admin.ChucVu?.TenChucVu == "Admin"),
                    message = tokenRole?.ToUpper() == "ADMIN" && (admin.ChucVu?.TenChucVuVietHoa?.ToUpper() == "ADMIN" || admin.ChucVu?.TenChucVu == "Admin")
                        ? "✅ Tất cả đã đúng! Token có role ADMIN, database cũng đúng. Bạn có thể truy cập /NhanVien"
                        : tokenRole?.ToUpper() != "ADMIN" && (admin.ChucVu?.TenChucVuVietHoa?.ToUpper() == "ADMIN" || admin.ChucVu?.TenChucVu == "Admin")
                        ? "⚠️ Database đã đúng, nhưng JWT token vẫn có role cũ. Vui lòng đăng xuất và đăng nhập lại!"
                        : admin.ChucVu?.TenChucVuVietHoa?.ToUpper() != "ADMIN" && admin.ChucVu?.TenChucVu != "Admin"
                        ? "❌ Role trong database chưa đúng. Vui lòng chạy /FixAdmin để sửa"
                        : "⚠️ Cần kiểm tra lại"
                });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    error = ex.Message 
                });
            }
        }

        [HttpGet("CheckNhanVien/{tenTaiKhoan}")]
        public async Task<IActionResult> CheckNhanVien(string tenTaiKhoan)
        {
            try
            {
                var nv = await _context.NhanViens
                    .Include(nv => nv.ChucVu)
                    .FirstOrDefaultAsync(nv => nv.TenTaiKhoanNV == tenTaiKhoan);

                if (nv == null)
                {
                    return Json(new { 
                        exists = false,
                        message = "Không tìm thấy nhân viên với tài khoản: " + tenTaiKhoan 
                    });
                }

                // Kiểm tra password có được hash chưa (hash SHA256 thường có độ dài 44 ký tự base64)
                var isPasswordHashed = !string.IsNullOrEmpty(nv.Password) && 
                                       nv.Password.Length >= 40 && 
                                       !nv.Password.Contains(" "); // Password hash không có khoảng trắng

                return Json(new 
                { 
                    exists = true,
                    idNhanVien = nv.IdNhanVien,
                    tenTaiKhoan = nv.TenTaiKhoanNV,
                    hoTen = nv.HoTenNhanVien,
                    idChucVu = nv.IdChucVu,
                    tenChucVu = nv.ChucVu?.TenChucVu,
                    tenChucVuVietHoa = nv.ChucVu?.TenChucVuVietHoa,
                    trangThaiNV = nv.TrangThaiNV,
                    trangThaiText = nv.TrangThaiNV == 0 ? "Đang làm việc" : nv.TrangThaiNV == 1 ? "Đã nghỉ" : "Nghỉ phép",
                    passwordLength = nv.Password?.Length ?? 0,
                    isPasswordHashed = isPasswordHashed,
                    canLogin = nv.TrangThaiNV == 0 || (nv.ChucVu?.TenChucVuVietHoa?.ToUpper() == "ADMIN"),
                    issues = new List<string>
                    {
                        !isPasswordHashed ? "⚠️ Password chưa được hash - cần sửa" : null,
                        nv.TrangThaiNV != 0 && nv.ChucVu?.TenChucVuVietHoa?.ToUpper() != "ADMIN" ? "⚠️ TrangThaiNV không phải 0 (Đang làm việc)" : null
                    }.Where(x => x != null).ToList()
                });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    error = ex.Message 
                });
            }
        }

        [HttpPost("FixNhanVien/{tenTaiKhoan}")]
        public async Task<IActionResult> FixNhanVien(string tenTaiKhoan, [FromBody] FixNhanVienRequest? request)
        {
            try
            {
                var nv = await _context.NhanViens
                    .Include(nv => nv.ChucVu)
                    .FirstOrDefaultAsync(nv => nv.TenTaiKhoanNV == tenTaiKhoan);

                if (nv == null)
                {
                    return Json(new { 
                        success = false,
                        message = "Không tìm thấy nhân viên với tài khoản: " + tenTaiKhoan 
                    });
                }

                var changes = new List<string>();

                // 1. Sửa password nếu chưa hash và có password mới
                var isPasswordHashed = !string.IsNullOrEmpty(nv.Password) && 
                                       nv.Password.Length >= 40 && 
                                       !nv.Password.Contains(" ");
                
                if (!isPasswordHashed && !string.IsNullOrEmpty(request?.NewPassword))
                {
                    using (var sha = System.Security.Cryptography.SHA256.Create())
                    {
                        var bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(request.NewPassword));
                        nv.Password = Convert.ToBase64String(bytes);
                    }
                    changes.Add("Đã hash password mới");
                }

                // 2. Sửa TrangThaiNV nếu cần
                if (request?.SetTrangThaiNV.HasValue == true && nv.TrangThaiNV != request.SetTrangThaiNV.Value)
                {
                    nv.TrangThaiNV = request.SetTrangThaiNV.Value;
                    changes.Add($"Đã cập nhật TrangThaiNV thành {request.SetTrangThaiNV.Value}");
                }
                else if ((nv.TrangThaiNV == null || nv.TrangThaiNV != 0) && nv.ChucVu?.TenChucVuVietHoa?.ToUpper() != "ADMIN")
                {
                    nv.TrangThaiNV = 0; // Mặc định là Đang làm việc
                    changes.Add("Đã cập nhật TrangThaiNV thành 0 (Đang làm việc)");
                }

                // 3. Sửa IdChucVu nếu null hoặc không có
                if (nv.IdChucVu == null || nv.IdChucVu == 0)
                {
                    // Tìm chức vụ NHANVIEN hoặc chức vụ đầu tiên (trừ ADMIN)
                    var defaultChucVu = await _context.ChucVus
                        .Where(cv => cv.TenChucVuVietHoa != "ADMIN" && cv.TenChucVuVietHoa != null)
                        .FirstOrDefaultAsync();
                    
                    if (defaultChucVu != null)
                    {
                        nv.IdChucVu = defaultChucVu.IdChucVu;
                        changes.Add($"Đã gán chức vụ: {defaultChucVu.TenChucVu}");
                    }
                }

                if (changes.Any())
                {
                    _context.NhanViens.Update(nv);
                    await _context.SaveChangesAsync();
                }

                return Json(new 
                { 
                    success = true,
                    message = changes.Any() ? "Đã sửa: " + string.Join(", ", changes) : "Không có gì cần sửa",
                    changes = changes
                });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false,
                    message = "Lỗi: " + ex.Message 
                });
            }
        }
    }

    public class FixNhanVienRequest
    {
        public string? NewPassword { get; set; }
        public int? SetTrangThaiNV { get; set; }
    }
}

