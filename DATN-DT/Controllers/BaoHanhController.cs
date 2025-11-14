using DATN_DT.Data;
using DATN_DT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DATN_DT.Controllers
{
    public class BaoHanhController : Controller
    {
        private readonly MyDbContext _context;

        public BaoHanhController(MyDbContext context)
        {
            _context = context;
        }

        // --- Index: Lấy danh sách Phiếu Bảo Hành ---
        public async Task<IActionResult> Index()
        {
            // Eager loading các đối tượng liên quan để hiển thị thông tin
            var baoHanhs = await _context.BaoHanhs
                .Include(bh => bh.Imei)
                .Include(bh => bh.KhachHang)
                .Include(bh => bh.NhanVien)
                .OrderByDescending(bh => bh.NgayNhan)
                .ToListAsync();

            return View(baoHanhs);
        }

        // --- Create: Thêm Phiếu Bảo Hành mới ---
        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> Create([FromBody] BaoHanh? baoHanh)
        {
            if (baoHanh == null)
                return BadRequest(new { message = "Dữ liệu không hợp lệ!" });

            var errors = new Dictionary<string, string>();

            // Validation
            if (baoHanh.IdImei == null || baoHanh.IdImei == 0)
                errors["IdImei"] = "Phải chọn Imei bảo hành!";
            if (baoHanh.IdKhachHang == null || baoHanh.IdKhachHang == 0)
                errors["IdKhachHang"] = "Phải chọn Khách Hàng!";
            if (baoHanh.IdNhanVien == null || baoHanh.IdNhanVien == 0)
                errors["IdNhanVien"] = "Phải chọn Nhân Viên tiếp nhận!";
            if (string.IsNullOrWhiteSpace(baoHanh.MoTaLoi))
                errors["MoTaLoi"] = "Phải mô tả lỗi chi tiết!";

            baoHanh.NgayNhan ??= DateTime.Now; // Ngày nhận mặc định là hôm nay
            baoHanh.TrangThai ??= "Đang tiếp nhận"; // Trạng thái mặc định

            if (errors.Count > 0)
                return BadRequest(errors);

            try
            {
                // Chuẩn hóa dữ liệu
                baoHanh.MoTaLoi = baoHanh.MoTaLoi.Trim();
                baoHanh.XuLy = baoHanh.XuLy?.Trim();
                baoHanh.TrangThai = baoHanh.TrangThai.Trim();

                _context.BaoHanhs.Add(baoHanh);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Thêm Phiếu Bảo Hành thành công!" });
            }
            catch (Exception ex)
            {
                // Log exception (ex)
                return StatusCode(500, new { message = "Lỗi khi thêm Phiếu Bảo Hành. Vui lòng thử lại!" });
            }
        }

        // --- Edit: Cập nhật Phiếu Bảo Hành ---
        [HttpPost]
        [Route("BaoHanh/Edit/{id}")]
        [Consumes("application/json")]
        public async Task<IActionResult> Edit(int id, [FromBody] BaoHanh? baoHanh)
        {
            if (baoHanh == null)
                return BadRequest(new { message = "Dữ liệu không hợp lệ!" });

            var errors = new Dictionary<string, string>();

            // Validation
            if (baoHanh.IdImei == null || baoHanh.IdImei == 0)
                errors["IdImei"] = "Phải chọn Imei bảo hành!";
            if (baoHanh.IdKhachHang == null || baoHanh.IdKhachHang == 0)
                errors["IdKhachHang"] = "Phải chọn Khách Hàng!";
            if (baoHanh.IdNhanVien == null || baoHanh.IdNhanVien == 0)
                errors["IdNhanVien"] = "Phải chọn Nhân Viên tiếp nhận!";
            if (string.IsNullOrWhiteSpace(baoHanh.MoTaLoi))
                errors["MoTaLoi"] = "Phải mô tả lỗi chi tiết!";

            if (errors.Count > 0)
                return BadRequest(errors);

            var existing = await _context.BaoHanhs.FindAsync(id);
            if (existing == null)
                return NotFound(new { message = "Không tìm thấy Phiếu Bảo Hành!" });

            try
            {
                // Cập nhật thông tin
                existing.IdImei = baoHanh.IdImei;
                existing.IdKhachHang = baoHanh.IdKhachHang;
                existing.IdNhanVien = baoHanh.IdNhanVien;
                existing.NgayNhan = baoHanh.NgayNhan;
                existing.NgayTra = baoHanh.NgayTra; // Cần thiết lập khi hoàn thành
                existing.TrangThai = baoHanh.TrangThai?.Trim();
                existing.MoTaLoi = baoHanh.MoTaLoi.Trim();
                existing.XuLy = baoHanh.XuLy?.Trim();
                existing.ChiPhiPhatSinh = baoHanh.ChiPhiPhatSinh;

                _context.BaoHanhs.Update(existing);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Cập nhật Phiếu Bảo Hành thành công!" });
            }
            catch
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật Phiếu Bảo Hành. Vui lòng thử lại!" });
            }
        }

        // --- API: Lấy danh sách Imei (Imei có thể được liên kết với Model Sản phẩm) ---
        [HttpGet]
        public async Task<IActionResult> GetImeis()
        {
            var list = await _context.Imeis
                .Select(i => new { i.IdImei, DisplayText = i.MaImei + (i.IdModelSanPham.HasValue ? $" ({i.IdModelSanPham})" : "") })
                .ToListAsync();
            return Ok(list);
        }

        // --- API: Lấy danh sách Khách Hàng ---
        [HttpGet]
        public async Task<IActionResult> GetKhachHangs()
        {
            var list = await _context.KhachHangs
                .Select(kh => new { kh.IdKhachHang, DisplayText = kh.HoTenKhachHang + " - " + kh.SdtKhachHang })
                .ToListAsync();
            return Ok(list);
        }

        // --- API: Lấy danh sách Nhân Viên ---
        [HttpGet]
        public async Task<IActionResult> GetNhanViens()
        {
            var list = await _context.NhanViens
                .Select(nv => new { nv.IdNhanVien, DisplayText = nv.HoTenNhanVien + " - " + nv.TenTaiKhoanNV })
                .ToListAsync();
            return Ok(list);
        }
    }
}