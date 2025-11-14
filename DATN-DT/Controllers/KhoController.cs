using DATN_DT.Data;
using DATN_DT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DATN_DT.Controllers
{
    public class KhoController : Controller
    {
        private readonly MyDbContext _context;

        public KhoController(MyDbContext context)
        {
            _context = context;
        }

     
        public async Task<IActionResult> Index()
        {
            var list = await _context.Khos.ToListAsync();
            return View(list);
        }

      
        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> Create([FromBody] Kho? kho)
        {
            var errors = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(kho?.TenKho))
                errors["TenKho"] = "Phải nhập tên kho!";
            if (string.IsNullOrWhiteSpace(kho?.DiaChiKho))
                errors["DiaChiKho"] = "Phải nhập địa chỉ kho!";

            if (errors.Count > 0)
                return BadRequest(errors);

            // Check trùng tên kho
            bool exists = await _context.Khos.AnyAsync(k =>
                k.TenKho!.Trim().ToLower() == kho!.TenKho!.Trim().ToLower()
            );
            if (exists)
                return Conflict(new { message = "Kho đã tồn tại!" });

            try
            {
                kho.TenKho = kho.TenKho.Trim();
                kho.DiaChiKho = kho.DiaChiKho.Trim();

                _context.Khos.Add(kho);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Thêm kho thành công!" });
            }
            catch
            {
                return StatusCode(500, new { message = "Lỗi khi thêm kho. Vui lòng thử lại!" });
            }
        }

      
        [HttpPost]
        [Route("Kho/Edit/{id}")]
        [Consumes("application/json")]
        public async Task<IActionResult> Edit(int id, [FromBody] Kho? kho)
        {
            var errors = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(kho?.TenKho))
                errors["TenKho"] = "Phải nhập tên kho!";
            if (string.IsNullOrWhiteSpace(kho?.DiaChiKho))
                errors["DiaChiKho"] = "Phải nhập địa chỉ kho!";

            if (errors.Count > 0)
                return BadRequest(errors);

            var existing = await _context.Khos.FindAsync(id);
            if (existing == null)
                return NotFound(new { message = "Không tìm thấy kho!" });

            // Check trùng ngoại trừ chính nó
            bool exists = await _context.Khos.AnyAsync(k =>
                k.TenKho!.Trim().ToLower() == kho!.TenKho!.Trim().ToLower() &&
                k.IdKho != id
            );
            if (exists)
                return Conflict(new { message = "Kho đã tồn tại!" });

            try
            {
                existing.TenKho = kho.TenKho.Trim();
                existing.DiaChiKho = kho.DiaChiKho.Trim();

                _context.Khos.Update(existing);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Cập nhật kho thành công!" });
            }
            catch
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật kho. Vui lòng thử lại!" });
            }
        }
    }
}