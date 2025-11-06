using DATN_DT.Data;
using DATN_DT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DATN_DT.Controllers
{
    public class ThuongHieuController : Controller
    {
        private readonly MyDbContext _context;

        public ThuongHieuController(MyDbContext context)
        {
            _context = context;
        }

        // index
        public async Task<IActionResult> Index()
        {
            var list = await _context.ThuongHieus.ToListAsync();
            return View(list);
        }

        //cream ate
        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> Create([FromBody] ThuongHieu? th)
        {
            var errors = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(th?.TenThuongHieu))
                errors["TenThuongHieu"] = "Phải nhập tên thương hiệu!";
            if (string.IsNullOrWhiteSpace(th?.TrangThaiThuongHieu))
                errors["TrangThaiThuongHieu"] = "Phải nhập trạng thái thương hiệu!";

            if (errors.Count > 0)
                return BadRequest(errors);

            // Check trùng
            bool exists = await _context.ThuongHieus.AnyAsync(t =>
                t.TenThuongHieu!.Trim().ToLower() == th!.TenThuongHieu!.Trim().ToLower()
            );
            if (exists)
                return Conflict(new { message = "Thương hiệu đã tồn tại!" });

            try
            {
                th.TenThuongHieu = th.TenThuongHieu.Trim();
                th.TrangThaiThuongHieu = th.TrangThaiThuongHieu.Trim();

                _context.ThuongHieus.Add(th);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Thêm thương hiệu thành công!" });
            }
            catch
            {
                return StatusCode(500, new { message = "Lỗi khi thêm thương hiệu. Vui lòng thử lại!" });
            }
        }

        // Tide
        [HttpPost]
        [Route("ThuongHieu/Edit/{id}")]
        [Consumes("application/json")]
        public async Task<IActionResult> Edit(int id, [FromBody] ThuongHieu? th)
        {
            var errors = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(th?.TenThuongHieu))
                errors["TenThuongHieu"] = "Phải nhập tên thương hiệu!";
            if (string.IsNullOrWhiteSpace(th?.TrangThaiThuongHieu))
                errors["TrangThaiThuongHieu"] = "Phải nhập trạng thái thương hiệu!";

            if (errors.Count > 0)
                return BadRequest(errors);

            var existing = await _context.ThuongHieus.FindAsync(id);
            if (existing == null)
                return NotFound(new { message = "Không tìm thấy thương hiệu!" });

            // Check trùng 
            bool exists = await _context.ThuongHieus.AnyAsync(t =>
                t.TenThuongHieu!.Trim().ToLower() == th!.TenThuongHieu!.Trim().ToLower() &&
                t.IdThuongHieu != id
            );
            if (exists)
                return Conflict(new { message = "Thương hiệu đã tồn tại!" });

            try
            {
                existing.TenThuongHieu = th.TenThuongHieu.Trim();
                existing.TrangThaiThuongHieu = th.TrangThaiThuongHieu.Trim();

                _context.ThuongHieus.Update(existing);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Cập nhật thương hiệu thành công!" });
            }
            catch
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật thương hiệu. Vui lòng thử lại!" });
            }
        }
    }
}
