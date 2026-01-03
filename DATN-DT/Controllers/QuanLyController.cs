using DATN_DT.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DATN_DT.Controllers
{
    public class QuanLyController : Controller
    {
        private readonly MyDbContext _context;

        public QuanLyController(MyDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        // API: Lấy thống kê số lượng cho dashboard
        [HttpGet]
        [Route("QuanLy/GetStats")]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                // Đếm Nhân Viên (không bao gồm Admin)
                var soLuongNhanVien = await _context.NhanViens
                    .Include(nv => nv.ChucVu)
                    .Where(nv => nv.ChucVu.TenChucVuVietHoa != "ADMIN" && nv.ChucVu.TenChucVu != "Admin")
                    .CountAsync();

                // Đếm Khách Hàng
                var soLuongKhachHang = await _context.KhachHangs.CountAsync();

                // Đếm Sản Phẩm
                var soLuongSanPham = await _context.SanPhams.CountAsync();

                // Đếm Hóa Đơn
                var soLuongHoaDon = await _context.HoaDons.CountAsync();

                // Đếm Voucher
                var soLuongVoucher = await _context.Vouchers.CountAsync();

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        nhanVien = soLuongNhanVien,
                        khachHang = soLuongKhachHang,
                        sanPham = soLuongSanPham,
                        hoaDon = soLuongHoaDon,
                        voucher = soLuongVoucher
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi server: " + ex.Message });
            }
        }
    }
}
