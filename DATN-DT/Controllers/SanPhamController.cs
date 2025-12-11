using DATN_DT.IServices;
using DATN_DT.Models;
using DATN_DT.Data;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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
            try
            {
                var list = await _sanPhamService.GetAllSanPhams();
                // Load thương hiệu cho ViewBag
                var thuongHieus = await _context.ThuongHieus
                    .Where(t => t.TrangThaiThuongHieu == "co")
                    .Select(th => new {
                        IdThuongHieu = th.IdThuongHieu,
                        TenThuongHieu = th.TenThuongHieu
                    })
                    .OrderBy(th => th.TenThuongHieu)
                    .ToListAsync();
                ViewBag.ThuongHieus = thuongHieus;
                return View(list);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Không thể tải danh sách sản phẩm";
                Console.WriteLine($"Lỗi Index: {ex.Message}");
                return View(new List<SanPham>());
            }
        }

        // ----------------------------
        // POST: Thêm sản phẩm
        // ----------------------------
        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> Create([FromBody] SanPham? sp)
        {
            // VALIDATION
            var errors = new Dictionary<string, string>();

            if (sp == null)
                return BadRequest(new { success = false, message = "Dữ liệu sản phẩm không hợp lệ!" });

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
                return BadRequest(new { success = false, errors });

            // CHECK TRÙNG MÃ SẢN PHẨM
            bool exists = await _context.SanPhams.AnyAsync(s =>
                s.MaSanPham!.Trim().ToLower() == sp.MaSanPham!.Trim().ToLower()
            );
            if (exists)
                return Conflict(new { success = false, message = "Mã sản phẩm đã tồn tại!" });

            try
            {
                // FORMAT DỮ LIỆU
                sp.MaSanPham = sp.MaSanPham.Trim();
                sp.TenSanPham = sp.TenSanPham.Trim();
                sp.MoTa = sp.MoTa?.Trim();

                // TÍNH GIÁ NIÊM YẾT TỰ ĐỘNG
                sp.GiaNiemYet = sp.GiaGoc * (1 + (sp.VAT ?? 0) / 100);

                await _sanPhamService.Create(sp);
                await _context.SaveChangesAsync(); // Lưu vào DB
                
                return Ok(new { success = true, message = "Thêm sản phẩm thành công!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi thêm sản phẩm: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi thêm sản phẩm. Vui lòng thử lại!"
                });
            }
        }

        // ----------------------------
        // POST: Update sản phẩm
        // ----------------------------
        [HttpPost]
        [Route("SanPham/Edit/{id}")]
        [Consumes("application/json")]
        public async Task<IActionResult> Edit(int id, [FromBody] SanPham? sp)
        {
            // VALIDATION
            var errors = new Dictionary<string, string>();

            if (sp == null)
                return BadRequest(new { success = false, message = "Dữ liệu sản phẩm không hợp lệ!" });

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
                return BadRequest(new { success = false, errors });

            // KIỂM TRA SẢN PHẨM TỒN TẠI
            var existing = await _sanPhamService.GetSanPhamById(id);
            if (existing == null)
                return NotFound(new { success = false, message = "Không tìm thấy sản phẩm!" });

            // CHECK TRÙNG MÃ SẢN PHẨM (TRỪ SẢN PHẨM HIỆN TẠI)
            bool duplicateExists = await _context.SanPhams.AnyAsync(s =>
                s.MaSanPham!.Trim().ToLower() == sp.MaSanPham!.Trim().ToLower() &&
                s.IdSanPham != id
            );
            if (duplicateExists)
                return Conflict(new { success = false, message = "Mã sản phẩm đã tồn tại!" });

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
                await _context.SaveChangesAsync(); // Lưu vào DB
                
                return Ok(new { success = true, message = "Cập nhật sản phẩm thành công!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi cập nhật sản phẩm: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi cập nhật sản phẩm. Vui lòng thử lại!"
                });
            }
        }

        // ----------------------------
        // GET: Load thương hiệu (API cho dropdown)
        // ----------------------------
        [HttpGet("SanPham/GetThuongHieu")]
        public async Task<IActionResult> GetThuongHieu()
        {
            try
            {
                var list = await _context.ThuongHieus
                    .Where(t => t.TrangThaiThuongHieu == "Còn hoạt động")
                    .Select(th => new {
                        IdThuongHieu = th.IdThuongHieu,
                        TenThuongHieu = th.TenThuongHieu
                    })
                    .OrderBy(th => th.TenThuongHieu)
                    .ToListAsync();

                return Ok(new { success = true, data = list });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi GetThuongHieu: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi load thương hiệu: " + ex.Message
                });
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
                // Kiểm tra sản phẩm có model nào không
                var hasModels = await _context.ModelSanPhams
                    .AnyAsync(m => m.IdSanPham == id && m.TrangThai == 1);

                if (hasModels)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Không thể xóa sản phẩm vì còn model sản phẩm. Vui lòng xóa tất cả model trước."
                    });
                }

                await _sanPhamService.Delete(id);
                return Ok(new { success = true, message = "Xóa sản phẩm thành công!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi xóa sản phẩm: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi xóa sản phẩm: " + ex.Message
                });
            }
        }

        // ----------------------------
        // GET: Lấy thông tin sản phẩm theo ID
        // ----------------------------
        [HttpGet("SanPham/GetById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var sanPham = await _sanPhamService.GetSanPhamById(id);
                if (sanPham == null)
                    return NotFound(new { success = false, message = "Không tìm thấy sản phẩm" });

                return Ok(new { success = true, data = sanPham });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi GetById: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lấy thông tin sản phẩm: " + ex.Message
                });
            }
        }

        // ----------------------------
        // GET: Chuyển sang trang ModelSanPham
        // ----------------------------
        [HttpGet("SanPham/ModelSanPham/{id}")]
        public IActionResult ModelSanPham(int id)
        {
            try
            {
                return RedirectToAction("Index", "ModelSanPham", new { sanPhamId = id });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi ModelSanPham: {ex.Message}");
                TempData["Error"] = "Có lỗi xảy ra khi chuyển trang";
                return RedirectToAction("Index");
            }
        }

        // ----------------------------
        // GET: Tìm kiếm sản phẩm
        // ----------------------------
        [HttpGet("SanPham/Search")]
        public async Task<IActionResult> Search(string keyword, string status)
        {
            try
            {
                var query = _context.SanPhams
                    .Include(s => s.ThuongHieu)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    keyword = keyword.ToLower();
                    query = query.Where(s =>
                        s.MaSanPham.ToLower().Contains(keyword) ||
                        s.TenSanPham.ToLower().Contains(keyword) ||
                        (s.ThuongHieu != null && s.ThuongHieu.TenThuongHieu.ToLower().Contains(keyword))
                    );
                }

                if (!string.IsNullOrWhiteSpace(status))
                {
                    query = query.Where(s => s.TrangThaiSP == status);
                }

                var result = await query
                    .OrderByDescending(s => s.IdSanPham)
                    .Select(s => new
                    {
                        s.IdSanPham,
                        s.MaSanPham,
                        s.TenSanPham,
                        ThuongHieu = s.ThuongHieu != null ? s.ThuongHieu.TenThuongHieu : "",
                        s.GiaGoc,
                        s.GiaNiemYet,
                        s.VAT,
                        s.TrangThaiSP,
                        s.MoTa
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi Search: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi tìm kiếm sản phẩm: " + ex.Message
                });
            }
        }

        // ----------------------------
        // GET: Lấy tất cả sản phẩm cho dropdown
        // ----------------------------
        [HttpGet("SanPham/GetAllForDropdown")]
        public async Task<IActionResult> GetAllForDropdown()
        {
            try
            {
                var list = await _context.SanPhams
                    .Where(s => s.TrangThaiSP == "Còn hàng" || s.TrangThaiSP == "Đang nhập hàng")
                    .Select(s => new
                    {
                        IdSanPham = s.IdSanPham,
                        TenSanPham = s.TenSanPham,
                        MaSanPham = s.MaSanPham
                    })
                    .OrderBy(s => s.TenSanPham)
                    .ToListAsync();

                return Ok(new { success = true, data = list });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi GetAllForDropdown: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lấy danh sách sản phẩm: " + ex.Message
                });
            }
        }

        // ----------------------------
        // GET: Chi tiết sản phẩm
        // ----------------------------
        [HttpGet("SanPham/Detail/{id}")]
        public async Task<IActionResult> Detail(int id)
        {
            try
            {
                var sanPham = await _context.SanPhams
                    .Include(s => s.ThuongHieu)
                    .Include(s => s.ModelSanPhams)
                        .ThenInclude(m => m.Imeis)
                    .FirstOrDefaultAsync(s => s.IdSanPham == id);

                if (sanPham == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy sản phẩm" });
                }

                var result = new
                {
                    sanPham.IdSanPham,
                    sanPham.MaSanPham,
                    sanPham.TenSanPham,
                    ThuongHieu = sanPham.ThuongHieu != null ? new
                    {
                        sanPham.ThuongHieu.IdThuongHieu,
                        sanPham.ThuongHieu.TenThuongHieu
                    } : null,
                    sanPham.GiaGoc,
                    sanPham.GiaNiemYet,
                    sanPham.VAT,
                    sanPham.TrangThaiSP,
                    sanPham.MoTa,
                    TotalModels = sanPham.ModelSanPhams?.Count(m => m.TrangThai == 1) ?? 0,
                    TotalImeis = sanPham.ModelSanPhams?
                        .SelectMany(m => m.Imeis ?? new List<Imei>())
                        .Count() ?? 0
                };

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi Detail: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lấy chi tiết sản phẩm: " + ex.Message
                });
            }
        }
    }
}