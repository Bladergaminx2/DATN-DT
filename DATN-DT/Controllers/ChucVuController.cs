
using DATN_DT.CustomAttribute;
using DATN_DT.Data;
using DATN_DT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DATN_DT.Controllers
{
    [Route("ChucVu")]
    [AuthorizeRoleFromToken("ADMIN")] // Chỉ ADMIN mới được quản lý chức vụ
    public class ChucVuController : Controller
    {
        private readonly MyDbContext _context;

        public ChucVuController(MyDbContext context)
        {
            _context = context;
        }

        // --- GET ALL ---
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var chucVus = await _context.ChucVus
                .OrderBy(cv => cv.TenChucVu)
                .ToListAsync();
            return View(chucVus);
        }

        // --- GET ALL (API) ---
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var chucVus = await _context.ChucVus
                .OrderBy(cv => cv.TenChucVu)
                .ToListAsync();
            return Ok(chucVus);
        }

        // --- CREATE ---
        [HttpPost("Create")]
        [Consumes("application/json")]
        public async Task<IActionResult> Create([FromBody] ChucVu? chucVu)
        {
            if (chucVu == null)
                return BadRequest(new { message = "Dữ liệu không hợp lệ!" });

            // Validation
            if (string.IsNullOrWhiteSpace(chucVu.TenChucVu))
                return BadRequest(new { message = "Tên chức vụ không được để trống!" });

            // Tự động tạo TenChucVuVietHoa nếu chưa có
            if (string.IsNullOrWhiteSpace(chucVu.TenChucVuVietHoa))
            {
                chucVu.TenChucVuVietHoa = chucVu.TenChucVu.ToUpper().Replace(" ", "");
            }

            // Kiểm tra trùng tên chức vụ
            var existing = await _context.ChucVus
                .FirstOrDefaultAsync(cv => cv.TenChucVu == chucVu.TenChucVu || 
                                          cv.TenChucVuVietHoa == chucVu.TenChucVuVietHoa);
            if (existing != null)
                return BadRequest(new { message = "Chức vụ đã tồn tại!" });

            try
            {
                _context.ChucVus.Add(chucVu);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Thêm chức vụ thành công!", data = chucVu });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        // --- UPDATE ---
        [HttpPost("Update/{id}")]
        [Consumes("application/json")]
        public async Task<IActionResult> Update(int id, [FromBody] ChucVu? chucVu)
        {
            if (chucVu == null)
                return BadRequest(new { message = "Dữ liệu không hợp lệ!" });

            var existing = await _context.ChucVus.FindAsync(id);
            if (existing == null)
                return NotFound(new { message = "Không tìm thấy chức vụ!" });

            // Không cho phép sửa ADMIN
            if (existing.TenChucVuVietHoa == "ADMIN" || existing.TenChucVu == "Admin")
                return BadRequest(new { message = "Không thể sửa chức vụ Admin!" });

            // Validation
            if (string.IsNullOrWhiteSpace(chucVu.TenChucVu))
                return BadRequest(new { message = "Tên chức vụ không được để trống!" });

            // Tự động tạo TenChucVuVietHoa nếu chưa có
            if (string.IsNullOrWhiteSpace(chucVu.TenChucVuVietHoa))
            {
                chucVu.TenChucVuVietHoa = chucVu.TenChucVu.ToUpper().Replace(" ", "");
            }

            // Kiểm tra trùng tên chức vụ (trừ chính nó)
            var duplicate = await _context.ChucVus
                .FirstOrDefaultAsync(cv => cv.IdChucVu != id && 
                                          (cv.TenChucVu == chucVu.TenChucVu || 
                                           cv.TenChucVuVietHoa == chucVu.TenChucVuVietHoa));
            if (duplicate != null)
                return BadRequest(new { message = "Chức vụ đã tồn tại!" });

            try
            {
                existing.TenChucVu = chucVu.TenChucVu;
                existing.TenChucVuVietHoa = chucVu.TenChucVuVietHoa;

                _context.ChucVus.Update(existing);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Cập nhật chức vụ thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        // --- DELETE ---
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var chucVu = await _context.ChucVus.FindAsync(id);
            if (chucVu == null)
                return NotFound(new { message = "Không tìm thấy chức vụ!" });

            // Không cho phép xóa ADMIN
            if (chucVu.TenChucVuVietHoa == "ADMIN" || chucVu.TenChucVu == "Admin")
                return BadRequest(new { message = "Không thể xóa chức vụ Admin!" });

            // Kiểm tra xem có nhân viên nào đang dùng chức vụ này không
            var hasNhanVien = await _context.NhanViens
                .AnyAsync(nv => nv.IdChucVu == id);
            
            if (hasNhanVien)
                return BadRequest(new { message = "Không thể xóa chức vụ đang được sử dụng bởi nhân viên!" });

            try
            {
                _context.ChucVus.Remove(chucVu);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Xóa chức vụ thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }
    }
}

