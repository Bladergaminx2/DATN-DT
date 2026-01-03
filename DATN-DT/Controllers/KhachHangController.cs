using DATN_DT.Data;
using DATN_DT.Form;
using DATN_DT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DATN_DT.Controllers
{
    public class KhachHangController : Controller
    {
        private readonly MyDbContext _context;

        public KhachHangController(MyDbContext context)
        {
            _context = context;
        }

        // --- Index: Lấy danh sách Khách hàng ---
        public async Task<IActionResult> Index()
        {
            var khachHangs = await _context.KhachHangs.ToListAsync();
            return View(khachHangs);
        }

        // --- Create: Thêm Khách hàng mới ---
        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> Create([FromBody] KhachHangAdmninForm? khachHang)
        {
            if (khachHang == null)
                return BadRequest(new { message = "Dữ liệu khách hàng không hợp lệ!" });

            var errors = new Dictionary<string, string>();

            // Validation cơ bản cho KhachHang
            if (string.IsNullOrWhiteSpace(khachHang.HoTenKhachHang))
                errors["HoTenKhachHang"] = "Phải nhập họ tên khách hàng!";

            // Validation cho SdtKhachHang (số điện thoại)
            if (string.IsNullOrWhiteSpace(khachHang.SdtKhachHang))
                errors["SdtKhachHang"] = "Phải nhập số điện thoại!";

            // Validation cho EmailKhachHang 
            if (string.IsNullOrWhiteSpace(khachHang.EmailKhachHang))
                errors["EmailKhachHang"] = "Phải nhập Email!";

            if (errors.Count > 0)
                return BadRequest(errors);

            // Kiểm tra trùng Số điện thoại hoặc Email (giả định Sdt là duy nhất)
            bool sdtExists = await _context.KhachHangs.AnyAsync(kh =>
                kh.SdtKhachHang != null && kh.SdtKhachHang.Trim().ToLower() == khachHang.SdtKhachHang!.Trim().ToLower()
            );
            if (sdtExists)
                return Conflict(new { message = "Số điện thoại này đã được đăng ký!" });

            // Kiểm tra trùng Email (nếu Email là duy nhất)
            if (!string.IsNullOrWhiteSpace(khachHang.EmailKhachHang))
            {
                bool emailExists = await _context.KhachHangs.AnyAsync(kh =>
                    kh.EmailKhachHang != null && kh.EmailKhachHang.Trim().ToLower() == khachHang.EmailKhachHang.Trim().ToLower()
                );
                if (emailExists)
                    return Conflict(new { message = "Email này đã được sử dụng!" });
            }


            try
            {
                var data = new KhachHang()
                {
                    HoTenKhachHang = khachHang.HoTenKhachHang!.Trim(),
                    SdtKhachHang = khachHang.SdtKhachHang!.Trim(),
                    EmailKhachHang = khachHang.EmailKhachHang?.Trim(),
                    DiemTichLuy = 0,
                    // Sử dụng ảnh mặc định từ thư mục www
                    DefaultImage = "/images/hARUdummiepfpcyr.png", // Đường dẫn ảnh mặc định
                    TrangThaiKhachHang = 1,
                };

                _context.KhachHangs.Add(data);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Thêm Khách hàng thành công!" });
            }
            catch
            {
                return StatusCode(500, new { message = "Lỗi khi thêm Khách hàng. Vui lòng thử lại!" });
            }
        }

        // --- Edit: Cập nhật thông tin Khách hàng ---
        [HttpPost]
        [Route("KhachHang/Edit/{id}")]
        [Consumes("application/json")]
        public async Task<IActionResult> Edit(int id, [FromBody] KhachHangAdmninForm? khachHang)
        {
            if (khachHang == null)
                return BadRequest(new { message = "Dữ liệu khách hàng không hợp lệ!" });

            var errors = new Dictionary<string, string>();

            // Validation cơ bản
            if (string.IsNullOrWhiteSpace(khachHang.HoTenKhachHang))
                errors["HoTenKhachHang"] = "Phải nhập họ tên khách hàng!";
            if (string.IsNullOrWhiteSpace(khachHang.SdtKhachHang))
                errors["SdtKhachHang"] = "Phải nhập số điện thoại!";

            if (errors.Count > 0)
                return BadRequest(errors);

            var existing = await _context.KhachHangs.FindAsync(id);
            if (existing == null)
                return NotFound(new { message = "Không tìm thấy Khách hàng!" });

            // Kiểm tra trùng Số điện thoại ngoại trừ chính nó
            bool sdtExists = await _context.KhachHangs.AnyAsync(kh =>
                kh.SdtKhachHang != null && kh.SdtKhachHang.Trim().ToLower() == khachHang.SdtKhachHang!.Trim().ToLower() &&
                kh.IdKhachHang != id
            );
            if (sdtExists)
                return Conflict(new { message = "Số điện thoại này đã được đăng ký cho khách hàng khác!" });

            // Kiểm tra trùng Email ngoại trừ chính nó
            if (!string.IsNullOrWhiteSpace(khachHang.EmailKhachHang))
            {
                bool emailExists = await _context.KhachHangs.AnyAsync(kh =>
                    kh.EmailKhachHang != null && kh.EmailKhachHang.Trim().ToLower() == khachHang.EmailKhachHang.Trim().ToLower() &&
                    kh.IdKhachHang != id
                );
                if (emailExists)
                    return Conflict(new { message = "Email này đã được sử dụng bởi khách hàng khác!" });
            }

            try
            {
                // Cập nhật thông tin (không thay đổi ảnh đại diện)
                existing.HoTenKhachHang = khachHang.HoTenKhachHang!.Trim();
                existing.SdtKhachHang = khachHang.SdtKhachHang!.Trim();
                existing.EmailKhachHang = khachHang.EmailKhachHang?.Trim();
                existing.TrangThaiKhachHang = khachHang.TrangThaiKhachHang;
                // Giữ nguyên DefaultImage hiện tại, không cập nhật

                _context.KhachHangs.Update(existing);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Cập nhật Khách hàng thành công!" });
            }
            catch
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật Khách hàng. Vui lòng thử lại!" });
            }
        }

        // --- Delete: Xóa Khách hàng (Kiểm tra ràng buộc trước khi xóa) ---
        [HttpPost]
        [Route("KhachHang/Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _context.KhachHangs
                .Include(kh => kh.Diachi)
                .FirstOrDefaultAsync(kh => kh.IdKhachHang == id);
            
            if (existing == null)
                return NotFound(new { message = "Không tìm thấy Khách hàng để xóa!" });

            // Kiểm tra ràng buộc với HoaDon
            var hasHoaDon = await _context.HoaDons.AnyAsync(h => h.IdKhachHang == id);
            if (hasHoaDon)
            {
                var hoaDonCount = await _context.HoaDons.CountAsync(h => h.IdKhachHang == id);
                return BadRequest(new { 
                    message = $"Không thể xóa khách hàng này! Khách hàng đã có {hoaDonCount} hóa đơn liên quan. Vui lòng xử lý các hóa đơn trước khi xóa.",
                    hasConstraints = true,
                    constraintType = "HoaDon",
                    count = hoaDonCount
                });
            }

            // Kiểm tra ràng buộc với DonHang
            var hasDonHang = await _context.DonHangs.AnyAsync(d => d.IdKhachHang == id);
            if (hasDonHang)
            {
                var donHangCount = await _context.DonHangs.CountAsync(d => d.IdKhachHang == id);
                return BadRequest(new { 
                    message = $"Không thể xóa khách hàng này! Khách hàng đã có {donHangCount} đơn hàng liên quan. Vui lòng xử lý các đơn hàng trước khi xóa.",
                    hasConstraints = true,
                    constraintType = "DonHang",
                    count = donHangCount
                });
            }

            // Kiểm tra ràng buộc với GioHang
            var hasGioHang = await _context.GioHangs.AnyAsync(g => g.IdKhachHang == id);
            if (hasGioHang)
            {
                var gioHangCount = await _context.GioHangs.CountAsync(g => g.IdKhachHang == id);
                return BadRequest(new { 
                    message = $"Không thể xóa khách hàng này! Khách hàng đã có {gioHangCount} giỏ hàng liên quan.",
                    hasConstraints = true,
                    constraintType = "GioHang",
                    count = gioHangCount
                });
            }

            // Kiểm tra ràng buộc với BaoHanh
            var hasBaoHanh = await _context.BaoHanhs.AnyAsync(b => b.IdKhachHang == id);
            if (hasBaoHanh)
            {
                var baoHanhCount = await _context.BaoHanhs.CountAsync(b => b.IdKhachHang == id);
                return BadRequest(new { 
                    message = $"Không thể xóa khách hàng này! Khách hàng đã có {baoHanhCount} bảo hành liên quan. Vui lòng xử lý các bảo hành trước khi xóa.",
                    hasConstraints = true,
                    constraintType = "BaoHanh",
                    count = baoHanhCount
                });
            }

            try
            {
                // Xóa các địa chỉ liên quan (Cascade delete)
                if (existing.Diachi != null && existing.Diachi.Any())
                {
                    _context.diachis.RemoveRange(existing.Diachi);
                }

                // Xóa khách hàng
                _context.KhachHangs.Remove(existing);
                await _context.SaveChangesAsync();
                
                return Ok(new { message = "Xóa Khách hàng thành công!" });
            }
            catch (DbUpdateException ex)
            {
                // Xử lý lỗi ràng buộc từ database
                return StatusCode(500, new { 
                    message = "Lỗi khi xóa Khách hàng. Có thể do Khách hàng này đã phát sinh giao dịch hoặc có ràng buộc dữ liệu.",
                    error = ex.InnerException?.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    message = "Lỗi hệ thống khi xóa Khách hàng. Vui lòng thử lại!",
                    error = ex.Message
                });
            }
        }
    }
}