using DATN_DT.Data;
using DATN_DT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace DATN_DT.Controllers
{
    public class PinController : Controller
    {
        private readonly MyDbContext _context;

        public PinController(MyDbContext context)
        {
            _context = context;
        }

        // Pindex
        public async Task<IActionResult> Index()
        {
            var pins = await _context.Pins.ToListAsync();
            return View(pins);
        }

        // Creete
        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> Create([FromBody] Pin? pin)
        {
            var errors = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(pin?.LoaiPin))
                errors["LoaiPin"] = "Phải nhập loại pin!";
            if (string.IsNullOrWhiteSpace(pin?.DungLuongPin))
                errors["DungLuongPin"] = "Phải nhập dung lượng pin!";
            if (string.IsNullOrWhiteSpace(pin?.CongNgheSac))
                errors["CongNgheSac"] = "Phải nhập công nghệ sạc!";

            if (errors.Count > 0)
                return BadRequest(errors);

            // Validate dung lượng pin chỉ chứa số
            if (!string.IsNullOrWhiteSpace(pin?.DungLuongPin))
            {
                // Kiểm tra nếu chỉ chứa số
                if (!Regex.IsMatch(pin.DungLuongPin.Trim(), @"^\d+$"))
                {
                    errors["DungLuongPin"] = "Dung lượng pin chỉ được nhập số!";
                    return BadRequest(errors);
                }

                // Tự động thêm "mAh" vào sau số
                pin.DungLuongPin = pin.DungLuongPin.Trim() + "mAh";
            }

            // Check trùng
            bool exists = await _context.Pins.AnyAsync(p =>
                p.LoaiPin!.Trim().ToLower() == pin!.LoaiPin!.Trim().ToLower() &&
                p.DungLuongPin!.Trim().ToLower() == pin.DungLuongPin!.Trim().ToLower() &&
                p.CongNgheSac!.Trim().ToLower() == pin.CongNgheSac!.Trim().ToLower()
            );
            if (exists)
                return Conflict(new { message = "Pin đã tồn tại!" });

            try
            {
                pin.LoaiPin = pin.LoaiPin.Trim();
                pin.DungLuongPin = pin.DungLuongPin.Trim();
                pin.CongNgheSac = pin.CongNgheSac.Trim();
                pin.MoTaPin = pin.MoTaPin?.Trim();

                _context.Pins.Add(pin);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Thêm pin thành công!" });
            }
            catch
            {
                return StatusCode(500, new { message = "Lỗi khi thêm pin. Vui lòng thử lại!" });
            }
        }

        // edit
        [HttpPost]
        [Route("Pin/Edit/{id}")]
        [Consumes("application/json")]
        public async Task<IActionResult> Edit(int id, [FromBody] Pin? pin)
        {
            var errors = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(pin?.LoaiPin))
                errors["LoaiPin"] = "Phải nhập loại pin!";
            if (string.IsNullOrWhiteSpace(pin?.DungLuongPin))
                errors["DungLuongPin"] = "Phải nhập dung lượng pin!";
            if (string.IsNullOrWhiteSpace(pin?.CongNgheSac))
                errors["CongNgheSac"] = "Phải nhập công nghệ sạc!";

            if (errors.Count > 0)
                return BadRequest(errors);

            // Validate dung lượng pin chỉ chứa số
            if (!string.IsNullOrWhiteSpace(pin?.DungLuongPin))
            {
                // Kiểm tra nếu chỉ chứa số
                if (!Regex.IsMatch(pin.DungLuongPin.Trim(), @"^\d+$"))
                {
                    errors["DungLuongPin"] = "Dung lượng pin chỉ được nhập số!";
                    return BadRequest(errors);
                }

                // Tự động thêm "mAh" vào sau số
                pin.DungLuongPin = pin.DungLuongPin.Trim() + "mAh";
            }

            var existing = await _context.Pins.FindAsync(id);
            if (existing == null)
                return NotFound(new { message = "Không tìm thấy pin!" });

            // Check trùng 
            bool exists = await _context.Pins.AnyAsync(p =>
                p.LoaiPin!.Trim().ToLower() == pin!.LoaiPin!.Trim().ToLower() &&
                p.DungLuongPin!.Trim().ToLower() == pin.DungLuongPin!.Trim().ToLower() &&
                p.CongNgheSac!.Trim().ToLower() == pin.CongNgheSac!.Trim().ToLower() &&
                p.IdPin != id
            );
            if (exists)
                return Conflict(new { message = "Pin đã tồn tại!" });

            try
            {
                existing.LoaiPin = pin.LoaiPin.Trim();
                existing.DungLuongPin = pin.DungLuongPin.Trim();
                existing.CongNgheSac = pin.CongNgheSac.Trim();
                existing.MoTaPin = pin.MoTaPin?.Trim();

                _context.Pins.Update(existing);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Cập nhật pin thành công!" });
            }
            catch
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật pin. Vui lòng thử lại!" });
            }
        }
    }
}