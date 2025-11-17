using DATN_DT.Data;
using DATN_DT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

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
                var khachHangId = GetCurrentKhachHangId();
                if (khachHangId == null)
                {
                    return Unauthorized(new { message = "Không tìm thấy thông tin người dùng" });
                }

                var khachHang = await _context.KhachHangs
                    .FirstOrDefaultAsync(k => k.IdKhachHang == khachHangId);

                if (khachHang == null)
                {
                    return NotFound(new { message = "Không tìm thấy thông tin khách hàng" });
                }

                return Ok(khachHang);
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
                var khachHangId = GetCurrentKhachHangId();
                if (khachHangId == null)
                {
                    return Unauthorized(new { message = "Không tìm thấy thông tin người dùng" });
                }

                var existingKhachHang = await _context.KhachHangs
                    .FirstOrDefaultAsync(k => k.IdKhachHang == khachHangId);

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
                    k.IdKhachHang != khachHangId);

                if (sdtExists)
                {
                    return Conflict(new { message = "Số điện thoại này đã được sử dụng" });
                }

                // Kiểm tra trùng email
                if (!string.IsNullOrWhiteSpace(model.EmailKhachHang))
                {
                    bool emailExists = await _context.KhachHangs.AnyAsync(k =>
                        k.EmailKhachHang == model.EmailKhachHang &&
                        k.IdKhachHang != khachHangId);

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
                var khachHangId = GetCurrentKhachHangId();
                if (khachHangId == null)
                {
                    return Unauthorized(new { message = "Không tìm thấy thông tin người dùng" });
                }

                var existingKhachHang = await _context.KhachHangs
                    .FirstOrDefaultAsync(k => k.IdKhachHang == khachHangId);

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
                    var fileName = $"avatar_{khachHangId}_{DateTime.Now:yyyyMMddHHmmss}{fileExtension}";
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

        // Helper: Lấy ID khách hàng từ JWT token
        private int? GetCurrentKhachHangId()
        {
            var idClaim = User.FindFirstValue("IdKhachHang"); // Hoặc ClaimTypes.NameIdentifier tùy vào cách bạn lưu
            if (int.TryParse(idClaim, out int khachHangId))
            {
                return khachHangId;
            }
            return null;
        }
    }
}