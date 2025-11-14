using DATN_DT.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DATN_DT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KhachHangController : Controller
    {
        private readonly MyDbContext _context;

        public KhachHangController(MyDbContext context)
        {
            _context = context;
        }

        // --- Index: Lấy danh sách Khách hàng ---
        [Authorize(Roles = "KhachHang")]
        [HttpGet("profile")]
        public async Task<IActionResult> Index()
        {
            var khachHangs = await _context.KhachHangs.ToListAsync();
            return View(khachHangs);
        }

        // --- Create: Thêm Khách hàng mới ---
        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> Create([FromBody] KhachHang? khachHang)
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
                // Chuẩn hóa dữ liệu trước khi lưu
                khachHang.HoTenKhachHang = khachHang.HoTenKhachHang!.Trim();
                khachHang.SdtKhachHang = khachHang.SdtKhachHang!.Trim();
                khachHang.EmailKhachHang = khachHang.EmailKhachHang?.Trim();
                khachHang.DiaChiKhachHang = khachHang.DiaChiKhachHang?.Trim();
                khachHang.DiemTichLuy ??= 0;
                khachHang.TrangThaiKhachHang ??= 1;

                _context.KhachHangs.Add(khachHang);
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
        public async Task<IActionResult> Edit(int id, [FromBody] KhachHang? khachHang)
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
                // Cập nhật thông tin
                existing.HoTenKhachHang = khachHang.HoTenKhachHang!.Trim();
                existing.SdtKhachHang = khachHang.SdtKhachHang!.Trim();
                existing.EmailKhachHang = khachHang.EmailKhachHang?.Trim();
                existing.DiaChiKhachHang = khachHang.DiaChiKhachHang?.Trim();
                existing.DiemTichLuy = khachHang.DiemTichLuy;
                existing.TrangThaiKhachHang = khachHang.TrangThaiKhachHang;
                _context.KhachHangs.Update(existing);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Cập nhật Khách hàng thành công!" });
            }
            catch
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật Khách hàng. Vui lòng thử lại!" });
            }
        }

        // --- Delete: Xóa Khách hàng (Thao tác nguy hiểm, cân nhắc thay bằng thay đổi trạng thái) ---
        [HttpPost]
        [Route("KhachHang/Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _context.KhachHangs.FindAsync(id);
            if (existing == null)
                return NotFound(new { message = "Không tìm thấy Khách hàng để xóa!" });

            try
            {
                _context.KhachHangs.Remove(existing);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Xóa Khách hàng thành công!" });
            }
            catch
            {
                // Xử lý lỗi nếu khách hàng có ràng buộc khóa ngoại (FK) với các bảng khác
                return StatusCode(500, new { message = "Lỗi khi xóa Khách hàng. Có thể do Khách hàng này đã phát sinh giao dịch." });
            }
        }
    }
}