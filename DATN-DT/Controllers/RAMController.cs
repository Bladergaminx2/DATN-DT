using DATN_DT.Data;
using DATN_DT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DATN_DT.Controllers
{
    public class RAMController : Controller
    {
        private readonly MyDbContext _context;

        public RAMController(MyDbContext context)
        {
            _context = context;
        }

        // index
        public async Task<IActionResult> Index()
        {
            var rams = await _context.RAMs.ToListAsync();
            return View(rams);
        }

        // create
        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> Create([FromBody] RAM? ram)
        {
            var errors = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(ram?.DungLuongRAM))
                errors["DungLuongRAM"] = "Phải điền dung lượng của RAM!";

            if (errors.Count > 0)
                return BadRequest(errors);

            bool exists = await _context.RAMs.AnyAsync(r => r.DungLuongRAM!.Trim().ToLower() == ram!.DungLuongRAM!.Trim().ToLower());
            if (exists)
                return Conflict(new { message = "Dung lượng RAM đã tồn tại!" });

            try
            {
                ram.DungLuongRAM = ram.DungLuongRAM.Trim();
                ram.MoTaRAM = ram.MoTaRAM?.Trim();

                _context.RAMs.Add(ram);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Thêm RAM thành công!" });
            }
            catch
            {
                return StatusCode(500, new { message = "Lỗi khi thêm RAM. Vui lòng thử lại!" });
            }
        }

        // edit
        [HttpPost]
        [Route("RAM/Edit/{id}")]
        [Consumes("application/json")]
        public async Task<IActionResult> Edit(int id, [FromBody] RAM? ram)
        {
            var errors = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(ram?.DungLuongRAM))
                errors["DungLuongRAM"] = "Phải điền dung lượng của RAM!";

            if (errors.Count > 0)
                return BadRequest(errors);

            var existingRAM = await _context.RAMs.FindAsync(id);
            if (existingRAM == null)
                return NotFound(new { message = "Không tìm thấy RAM!" });

            bool exists = await _context.RAMs.AnyAsync(r => r.DungLuongRAM!.Trim().ToLower() == ram!.DungLuongRAM!.Trim().ToLower() && r.IdRAM != id);
            if (exists)
                return Conflict(new { message = "Dung lượng RAM đã tồn tại!" });

            try
            {
                existingRAM.DungLuongRAM = ram.DungLuongRAM.Trim();
                existingRAM.MoTaRAM = ram.MoTaRAM?.Trim();

                _context.RAMs.Update(existingRAM);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Cập nhật RAM thành công!" });
            }
            catch
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật RAM. Vui lòng thử lại!" });
            }
        }
    }
}
