using DATN_DT.Data;
using DATN_DT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DATN_DT.Controllers
{
    public class ManHinhController : Controller
    {
        private readonly MyDbContext _context;

        public ManHinhController(MyDbContext context)
        {
            _context = context;
        }

        // a certain
        public async Task<IActionResult> Index()
        {
            var list = await _context.ManHinhs.ToListAsync();
            return View(list);
        }

        // cree ate
        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> Create([FromBody] ManHinh? mh)
        {
            var errors = new Dictionary<string, string>();

            
            if (string.IsNullOrWhiteSpace(mh?.CongNgheManHinh))
                errors["CongNgheManHinh"] = "Phải nhập công nghệ màn hình!";
            if (string.IsNullOrWhiteSpace(mh?.KichThuoc))
                errors["KichThuoc"] = "Phải nhập kích thước màn hình!";
            if (string.IsNullOrWhiteSpace(mh?.DoPhanGiai))
                errors["DoPhanGiai"] = "Phải nhập độ phân giải!";
            if (string.IsNullOrWhiteSpace(mh?.TinhNangMan))
                errors["TinhNangMan"] = "Phải nhập tính năng màn hình!";

            if (errors.Count > 0)
                return BadRequest(errors);

            // Check trùng
            bool exists = await _context.ManHinhs.AnyAsync(m =>
                m.CongNgheManHinh!.Trim().ToLower() == mh!.CongNgheManHinh!.Trim().ToLower() &&
                m.KichThuoc!.Trim().ToLower() == mh.KichThuoc!.Trim().ToLower() &&
                m.DoPhanGiai!.Trim().ToLower() == mh.DoPhanGiai!.Trim().ToLower() &&
                m.TinhNangMan!.Trim().ToLower() == mh.TinhNangMan!.Trim().ToLower()
            );
            if (exists)
                return Conflict(new { message = "Màn hình đã tồn tại!" });

            
            try
            {
                mh.CongNgheManHinh = mh.CongNgheManHinh.Trim();
                mh.KichThuoc = mh.KichThuoc.Trim();
                mh.DoPhanGiai = mh.DoPhanGiai.Trim();
                mh.TinhNangMan = mh.TinhNangMan.Trim();
                mh.MoTaMan = mh.MoTaMan?.Trim();

                _context.ManHinhs.Add(mh);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Thêm màn hình thành công!" });
            }
            catch
            {
                return StatusCode(500, new { message = "Lỗi khi thêm màn hình. Vui lòng thử lại!" });
            }
        }

        // e diss
        [HttpPost]
        [Route("ManHinh/Edit/{id}")]
        [Consumes("application/json")]
        public async Task<IActionResult> Edit(int id, [FromBody] ManHinh? mh)
        {
            var errors = new Dictionary<string, string>();

           
            if (string.IsNullOrWhiteSpace(mh?.CongNgheManHinh))
                errors["CongNgheManHinh"] = "Phải nhập công nghệ màn hình!";
            if (string.IsNullOrWhiteSpace(mh?.KichThuoc))
                errors["KichThuoc"] = "Phải nhập kích thước màn hình!";
            if (string.IsNullOrWhiteSpace(mh?.DoPhanGiai))
                errors["DoPhanGiai"] = "Phải nhập độ phân giải!";
            if (string.IsNullOrWhiteSpace(mh?.TinhNangMan))
                errors["TinhNangMan"] = "Phải nhập tính năng màn hình!";

            if (errors.Count > 0)
                return BadRequest(errors);

            var existing = await _context.ManHinhs.FindAsync(id);
            if (existing == null)
                return NotFound(new { message = "Không tìm thấy màn hình!" });

            // Check trùng 
            bool exists = await _context.ManHinhs.AnyAsync(m =>
                m.CongNgheManHinh!.Trim().ToLower() == mh!.CongNgheManHinh!.Trim().ToLower() &&
                m.KichThuoc!.Trim().ToLower() == mh.KichThuoc!.Trim().ToLower() &&
                m.DoPhanGiai!.Trim().ToLower() == mh.DoPhanGiai!.Trim().ToLower() &&
                m.TinhNangMan!.Trim().ToLower() == mh.TinhNangMan!.Trim().ToLower() &&
                m.IdManHinh != id
            );
            if (exists)
                return Conflict(new { message = "Màn hình đã tồn tại!" });

         
            try
            {
                existing.CongNgheManHinh = mh.CongNgheManHinh.Trim();
                existing.KichThuoc = mh.KichThuoc.Trim();
                existing.DoPhanGiai = mh.DoPhanGiai.Trim();
                existing.TinhNangMan = mh.TinhNangMan.Trim();
                existing.MoTaMan = mh.MoTaMan?.Trim();

                _context.ManHinhs.Update(existing);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Cập nhật màn hình thành công!" });
            }
            catch
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật màn hình. Vui lòng thử lại!" });
            }
        }
    }
}
