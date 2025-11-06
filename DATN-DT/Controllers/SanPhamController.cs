using DATN_DT.Data;
using DATN_DT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DATN_DT.Controllers
{
    public class SanPhamController : Controller
    {
        private readonly MyDbContext _context;

        public SanPhamController(MyDbContext context)
        {
            _context = context;
        }

        
        public async Task<IActionResult> Index()
        {
         
            var list = await (from sp in _context.SanPhams
                              join th in _context.ThuongHieus on sp.IdThuongHieu equals th.IdThuongHieu
                              select new SanPham
                              {
                                  IdSanPham = sp.IdSanPham,
                                  MaSanPham = sp.MaSanPham,
                                  TenSanPham = sp.TenSanPham,
                                  IdThuongHieu = sp.IdThuongHieu,
                                  ThuongHieu = th, 
                                  MoTa = sp.MoTa,
                                  GiaGoc = sp.GiaGoc,
                                  GiaNiemYet = sp.GiaNiemYet,
                                  TrangThaiSP = sp.TrangThaiSP,
                                  VAT = sp.VAT
                              }).ToListAsync();

            return View(list);
        }

       
        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> Create([FromBody] SanPham? sp)
        {
            var errors = new Dictionary<string, string>();

          
            if (string.IsNullOrWhiteSpace(sp?.MaSanPham))
                errors["MaSanPham"] = "Phải nhập mã sản phẩm!";
            if (string.IsNullOrWhiteSpace(sp?.TenSanPham))
                errors["TenSanPham"] = "Phải nhập tên sản phẩm!";
            if (sp?.IdThuongHieu == null || sp.IdThuongHieu == 0)
                errors["IdThuongHieu"] = "Phải chọn thương hiệu!";
            if (sp?.GiaGoc == null || sp.GiaGoc <= 0)
                errors["GiaGoc"] = "Giá gốc phải lớn hơn 0!";
            if (string.IsNullOrWhiteSpace(sp?.TrangThaiSP))
                errors["TrangThaiSP"] = "Phải chọn trạng thái sản phẩm!";

            if (errors.Count > 0)
                return BadRequest(errors);

       
            bool maExists = await _context.SanPhams.AnyAsync(s =>
                s.MaSanPham!.Trim().ToLower() == sp!.MaSanPham!.Trim().ToLower()
            );
            if (maExists)
                return Conflict(new { message = "Mã sản phẩm đã tồn tại!" });

       
            bool tenExists = await _context.SanPhams.AnyAsync(s =>
                s.TenSanPham!.Trim().ToLower() == sp!.TenSanPham!.Trim().ToLower()
            );
            if (tenExists)
                return Conflict(new { message = "Tên sản phẩm đã tồn tại!" });

            try
            {
                sp.MaSanPham = sp.MaSanPham.Trim();
                sp.TenSanPham = sp.TenSanPham.Trim();
                sp.MoTa = sp.MoTa?.Trim();
                sp.TrangThaiSP = sp.TrangThaiSP.Trim();

           
                sp.GiaNiemYet = sp.GiaGoc * (1 + (sp.VAT ?? 0) / 100);

                _context.SanPhams.Add(sp);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Thêm sản phẩm thành công!" });
            }
            catch
            {
                return StatusCode(500, new { message = "Lỗi khi thêm sản phẩm. Vui lòng thử lại!" });
            }
        }

        // edd
        [HttpPost]
        [Route("SanPham/Edit/{id}")]
        [Consumes("application/json")]
        public async Task<IActionResult> Edit(int id, [FromBody] SanPham? sp)
        {
            var errors = new Dictionary<string, string>();

            // Validate input
            if (string.IsNullOrWhiteSpace(sp?.MaSanPham))
                errors["MaSanPham"] = "Phải nhập mã sản phẩm!";
            if (string.IsNullOrWhiteSpace(sp?.TenSanPham))
                errors["TenSanPham"] = "Phải nhập tên sản phẩm!";
            if (sp?.IdThuongHieu == null || sp.IdThuongHieu == 0)
                errors["IdThuongHieu"] = "Phải chọn thương hiệu!";
            if (sp?.GiaGoc == null || sp.GiaGoc <= 0)
                errors["GiaGoc"] = "Giá gốc phải lớn hơn 0!";
            if (string.IsNullOrWhiteSpace(sp?.TrangThaiSP))
                errors["TrangThaiSP"] = "Phải chọn trạng thái sản phẩm!";

            if (errors.Count > 0)
                return BadRequest(errors);

            var existing = await _context.SanPhams.FindAsync(id);
            if (existing == null)
                return NotFound(new { message = "Không tìm thấy sản phẩm!" });

            // Check trùng mã sản phẩm 
            bool maExists = await _context.SanPhams.AnyAsync(s =>
                s.MaSanPham!.Trim().ToLower() == sp!.MaSanPham!.Trim().ToLower() &&
                s.IdSanPham != id
            );
            if (maExists)
                return Conflict(new { message = "Mã sản phẩm đã tồn tại!" });

            // Check trùng tên sản phẩm 
            bool tenExists = await _context.SanPhams.AnyAsync(s =>
                s.TenSanPham!.Trim().ToLower() == sp!.TenSanPham!.Trim().ToLower() &&
                s.IdSanPham != id
            );
            if (tenExists)
                return Conflict(new { message = "Tên sản phẩm đã tồn tại!" });

            try
            {
                existing.MaSanPham = sp.MaSanPham.Trim();
                existing.TenSanPham = sp.TenSanPham.Trim();
                existing.IdThuongHieu = sp.IdThuongHieu;
                existing.MoTa = sp.MoTa?.Trim();
                existing.GiaGoc = sp.GiaGoc;
                // Tính toán giá niêm yết tự động
                existing.GiaNiemYet = sp.GiaGoc * (1 + (sp.VAT ?? 0) / 100);
                existing.TrangThaiSP = sp.TrangThaiSP.Trim();
                existing.VAT = sp.VAT;

                _context.SanPhams.Update(existing);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Cập nhật sản phẩm thành công!" });
            }
            catch
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật sản phẩm. Vui lòng thử lại!" });
            }
        }

     
        [HttpGet]
        public async Task<IActionResult> GetThuongHieu()
        {
            var thuongHieus = await _context.ThuongHieus
                .Where(th => th.TrangThaiThuongHieu == "Còn hoạt động")
                .Select(th => new { th.IdThuongHieu, th.TenThuongHieu })
                .ToListAsync();
            return Ok(thuongHieus);
        }
    }
}