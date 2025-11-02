using DATN_DT.Data;
using DATN_DT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DATN_DT.Controllers
{
    public class TonKhoController : Controller
    {
        private readonly MyDbContext _context;

        public TonKhoController(MyDbContext context)
        {
            _context = context;
        }

        // INDEX 
        public async Task<IActionResult> Index()
        {
            var tonKhos = await _context.TonKhos.ToListAsync();
            var modelSanPhams = await _context.ModelSanPhams.ToDictionaryAsync(x => x.IdModelSanPham);
            var khos = await _context.Khos.ToDictionaryAsync(x => x.IdKho);

            ViewBag.ModelSanPhams = modelSanPhams;
            ViewBag.Khos = khos;

            return View(tonKhos);
        }

        // CREATE 
        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> Create([FromBody] TonKho? tonKho)
        {
            var errors = new Dictionary<string, string>();

            if (tonKho?.IdModelSanPham == null || tonKho.IdModelSanPham == 0)
                errors["IdModelSanPham"] = "Phải chọn model sản phẩm!";
            if (tonKho?.IdKho == null || tonKho.IdKho == 0)
                errors["IdKho"] = "Phải chọn kho!";

            if (errors.Count > 0)
                return BadRequest(errors);

            // Check trùng 
            bool exists = await _context.TonKhos.AnyAsync(t =>
                t.IdModelSanPham == tonKho!.IdModelSanPham &&
                t.IdKho == tonKho.IdKho
            );
            if (exists)
                return Conflict(new { message = "Tồn kho cho model sản phẩm này đã tồn tại trong kho!" });

            try
            {
               
                var soLuongConHang = await _context.Imeis
                    .CountAsync(i => i.IdModelSanPham == tonKho.IdModelSanPham && i.TrangThai == "Còn hàng");

                tonKho.SoLuong = soLuongConHang;

                _context.TonKhos.Add(tonKho);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Thêm tồn kho thành công!" });
            }
            catch
            {
                return StatusCode(500, new { message = "Lỗi khi thêm tồn kho. Vui lòng thử lại!" });
            }
        }

        // EDIT
        [HttpPost]
        [Route("TonKho/Edit/{id}")]
        [Consumes("application/json")]
        public async Task<IActionResult> Edit(int id, [FromBody] TonKho? tonKho)
        {
            var errors = new Dictionary<string, string>();

            if (tonKho?.IdModelSanPham == null || tonKho.IdModelSanPham == 0)
                errors["IdModelSanPham"] = "Phải chọn model sản phẩm!";
            if (tonKho?.IdKho == null || tonKho.IdKho == 0)
                errors["IdKho"] = "Phải chọn kho!";

            if (errors.Count > 0)
                return BadRequest(errors);

            var existing = await _context.TonKhos.FindAsync(id);
            if (existing == null)
                return NotFound(new { message = "Không tìm thấy tồn kho!" });

            // Check trùng 
            bool exists = await _context.TonKhos.AnyAsync(t =>
                t.IdModelSanPham == tonKho!.IdModelSanPham &&
                t.IdKho == tonKho.IdKho &&
                t.IdTonKho != id
            );
            if (exists)
                return Conflict(new { message = "Tồn kho cho model sản phẩm này đã tồn tại trong kho!" });

            try
            {
                existing.IdModelSanPham = tonKho.IdModelSanPham;
                existing.IdKho = tonKho.IdKho;
               

                _context.TonKhos.Update(existing);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Cập nhật tồn kho thành công!" });
            }
            catch
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật tồn kho. Vui lòng thử lại!" });
            }
        }

     
        [HttpPost]
        [Route("TonKho/SyncInventory")]
        public async Task<IActionResult> SyncInventory()
        {
            try
            {
              
                var models = await _context.ModelSanPhams.ToListAsync();
                var khoMacDinh = await _context.Khos.FirstOrDefaultAsync();

                if (khoMacDinh == null)
                {
                    return BadRequest(new { message = "Không tìm thấy kho mặc định!" });
                }

                foreach (var model in models)
                {
                   
                    var soLuongConHang = await _context.Imeis
                        .CountAsync(i => i.IdModelSanPham == model.IdModelSanPham && i.TrangThai == "Còn hàng");

                    var tonKhos = await _context.TonKhos
                        .Where(t => t.IdModelSanPham == model.IdModelSanPham)
                        .ToListAsync();

                    if (tonKhos.Any())
                    {
                       
                        foreach (var tonKho in tonKhos)
                        {
                            tonKho.SoLuong = soLuongConHang;
                        }
                    }
                    else
                    {
                   
                        var newTonKho = new TonKho
                        {
                            IdModelSanPham = model.IdModelSanPham,
                            IdKho = khoMacDinh.IdKho,
                            SoLuong = soLuongConHang
                        };
                        _context.TonKhos.Add(newTonKho);
                    }
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = "Đồng bộ tồn kho thành công! Số lượng đã được cập nhật theo IMEI có trạng thái 'Còn hàng'." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi khi đồng bộ tồn kho: {ex.Message}" });
            }
        }

       
        [HttpGet]
        public async Task<IActionResult> GetAllModelSanPham()
        {
            var list = await _context.ModelSanPhams
                .Select(m => new { m.IdModelSanPham, DisplayText = m.TenModel })
                .ToListAsync();
            return Ok(list);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllKho()
        {
            var list = await _context.Khos
                .Select(k => new { k.IdKho, DisplayText = k.TenKho })
                .ToListAsync();
            return Ok(list);
        }
    }
}