using DATN_DT.IServices;
using DATN_DT.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DATN_DT.Controllers
{
    [Route("api/[controller]")]
    public class ManHinhController : Controller
    {
        private readonly IManHinhService _manHinhService;

        public ManHinhController(IManHinhService manHinhService)
        {
            _manHinhService = manHinhService;
        }

        // ===== GET ALL =====
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var manHinhs = await _manHinhService.GetAllManHinhs();
            return View(manHinhs);
        }

        // ===== CREATE =====
        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> Create([FromBody] ManHinh? manHinh)
        {
            var errors = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(manHinh?.KichThuoc))
                errors["KichThuoc"] = "Phải điền kích thước màn hình!";

            if (string.IsNullOrWhiteSpace(manHinh?.DoPhanGiai))
                errors["DoPhanGiai"] = "Phải điền độ phân giải!";

            if (string.IsNullOrWhiteSpace(manHinh?.CongNgheManHinh))
                errors["CongNgheManHinh"] = "Phải điền công nghệ màn hình!";

            if (errors.Count > 0)
                return BadRequest(errors);

            // Check trùng
            var allManHinhs = await _manHinhService.GetAllManHinhs();
            bool exists = allManHinhs.Any(m =>
                m.KichThuoc!.Trim().ToLower() == manHinh!.KichThuoc.Trim().ToLower() &&
                m.DoPhanGiai!.Trim().ToLower() == manHinh.DoPhanGiai.Trim().ToLower() &&
                m.CongNgheManHinh!.Trim().ToLower() == manHinh.CongNgheManHinh.Trim().ToLower());

            if (exists)
                return Conflict(new { message = "Màn hình đã tồn tại!" });

            // Chuẩn hóa trước khi lưu
            manHinh.KichThuoc = manHinh.KichThuoc.Trim();
            manHinh.DoPhanGiai = manHinh.DoPhanGiai.Trim();
            manHinh.CongNgheManHinh = manHinh.CongNgheManHinh.Trim();
            manHinh.TinhNangMan = manHinh.TinhNangMan?.Trim();
            manHinh.MoTaMan = manHinh.MoTaMan?.Trim();

            await _manHinhService.Create(manHinh);

            return Ok(new { message = "Thêm màn hình thành công!" });
        }

        // ===== EDIT =====
        [HttpPut]
        [Consumes("application/json")]
        public async Task<IActionResult> Edit(int id, [FromBody] ManHinh? manHinh)
        {
            var errors = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(manHinh?.KichThuoc))
                errors["KichThuoc"] = "Phải điền kích thước màn hình!";

            if (string.IsNullOrWhiteSpace(manHinh?.DoPhanGiai))
                errors["DoPhanGiai"] = "Phải điền độ phân giải!";

            if (string.IsNullOrWhiteSpace(manHinh?.CongNgheManHinh))
                errors["CongNgheManHinh"] = "Phải điền công nghệ màn hình!";

            if (errors.Count > 0)
                return BadRequest(errors);

            var existing = await _manHinhService.GetManHinhById(id);
            if (existing == null)
                return NotFound(new { message = "Không tìm thấy màn hình!" });

            // Check trùng (trừ chính nó)
            var allManHinhs = await _manHinhService.GetAllManHinhs();
            bool exists = allManHinhs.Any(m =>
                m.IdManHinh != id &&
                m.KichThuoc!.Trim().ToLower() == manHinh!.KichThuoc.Trim().ToLower() &&
                m.DoPhanGiai!.Trim().ToLower() == manHinh.DoPhanGiai.Trim().ToLower() &&
                m.CongNgheManHinh!.Trim().ToLower() == manHinh.CongNgheManHinh.Trim().ToLower());

            if (exists)
                return Conflict(new { message = "Màn hình đã tồn tại!" });

            // Cập nhật dữ liệu
            existing.KichThuoc = manHinh.KichThuoc.Trim();
            existing.DoPhanGiai = manHinh.DoPhanGiai.Trim();
            existing.CongNgheManHinh = manHinh.CongNgheManHinh.Trim();
            existing.TinhNangMan = manHinh.TinhNangMan?.Trim();
            existing.MoTaMan = manHinh.MoTaMan?.Trim();

            await _manHinhService.Update(existing);

            return Ok(new { message = "Cập nhật màn hình thành công!" });
        }

        // ===== DELETE =====
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            await _manHinhService.Delete(id);
            return Ok(new { message = "Xoá màn hình thành công!" });
        }
    }
}