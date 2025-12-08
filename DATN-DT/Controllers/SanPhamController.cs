using DATN_DT.IServices;
using DATN_DT.Models;
using DATN_DT.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DATN_DT.Controllers
{
    public class SanPhamController : Controller
    {
        private readonly ISanPhamService _sanPhamService;
        private readonly MyDbContext _context;

        public SanPhamController(ISanPhamService sanPhamService, MyDbContext context)
        {
            _sanPhamService = sanPhamService;
            _context = context;
        }

        // ----------------------------
        // GET: Danh sách sản phẩm
        // ----------------------------
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var list = await _sanPhamService.GetAllSanPhams();
            return View(list);
        }

        // ----------------------------
        // POST: Thêm sản phẩm - SỬA LẠI
        // ----------------------------
        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> Create([FromBody] SanPham? sp)
        {
            // VALIDATION GIỐNG ModelSanPhamController
            var errors = new Dictionary<string, string>();

            if (sp == null)
                return BadRequest(new { message = "Dữ liệu sản phẩm không hợp lệ!" });

            if (string.IsNullOrWhiteSpace(sp.MaSanPham))
                errors["MaSanPham"] = "Phải nhập mã sản phẩm!";
            if (string.IsNullOrWhiteSpace(sp.TenSanPham))
                errors["TenSanPham"] = "Phải nhập tên sản phẩm!";
            if (sp.IdThuongHieu == null || sp.IdThuongHieu == 0)
                errors["IdThuongHieu"] = "Phải chọn thương hiệu!";
            if (sp.GiaGoc == null || sp.GiaGoc <= 0)
                errors["GiaGoc"] = "Giá gốc phải lớn hơn 0!";
            if (string.IsNullOrWhiteSpace(sp.TrangThaiSP))
                errors["TrangThaiSP"] = "Phải chọn trạng thái sản phẩm!";

            if (errors.Count > 0)
                return BadRequest(errors);

            // CHECK TRÙNG MÃ SẢN PHẨM - GIỐNG ModelSanPhamController
            bool exists = await _context.SanPhams.AnyAsync(s =>
                s.MaSanPham!.Trim().ToLower() == sp.MaSanPham!.Trim().ToLower()
            );
            if (exists)
                return Conflict(new { message = "Mã sản phẩm đã tồn tại!" });

            try
            {
                // FORMAT DỮ LIỆU - GIỐNG ModelSanPhamController
                sp.MaSanPham = sp.MaSanPham.Trim();
                sp.TenSanPham = sp.TenSanPham.Trim();
                sp.MoTa = sp.MoTa?.Trim();

                // TÍNH GIÁ NIÊM YẾT TỰ ĐỘNG
                sp.GiaNiemYet = sp.GiaGoc * (1 + (sp.VAT ?? 0) / 100);

                await _sanPhamService.Create(sp);
                return Ok(new { message = "Thêm sản phẩm thành công!" });
            }
            catch (Exception ex)
            {
                // LOG LỖI CHI TIẾT
                Console.WriteLine($"Lỗi khi thêm sản phẩm: {ex.Message}");
                return StatusCode(500, new { message = "Lỗi khi thêm sản phẩm. Vui lòng thử lại!" });
            }
        }

        // ----------------------------
        // POST: Update sản phẩm - SỬA LẠI
        // ----------------------------
        [HttpPost]
        [Route("SanPham/Edit/{id}")]
        [Consumes("application/json")]
        public async Task<IActionResult> Edit(int id, [FromBody] SanPham? sp)
        {
            // VALIDATION GIỐNG ModelSanPhamController
            var errors = new Dictionary<string, string>();

            if (sp == null)
                return BadRequest(new { message = "Dữ liệu sản phẩm không hợp lệ!" });

            if (string.IsNullOrWhiteSpace(sp.MaSanPham))
                errors["MaSanPham"] = "Phải nhập mã sản phẩm!";
            if (string.IsNullOrWhiteSpace(sp.TenSanPham))
                errors["TenSanPham"] = "Phải nhập tên sản phẩm!";
            if (sp.IdThuongHieu == null || sp.IdThuongHieu == 0)
                errors["IdThuongHieu"] = "Phải chọn thương hiệu!";
            if (sp.GiaGoc == null || sp.GiaGoc <= 0)
                errors["GiaGoc"] = "Giá gốc phải lớn hơn 0!";
            if (string.IsNullOrWhiteSpace(sp.TrangThaiSP))
                errors["TrangThaiSP"] = "Phải chọn trạng thái sản phẩm!";

            if (errors.Count > 0)
                return BadRequest(errors);

            // KIỂM TRA SẢN PHẨM TỒN TẠI - GIỐNG ModelSanPhamController
            var existing = await _sanPhamService.GetSanPhamById(id);
            if (existing == null)
                return NotFound(new { message = "Không tìm thấy sản phẩm!" });

            // CHECK TRÙNG MÃ SẢN PHẨM (TRỪ SẢN PHẨM HIỆN TẠI)
            bool exists = await _context.SanPhams.AnyAsync(s =>
                s.MaSanPham!.Trim().ToLower() == sp.MaSanPham!.Trim().ToLower() &&
                s.IdSanPham != id
            );
            if (exists)
                return Conflict(new { message = "Mã sản phẩm đã tồn tại!" });

            try
            {
                sp.IdSanPham = id; // ĐẢM BẢO ID ĐÚNG

                // FORMAT DỮ LIỆU
                sp.MaSanPham = sp.MaSanPham.Trim();
                sp.TenSanPham = sp.TenSanPham.Trim();
                sp.MoTa = sp.MoTa?.Trim();

                // TÍNH GIÁ NIÊM YẾT TỰ ĐỘNG
                sp.GiaNiemYet = sp.GiaGoc * (1 + (sp.VAT ?? 0) / 100);

                await _sanPhamService.Update(sp);
                return Ok(new { message = "Cập nhật sản phẩm thành công!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi cập nhật sản phẩm: {ex.Message}");
                return StatusCode(500, new { message = "Lỗi khi cập nhật sản phẩm. Vui lòng thử lại!" });
            }
        }

        // ----------------------------
        // GET: Load thương hiệu
        // ----------------------------
        [HttpGet]
        public async Task<IActionResult> GetThuongHieu()
        {
            try
            {
                var list = await _context.ThuongHieus
                    .Select(th => new {
                        IdThuongHieu = th.IdThuongHieu,
                        TenThuongHieu = th.TenThuongHieu
                    })
                    .ToListAsync();

                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi load thương hiệu: " + ex.Message });
            }
        }

        // ----------------------------
        // POST: Xóa sản phẩm
        // ----------------------------
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _sanPhamService.Delete(id);
                return Ok(new { message = "Xóa sản phẩm thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi xóa sản phẩm: " + ex.Message });
            }
        }
    }
}