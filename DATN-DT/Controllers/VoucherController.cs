using DATN_DT.Data;
using DATN_DT.Models;
using DATN_DT.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System;
using System.Linq;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace DATN_DT.Controllers
{
    [Route("Voucher")]
    public class VoucherController : Controller
    {
        private readonly MyDbContext _context;
        private readonly IVoucherService _voucherService;

        public VoucherController(MyDbContext context, IVoucherService voucherService)
        {
            _context = context;
            _voucherService = voucherService;
        }

        // GET: Voucher
        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            // Tự động cập nhật trạng thái
            await _voucherService.UpdateVoucherStatusAsync();
            
            var vouchers = await _voucherService.GetAllVouchersAsync();
            return View(vouchers);
        }

        // GET: Voucher/Details/5
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var voucher = await _voucherService.GetVoucherByIdAsync(id);
            if (voucher == null)
                return NotFound();

            // Lấy lịch sử sử dụng
            var lichSuSuDung = await _context.VoucherSuDungs
                .Include(v => v.KhachHang)
                .Include(v => v.HoaDon)
                .Where(v => v.IdVoucher == id)
                .OrderByDescending(v => v.NgaySuDung)
                .ToListAsync();

            ViewBag.LichSuSuDung = lichSuSuDung;
            return View(voucher);
        }

        // POST: Voucher/Create
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Voucher voucher)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(voucher.MaVoucher))
                {
                    return BadRequest(new { success = false, message = "Mã voucher không được để trống" });
                }

                // Kiểm tra mã voucher đã tồn tại chưa
                var existingVoucher = await _voucherService.GetVoucherByCodeAsync(voucher.MaVoucher);
                if (existingVoucher != null)
                {
                    return Conflict(new { success = false, message = "Mã voucher đã tồn tại" });
                }

                // Tự động set trạng thái
                var now = DateTime.Now;
                if (now < voucher.NgayBatDau)
                    voucher.TrangThai = "SapDienRa";
                else if (now >= voucher.NgayBatDau && now <= voucher.NgayKetThuc)
                    voucher.TrangThai = "HoatDong";
                else
                    voucher.TrangThai = "HetHan";

                voucher.SoLuongDaSuDung = 0;

                var createdVoucher = await _voucherService.CreateVoucherAsync(voucher);
                return Ok(new { success = true, data = createdVoucher });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // POST: Voucher/Update
        [HttpPost]
        public async Task<IActionResult> Update([FromBody] Voucher voucher)
        {
            try
            {
                var existingVoucher = await _voucherService.GetVoucherByIdAsync(voucher.IdVoucher);
                if (existingVoucher == null)
                    return NotFound(new { success = false, message = "Không tìm thấy voucher" });

                // Kiểm tra mã voucher có bị trùng không (nếu đổi mã)
                if (existingVoucher.MaVoucher != voucher.MaVoucher)
                {
                    var duplicateVoucher = await _voucherService.GetVoucherByCodeAsync(voucher.MaVoucher);
                    if (duplicateVoucher != null)
                    {
                        return Conflict(new { success = false, message = "Mã voucher đã tồn tại" });
                    }
                }

                // Cập nhật trạng thái dựa trên thời gian
                var now = DateTime.Now;
                if (now < voucher.NgayBatDau)
                    voucher.TrangThai = "SapDienRa";
                else if (now >= voucher.NgayBatDau && now <= voucher.NgayKetThuc)
                    voucher.TrangThai = "HoatDong";
                else
                    voucher.TrangThai = "HetHan";

                // Giữ nguyên số lượng đã sử dụng
                voucher.SoLuongDaSuDung = existingVoucher.SoLuongDaSuDung;

                var result = await _voucherService.UpdateVoucherAsync(voucher);
                if (result)
                    return Ok(new { success = true, message = "Cập nhật voucher thành công" });
                else
                    return BadRequest(new { success = false, message = "Cập nhật voucher thất bại" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // POST: Voucher/Delete
        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] DeleteVoucherRequest request)
        {
            try
            {
                if (request == null || request.Id <= 0)
                    return BadRequest(new { success = false, message = "ID voucher không hợp lệ" });

                var result = await _voucherService.DeleteVoucherAsync(request.Id);
                if (result)
                    return Ok(new { success = true, message = "Xóa voucher thành công" });
                else
                    return NotFound(new { success = false, message = "Không tìm thấy voucher" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // API: Lấy danh sách voucher đang hoạt động (cho customer)
        [HttpGet("GetActiveVouchers")]
        public async Task<IActionResult> GetActiveVouchers()
        {
            try
            {
                // Tự động cập nhật trạng thái
                await _voucherService.UpdateVoucherStatusAsync();
                
                var now = DateTime.Now;
                
                // Debug: Kiểm tra tổng số voucher
                var totalVouchers = await _context.Vouchers.CountAsync();
                Console.WriteLine($"[GetActiveVouchers] Total vouchers in DB: {totalVouchers}");
                
                // Lấy tất cả voucher trước để debug
                var allVouchers = await _context.Vouchers.ToListAsync();
                Console.WriteLine($"[GetActiveVouchers] Vouchers by status:");
                foreach (var statusGroup in allVouchers.GroupBy(v => v.TrangThai))
                {
                    Console.WriteLine($"  - {statusGroup.Key}: {statusGroup.Count()}");
                }
                
                var vouchers = await _context.Vouchers
                    .Where(v => v.TrangThai == "HoatDong" 
                        && v.NgayBatDau <= now 
                        && v.NgayKetThuc >= now
                        && (!v.SoLuongSuDung.HasValue || v.SoLuongDaSuDung < v.SoLuongSuDung.Value))
                    .OrderByDescending(v => v.NgayBatDau)
                    .Select(v => new
                    {
                        idVoucher = v.IdVoucher,
                        maVoucher = v.MaVoucher,
                        tenVoucher = v.TenVoucher,
                        moTa = v.MoTa,
                        loaiGiam = v.LoaiGiam,
                        giaTri = v.GiaTri,
                        donHangToiThieu = v.DonHangToiThieu,
                        giamToiDa = v.GiamToiDa,
                        ngayBatDau = v.NgayBatDau,
                        ngayKetThuc = v.NgayKetThuc,
                        soLuongSuDung = v.SoLuongSuDung,
                        soLuongDaSuDung = v.SoLuongDaSuDung,
                        soLuongMoiKhachHang = v.SoLuongMoiKhachHang,
                        apDungCho = v.ApDungCho,
                        danhSachId = v.DanhSachId
                    })
                    .ToListAsync();

                Console.WriteLine($"[GetActiveVouchers] Filtered vouchers count: {vouchers.Count}");
                return Ok(new { success = true, data = vouchers });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetActiveVouchers] Error: {ex.Message}");
                Console.WriteLine($"[GetActiveVouchers] StackTrace: {ex.StackTrace}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // API: Validate và tính giảm giá voucher (cho customer)
        [HttpPost("ValidateVoucher")]
        public async Task<IActionResult> ValidateVoucher([FromBody] ValidateVoucherRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.MaVoucher))
                {
                    return BadRequest(new { success = false, message = "Mã voucher không được để trống" });
                }

                // Lấy ID khách hàng từ JWT token
                var khachHangEmail = GetCurrentKhachHangEmail();
                if (string.IsNullOrEmpty(khachHangEmail))
                    return Unauthorized(new { success = false, message = "Vui lòng đăng nhập để sử dụng voucher" });

                var khachHang = await _context.KhachHangs
                    .FirstOrDefaultAsync(k => k.EmailKhachHang == khachHangEmail);
                
                if (khachHang == null)
                    return Unauthorized(new { success = false, message = "Không tìm thấy thông tin khách hàng" });

                var voucher = await _voucherService.GetVoucherByCodeAsync(request.MaVoucher);
                if (voucher == null)
                    return BadRequest(new { success = false, message = "Mã voucher không tồn tại" });

                // Validate voucher
                var isValid = await _voucherService.ValidateVoucherAsync(
                    request.MaVoucher, 
                    khachHang.IdKhachHang, 
                    request.TongTienDonHang
                );

                if (!isValid)
                {
                    // Kiểm tra chi tiết lý do không hợp lệ để trả về thông báo cụ thể
                    var now = DateTime.Now;
                    if (voucher.TrangThai != "HoatDong")
                        return BadRequest(new { success = false, message = "Voucher không còn hoạt động" });
                    
                    if (now < voucher.NgayBatDau)
                        return BadRequest(new { success = false, message = $"Voucher chưa đến thời gian sử dụng (bắt đầu từ {voucher.NgayBatDau:dd/MM/yyyy})" });
                    
                    if (now > voucher.NgayKetThuc)
                        return BadRequest(new { success = false, message = $"Voucher đã hết hạn (hết hạn ngày {voucher.NgayKetThuc:dd/MM/yyyy})" });
                    
                    if (voucher.DonHangToiThieu.HasValue && request.TongTienDonHang < voucher.DonHangToiThieu.Value)
                        return BadRequest(new { success = false, message = $"Đơn hàng tối thiểu {voucher.DonHangToiThieu.Value:N0} ₫. Đơn hàng của bạn: {request.TongTienDonHang:N0} ₫" });
                    
                    if (voucher.SoLuongSuDung.HasValue && voucher.SoLuongDaSuDung >= voucher.SoLuongSuDung.Value)
                        return BadRequest(new { success = false, message = "Voucher đã hết số lượng sử dụng" });
                    
                    // Chỉ kiểm tra nếu SoLuongMoiKhachHang > 0 (0 hoặc null = không giới hạn)
                    if (voucher.SoLuongMoiKhachHang.HasValue && voucher.SoLuongMoiKhachHang.Value > 0)
                    {
                        var soLanDaSuDung = await _context.VoucherSuDungs
                            .CountAsync(v => v.IdVoucher == voucher.IdVoucher && v.IdKhachHang == khachHang.IdKhachHang);
                        
                        if (soLanDaSuDung >= voucher.SoLuongMoiKhachHang.Value)
                            return BadRequest(new { success = false, message = $"Bạn đã sử dụng hết số lần được phép ({voucher.SoLuongMoiKhachHang.Value} lần)" });
                    }
                    
                    return BadRequest(new { success = false, message = "Voucher không thể sử dụng. Vui lòng kiểm tra lại điều kiện sử dụng." });
                }

                // Tính số tiền giảm
                var soTienGiam = await _voucherService.CalculateDiscountAsync(
                    voucher, 
                    request.TongTienDonHang,
                    request.DanhSachIdSanPham
                );

                return Ok(new 
                { 
                    success = true, 
                    data = new
                    {
                        idVoucher = voucher.IdVoucher,
                        maVoucher = voucher.MaVoucher,
                        tenVoucher = voucher.TenVoucher,
                        moTa = voucher.MoTa,
                        soTienGiam = soTienGiam,
                        loaiGiam = voucher.LoaiGiam,
                        giaTri = voucher.GiaTri,
                        donHangToiThieu = voucher.DonHangToiThieu,
                        giamToiDa = voucher.GiamToiDa,
                        ngayBatDau = voucher.NgayBatDau,
                        ngayKetThuc = voucher.NgayKetThuc,
                        soLuongSuDung = voucher.SoLuongSuDung,
                        soLuongDaSuDung = voucher.SoLuongDaSuDung,
                        soLuongMoiKhachHang = voucher.SoLuongMoiKhachHang,
                        apDungCho = voucher.ApDungCho,
                        danhSachId = voucher.DanhSachId
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Helper: Lấy email khách hàng từ JWT token
        private string? GetCurrentKhachHangEmail()
        {
            try
            {
                // Thử lấy từ cookie JWT trước (như UserProfileController)
                var token = Request.Cookies["jwt"];
                if (!string.IsNullOrEmpty(token))
                {
                    try
                    {
                        var handler = new JwtSecurityTokenHandler();
                        var jsonToken = handler.ReadJwtToken(token);
                        var emailClaim = jsonToken.Claims.FirstOrDefault(c => 
                            c.Type == ClaimTypes.Name || 
                            c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name" ||
                            c.Type == "name" ||
                            c.Type == "Email");
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
            catch (Exception ex)
            {
                // Log error for debugging
                Console.WriteLine($"[Voucher] Error getting customer email: {ex.Message}");
                return null;
            }
        }
    }

    // DTO cho validate voucher request
    public class ValidateVoucherRequest
    {
        public string MaVoucher { get; set; } = string.Empty;
        public decimal TongTienDonHang { get; set; }
        public List<int>? DanhSachIdSanPham { get; set; }
    }

    // DTO cho delete voucher request
    public class DeleteVoucherRequest
    {
        public int Id { get; set; }
    }
}

