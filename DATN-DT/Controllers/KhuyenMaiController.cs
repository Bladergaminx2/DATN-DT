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
    public class KhuyenMaiController : Controller
    {
        private readonly MyDbContext _context;

        public KhuyenMaiController(MyDbContext context)
        {
            _context = context;
        }

        // --- Index: Lấy danh sách Khuyến Mãi ---
        public async Task<IActionResult> Index()
        {
            var khuyenMais = await _context.KhuyenMais
                .OrderByDescending(km => km.NgayBatDau)
                .ToListAsync();

            return View(khuyenMais);
        }

        // --- Create: Thêm Khuyến Mãi mới ---
        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> Create([FromBody] KhuyenMai? khuyenMai)
        {
            if (khuyenMai == null)
                return BadRequest(new { message = "Dữ liệu không hợp lệ!" });

            var errors = new Dictionary<string, string>();

            // Validation cơ bản
            if (string.IsNullOrWhiteSpace(khuyenMai.MaKM))
                errors["MaKM"] = "Phải nhập Mã Khuyến Mãi!";
            if (string.IsNullOrWhiteSpace(khuyenMai.LoaiGiam))
                errors["LoaiGiam"] = "Phải chọn Loại Giảm!";
            if (khuyenMai.GiaTri == null || khuyenMai.GiaTri <= 0)
                errors["GiaTri"] = "Giá Trị Khuyến Mãi phải lớn hơn 0!";
            if (khuyenMai.NgayBatDau == null)
                errors["NgayBatDau"] = "Phải chọn Ngày Bắt Đầu!";
            if (khuyenMai.NgayKetThuc == null)
                errors["NgayKetThuc"] = "Phải chọn Ngày Kết Thúc!";

            if (khuyenMai.NgayBatDau >= khuyenMai.NgayKetThuc)
                errors["NgayKetThuc"] = "Ngày Kết Thúc phải sau Ngày Bắt Đầu!";

            if (errors.Count > 0)
                return BadRequest(errors);

            // Check trùng Mã Khuyến Mãi
            bool exists = await _context.KhuyenMais.AnyAsync(km =>
                km.MaKM!.Trim().ToLower() == khuyenMai.MaKM!.Trim().ToLower()
            );
            if (exists)
                return Conflict(new { message = "Mã Khuyến Mãi đã tồn tại!" });

            try
            {
                // Chuẩn hóa dữ liệu
                khuyenMai.MaKM = khuyenMai.MaKM.Trim();
                khuyenMai.MoTaKhuyenMai = khuyenMai.MoTaKhuyenMai?.Trim();
                khuyenMai.ApDungVoi ??= "Tất cả";
                khuyenMai.TrangThaiKM ??= "Sắp diễn ra";

                _context.KhuyenMais.Add(khuyenMai);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Thêm Khuyến Mãi thành công!" });
            }
            catch (Exception ex)
            {
                // Log exception (ex)
                return StatusCode(500, new { message = "Lỗi khi thêm Khuyến Mãi. Vui lòng thử lại!" });
            }
        }

        // --- Edit: Cập nhật Khuyến Mãi ---
        [HttpPost]
        [Route("KhuyenMai/Edit/{id}")]
        [Consumes("application/json")]
        public async Task<IActionResult> Edit(int id, [FromBody] KhuyenMai? khuyenMai)
        {
            if (khuyenMai == null)
                return BadRequest(new { message = "Dữ liệu không hợp lệ!" });

            var errors = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(khuyenMai.MaKM))
                errors["MaKM"] = "Phải nhập Mã Khuyến Mãi!";
            if (string.IsNullOrWhiteSpace(khuyenMai.LoaiGiam))
                errors["LoaiGiam"] = "Phải chọn Loại Giảm!";
            if (khuyenMai.GiaTri == null || khuyenMai.GiaTri <= 0)
                errors["GiaTri"] = "Giá Trị Khuyến Mãi phải lớn hơn 0!";
            if (khuyenMai.NgayBatDau == null)
                errors["NgayBatDau"] = "Phải chọn Ngày Bắt Đầu!";
            if (khuyenMai.NgayKetThuc == null)
                errors["NgayKetThuc"] = "Phải chọn Ngày Kết Thúc!";

            if (khuyenMai.NgayBatDau >= khuyenMai.NgayKetThuc)
                errors["NgayKetThuc"] = "Ngày Kết Thúc phải sau Ngày Bắt Đầu!";

            if (errors.Count > 0)
                return BadRequest(errors);

            var existing = await _context.KhuyenMais.FindAsync(id);
            if (existing == null)
                return NotFound(new { message = "Không tìm thấy Khuyến Mãi!" });

            // Check trùng Mã Khuyến Mãi (ngoại trừ chính nó)
            bool exists = await _context.KhuyenMais.AnyAsync(km =>
                km.MaKM!.Trim().ToLower() == khuyenMai.MaKM!.Trim().ToLower() &&
                km.IdKhuyenMai != id
            );
            if (exists)
                return Conflict(new { message = "Mã Khuyến Mãi đã tồn tại cho chương trình khác!" });

            try
            {
                // Cập nhật thông tin
                existing.MaKM = khuyenMai.MaKM.Trim();
                existing.MoTaKhuyenMai = khuyenMai.MoTaKhuyenMai?.Trim();
                existing.LoaiGiam = khuyenMai.LoaiGiam;
                existing.ApDungVoi = khuyenMai.ApDungVoi;
                existing.GiaTri = khuyenMai.GiaTri;
                existing.NgayBatDau = khuyenMai.NgayBatDau;
                existing.NgayKetThuc = khuyenMai.NgayKetThuc;
                existing.TrangThaiKM = khuyenMai.TrangThaiKM?.Trim();

                _context.KhuyenMais.Update(existing);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Cập nhật Khuyến Mãi thành công!" });
            }
            catch
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật Khuyến Mãi. Vui lòng thử lại!" });
            }
        }

        // --- Xóa Khuyến Mãi ---
        [HttpPost]
        [Route("KhuyenMai/Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _context.KhuyenMais.FindAsync(id);
            if (existing == null)
                return NotFound(new { message = "Không tìm thấy Khuyến Mãi để xóa!" });

            try
            {
                _context.KhuyenMais.Remove(existing);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Xóa Khuyến Mãi thành công!" });
            }
            catch
            {
                return StatusCode(500, new { message = "Lỗi khi xóa Khuyến Mãi. Có thể do Khuyến Mãi này đang được áp dụng trong đơn hàng." });
            }
        }
    }
}