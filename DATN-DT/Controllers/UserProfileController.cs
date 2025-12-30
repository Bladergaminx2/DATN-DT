using DATN_DT.Data;
using DATN_DT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;

namespace DATN_DT.Controllers
{
    public class UserProfileController : Controller
    {
        private readonly MyDbContext _context;

        public UserProfileController(MyDbContext context)
        {
            _context = context;
        }

        // GET: Hiển thị trang profile
        [HttpGet]
        public async Task<IActionResult> Index()
        {
           return View();
        }

        [HttpGet]
        [Route("UserProfile/GetProfileData")]
        public async Task<IActionResult> GetProfileData()
        {
            try
            {
                var email = GetCurrentKhachHangEmail();
                if (string.IsNullOrEmpty(email))
                {
                    return Unauthorized(new { message = "Không tìm thấy thông tin người dùng" });
                }

                var khachHang = await _context.KhachHangs
                    .FirstOrDefaultAsync(k => k.EmailKhachHang == email);

                if (khachHang == null)
                {
                    return NotFound(new { message = "Không tìm thấy thông tin khách hàng" });
                }

                // Tính thống kê đơn hàng
                var hoaDons = await _context.HoaDons
                    .Where(h => h.IdKhachHang == khachHang.IdKhachHang)
                    .ToListAsync();

                var totalOrders = hoaDons.Count;
                var processingOrders = hoaDons.Count(h => h.TrangThaiHoaDon == "Chờ thanh toán" || h.TrangThaiHoaDon == "Đang xử lý");
                var completedOrders = hoaDons.Count(h => h.TrangThaiHoaDon == "Đã thanh toán" || h.TrangThaiHoaDon == "Thành công");
                var totalSpent = hoaDons.Where(h => h.TrangThaiHoaDon == "Đã thanh toán" || h.TrangThaiHoaDon == "Thành công")
                    .Sum(h => h.TongTien ?? 0);

                return Ok(new
                {
                    idKhachHang = khachHang.IdKhachHang,
                    hoTenKhachHang = khachHang.HoTenKhachHang,
                    sdtKhachHang = khachHang.SdtKhachHang,
                    emailKhachHang = khachHang.EmailKhachHang,
                    defaultImage = khachHang.DefaultImage,
                    diemTichLuy = khachHang.DiemTichLuy ?? 0,
                    trangThaiKhachHang = khachHang.TrangThaiKhachHang == 1 ? "Hoạt động" : "Không hoạt động",
                    // Thống kê
                    totalOrders = totalOrders,
                    processingOrders = processingOrders,
                    completedOrders = completedOrders,
                    totalSpent = totalSpent
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        // POST: Cập nhật profile
        // POST: API cập nhật thông tin profile
        [HttpPost]
        [Route("UserProfile/UpdateProfileData")]
        public async Task<IActionResult> UpdateProfileData([FromBody] ProfileUpdateModel model)
        {
            try
            {
                var email = GetCurrentKhachHangEmail();
                if (string.IsNullOrEmpty(email))
                {
                    return Unauthorized(new { message = "Không tìm thấy thông tin người dùng" });
                }

                var existingKhachHang = await _context.KhachHangs
                    .FirstOrDefaultAsync(k => k.EmailKhachHang == email);

                if (existingKhachHang == null)
                {
                    return NotFound(new { message = "Không tìm thấy thông tin khách hàng" });
                }

                // Validation
                if (string.IsNullOrWhiteSpace(model.HoTenKhachHang))
                {
                    return BadRequest(new { message = "Họ tên không được để trống" });
                }

                if (string.IsNullOrWhiteSpace(model.SdtKhachHang))
                {
                    return BadRequest(new { message = "Số điện thoại không được để trống" });
                }

                // Kiểm tra trùng số điện thoại
                bool sdtExists = await _context.KhachHangs.AnyAsync(k =>
                    k.SdtKhachHang == model.SdtKhachHang &&
                    k.IdKhachHang != existingKhachHang.IdKhachHang);

                if (sdtExists)
                {
                    return Conflict(new { message = "Số điện thoại này đã được sử dụng" });
                }

                // Kiểm tra trùng email
                if (!string.IsNullOrWhiteSpace(model.EmailKhachHang))
                {
                    bool emailExists = await _context.KhachHangs.AnyAsync(k =>
                        k.EmailKhachHang == model.EmailKhachHang &&
                        k.IdKhachHang != existingKhachHang.IdKhachHang);

                    if (emailExists)
                    {
                        return Conflict(new { message = "Email này đã được sử dụng" });
                    }
                }

                // Cập nhật thông tin
                existingKhachHang.HoTenKhachHang = model.HoTenKhachHang.Trim();
                existingKhachHang.SdtKhachHang = model.SdtKhachHang.Trim();
                existingKhachHang.EmailKhachHang = model.EmailKhachHang?.Trim();

                _context.KhachHangs.Update(existingKhachHang);
                await _context.SaveChangesAsync();

                // Trả về data đã được cập nhật
                return Ok(new
                {
                    message = "Cập nhật thông tin thành công!",
                    data = new
                    {
                        HoTenKhachHang = existingKhachHang.HoTenKhachHang,
                        SdtKhachHang = existingKhachHang.SdtKhachHang,
                        EmailKhachHang = existingKhachHang.EmailKhachHang,
                        DefaultImage = existingKhachHang.DefaultImage,
                        DiemTichLuy = existingKhachHang.DiemTichLuy,
                        TrangThaiKhachHang = existingKhachHang.TrangThaiKhachHang
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        // Model cho update
        public class ProfileUpdateModel
        {
            public string HoTenKhachHang { get; set; }
            public string SdtKhachHang { get; set; }
            public string EmailKhachHang { get; set; }
            public string DiaChiKhachHang { get; set; }
        }

        // POST: Cập nhật ảnh đại diện
        [HttpPost]
        [Route("UserProfile/UpdateAvatar")]
        public async Task<IActionResult> UpdateAvatar(IFormFile avatarFile)
        {
            try
            {
                var email = GetCurrentKhachHangEmail();
                if (string.IsNullOrEmpty(email))
                {
                    return Unauthorized(new { message = "Không tìm thấy thông tin người dùng" });
                }

                var existingKhachHang = await _context.KhachHangs
                    .FirstOrDefaultAsync(k => k.EmailKhachHang == email);

                if (existingKhachHang == null)
                {
                    return NotFound(new { message = "Không tìm thấy thông tin khách hàng" });
                }

                if (avatarFile != null && avatarFile.Length > 0)
                {
                    // Kiểm tra định dạng file
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var fileExtension = Path.GetExtension(avatarFile.FileName).ToLowerInvariant();

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        return BadRequest(new { message = "Chỉ chấp nhận file ảnh (jpg, jpeg, png, gif)" });
                    }

                    // Kiểm tra kích thước file (max 5MB)
                    if (avatarFile.Length > 5 * 1024 * 1024)
                    {
                        return BadRequest(new { message = "Kích thước file không được vượt quá 5MB" });
                    }

                    // Tạo thư mục nếu chưa tồn tại
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "avatars");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // XÓA ẢNH CŨ NẾU CÓ (trừ ảnh mặc định)
                    if (!string.IsNullOrEmpty(existingKhachHang.DefaultImage) &&
                        !existingKhachHang.DefaultImage.Contains("default-avatar"))
                    {
                        try
                        {
                            var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot",
                                existingKhachHang.DefaultImage.TrimStart('/'));

                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                                Console.WriteLine($"Đã xóa ảnh cũ: {oldImagePath}");
                            }
                        }
                        catch (Exception deleteEx)
                        {
                            // Log lỗi nhưng vẫn tiếp tục xử lý upload ảnh mới
                            Console.WriteLine($"Lỗi khi xóa ảnh cũ: {deleteEx.Message}");
                            // Không throw exception để tiếp tục upload ảnh mới
                        }
                    }

                    // Tạo tên file duy nhất
                    var fileName = $"avatar_{existingKhachHang.IdKhachHang}_{DateTime.Now:yyyyMMddHHmmss}{fileExtension}";
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    // Lưu file mới
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await avatarFile.CopyToAsync(stream);
                    }

                    // Cập nhật đường dẫn ảnh trong database
                    var avatarUrl = $"/images/avatars/{fileName}";
                    existingKhachHang.DefaultImage = avatarUrl;
                    _context.KhachHangs.Update(existingKhachHang);
                    await _context.SaveChangesAsync();

                    return Ok(new
                    {
                        message = "Cập nhật ảnh đại diện thành công!",
                        avatarUrl = avatarUrl
                    });
                }
                else
                {
                    return BadRequest(new { message = "Vui lòng chọn ảnh để upload" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        // Helper: Lấy email khách hàng từ JWT token
        private string GetCurrentKhachHangEmail()
        {
            // Thử lấy từ cookie JWT
            var token = Request.Cookies["jwt"];
            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    var handler = new JwtSecurityTokenHandler();
                    var jsonToken = handler.ReadJwtToken(token);
                    var emailClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name || c.Type == "Email");
                    if (emailClaim != null)
                    {
                        return emailClaim.Value;
                    }
                }
                catch { }
            }
            
            // Fallback: Lấy từ User claims
            return User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue("Email");
        }

        // Helper: Lấy ID khách hàng từ JWT token
        private async Task<int?> GetCurrentKhachHangIdAsync()
        {
            var email = GetCurrentKhachHangEmail();
            if (string.IsNullOrEmpty(email))
            {
                return null;
            }

            var khachHang = await _context.KhachHangs
                .FirstOrDefaultAsync(k => k.EmailKhachHang == email);
            
            return khachHang?.IdKhachHang;
        }

        // GET: Lấy danh sách đơn hàng của khách hàng
        [HttpGet]
        [Route("UserProfile/GetOrders")]
        public async Task<IActionResult> GetOrders()
        {
            try
            {
                var email = GetCurrentKhachHangEmail();
                if (string.IsNullOrEmpty(email))
                {
                    return Unauthorized(new { success = false, message = "Không tìm thấy thông tin người dùng" });
                }

                var khachHang = await _context.KhachHangs
                    .FirstOrDefaultAsync(k => k.EmailKhachHang == email);

                if (khachHang == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy thông tin khách hàng" });
                }

                var hoaDons = await _context.HoaDons
                    .Include(h => h.HoaDonChiTiets)
                        .ThenInclude(hdct => hdct.ModelSanPham)
                            .ThenInclude(m => m.SanPham)
                    .Include(h => h.HoaDonChiTiets)
                        .ThenInclude(hdct => hdct.ModelSanPham)
                            .ThenInclude(m => m.AnhSanPhams)
                    .Where(h => h.IdKhachHang == khachHang.IdKhachHang)
                    .OrderByDescending(h => h.NgayLapHoaDon)
                    .ToListAsync();

                var orders = hoaDons.Select(h => new
                {
                    idHoaDon = h.IdHoaDon,
                    maDon = $"HD{h.IdHoaDon:D6}",
                    ngayLap = h.NgayLapHoaDon?.ToString("dd/MM/yyyy HH:mm"),
                    trangThai = h.TrangThaiHoaDon ?? "Chưa xác định",
                    tongTien = h.TongTien ?? 0,
                    phuongThucThanhToan = h.PhuongThucThanhToan ?? "COD",
                    soLuongSanPham = h.HoaDonChiTiets?.Count ?? 0,
                    chiTiet = h.HoaDonChiTiets?.Select(hdct => new
                    {
                        tenSanPham = hdct.ModelSanPham?.SanPham?.TenSanPham ?? "N/A",
                        tenModel = hdct.ModelSanPham?.TenModel ?? "N/A",
                        mau = hdct.ModelSanPham?.Mau ?? "N/A",
                        soLuong = hdct.SoLuong ?? 0,
                        donGia = hdct.DonGia ?? 0,
                        thanhTien = hdct.ThanhTien ?? 0,
                        hinhAnh = hdct.ModelSanPham?.AnhSanPhams?.FirstOrDefault()?.DuongDan ?? "/images/default-product.jpg"
                    }).ToList()
                }).ToList();

                return Ok(new { success = true, data = orders });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi server: " + ex.Message });
            }
        }

        // POST: Đổi mật khẩu
        [HttpPost]
        [Route("UserProfile/ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
        {
            try
            {
                var email = GetCurrentKhachHangEmail();
                if (string.IsNullOrEmpty(email))
                {
                    return Unauthorized(new { message = "Không tìm thấy thông tin người dùng" });
                }

                var khachHang = await _context.KhachHangs
                    .FirstOrDefaultAsync(k => k.EmailKhachHang == email);

                if (khachHang == null)
                {
                    return NotFound(new { message = "Không tìm thấy thông tin khách hàng" });
                }

                // Kiểm tra mật khẩu hiện tại
                if (!VerifyPassword(model.CurrentPassword, khachHang.Password))
                {
                    return BadRequest(new { message = "Mật khẩu hiện tại không đúng" });
                }

                // Kiểm tra mật khẩu mới
                if (string.IsNullOrWhiteSpace(model.NewPassword) || model.NewPassword.Length < 6)
                {
                    return BadRequest(new { message = "Mật khẩu mới phải có ít nhất 6 ký tự" });
                }

                if (model.NewPassword != model.ConfirmPassword)
                {
                    return BadRequest(new { message = "Mật khẩu xác nhận không khớp" });
                }

                // Cập nhật mật khẩu mới
                khachHang.Password = HashPassword(model.NewPassword);
                _context.KhachHangs.Update(khachHang);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Đổi mật khẩu thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        // POST: Đổi email
        [HttpPost]
        [Route("UserProfile/ChangeEmail")]
        public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailModel model)
        {
            try
            {
                var email = GetCurrentKhachHangEmail();
                if (string.IsNullOrEmpty(email))
                {
                    return Unauthorized(new { message = "Không tìm thấy thông tin người dùng" });
                }

                var khachHang = await _context.KhachHangs
                    .FirstOrDefaultAsync(k => k.EmailKhachHang == email);

                if (khachHang == null)
                {
                    return NotFound(new { message = "Không tìm thấy thông tin khách hàng" });
                }

                // Kiểm tra mật khẩu xác nhận
                if (!VerifyPassword(model.ConfirmPassword, khachHang.Password))
                {
                    return BadRequest(new { message = "Mật khẩu xác nhận không đúng" });
                }

                // Kiểm tra email mới
                if (string.IsNullOrWhiteSpace(model.NewEmail))
                {
                    return BadRequest(new { message = "Email mới không được để trống" });
                }

                // Validate email format
                if (!System.Text.RegularExpressions.Regex.IsMatch(model.NewEmail, @"^[^\s@]+@[^\s@]+\.[^\s@]+$"))
                {
                    return BadRequest(new { message = "Email không hợp lệ" });
                }

                // Kiểm tra email mới có khác email hiện tại không
                if (model.NewEmail == email)
                {
                    return BadRequest(new { message = "Email mới phải khác email hiện tại" });
                }

                // Kiểm tra email đã tồn tại chưa
                var existingEmail = await _context.KhachHangs
                    .FirstOrDefaultAsync(k => k.EmailKhachHang == model.NewEmail);

                if (existingEmail != null)
                {
                    return BadRequest(new { message = "Email này đã được sử dụng bởi tài khoản khác" });
                }

                // Cập nhật email mới
                khachHang.EmailKhachHang = model.NewEmail;
                _context.KhachHangs.Update(khachHang);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Đổi email thành công! Vui lòng đăng nhập lại với email mới." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        // Model cho đổi mật khẩu
        public class ChangePasswordModel
        {
            public string CurrentPassword { get; set; }
            public string NewPassword { get; set; }
            public string ConfirmPassword { get; set; }
        }

        // Model cho đổi email
        public class ChangeEmailModel
        {
            public string NewEmail { get; set; }
            public string ConfirmPassword { get; set; }
        }

        // Helper: Hash password
        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        // Helper: Verify password
        private bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }
    }
}