using DATN_DT.Data;
using DATN_DT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DATN_DT.Controllers
{
    public class CameraTruocController : Controller
    {
        private readonly MyDbContext _context;

        public CameraTruocController(MyDbContext context)
        {
            _context = context;
        }

        // InDex
        public async Task<IActionResult> Index()
        {
            var list = await _context.CameraTruocs.ToListAsync();
            return View(list);
        }

        // Created
        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> Create([FromBody] CameraTruoc? cam)
        {
            var errors = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(cam?.DoPhanGiaiCamTruoc))
                errors["DoPhanGiaiCamTruoc"] = "Phải nhập độ phân giải camera trước!";
            if (string.IsNullOrWhiteSpace(cam?.TinhNangCamTruoc))
                errors["TinhNangCamTruoc"] = "Phải nhập tính năng camera!";
            if (string.IsNullOrWhiteSpace(cam?.QuayVideoCamTruoc))
                errors["QuayVideoCamTruoc"] = "Phải nhập thông tin quay video!";

            if (errors.Count > 0)
                return BadRequest(errors);

            bool exists = await _context.CameraTruocs.AnyAsync(c =>
                c.DoPhanGiaiCamTruoc!.Trim().ToLower() == cam!.DoPhanGiaiCamTruoc!.Trim().ToLower() &&
                c.TinhNangCamTruoc!.Trim().ToLower() == cam.TinhNangCamTruoc!.Trim().ToLower() &&
                c.QuayVideoCamTruoc!.Trim().ToLower() == cam.QuayVideoCamTruoc!.Trim().ToLower()
            );
            if (exists)
                return Conflict(new { message = "Camera trước đã tồn tại!" });

            try
            {
                cam.DoPhanGiaiCamTruoc = cam.DoPhanGiaiCamTruoc.Trim();
                cam.TinhNangCamTruoc = cam.TinhNangCamTruoc.Trim();
                cam.QuayVideoCamTruoc = cam.QuayVideoCamTruoc.Trim();
                cam.MoTaCamTruoc = cam.MoTaCamTruoc?.Trim();

                _context.CameraTruocs.Add(cam);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Thêm camera trước thành công!" });
            }
            catch
            {
                return StatusCode(500, new { message = "Lỗi khi thêm camera trước. Vui lòng thử lại!" });
            }
        }

        // ediT
        [HttpPost]
        [Route("CameraTruoc/Edit/{id}")]
        [Consumes("application/json")]
        public async Task<IActionResult> Edit(int id, [FromBody] CameraTruoc? cam)
        {
            var errors = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(cam?.DoPhanGiaiCamTruoc))
                errors["DoPhanGiaiCamTruoc"] = "Phải nhập độ phân giải camera trước!";
            if (string.IsNullOrWhiteSpace(cam?.TinhNangCamTruoc))
                errors["TinhNangCamTruoc"] = "Phải nhập tính năng camera!";
            if (string.IsNullOrWhiteSpace(cam?.QuayVideoCamTruoc))
                errors["QuayVideoCamTruoc"] = "Phải nhập thông tin quay video!";

            if (errors.Count > 0)
                return BadRequest(errors);

            var existing = await _context.CameraTruocs.FindAsync(id);
            if (existing == null)
                return NotFound(new { message = "Không tìm thấy camera trước!" });

            bool exists = await _context.CameraTruocs.AnyAsync(c =>
                c.DoPhanGiaiCamTruoc!.Trim().ToLower() == cam!.DoPhanGiaiCamTruoc!.Trim().ToLower() &&
                c.TinhNangCamTruoc!.Trim().ToLower() == cam.TinhNangCamTruoc!.Trim().ToLower() &&
                c.QuayVideoCamTruoc!.Trim().ToLower() == cam.QuayVideoCamTruoc!.Trim().ToLower() &&
                c.IdCamTruoc != id
            );
            if (exists)
                return Conflict(new { message = "Camera trước đã tồn tại!" });

            try
            {
                existing.DoPhanGiaiCamTruoc = cam.DoPhanGiaiCamTruoc.Trim();
                existing.TinhNangCamTruoc = cam.TinhNangCamTruoc.Trim();
                existing.QuayVideoCamTruoc = cam.QuayVideoCamTruoc.Trim();
                existing.MoTaCamTruoc = cam.MoTaCamTruoc?.Trim();

                _context.CameraTruocs.Update(existing);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Cập nhật camera trước thành công!" });
            }
            catch
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật camera trước. Vui lòng thử lại!" });
            }
        }
    }
}
