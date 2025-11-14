using DATN_DT.Data;
using DATN_DT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DATN_DT.Controllers
{
    public class ImeiController : Controller
    {
        private readonly MyDbContext _context;

        public ImeiController(MyDbContext context)
        {
            _context = context;
        }

        // Idex
        public async Task<IActionResult> Index()
        {
            var imeis = await _context.Imeis.ToListAsync();
            var modelSanPhams = await _context.ModelSanPhams.ToDictionaryAsync(x => x.IdModelSanPham);
            ViewBag.ModelSanPhams = modelSanPhams;

            return View(imeis);
        }

        // CCreate
        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> Create([FromBody] Imei? imei)
        {
            if (imei == null)
                return BadRequest(new { message = "Dữ liệu không hợp lệ!" });

            var errors = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(imei.MaImei))
                errors["MaImei"] = "Phải nhập mã Imei!";
            if (imei.IdModelSanPham == null || imei.IdModelSanPham == 0)
                errors["IdModelSanPham"] = "Phải chọn model sản phẩm!";
            if (string.IsNullOrWhiteSpace(imei.TrangThai))
                errors["TrangThai"] = "Phải chọn trạng thái!";

            if (errors.Count > 0)
                return BadRequest(errors);

            // Check trùng mã Imei
            bool exists = await _context.Imeis.AnyAsync(i =>
                i.MaImei!.Trim().ToLower() == imei.MaImei.Trim().ToLower()
            );
            if (exists)
                return Conflict(new { message = "Mã Imei đã tồn tại!" });

            try
            {
                imei.MaImei = imei.MaImei.Trim();
                imei.MoTa = imei.MoTa?.Trim();
                imei.TrangThai = imei.TrangThai.Trim();

                _context.Imeis.Add(imei);
                await _context.SaveChangesAsync();

                // CapNhatt Kho sau khi them imei
                await UpdateTonKhoForModel(imei.IdModelSanPham.Value);

                return Ok(new { message = "Thêm Imei thành công!" });
            }
            catch
            {
                return StatusCode(500, new { message = "Lỗi khi thêm Imei. Vui lòng thử lại!" });
            }
        }

        // edit
        [HttpPost]
        [Route("Imei/Edit/{id}")]
        [Consumes("application/json")]
        public async Task<IActionResult> Edit(int id, [FromBody] Imei? imei)
        {
            if (imei == null)
                return BadRequest(new { message = "Dữ liệu không hợp lệ!" });

            var errors = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(imei.MaImei))
                errors["MaImei"] = "Phải nhập mã Imei!";
            if (imei.IdModelSanPham == null || imei.IdModelSanPham == 0)
                errors["IdModelSanPham"] = "Phải chọn model sản phẩm!";
            if (string.IsNullOrWhiteSpace(imei.TrangThai))
                errors["TrangThai"] = "Phải chọn trạng thái!";

            if (errors.Count > 0)
                return BadRequest(errors);

            var existing = await _context.Imeis.FindAsync(id);
            if (existing == null)
                return NotFound(new { message = "Không tìm thấy Imei!" });

            // Lưu lại thông tin cũ để xử lý tồn kho
            var oldModelId = existing.IdModelSanPham;
            var oldStatus = existing.TrangThai;

            // Check trùng mã Imei ngoại trừ chính nó
            bool exists = await _context.Imeis.AnyAsync(i =>
                i.MaImei!.Trim().ToLower() == imei.MaImei.Trim().ToLower() &&
                i.IdImei != id
            );
            if (exists)
                return Conflict(new { message = "Mã Imei đã tồn tại!" });

            try
            {
                existing.MaImei = imei.MaImei.Trim();
                existing.IdModelSanPham = imei.IdModelSanPham;
                existing.MoTa = imei.MoTa?.Trim();
                existing.TrangThai = imei.TrangThai.Trim();

                _context.Imeis.Update(existing);
                await _context.SaveChangesAsync();

                
                await HandleInventoryUpdate(oldModelId, imei.IdModelSanPham.Value, oldStatus, imei.TrangThai);

                return Ok(new { message = "Cập nhật Imei thành công!" });
            }
            catch
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật Imei. Vui lòng thử lại!" });
            }
        }

        // cap nhat ton kho
        private async Task HandleInventoryUpdate(int? oldModelId, int newModelId, string oldStatus, string newStatus)
        {
            // Nếu model thay đổi
            if (oldModelId.HasValue && oldModelId.Value != newModelId)
            {
                //
                await UpdateTonKhoForModel(oldModelId.Value);
                await UpdateTonKhoForModel(newModelId);
            }
            else
            {
                // Nếu cùng model, cập nhật tồn kho
                await UpdateTonKhoForModel(newModelId);
            }
        }

        private async Task UpdateTonKhoForModel(int modelSanPhamId)
        {
      
            var soLuongConHang = await _context.Imeis
                .CountAsync(i => i.IdModelSanPham == modelSanPhamId && i.TrangThai == "Còn hàng");

          
            var tonKhos = await _context.TonKhos
                .Where(t => t.IdModelSanPham == modelSanPhamId)
                .ToListAsync();

            if (tonKhos.Any())
            {
                foreach (var tonKho in tonKhos)
                {
                    tonKho.SoLuong = soLuongConHang;
                }
                await _context.SaveChangesAsync();
            }
            else
            {
                var khoMacDinh = await _context.Khos.FirstOrDefaultAsync();
                if (khoMacDinh != null)
                {
                    var newTonKho = new TonKho
                    {
                        IdModelSanPham = modelSanPhamId,
                        IdKho = khoMacDinh.IdKho,
                        SoLuong = soLuongConHang
                    };
                    _context.TonKhos.Add(newTonKho);
                    await _context.SaveChangesAsync();
                }
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
    }
}