using DATN_DT.Data;
using DATN_DT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DATN_DT.Controllers
{
    public class ROMController : Controller
    {
        private readonly MyDbContext _context;

        public ROMController(MyDbContext context)
        {
            _context = context;
        }

        // Index
        public async Task<IActionResult> Index()
        {
            var roms = await _context.ROMs.ToListAsync();
            return View(roms);
        }

        // Create
        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> Create([FromBody] ROM? rom)
        {
            var errors = new Dictionary<string, string>();

      
            if (string.IsNullOrWhiteSpace(rom?.DungLuongROM))
                errors["DungLuongROM"] = "Phải điền dung lượng của ROM!";

            if (errors.Count > 0)
                return BadRequest(errors);

            // Check trùng 
            bool exists = await _context.ROMs.AnyAsync(r => r.DungLuongROM!.Trim().ToLower() == rom!.DungLuongROM!.Trim().ToLower());
            if (exists)
                return Conflict(new { message = "Dung lượng ROM đã tồn tại!" });

 
            try
            {
                rom.DungLuongROM = rom.DungLuongROM.Trim();
                rom.MoTaROM = rom.MoTaROM?.Trim();

                _context.ROMs.Add(rom);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Thêm ROM thành công!" });
            }
            catch
            {
                return StatusCode(500, new { message = "Lỗi khi thêm ROM. Vui lòng thử lại!" });
            }
        }

        // edit
        [HttpPost]
        [Route("ROM/Edit/{id}")]
        [Consumes("application/json")]
        public async Task<IActionResult> Edit(int id, [FromBody] ROM? rom)
        {
            var errors = new Dictionary<string, string>();

       
            if (string.IsNullOrWhiteSpace(rom?.DungLuongROM))
                errors["DungLuongROM"] = "Phải điền dung lượng của ROM!";

            if (errors.Count > 0)
                return BadRequest(errors);

  
            var existingROM = await _context.ROMs.FindAsync(id);
            if (existingROM == null)
                return NotFound(new { message = "Không tìm thấy ROM!" });

            // Check trùng dung lượng 
            bool exists = await _context.ROMs.AnyAsync(r => r.DungLuongROM!.Trim().ToLower() == rom!.DungLuongROM!.Trim().ToLower() && r.IdROM != id);
            if (exists)
                return Conflict(new { message = "Dung lượng ROM đã tồn tại!" });

      
            try
            {
                existingROM.DungLuongROM = rom.DungLuongROM.Trim();
                existingROM.MoTaROM = rom.MoTaROM?.Trim();

                _context.ROMs.Update(existingROM);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Cập nhật ROM thành công!" });
            }
            catch
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật ROM. Vui lòng thử lại!" });
            }
        }
    }
}
