using DATN_DT.Data;
using DATN_DT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DATN_DT.Controllers
{
    public class ModelSanPhamController : Controller
    {
        private readonly MyDbContext _context;

        public ModelSanPhamController(MyDbContext context)
        {
            _context = context;
        }

        // dex
        public async Task<IActionResult> Index()
        {
           
            var modelSanPhams = await _context.ModelSanPhams.ToListAsync();

           
            var sanPhams = await _context.SanPhams.ToDictionaryAsync(x => x.IdSanPham);
            var manHinhs = await _context.ManHinhs.ToDictionaryAsync(x => x.IdManHinh);
            var cameraTruocs = await _context.CameraTruocs.ToDictionaryAsync(x => x.IdCamTruoc);
            var cameraSaus = await _context.CameraSaus.ToDictionaryAsync(x => x.IdCameraSau);
            var pins = await _context.Pins.ToDictionaryAsync(x => x.IdPin);
            var rams = await _context.RAMs.ToDictionaryAsync(x => x.IdRAM);
            var roms = await _context.ROMs.ToDictionaryAsync(x => x.IdROM);

            
            ViewBag.SanPhams = sanPhams;
            ViewBag.ManHinhs = manHinhs;
            ViewBag.CameraTruocs = cameraTruocs;
            ViewBag.CameraSaus = cameraSaus;
            ViewBag.Pins = pins;
            ViewBag.RAMs = rams;
            ViewBag.ROMs = roms;

            return View(modelSanPhams);
        }

        // crate
        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> Create([FromBody] ModelSanPham? model)
        {
            var errors = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(model?.TenModel))
                errors["TenModel"] = "Phải nhập tên model!";
            if (model?.IdSanPham == null || model.IdSanPham == 0)
                errors["IdSanPham"] = "Phải chọn sản phẩm!";
            if (model?.IdManHinh == null || model.IdManHinh == 0)
                errors["IdManHinh"] = "Phải chọn màn hình!";
            if (model?.IdCameraTruoc == null || model.IdCameraTruoc == 0)
                errors["IdCameraTruoc"] = "Phải chọn camera trước!";
            if (model?.IdCameraSau == null || model.IdCameraSau == 0)
                errors["IdCameraSau"] = "Phải chọn camera sau!";
            if (model?.IdPin == null || model.IdPin == 0)
                errors["IdPin"] = "Phải chọn pin!";
            if (model?.IdRAM == null || model.IdRAM == 0)
                errors["IdRAM"] = "Phải chọn RAM!";
            if (model?.IdROM == null || model.IdROM == 0)
                errors["IdROM"] = "Phải chọn ROM!";
            if (string.IsNullOrWhiteSpace(model?.Mau))
                errors["Mau"] = "Phải nhập màu!";
            if (model?.GiaBanModel == null || model.GiaBanModel <= 0)
                errors["GiaBanModel"] = "Giá bán phải lớn hơn 0!";

            if (errors.Count > 0)
                return BadRequest(errors);

            // Check trùng
            bool exists = await _context.ModelSanPhams.AnyAsync(m =>
                m.TenModel!.Trim().ToLower() == model!.TenModel!.Trim().ToLower() &&
                m.Mau!.Trim().ToLower() == model.Mau!.Trim().ToLower() &&
                m.IdSanPham == model.IdSanPham
            );
            if (exists)
                return Conflict(new { message = "Model sản phẩm đã tồn tại!" });

            try
            {
                model.TenModel = model.TenModel.Trim();
                model.Mau = model.Mau.Trim();

                _context.ModelSanPhams.Add(model);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Thêm model sản phẩm thành công!" });
            }
            catch
            {
                return StatusCode(500, new { message = "Lỗi khi thêm model sản phẩm. Vui lòng thử lại!" });
            }
        }

        // edit
        [HttpPost]
        [Route("ModelSanPham/Edit/{id}")]
        [Consumes("application/json")]
        public async Task<IActionResult> Edit(int id, [FromBody] ModelSanPham? model)
        {
            var errors = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(model?.TenModel))
                errors["TenModel"] = "Phải nhập tên model!";
            if (model?.IdSanPham == null || model.IdSanPham == 0)
                errors["IdSanPham"] = "Phải chọn sản phẩm!";
            if (model?.IdManHinh == null || model.IdManHinh == 0)
                errors["IdManHinh"] = "Phải chọn màn hình!";
            if (model?.IdCameraTruoc == null || model.IdCameraTruoc == 0)
                errors["IdCameraTruoc"] = "Phải chọn camera trước!";
            if (model?.IdCameraSau == null || model.IdCameraSau == 0)
                errors["IdCameraSau"] = "Phải chọn camera sau!";
            if (model?.IdPin == null || model.IdPin == 0)
                errors["IdPin"] = "Phải chọn pin!";
            if (model?.IdRAM == null || model.IdRAM == 0)
                errors["IdRAM"] = "Phải chọn RAM!";
            if (model?.IdROM == null || model.IdROM == 0)
                errors["IdROM"] = "Phải chọn ROM!";
            if (string.IsNullOrWhiteSpace(model?.Mau))
                errors["Mau"] = "Phải nhập màu!";
            if (model?.GiaBanModel == null || model.GiaBanModel <= 0)
                errors["GiaBanModel"] = "Giá bán phải lớn hơn 0!";

            if (errors.Count > 0)
                return BadRequest(errors);

            var existing = await _context.ModelSanPhams.FindAsync(id);
            if (existing == null)
                return NotFound(new { message = "Không tìm thấy model sản phẩm!" });

            // Check clone
            bool exists = await _context.ModelSanPhams.AnyAsync(m =>
                m.TenModel!.Trim().ToLower() == model!.TenModel!.Trim().ToLower() &&
                m.Mau!.Trim().ToLower() == model.Mau!.Trim().ToLower() &&
                m.IdSanPham == model.IdSanPham &&
                m.IdModelSanPham != id
            );
            if (exists)
                return Conflict(new { message = "Model sản phẩm đã tồn tại!" });

            try
            {
                existing.TenModel = model.TenModel.Trim();
                existing.IdSanPham = model.IdSanPham;
                existing.IdManHinh = model.IdManHinh;
                existing.IdCameraTruoc = model.IdCameraTruoc;
                existing.IdCameraSau = model.IdCameraSau;
                existing.IdPin = model.IdPin;
                existing.IdRAM = model.IdRAM;
                existing.IdROM = model.IdROM;
                existing.Mau = model.Mau.Trim();
                existing.GiaBanModel = model.GiaBanModel;

                _context.ModelSanPhams.Update(existing);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Cập nhật model sản phẩm thành công!" });
            }
            catch
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật model sản phẩm. Vui lòng thử lại!" });
            }
        }

        
        [HttpGet]
        public async Task<IActionResult> GetAllSanPham()
        {
            var list = await _context.SanPhams.ToListAsync();
            return Ok(list);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllManHinh()
        {
            var list = await _context.ManHinhs
                .Select(m => new { m.IdManHinh, DisplayText = $"{m.CongNgheManHinh} - {m.KichThuoc}" })
                .ToListAsync();
            return Ok(list);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCameraTruoc()
        {
            var list = await _context.CameraTruocs
                .Select(c => new { c.IdCamTruoc, DisplayText = c.DoPhanGiaiCamTruoc })
                .ToListAsync();
            return Ok(list);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCameraSau()
        {
            var list = await _context.CameraSaus
                .Select(c => new { c.IdCameraSau, DisplayText = c.DoPhanGiaiCamSau })
                .ToListAsync();
            return Ok(list);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPin()
        {
            var list = await _context.Pins
                .Select(p => new { p.IdPin, DisplayText = $"{p.LoaiPin} - {p.DungLuongPin}" })
                .ToListAsync();
            return Ok(list);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRAM()
        {
            var list = await _context.RAMs
                .Select(r => new { r.IdRAM, DisplayText = r.DungLuongRAM })
                .ToListAsync();
            return Ok(list);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllROM()
        {
            var list = await _context.ROMs
                .Select(r => new { r.IdROM, DisplayText = r.DungLuongROM })
                .ToListAsync();
            return Ok(list);
        }
    }
}