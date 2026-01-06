using DATN_DT.Data;
using DATN_DT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;

namespace DATN_DT.Controllers
{
    public class UserProfileController : Controller
    {
        private readonly MyDbContext _context;
        private readonly string _jwtKey;
        private readonly ILogger<UserProfileController> _logger;
        private readonly IWebHostEnvironment _environment;

        public UserProfileController(MyDbContext context, IConfiguration config, ILogger<UserProfileController> logger, IWebHostEnvironment environment)
        {
            _context = context;
            _jwtKey = config["Jwt:Key"] ?? "your-secret-key-123456789";
            _logger = logger;
            _environment = environment;
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
                // ===== 1. Auth =====
                var email = GetCurrentKhachHangEmail();
                if (string.IsNullOrEmpty(email))
                    return Unauthorized(new { message = "Không xác thực được người dùng" });

                var khachHang = await _context.KhachHangs
                    .FirstOrDefaultAsync(x => x.EmailKhachHang == email);

                if (khachHang == null)
                    return NotFound(new { message = "Không tìm thấy khách hàng" });

                // ===== 2. Orders statistic =====
                var hoaDons = await _context.HoaDons
                    .Where(x => x.IdKhachHang == khachHang.IdKhachHang)
                    .ToListAsync();

                var totalOrders = hoaDons.Count;
                var processingOrders = hoaDons.Count(x =>
                    x.TrangThaiHoaDon == "Chờ thanh toán" ||
                    x.TrangThaiHoaDon == "Đang xử lý");

                var completedOrders = hoaDons.Count(x =>
                    x.TrangThaiHoaDon == "Đã thanh toán" ||
                    x.TrangThaiHoaDon == "Thành công");

                var totalSpent = hoaDons
                    .Where(x => x.TrangThaiHoaDon == "Đã thanh toán" || x.TrangThaiHoaDon == "Thành công")
                    .Sum(x => x.TongTien ?? 0);

                // ===== 3. Avatar URL =====
                var avatarUrl = string.IsNullOrEmpty(khachHang.DefaultImage)
                    ? "/images/default-avatar.png"
                    : $"/Storage/Avatars/{khachHang.DefaultImage}";

                // ===== 4. Response =====
                return Ok(new
                {
                    idKhachHang = khachHang.IdKhachHang,
                    hoTenKhachHang = khachHang.HoTenKhachHang,
                    sdtKhachHang = khachHang.SdtKhachHang,
                    emailKhachHang = khachHang.EmailKhachHang,
                    avatarUrl = avatarUrl,
                    diemTichLuy = khachHang.DiemTichLuy ?? 0,
                    trangThaiKhachHang = khachHang.TrangThaiKhachHang == 1 ? "Hoạt động" : "Không hoạt động",

                    // statistics
                    totalOrders,
                    processingOrders,
                    completedOrders,
                    totalSpent
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetProfileData error");
                return StatusCode(500, new { message = "Lỗi server khi lấy thông tin cá nhân" });
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

        [HttpPost]
        public async Task<IActionResult> UpdateAvatar(IFormFile avatarFile)
        {
            try
            {
                if (avatarFile == null || avatarFile.Length == 0)
                    return BadRequest(new { message = "Vui lòng chọn ảnh" });

                // Validate user
                var email = GetCurrentKhachHangEmail();
                if (string.IsNullOrEmpty(email))
                    return Unauthorized(new { message = "Không xác thực được người dùng" });

                var khachHang = await _context.KhachHangs
                    .FirstOrDefaultAsync(x => x.EmailKhachHang == email);

                if (khachHang == null)
                    return NotFound(new { message = "Không tìm thấy khách hàng" });

                // Kiểm tra định dạng file
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var fileExtension = Path.GetExtension(avatarFile.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                    return BadRequest(new { message = "Chỉ chấp nhận file ảnh (jpg, jpeg, png, gif, webp)!" });

                // Kiểm tra kích thước file (tối đa 5MB)
                if (avatarFile.Length > 5 * 1024 * 1024)
                    return BadRequest(new { message = "Kích thước file không được vượt quá 5MB!" });

                // Tạo thư mục lưu trữ nếu chưa tồn tại
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "Storage", "Avatars");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // Tạo tên file unique
                var fileName = $"avatar_{khachHang.IdKhachHang}_{Guid.NewGuid():N}{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                // Lưu file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await avatarFile.CopyToAsync(stream);
                }

                // Tạo đường dẫn tương đối để lưu trong database và trả về
                var relativePath = $"/Storage/Avatars/{fileName}";

                // Xóa file cũ nếu có
                if (!string.IsNullOrEmpty(khachHang.DefaultImage))
                {
                    try
                    {
                        // Nếu DefaultImage là đường dẫn đầy đủ, chỉ lấy tên file
                        var oldFileName = Path.GetFileName(khachHang.DefaultImage);
                        if (string.IsNullOrEmpty(oldFileName))
                        {
                            // Nếu không có extension, có thể là tên file cũ
                            oldFileName = khachHang.DefaultImage;
                        }
                        
                        if (!string.IsNullOrEmpty(oldFileName))
                        {
                            var oldFilePath = Path.Combine(uploadsFolder, oldFileName);
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }
                    }
                    catch { }
                }

                // Cập nhật database - lưu tên file (không lưu đường dẫn đầy đủ)
                khachHang.DefaultImage = fileName;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Cập nhật avatar thành công",
                    avatarUrl = relativePath
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi khi upload avatar: {ex.Message}" });
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
                                .ThenInclude(sp => sp.ThuongHieu)
                    .Include(h => h.HoaDonChiTiets)
                        .ThenInclude(hdct => hdct.ModelSanPham)
                            .ThenInclude(m => m.RAM)
                    .Include(h => h.HoaDonChiTiets)
                        .ThenInclude(hdct => hdct.ModelSanPham)
                            .ThenInclude(m => m.ROM)
                    .Include(h => h.HoaDonChiTiets)
                        .ThenInclude(hdct => hdct.ModelSanPham)
                            .ThenInclude(m => m.ManHinh)
                    .Include(h => h.HoaDonChiTiets)
                        .ThenInclude(hdct => hdct.ModelSanPham)
                            .ThenInclude(m => m.CameraTruoc)
                    .Include(h => h.HoaDonChiTiets)
                        .ThenInclude(hdct => hdct.ModelSanPham)
                            .ThenInclude(m => m.CameraSau)
                    .Include(h => h.HoaDonChiTiets)
                        .ThenInclude(hdct => hdct.ModelSanPham)
                            .ThenInclude(m => m.AnhSanPhams)
                    .Include(h => h.HoaDonChiTiets)
                        .ThenInclude(hdct => hdct.Imei)
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
                    hoTenNguoiNhan = h.HoTenNguoiNhan ?? "N/A",
                    sdtNguoiNhan = h.SdtKhachHang ?? "N/A",
                    diaChiGiaoHang = "N/A", // HoaDon model doesn't have DiaChiGiaoHang property
                    soLuongSanPham = h.HoaDonChiTiets?.Count ?? 0,
                    chiTiet = h.HoaDonChiTiets?.Select(hdct => new
                    {
                        tenSanPham = hdct.ModelSanPham?.SanPham?.TenSanPham ?? "N/A",
                        tenThuongHieu = hdct.ModelSanPham?.SanPham?.ThuongHieu?.TenThuongHieu ?? "N/A",
                        mau = hdct.ModelSanPham?.Mau ?? "N/A",
                        ram = hdct.ModelSanPham?.RAM?.DungLuongRAM ?? "N/A",
                        rom = hdct.ModelSanPham?.ROM?.DungLuongROM ?? "N/A",
                        manHinh = hdct.ModelSanPham?.ManHinh != null 
                            ? $"{hdct.ModelSanPham.ManHinh.KichThuoc ?? ""} {hdct.ModelSanPham.ManHinh.CongNgheManHinh ?? ""}".Trim()
                            : "N/A",
                        cameraTruoc = hdct.ModelSanPham?.CameraTruoc?.DoPhanGiaiCamTruoc ?? "N/A",
                        cameraSau = hdct.ModelSanPham?.CameraSau?.DoPhanGiaiCamSau ?? "N/A",
                        soLuong = hdct.SoLuong ?? 0,
                        donGia = hdct.DonGia ?? 0,
                        thanhTien = hdct.ThanhTien ?? 0,
                        giaKhuyenMai = hdct.GiaKhuyenMai,
                        hinhAnh = hdct.ModelSanPham?.AnhSanPhams?.FirstOrDefault()?.DuongDan ?? "/images/default-product.jpg",
                        idImei = hdct.IdImei,
                        maImei = hdct.Imei?.MaImei ?? "N/A",
                        trangThaiImei = hdct.Imei?.TrangThai ?? "N/A"
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

        // POST: Hủy đơn hàng
        [HttpPost]
        [Route("UserProfile/CancelOrder")]
        public async Task<IActionResult> CancelOrder([FromQuery] int id)
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

                // Lấy hóa đơn với chi tiết và IMEI
                var hoaDon = await _context.HoaDons
                    .Include(h => h.HoaDonChiTiets)
                        .ThenInclude(hdct => hdct.Imei)
                    .Include(h => h.HoaDonChiTiets)
                        .ThenInclude(hdct => hdct.ModelSanPham)
                    .FirstOrDefaultAsync(h => h.IdHoaDon == id && h.IdKhachHang == khachHang.IdKhachHang);

                if (hoaDon == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy hóa đơn" });
                }

                // Kiểm tra trạng thái - chỉ cho phép hủy đơn ở trạng thái "Chờ xác nhận" (status = 0)
                var currentStatus = hoaDon.TrangThaiHoaDon ?? "";
                var statusNumber = GetStatusNumber(currentStatus);
                var lowerStatus = currentStatus.ToLower();
                
                // Debug: Log trạng thái để kiểm tra
                Console.WriteLine($"CancelOrder - Order ID: {id}, Current Status: '{currentStatus}', Status Number: {statusNumber}");
                
                // Kiểm tra nếu đã bị hủy rồi
                if (statusNumber == 4 || lowerStatus.Contains("hủy"))
                {
                    return BadRequest(new { success = false, message = "Đơn hàng này đã bị hủy trước đó" });
                }
                
                // Cho phép hủy nếu:
                // 1. Status number = 0 (Chờ xác nhận)
                // 2. Hoặc trạng thái chứa "chờ xác nhận" hoặc "chờ" (fallback cho trường hợp không xác định được status number)
                var canCancel = statusNumber == 0 || 
                               lowerStatus.Contains("chờ xác nhận") || 
                               (lowerStatus.Contains("chờ") && lowerStatus.Contains("xác nhận")) ||
                               lowerStatus.Contains("pending");
                
                if (!canCancel)
                {
                    return BadRequest(new { success = false, message = $"Chỉ có thể hủy đơn hàng ở trạng thái 'Chờ xác nhận'. Trạng thái hiện tại: '{currentStatus}'" });
                }

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // 1. Trả lại IMEI về "Còn hàng"
                    var modelIdsToRefresh = new HashSet<int>();
                    foreach (var chiTiet in hoaDon.HoaDonChiTiets ?? new List<HoaDonChiTiet>())
                    {
                        if (chiTiet.IdImei.HasValue && chiTiet.Imei != null)
                        {
                            chiTiet.Imei.TrangThai = "Còn hàng";
                            _context.Imeis.Update(chiTiet.Imei);
                            
                            // Lưu IdModelSanPham để refresh tồn kho sau
                            if (chiTiet.IdModelSanPham.HasValue)
                            {
                                modelIdsToRefresh.Add(chiTiet.IdModelSanPham.Value);
                            }
                        }
                    }

                    // 2. Refresh tồn kho cho các model đã được trả lại
                    var tonKhoRepo = new Repos.TonKhoRepo(_context);
                    foreach (var modelId in modelIdsToRefresh)
                    {
                        await tonKhoRepo.RefreshTonKhoForModel(modelId);
                    }

                    // 3. Cập nhật trạng thái hóa đơn thành "Hủy đơn hàng"
                    hoaDon.TrangThaiHoaDon = "Hủy đơn hàng";
                    _context.HoaDons.Update(hoaDon);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Ok(new { success = true, message = "Hủy đơn hàng thành công! Số lượng sản phẩm đã được trả lại vào kho." });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, new { success = false, message = "Lỗi server: " + ex.Message });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi server: " + ex.Message });
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

                // Tạo lại JWT token với email mới
                var newToken = GenerateJwtToken(model.NewEmail, "KhachHang", khachHang.IdKhachHang);
                
                // Cập nhật cookie với token mới
                Response.Cookies.Append("jwt", newToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.Now.AddHours(3)
                });

                return Ok(new { message = "Đổi email thành công!" });
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

        // Helper: Chuyển đổi trạng thái từ string/số sang số
        private int GetStatusNumber(string? trangThai)
        {
            if (string.IsNullOrEmpty(trangThai))
                return -1;

            // Nếu là số dạng string
            if (int.TryParse(trangThai, out int statusNum))
                return statusNum;

            // Map từ string sang số
            var lower = trangThai.ToLower();
            // Kiểm tra "chờ xác nhận" trước (vì nó chứa cả "chờ" và "xác nhận")
            if (lower.Contains("chờ xác nhận") || (lower.Contains("chờ") && lower.Contains("xác nhận")) || lower.Contains("pending") || lower == "0") return 0;
            if ((lower.Contains("xác nhận") && !lower.Contains("chờ")) || lower.Contains("confirmed") || lower == "1") return 1;
            if (lower.Contains("vận chuyển") || lower.Contains("shipping") || lower == "2") return 2;
            if (lower.Contains("thành công") || lower.Contains("completed") || lower == "3") return 3;
            if (lower.Contains("hủy") || lower.Contains("cancel") || lower == "4") return 4;

            return -1;
        }

        // Helper: Generate JWT Token
        private string GenerateJwtToken(string email, string role, int idKhachHang)
        {
            var key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
            var creds = new Microsoft.IdentityModel.Tokens.SigningCredentials(key, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("IdKhachHang", idKhachHang.ToString()),
                new Claim(ClaimTypes.Name, email),
                new Claim(ClaimTypes.Role, role)
            };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(3),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}