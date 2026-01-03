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

namespace DATN_DT.Controllers
{
    public class HoaDonController : Controller
    {
        private readonly MyDbContext _context;

        public HoaDonController(MyDbContext context)
        {
            _context = context;
        }

        // GET: HoaDon
        public IActionResult Index()
        {
            return View();
        }

        // GET: API - Lấy danh sách hóa đơn
        [HttpGet]
        public async Task<IActionResult> GetInvoices(int? status, string? search, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var query = _context.HoaDons
                    .Include(h => h.KhachHang)
                    .Include(h => h.NhanVien)
                    .Include(h => h.HoaDonChiTiets)
                        .ThenInclude(hdct => hdct.ModelSanPham)
                            .ThenInclude(m => m.SanPham)
                    .Include(h => h.HoaDonChiTiets)
                        .ThenInclude(hdct => hdct.ModelSanPham)
                            .ThenInclude(m => m.AnhSanPhams)
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
                        mau = hdct.ModelSanPham?.Mau ?? "N/A",
                        soLuong = hdct.SoLuong ?? 0,
                        donGia = hdct.DonGia ?? 0,
                        thanhTien = hdct.ThanhTien ?? 0,
                        hinhAnh = hdct.ModelSanPham?.AnhSanPhams?.FirstOrDefault()?.DuongDan ?? "/images/default-product.jpg"
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
                    .Include(h => h.HoaDonChiTiets)
                        .ThenInclude(hdct => hdct.ModelSanPham)
                            .ThenInclude(m => m.AnhSanPhams)
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
                        mau = hdct.ModelSanPham?.Mau ?? "N/A",
                        soLuong = hdct.SoLuong ?? 0,
                        donGia = hdct.DonGia ?? 0,
                        thanhTien = hdct.ThanhTien ?? 0,
                        hinhAnh = hdct.ModelSanPham?.AnhSanPhams?.FirstOrDefault()?.DuongDan ?? "/images/default-product.jpg"
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

                var hoaDon = await _context.HoaDons.FindAsync(id);
                if (hoaDon == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy hóa đơn" });
                }

                // Lấy ID nhân viên đang xử lý (nếu có)
                var idNhanVien = GetCurrentNhanVienId();
                
                // Cập nhật trạng thái và lưu ID nhân viên xử lý
                hoaDon.TrangThaiHoaDon = GetStatusName(model.TrangThai);
                // Nếu chưa có nhân viên thì lưu, nếu đã có thì giữ nguyên (để lưu nhân viên đầu tiên tạo đơn)
                if (!hoaDon.IdNhanVien.HasValue && idNhanVien.HasValue)
                {
                    hoaDon.IdNhanVien = idNhanVien;
                }
                
                _context.HoaDons.Update(hoaDon);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Cập nhật trạng thái thành công!" });
            }
            catch (Exception ex)
            {
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
    }

    // Model cho update status
    public class UpdateStatusModel
    {
        public int TrangThai { get; set; }
    }
}

