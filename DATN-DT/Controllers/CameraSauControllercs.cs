using DATN_DT.Data;
using DATN_DT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DATN_DT.Controllers
{
    public class CameraSauController : Controller
    {
        private readonly MyDbContext _context;

        public CameraSauController(MyDbContext context)
        {
            _context = context;
        }

        // Index
        public async Task<IActionResult> Index()
        {
            var list = await _context.CameraSaus.ToListAsync();
            return View(list);
        }

        // Create
        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> Create([FromBody] CameraSau? cam)
        {
            var errors = new Dictionary<string, string>();

            
            if (string.IsNullOrWhiteSpace(cam?.DoPhanGiaiCamSau))
                errors["DoPhanGiaiCamSau"] = "Phải nhập độ phân giải camera sau!";
            if (string.IsNullOrWhiteSpace(cam?.SoLuongOngKinh))
                errors["SoLuongOngKinh"] = "Phải nhập số lượng ống kính!";
            if (string.IsNullOrWhiteSpace(cam?.TinhNangCamSau))
                errors["TinhNangCamSau"] = "Phải nhập tính năng camera!";
            if (string.IsNullOrWhiteSpace(cam?.QuayVideoCamSau))
                errors["QuayVideoCamSau"] = "Phải nhập thông tin quay video!";

            if (errors.Count > 0)
                return BadRequest(errors);

            // Check trùng
            bool exists = await _context.CameraSaus.AnyAsync(c =>
                c.DoPhanGiaiCamSau!.Trim().ToLower() == cam!.DoPhanGiaiCamSau!.Trim().ToLower() &&
                c.SoLuongOngKinh!.Trim().ToLower() == cam.SoLuongOngKinh!.Trim().ToLower() &&
                c.TinhNangCamSau!.Trim().ToLower() == cam.TinhNangCamSau!.Trim().ToLower() &&
                c.QuayVideoCamSau!.Trim().ToLower() == cam.QuayVideoCamSau!.Trim().ToLower()
            );
            if (exists)
                return Conflict(new { message = "Camera sau đã tồn tại!" });

            
            try
            {
                cam.DoPhanGiaiCamSau = cam.DoPhanGiaiCamSau.Trim();
                cam.SoLuongOngKinh = cam.SoLuongOngKinh.Trim();
                cam.TinhNangCamSau = cam.TinhNangCamSau.Trim();
                cam.QuayVideoCamSau = cam.QuayVideoCamSau.Trim();
                cam.MoTaCamSau = cam.MoTaCamSau?.Trim();

                _context.CameraSaus.Add(cam);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Thêm camera sau thành công!" });
            }
            catch
            {
                return StatusCode(500, new { message = "Lỗi khi thêm camera sau. Vui lòng thử lại!" });
            }
        }

        // Edit
        [HttpPost]
        [Route("CameraSau/Edit/{id}")]
        [Consumes("application/json")]
        public async Task<IActionResult> Edit(int id, [FromBody] CameraSau? cam)
        {
            var errors = new Dictionary<string, string>();

            
            if (string.IsNullOrWhiteSpace(cam?.DoPhanGiaiCamSau))
                errors["DoPhanGiaiCamSau"] = "Phải nhập độ phân giải camera sau!";
            if (string.IsNullOrWhiteSpace(cam?.SoLuongOngKinh))
                errors["SoLuongOngKinh"] = "Phải nhập số lượng ống kính!";
            if (string.IsNullOrWhiteSpace(cam?.TinhNangCamSau))
                errors["TinhNangCamSau"] = "Phải nhập tính năng camera!";
            if (string.IsNullOrWhiteSpace(cam?.QuayVideoCamSau))
                errors["QuayVideoCamSau"] = "Phải nhập thông tin quay video!";

            if (errors.Count > 0)
                return BadRequest(errors);

            // Tìm CameraSau hiện tại
            var existing = await _context.CameraSaus.FindAsync(id);
            if (existing == null)
                return NotFound(new { message = "Không tìm thấy camera sau!" });

            // Check trùng 
            bool exists = await _context.CameraSaus.AnyAsync(c =>
                c.DoPhanGiaiCamSau!.Trim().ToLower() == cam!.DoPhanGiaiCamSau!.Trim().ToLower() &&
                c.SoLuongOngKinh!.Trim().ToLower() == cam.SoLuongOngKinh!.Trim().ToLower() &&
                c.TinhNangCamSau!.Trim().ToLower() == cam.TinhNangCamSau!.Trim().ToLower() &&
                c.QuayVideoCamSau!.Trim().ToLower() == cam.QuayVideoCamSau!.Trim().ToLower() &&
                c.IdCameraSau != id
            );
            if (exists)
                return Conflict(new { message = "Camera sau đã tồn tại!" });

            
            try
            {
                existing.DoPhanGiaiCamSau = cam.DoPhanGiaiCamSau.Trim();
                existing.SoLuongOngKinh = cam.SoLuongOngKinh.Trim();
                existing.TinhNangCamSau = cam.TinhNangCamSau.Trim();
                existing.QuayVideoCamSau = cam.QuayVideoCamSau.Trim();
                existing.MoTaCamSau = cam.MoTaCamSau?.Trim();

                _context.CameraSaus.Update(existing);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Cập nhật camera sau thành công!" });
            }
            catch
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật camera sau. Vui lòng thử lại!" });
            }
        }
    }
}
