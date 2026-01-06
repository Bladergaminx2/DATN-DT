using DATN_DT.Data;
using DATN_DT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using DATN_DT.IServices;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

namespace DATN_DT.Controllers
{
    public class HoaDonController : Controller
    {
        private readonly MyDbContext _context;
        private readonly IPayOSService _payOSService;
        private readonly ILogger<HoaDonController> _logger;
        private readonly ITonKhoService _tonKhoService;
        private readonly IModelSanPhamStatusService _statusService;

        public HoaDonController(
            MyDbContext context,
            IPayOSService payOSService,
            ILogger<HoaDonController> logger,
            ITonKhoService tonKhoService,
            IModelSanPhamStatusService statusService)
        {
            _context = context;
            _payOSService = payOSService;
            _logger = logger;
            _tonKhoService = tonKhoService;
            _statusService = statusService;
        }

        // GET: HoaDon
        public IActionResult Index()
        {
            return View();
        }

        // GET: API - Lấy danh sách hóa đơn
        [HttpGet]
        public async Task<IActionResult> GetInvoices(int? status, string? search, DateTime? fromDate, DateTime? toDate, string? paymentMethod)
        {
            try
            {
                var query = _context.HoaDons
                    .Include(h => h.KhachHang)
                    .Include(h => h.NhanVien)
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
                    .AsQueryable();

                // Filter theo trạng thái
                if (status.HasValue)
                {
                    var statusValue = status.Value;
                    query = query.Where(h => GetStatusNumber(h.TrangThaiHoaDon) == statusValue);
                }

                // Tìm kiếm
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var searchLower = search.ToLower();
                    query = query.Where(h =>
                        (h.IdHoaDon.ToString().Contains(searchLower)) ||
                        (h.KhachHang != null && h.KhachHang.HoTenKhachHang != null && h.KhachHang.HoTenKhachHang.ToLower().Contains(searchLower)) ||
                        (h.HoTenNguoiNhan != null && h.HoTenNguoiNhan.ToLower().Contains(searchLower)) ||
                        (h.SdtKhachHang != null && h.SdtKhachHang.Contains(searchLower))
                    );
                }

                // Filter theo ngày
                if (fromDate.HasValue)
                {
                    query = query.Where(h => h.NgayLapHoaDon >= fromDate.Value);
                }
                if (toDate.HasValue)
                {
                    query = query.Where(h => h.NgayLapHoaDon <= toDate.Value.AddDays(1).AddSeconds(-1));
                }

                // Filter theo phương thức thanh toán (Online/Offline)
                if (!string.IsNullOrWhiteSpace(paymentMethod))
                {
                    query = query.Where(h => h.PhuongThucThanhToan == paymentMethod);
                }

                var hoaDons = await query
                    .OrderByDescending(h => h.NgayLapHoaDon)
                    .ToListAsync();

                var invoices = hoaDons.Select(h => new
                {
                    idHoaDon = h.IdHoaDon,
                    maDon = $"HD{h.IdHoaDon:D6}",
                    ngayLap = h.NgayLapHoaDon?.ToString("dd/MM/yyyy HH:mm"),
                    trangThai = h.TrangThaiHoaDon ?? "Chưa xác định",
                    trangThaiSo = GetStatusNumber(h.TrangThaiHoaDon),
                    tongTien = h.TongTien ?? 0,
                    phuongThucThanhToan = h.PhuongThucThanhToan ?? "COD",
                    hoTenKhachHang = h.KhachHang?.HoTenKhachHang ?? h.HoTenNguoiNhan ?? "N/A",
                    sdtKhachHang = h.KhachHang?.SdtKhachHang ?? h.SdtKhachHang ?? "N/A",
                    emailKhachHang = h.KhachHang?.EmailKhachHang ?? "N/A",
                    hoTenNhanVien = h.NhanVien?.HoTenNhanVien ?? "N/A",
                    soLuongSanPham = h.HoaDonChiTiets?.Count ?? 0,
                    chiTiet = h.HoaDonChiTiets?.Select(hdct => new
                    {
                        tenSanPham = hdct.ModelSanPham?.SanPham?.TenSanPham ?? "N/A",
                        tenModel = hdct.ModelSanPham?.TenModel ?? "N/A",
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
                        giaKhuyenMai = hdct.GiaKhuyenMai,
                        thanhTien = hdct.ThanhTien ?? 0,
                        hinhAnh = hdct.ModelSanPham?.AnhSanPhams?.FirstOrDefault()?.DuongDan ?? "/images/default-product.jpg",
                        idImei = hdct.IdImei,
                        maImei = hdct.Imei?.MaImei ?? "N/A",
                        trangThaiImei = hdct.Imei?.TrangThai ?? "N/A"
                    }).ToList()
                }).ToList();

                return Ok(new { success = true, data = invoices });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi server: " + ex.Message });
            }
        }

        // GET: API - Lấy chi tiết hóa đơn
        [HttpGet]
        public async Task<IActionResult> GetInvoiceDetail(int id)
        {
            try
            {
                var hoaDon = await _context.HoaDons
                    .Include(h => h.KhachHang)
                    .Include(h => h.NhanVien)
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
                    .FirstOrDefaultAsync(h => h.IdHoaDon == id);

                if (hoaDon == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy hóa đơn" });
                }

                var invoice = new
                {
                    idHoaDon = hoaDon.IdHoaDon,
                    maDon = $"HD{hoaDon.IdHoaDon:D6}",
                    ngayLap = hoaDon.NgayLapHoaDon?.ToString("dd/MM/yyyy HH:mm"),
                    trangThai = hoaDon.TrangThaiHoaDon ?? "Chưa xác định",
                    trangThaiSo = GetStatusNumber(hoaDon.TrangThaiHoaDon),
                    tongTien = hoaDon.TongTien ?? 0,
                    phuongThucThanhToan = hoaDon.PhuongThucThanhToan ?? "COD",
                    hoTenKhachHang = hoaDon.KhachHang?.HoTenKhachHang ?? hoaDon.HoTenNguoiNhan ?? "N/A",
                    sdtKhachHang = hoaDon.KhachHang?.SdtKhachHang ?? hoaDon.SdtKhachHang ?? "N/A",
                    emailKhachHang = hoaDon.KhachHang?.EmailKhachHang ?? "N/A",
                    hoTenNhanVien = hoaDon.NhanVien?.HoTenNhanVien ?? "N/A",
                    chiTiet = hoaDon.HoaDonChiTiets?.Select(hdct => new
                    {
                        tenSanPham = hdct.ModelSanPham?.SanPham?.TenSanPham ?? "N/A",
                        tenModel = hdct.ModelSanPham?.TenModel ?? "N/A",
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
                        hinhAnh = hdct.ModelSanPham?.AnhSanPhams?.FirstOrDefault()?.DuongDan ?? "/images/default-product.jpg",
                        idImei = hdct.IdImei,
                        maImei = hdct.Imei?.MaImei ?? "N/A",
                        trangThaiImei = hdct.Imei?.TrangThai ?? "N/A"
                    }).ToList()
                };

                return Ok(new { success = true, data = invoice });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi server: " + ex.Message });
            }
        }

        // POST: API - Cập nhật trạng thái hóa đơn
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusModel model)
        {
            try
            {
                if (model == null || model.TrangThai < 0 || model.TrangThai > 4)
                {
                    return BadRequest(new { success = false, message = "Trạng thái không hợp lệ" });
                }

                var hoaDon = await _context.HoaDons
                    .Include(h => h.HoaDonChiTiets)
                        .ThenInclude(hdct => hdct.Imei)
                    .FirstOrDefaultAsync(h => h.IdHoaDon == id);
                    
                if (hoaDon == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy hóa đơn" });
                }

                // Lấy ID nhân viên đang xử lý (nếu có)
                var idNhanVien = GetCurrentNhanVienId();
                
                // Nếu hủy đơn hàng (trạng thái 4), cần khôi phục IMEI và tồn kho
                var isCancelling = model.TrangThai == 4 && hoaDon.TrangThaiHoaDon != "Hủy đơn hàng";
                
                // Nếu đang hủy đơn hàng, ProcessPaymentCancel sẽ tự cập nhật trạng thái
                if (isCancelling)
                {
                    // Lưu ID nhân viên nếu có
                    if (!hoaDon.IdNhanVien.HasValue && idNhanVien.HasValue)
                    {
                        hoaDon.IdNhanVien = idNhanVien;
                        _context.HoaDons.Update(hoaDon);
                        await _context.SaveChangesAsync();
                    }
                    
                    // ProcessPaymentCancel sẽ cập nhật trạng thái và khôi phục IMEI/tồn kho
                    await ProcessPaymentCancel(hoaDon);
                }
                else
                {
                // Cập nhật trạng thái và lưu ID nhân viên xử lý
                hoaDon.TrangThaiHoaDon = GetStatusName(model.TrangThai);
                // Nếu chưa có nhân viên thì lưu, nếu đã có thì giữ nguyên (để lưu nhân viên đầu tiên tạo đơn)
                if (!hoaDon.IdNhanVien.HasValue && idNhanVien.HasValue)
                {
                    hoaDon.IdNhanVien = idNhanVien;
                }
                
                _context.HoaDons.Update(hoaDon);
                await _context.SaveChangesAsync();
                }

                return Ok(new { success = true, message = "Cập nhật trạng thái thành công!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating invoice status for invoice {InvoiceId}", id);
                return StatusCode(500, new { success = false, message = "Lỗi server: " + ex.Message });
            }
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
            if (lower.Contains("chờ") || lower.Contains("pending") || lower == "0") return 0;
            if (lower.Contains("xác nhận") || lower.Contains("confirmed") || lower == "1") return 1;
            if (lower.Contains("vận chuyển") || lower.Contains("shipping") || lower == "2") return 2;
            if (lower.Contains("thành công") || lower.Contains("completed") || lower == "3") return 3;
            if (lower.Contains("hủy") || lower.Contains("cancel") || lower == "4") return 4;

            return -1;
        }

        // Helper: Chuyển đổi trạng thái từ số sang string
        private string GetStatusName(int statusNum)
        {
            return statusNum switch
            {
                0 => "Chờ xác nhận",
                1 => "Đã xác nhận",
                2 => "Đang vận chuyển",
                3 => "Giao hàng thành công",
                4 => "Hủy đơn hàng",
                _ => "Chưa xác định"
            };
        }

        // Helper method: Lấy ID nhân viên từ JWT token
        private int? GetCurrentNhanVienId()
        {
            try
            {
                var token = Request.Cookies["jwt"];
                if (string.IsNullOrEmpty(token)) return null;

                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadJwtToken(token);
                var nhanVienIdClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "IdNhanVien");
                
                if (nhanVienIdClaim != null && int.TryParse(nhanVienIdClaim.Value, out int nhanVienId))
                {
                    return nhanVienId;
                }
            }
            catch { }
            return null;
        }

        // ==================== PAYOS PAYMENT METHODS ====================

        /// <summary>
        /// Helper method: Parse mã đơn hàng (HD000030) thành IdHoaDon (30)
        /// </summary>
        private int? ParseMaDon(string maDon)
        {
            if (string.IsNullOrWhiteSpace(maDon))
                return null;

            // Loại bỏ khoảng trắng và chuyển thành chữ hoa
            maDon = maDon.Trim().ToUpper();

            // Nếu bắt đầu bằng "HD", lấy phần số sau "HD"
            if (maDon.StartsWith("HD"))
            {
                var idString = maDon.Substring(2);
                if (int.TryParse(idString, out int id) && id > 0)
                {
                    return id;
                }
            }

            // Nếu là số thuần túy, thử parse trực tiếp
            if (int.TryParse(maDon, out int directId) && directId > 0)
            {
                return directId;
            }

            return null;
        }

        /// <summary>
        /// Helper method: Format IdHoaDon thành mã đơn hàng (30 -> HD000030)
        /// </summary>
        private string FormatMaDon(int idHoaDon)
        {
            return $"HD{idHoaDon:D6}";
        }

        /// <summary>
        /// Tạo payment link PayOS cho hóa đơn chuyển khoản
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreatePayOSPayment([FromBody] CreatePayOSPaymentModel model)
        {
            try
            {
                if (model == null || model.IdHoaDon <= 0)
                {
                    return BadRequest(new { success = false, message = "Thông tin không hợp lệ" });
                }

                var hoaDon = await _context.HoaDons
                    .Include(h => h.KhachHang)
                    .Include(h => h.HoaDonChiTiets)
                        .ThenInclude(hdct => hdct.ModelSanPham)
                            .ThenInclude(m => m.SanPham)
                    .FirstOrDefaultAsync(h => h.IdHoaDon == model.IdHoaDon);

                if (hoaDon == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy hóa đơn" });
                }

                // Kiểm tra phương thức thanh toán
                var isBankTransfer = hoaDon.PhuongThucThanhToan?.ToLower().Contains("chuyển khoản") == true ||
                                    hoaDon.PhuongThucThanhToan?.ToLower().Contains("bank") == true;

                if (!isBankTransfer)
                {
                    return BadRequest(new { success = false, message = "Hóa đơn này không phải thanh toán chuyển khoản" });
                }

                // Kiểm tra đã thanh toán/xác nhận chưa
                if (hoaDon.TrangThaiHoaDon == "Đã xác nhận" || hoaDon.TrangThaiHoaDon == "Đã thanh toán")
                {
                    return BadRequest(new { success = false, message = "Hóa đơn đã được thanh toán" });
                }

                var tongTien = hoaDon.TongTien ?? 0;
                if (tongTien <= 0)
                {
                    return BadRequest(new { success = false, message = "Tổng tiền không hợp lệ" });
                }

                // PayOS yêu cầu orderCode là số nguyên dương, unique
                // Sử dụng IdHoaDon làm orderCode (đơn giản và đảm bảo unique)
                long orderCode = hoaDon.IdHoaDon;

                // Tạo mã đơn hàng (HD000030) để lưu vào description
                var maDon = FormatMaDon(hoaDon.IdHoaDon);
                var description = maDon;

                // Tạo danh sách sản phẩm từ HoaDonChiTiets
                var payOSItems = new List<DATN_DT.IServices.PayOSItem>();
                decimal totalItemsAmount = 0;
                
                if (hoaDon.HoaDonChiTiets != null && hoaDon.HoaDonChiTiets.Any())
                {
                    foreach (var chiTiet in hoaDon.HoaDonChiTiets)
                    {
                        var tenSanPham = chiTiet.ModelSanPham?.SanPham?.TenSanPham 
                            ?? chiTiet.ModelSanPham?.TenModel 
                            ?? "Sản phẩm";
                        
                        var donGia = chiTiet.DonGia ?? 0;
                        var soLuong = chiTiet.SoLuong ?? 1;
                        var thanhTien = donGia * soLuong;
                        totalItemsAmount += thanhTien;
                        
                        payOSItems.Add(new DATN_DT.IServices.PayOSItem
                        {
                            Name = $"{tenSanPham} - {chiTiet.ModelSanPham?.Mau ?? ""}".TrimEnd(' ', '-'),
                            Quantity = soLuong,
                            Price = (int)donGia // Giá đơn vị
                        });
                    }
                }

                // Log để debug
                _logger.LogInformation("PayOS Items: Total={Total}, Amount={Amount}, Items Count={Count}", 
                    totalItemsAmount, tongTien, payOSItems.Count);

                // Tạo return URL và cancel URL - sử dụng 2 endpoint riêng biệt
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var returnUrl = $"{baseUrl}/HoaDon/ProcessPaymentSuccess?maDon={Uri.EscapeDataString(maDon)}";
                var cancelUrl = $"{baseUrl}/HoaDon/ProcessPaymentCancel?maDon={Uri.EscapeDataString(maDon)}";

                // Tạo payment link với thông tin sản phẩm đầy đủ
                var paymentUrl = await _payOSService.CreatePaymentLinkAsync(
                    orderCode: orderCode,
                    amount: (int)tongTien, // PayOS nhận số tiền là int (VND)
                    description: description,
                    returnUrl: returnUrl,
                    cancelUrl: cancelUrl,
                    items: payOSItems.Any() ? payOSItems : null
                );

                // Tạo QR code URL
                var qrCodeUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=300x300&data={Uri.EscapeDataString(paymentUrl)}";

                return Ok(new
                {
                    success = true,
                    paymentUrl = paymentUrl,
                    qrCodeUrl = qrCodeUrl,
                    orderCode = orderCode,
                    maDon = maDon, // Trả về mã đơn hàng (HD000030)
                    tongTien = tongTien
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating PayOS payment link");
                return StatusCode(500, new { success = false, message = "Lỗi tạo payment link: " + ex.Message });
            }
        }

        /// <summary>
        /// Xử lý khi khách hàng quay lại từ PayOS (Return URL - thành công)
        /// Redirect đến API ProcessPaymentSuccess để xử lý
        /// </summary>
        [AllowAnonymous]
        [HttpGet]
        public IActionResult PayOSReturn([FromQuery] string? maDon, [FromQuery] long? orderCode, [FromQuery] string? status)
        {
            // Redirect đến API xử lý thanh toán thành công
            var queryParams = new List<string>();
            if (!string.IsNullOrWhiteSpace(maDon))
                queryParams.Add($"maDon={Uri.EscapeDataString(maDon)}");
            if (orderCode.HasValue && orderCode.Value > 0)
                queryParams.Add($"orderCode={orderCode.Value}");

            var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
            return Redirect($"/HoaDon/ProcessPaymentSuccess{queryString}");
        }

        /// <summary>
        /// Xử lý khi khách hàng hủy thanh toán (Cancel URL)
        /// Redirect đến API ProcessPaymentCancel để xử lý
        /// </summary>
        [AllowAnonymous]
        [HttpGet]
        public IActionResult PayOSCancel([FromQuery] string? maDon, [FromQuery] long? orderCode)
        {
            // Redirect đến API xử lý thanh toán bị hủy
            var queryParams = new List<string>();
            if (!string.IsNullOrWhiteSpace(maDon))
                queryParams.Add($"maDon={Uri.EscapeDataString(maDon)}");
            if (orderCode.HasValue && orderCode.Value > 0)
                queryParams.Add($"orderCode={orderCode.Value}");

            var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
            return Redirect($"/HoaDon/ProcessPaymentCancel{queryString}");
        }

        /// <summary>
        /// API: Xử lý thanh toán thành công từ PayOS (Return URL)
        /// Kiểm tra trạng thái thực tế từ PayOS và cập nhật hóa đơn
        /// </summary>
        [AllowAnonymous]
        [HttpGet]
        [Route("HoaDon/ProcessPaymentSuccess")]
        public async Task<IActionResult> HandlePaymentSuccess(
            [FromQuery] string? maDon, 
            [FromQuery] long? orderCode)
        {
            try
            {
                HoaDon? hoaDon = null;

                // Tìm hóa đơn theo mã đơn hàng (HD000030) - ưu tiên
                if (!string.IsNullOrWhiteSpace(maDon))
                {
                    var idHoaDon = ParseMaDon(maDon);
                    if (idHoaDon.HasValue)
                    {
                        hoaDon = await _context.HoaDons
                            .Include(h => h.KhachHang)
                            .Include(h => h.HoaDonChiTiets)
                                .ThenInclude(hdct => hdct.ModelSanPham)
                            .FirstOrDefaultAsync(h => h.IdHoaDon == idHoaDon.Value);
                    }
                }
                // Fallback: tìm theo orderCode
                else if (orderCode.HasValue && orderCode.Value > 0)
                {
                    hoaDon = await _context.HoaDons
                        .Include(h => h.KhachHang)
                        .Include(h => h.HoaDonChiTiets)
                            .ThenInclude(hdct => hdct.ModelSanPham)
                        .FirstOrDefaultAsync(h => h.IdHoaDon == orderCode.Value);
                }

                if (hoaDon == null)
                {
                    _logger.LogWarning("HandlePaymentSuccess: Invoice not found for maDon={MaDon}, orderCode={OrderCode}", maDon, orderCode);
                    return RedirectToAction("PaymentRedirect", "HoaDon", new { returnUrl = "/UserProfile?payment=error" });
                }

                var maDonHang = FormatMaDon(hoaDon.IdHoaDon);

                // Kiểm tra trạng thái thanh toán thực tế từ PayOS API (dùng IdHoaDon làm orderCode)
                var paymentInfo = await _payOSService.GetPaymentInfoAsync(hoaDon.IdHoaDon);
                var paymentStatus = paymentInfo?.Status?.ToLower() ?? "";

                _logger.LogInformation("HandlePaymentSuccess: Payment status from PayOS = '{Status}' for invoice {InvoiceId} (MaDon: {MaDon})", 
                    paymentStatus, hoaDon.IdHoaDon, maDonHang);

                // Chỉ xử lý nếu trạng thái thực tế là "paid"
                if (paymentStatus == "paid")
                {
                    // Thanh toán thành công - cập nhật trạng thái thành "Đã xác nhận" (1)
                    // Chỉ cập nhật nếu chưa được xác nhận (tránh xử lý trùng)
                    if (hoaDon.TrangThaiHoaDon != "Đã xác nhận" && hoaDon.TrangThaiHoaDon != "Đã thanh toán")
                    {
                        try
                        {
                            await ProcessPaymentSuccess(hoaDon);
                            _logger.LogInformation("HandlePaymentSuccess: Payment processed successfully for invoice {InvoiceId} (MaDon: {MaDon})", 
                                hoaDon.IdHoaDon, maDonHang);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "HandlePaymentSuccess: Error processing payment for invoice {InvoiceId}", hoaDon.IdHoaDon);
                            // Vẫn redirect dù có lỗi, webhook sẽ xử lý lại
                        }
                    }
                    else
                    {
                        _logger.LogInformation("HandlePaymentSuccess: Invoice {InvoiceId} already processed (status: {Status})", 
                            hoaDon.IdHoaDon, hoaDon.TrangThaiHoaDon);
                    }
                }
                else
                {
                    // Trạng thái chưa phải "paid" - có thể đang xử lý hoặc chưa thanh toán
                    _logger.LogWarning("HandlePaymentSuccess: Payment not completed yet (status: {Status}) for invoice {InvoiceId} (MaDon: {MaDon})", 
                        paymentStatus, hoaDon.IdHoaDon, maDonHang);
                }

                // Redirect đến trang redirect với returnUrl đến UserProfile
                var redirectUrl = $"/UserProfile?payment=success&maDon={Uri.EscapeDataString(maDonHang)}";
                return RedirectToAction("PaymentRedirect", "HoaDon", new { returnUrl = redirectUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HandlePaymentSuccess: Error processing payment result");
                // Redirect đến trang redirect với returnUrl đến UserProfile
                var redirectUrl = "/UserProfile?payment=success";
                return RedirectToAction("PaymentRedirect", "HoaDon", new { returnUrl = redirectUrl });
            }
        }

        /// <summary>
        /// API: Xử lý thanh toán bị hủy từ PayOS (Cancel URL)
        /// </summary>
        [AllowAnonymous]
        [HttpGet]
        [Route("HoaDon/ProcessPaymentCancel")]
        public async Task<IActionResult> HandlePaymentCancel(
            [FromQuery] string? maDon, 
            [FromQuery] long? orderCode)
        {
            try
            {
                HoaDon? hoaDon = null;

                // Tìm hóa đơn theo mã đơn hàng (HD000030) - ưu tiên
                if (!string.IsNullOrWhiteSpace(maDon))
                {
                    var idHoaDon = ParseMaDon(maDon);
                    if (idHoaDon.HasValue)
                    {
                        hoaDon = await _context.HoaDons
                            .Include(h => h.KhachHang)
                            .FirstOrDefaultAsync(h => h.IdHoaDon == idHoaDon.Value);
                    }
                }
                // Fallback: tìm theo orderCode
                else if (orderCode.HasValue && orderCode.Value > 0)
                {
                    hoaDon = await _context.HoaDons
                        .Include(h => h.KhachHang)
                        .FirstOrDefaultAsync(h => h.IdHoaDon == orderCode.Value);
                }

                if (hoaDon == null)
                {
                    _logger.LogWarning("HandlePaymentCancel: Invoice not found for maDon={MaDon}, orderCode={OrderCode}", maDon, orderCode);
                    return RedirectToAction("PaymentRedirect", "HoaDon", new { returnUrl = "/UserProfile?payment=error" });
                }

                var maDonHang = FormatMaDon(hoaDon.IdHoaDon);

                // Kiểm tra trạng thái từ PayOS để xác nhận
                var paymentInfo = await _payOSService.GetPaymentInfoAsync(hoaDon.IdHoaDon);
                var paymentStatus = paymentInfo?.Status?.ToLower() ?? "";

                _logger.LogInformation("HandlePaymentCancel: Payment cancelled for invoice {InvoiceId} (MaDon: {MaDon}), PayOS status: {Status}", 
                    hoaDon.IdHoaDon, maDonHang, paymentStatus);

                // Xử lý hủy đơn hàng: cập nhật trạng thái, khôi phục IMEI và tồn kho

                    await ProcessPaymentCancel(hoaDon);
                    _logger.LogInformation("HandlePaymentCancel: Order cancelled and inventory restored for invoice {InvoiceId} (MaDon: {MaDon})", 
                        hoaDon.IdHoaDon, maDonHang);

                // Redirect đến trang redirect với returnUrl đến UserProfile
                var redirectUrl = $"/UserProfile?payment=cancelled&maDon={Uri.EscapeDataString(maDonHang)}";
                return RedirectToAction("PaymentRedirect", "HoaDon", new { returnUrl = redirectUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HandlePaymentCancel: Error processing payment cancel");
                // Redirect đến trang redirect với returnUrl đến UserProfile
                var redirectUrl = "/UserProfile?payment=cancelled";
                return RedirectToAction("PaymentRedirect", "HoaDon", new { returnUrl = redirectUrl });
            }
        }

        /// <summary>
        /// Trang redirect trung gian để chuyển hướng đến UserProfile
        /// </summary>
        [AllowAnonymous]
        [HttpGet]
        public IActionResult PaymentRedirect([FromQuery] string? returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl ?? "/UserProfile";
            return View();
        }

        /// <summary>
        /// API: Lấy thông tin thanh toán PayOS qua mã đơn hàng (HD000030)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPaymentInfo([FromQuery] string maDon)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(maDon))
                {
                    return BadRequest(new { success = false, message = "Mã đơn hàng không hợp lệ" });
                }

                var idHoaDon = ParseMaDon(maDon);
                if (!idHoaDon.HasValue)
                {
                    return BadRequest(new { success = false, message = "Mã đơn hàng không hợp lệ" });
                }

                var hoaDon = await _context.HoaDons
                    .Include(h => h.KhachHang)
                    .FirstOrDefaultAsync(h => h.IdHoaDon == idHoaDon.Value);

                if (hoaDon == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy hóa đơn" });
                }

                // Lấy thông tin thanh toán từ PayOS (orderCode = IdHoaDon)
                var paymentInfo = await _payOSService.GetPaymentInfoAsync(idHoaDon.Value);

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        maDon = FormatMaDon(hoaDon.IdHoaDon),
                        idHoaDon = hoaDon.IdHoaDon,
                        tongTien = hoaDon.TongTien ?? 0,
                        trangThai = hoaDon.TrangThaiHoaDon ?? "Chưa xác định",
                        phuongThucThanhToan = hoaDon.PhuongThucThanhToan ?? "COD",
                        paymentInfo = paymentInfo != null ? new
                        {
                            orderCode = paymentInfo.OrderCode,
                            amount = paymentInfo.Amount,
                            status = paymentInfo.Status,
                            description = paymentInfo.Description,
                            createdAt = paymentInfo.CreatedAt,
                            paidAt = paymentInfo.PaidAt
                        } : null
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment info for maDon: {MaDon}", maDon);
                return StatusCode(500, new { success = false, message = "Lỗi server: " + ex.Message });
            }
        }

        /// <summary>
        /// Webhook từ PayOS - xử lý thông báo thanh toán
        /// </summary>
        [AllowAnonymous]
        [HttpPost("api/payos/webhook")]
        public async Task<IActionResult> PayOSWebhook()
        {
            try
            {
                // Đọc body của request
                using var reader = new StreamReader(Request.Body);
                var webhookBody = await reader.ReadToEndAsync();

                // Lấy signature từ header
                var signature = Request.Headers["x-payos-signature"].FirstOrDefault();

                if (string.IsNullOrEmpty(signature))
                {
                    _logger.LogWarning("PayOS webhook missing signature");
                    return BadRequest(new { error = "Missing signature" });
                }

                // Xác thực signature
                if (!_payOSService.VerifyWebhookSignature(webhookBody, signature))
                {
                    _logger.LogWarning("PayOS webhook invalid signature");
                    return BadRequest(new { error = "Invalid signature" });
                }

                // Parse webhook data
                var webhookData = JsonSerializer.Deserialize<JsonElement>(webhookBody);
                
                if (!webhookData.TryGetProperty("data", out var data))
                {
                    return BadRequest(new { error = "Invalid webhook data" });
                }

                var orderCode = data.TryGetProperty("orderCode", out var oc) ? oc.GetInt64() : 0;
                var status = data.TryGetProperty("status", out var st) ? st.GetString() : null;

                if (orderCode <= 0)
                {
                    return BadRequest(new { error = "Invalid order code" });
                }

                // Tìm hóa đơn - orderCode từ PayOS chính là IdHoaDon
                var hoaDon = await _context.HoaDons
                    .Include(h => h.HoaDonChiTiets)
                        .ThenInclude(hdct => hdct.ModelSanPham)
                    .FirstOrDefaultAsync(h => h.IdHoaDon == orderCode);

                if (hoaDon == null)
                {
                    var maDon = FormatMaDon((int)orderCode);
                    _logger.LogWarning("PayOS webhook: Invoice not found for orderCode {OrderCode} (MaDon: {MaDon})", orderCode, maDon);
                    return NotFound(new { error = "Invoice not found" });
                }

                // Kiểm tra trạng thái thanh toán
                // Xử lý nếu chưa được xác nhận (tránh xử lý trùng)
                if (status?.ToLower() == "paid" && 
                    hoaDon.TrangThaiHoaDon != "Đã xác nhận" && 
                    hoaDon.TrangThaiHoaDon != "Đã thanh toán")
                {
                    // Xử lý thanh toán thành công
                    await ProcessPaymentSuccess(hoaDon);
                }
                else if (status?.ToLower() == "cancelled")
                {
                    // Thanh toán bị hủy - không cần làm gì, giữ nguyên trạng thái
                    var maDon = FormatMaDon(hoaDon.IdHoaDon);
                    _logger.LogInformation("PayOS payment cancelled for orderCode {OrderCode} (MaDon: {MaDon})", orderCode, maDon);
                }

                // Trả về 200 OK để PayOS biết đã nhận được webhook
                return Ok(new { error = 0, message = "Success" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing PayOS webhook");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Xử lý hủy thanh toán: cập nhật trạng thái, khôi phục IMEI và tồn kho
        /// </summary>
        private async Task ProcessPaymentCancel(HoaDon hoaDon)
        {
            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Cập nhật trạng thái hóa đơn thành "Hủy đơn hàng"
                hoaDon.TrangThaiHoaDon = "Hủy đơn hàng";
                _context.HoaDons.Update(hoaDon);

                // 2. Load chi tiết hóa đơn với IMEI và ModelSanPham
                await _context.Entry(hoaDon)
                    .Collection(h => h.HoaDonChiTiets)
                    .Query()
                    .Include(hdct => hdct.Imei)
                    .Include(hdct => hdct.ModelSanPham)
                    .LoadAsync();

                var modelSanPhamIdsToRefresh = new HashSet<int>();

                // 3. Khôi phục IMEI về trạng thái "Còn hàng" cho các IMEI đã được gán
                var chiTietWithImei = hoaDon.HoaDonChiTiets?
                    .Where(hdct => hdct.IdImei.HasValue)
                    .ToList() ?? new List<HoaDonChiTiet>();

                foreach (var chiTiet in chiTietWithImei)
                {
                    if (chiTiet.Imei != null && chiTiet.IdModelSanPham.HasValue)
                    {
                        chiTiet.Imei.TrangThai = "Còn hàng";
                        _context.Imeis.Update(chiTiet.Imei);
                        
                        // Thêm vào danh sách cần refresh tồn kho
                        modelSanPhamIdsToRefresh.Add(chiTiet.IdModelSanPham.Value);
                    }
                }

                // 4. Lưu thay đổi IMEI trước
                await _context.SaveChangesAsync();

                // 5. Refresh tồn kho và tự động cập nhật trạng thái cho tất cả model đã được khôi phục IMEI
                foreach (var idModelSanPham in modelSanPhamIdsToRefresh)
                {
                    // Refresh tồn kho (sẽ tự động cập nhật trạng thái trong service)
                    await _tonKhoService.RefreshTonKhoForModel(idModelSanPham);
                    
                    // Trạng thái đã được tự động cập nhật bởi service
                }

                // 6. Lưu tất cả thay đổi
                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                _logger.LogInformation("ProcessPaymentCancel: Successfully cancelled invoice {InvoiceId}, restored {ImeiCount} IMEIs and refreshed inventory for {ModelCount} models", 
                    hoaDon.IdHoaDon, chiTietWithImei.Count, modelSanPhamIdsToRefresh.Count);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _logger.LogError(ex, "ProcessPaymentCancel: Error cancelling invoice {InvoiceId}", hoaDon.IdHoaDon);
                throw;
            }
        }

        /// <summary>
        /// Xử lý thanh toán thành công: trừ tồn kho, gán IMEI, cập nhật trạng thái
        /// </summary>
        private async Task ProcessPaymentSuccess(HoaDon hoaDon)
        {
            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                // Kiểm tra lại trạng thái (tránh xử lý trùng)
                // Kiểm tra cả "Đã thanh toán" (cũ) và "Đã xác nhận" (mới)
                if (hoaDon.TrangThaiHoaDon == "Đã xác nhận" || hoaDon.TrangThaiHoaDon == "Đã thanh toán")
                {
                    await tx.CommitAsync();
                    return;
                }

                // Trừ IMEI và TonKho cho các HoaDonChiTiet chưa có IdImei
                var tonKhoRepo = new Repos.TonKhoRepo(_context);

                // Nhóm HoaDonChiTiet theo IdModelSanPham để xử lý
                var chiTietGroups = hoaDon.HoaDonChiTiets?
                    .Where(hdct => !hdct.IdImei.HasValue)
                    .GroupBy(hdct => hdct.IdModelSanPham)
                    .ToList() ?? new List<IGrouping<int?, HoaDonChiTiet>>();

                foreach (var group in chiTietGroups)
                {
                    var idModelSanPham = group.Key;
                    var qty = group.Count();

                    if (!idModelSanPham.HasValue) continue;

                    // Lấy đủ IMEI còn hàng
                    var imeis = await _context.Imeis
                        .Where(i => i.IdModelSanPham == idModelSanPham && i.TrangThai == "Còn hàng")
                        .OrderBy(i => i.IdImei)
                        .Take(qty)
                        .ToListAsync();

                    if (imeis.Count < qty)
                    {
                        _logger.LogError("Insufficient IMEI for ModelSanPham {ModelId} in invoice {InvoiceId}",
                            idModelSanPham, hoaDon.IdHoaDon);
                        await tx.RollbackAsync();
                        return;
                    }

                    // Gán IMEI vào HoaDonChiTiet và set "Đã bán"
                    var chiTietList = group.ToList();
                    for (int i = 0; i < chiTietList.Count && i < imeis.Count; i++)
                    {
                        chiTietList[i].IdImei = imeis[i].IdImei;
                        imeis[i].TrangThai = "Đã bán";
                        _context.Imeis.Update(imeis[i]);
                    }

                    // Lưu thay đổi IMEI trước
                    await _context.SaveChangesAsync();

                    // Refresh tồn kho cho model này (tính lại từ IMEI "Còn hàng")
                    await tonKhoRepo.RefreshTonKhoForModel(idModelSanPham.Value);

                    // Kiểm tra số lượng tồn kho còn lại sau khi refresh
                    var tonKhoConLai = await _context.TonKhos
                        .Where(tk => tk.IdModelSanPham == idModelSanPham)
                        .SumAsync(tk => tk.SoLuong);

                    // Nếu số lượng tồn kho còn lại = 0, cập nhật trạng thái sang "Hết hàng" (TrangThai = 0)
                    var modelSanPham = await _context.ModelSanPhams
                        .FirstOrDefaultAsync(msp => msp.IdModelSanPham == idModelSanPham.Value);
                    
                    if (tonKhoConLai <= 0 && modelSanPham != null && modelSanPham.TrangThai == 1)
                    {
                        modelSanPham.TrangThai = 0; // Hết hàng
                        _context.ModelSanPhams.Update(modelSanPham);
                    }
                }

                // Cập nhật trạng thái hóa đơn thành "Đã xác nhận" (số 1)
                hoaDon.TrangThaiHoaDon = "Đã xác nhận";
                _context.HoaDons.Update(hoaDon);

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                var maDon = FormatMaDon(hoaDon.IdHoaDon);
                _logger.LogInformation("Payment processed successfully for invoice {InvoiceId} (MaDon: {MaDon})", hoaDon.IdHoaDon, maDon);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _logger.LogError(ex, "Error processing payment success for invoice {InvoiceId}", hoaDon.IdHoaDon);
                throw;
            }
        }
    }

    // Model cho update status
    public class UpdateStatusModel
    {
        public int TrangThai { get; set; }
    }

    // Model cho tạo PayOS payment
    public class CreatePayOSPaymentModel
    {
        public int IdHoaDon { get; set; }
    }
}

