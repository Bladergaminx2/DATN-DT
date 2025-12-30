using DATN_DT.IServices;
using DATN_DT.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace DATN_DT.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SanPhamController : Controller
    {
        private readonly ISanPhamService _sanPhamService;
        private readonly IThuongHieuService _thuongHieuService;

        public SanPhamController(ISanPhamService sanPhamService, IThuongHieuService thuongHieuService)
        {
            _sanPhamService = sanPhamService;
            _thuongHieuService = thuongHieuService;
        }

        // ===== GET: SanPham/Index =====
        [HttpGet]
        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var sanPhams = await _sanPhamService.GetAllSanPhams();
                return View(sanPhams);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading san phams: {ex.Message}");
                return View(new List<SanPham>());
            }
        }

        // ===== CREATE =====
        [HttpPost]
        [Route("Create")]
        [Consumes("application/json")]
        public async Task<IActionResult> Create([FromBody] SanPham sanPham)
        {
            try
            {
                Console.WriteLine("=== CREATE SAN PHAM ===");
                Console.WriteLine($"Received: {System.Text.Json.JsonSerializer.Serialize(sanPham)}");

                if (sanPham == null)
                {
                    return BadRequest(new { Message = "Dữ liệu sản phẩm không được rỗng!" });
                }

                // Validation
                var errors = new Dictionary<string, string>();
                if (string.IsNullOrWhiteSpace(sanPham.MaSanPham))
                    errors["MaSanPham"] = "Phải nhập mã sản phẩm!";
                if (string.IsNullOrWhiteSpace(sanPham.TenSanPham))
                    errors["TenSanPham"] = "Phải nhập tên sản phẩm!";
                if (sanPham.IdThuongHieu == null || sanPham.IdThuongHieu == 0)
                    errors["IdThuongHieu"] = "Phải chọn thương hiệu!";
                if (sanPham.GiaGoc == null || sanPham.GiaGoc <= 0)
                    errors["GiaGoc"] = "Giá gốc phải lớn hơn 0!";
                if (string.IsNullOrWhiteSpace(sanPham.TrangThaiSP))
                    errors["TrangThaiSP"] = "Phải chọn trạng thái sản phẩm!";

                if (errors.Count > 0)
                    return BadRequest(new { Errors = errors });

                // Validate VAT nếu có
                if (sanPham.VAT.HasValue && (sanPham.VAT < 0 || sanPham.VAT > 100))
                    return BadRequest(new { VAT = "VAT phải từ 0 đến 100%" });

                // Kiểm tra thương hiệu có tồn tại không
                var thuongHieu = await _thuongHieuService.GetThuongHieuById(sanPham.IdThuongHieu ?? 0);
                if (thuongHieu == null)
                    return BadRequest(new { IdThuongHieu = "Thương hiệu không tồn tại!" });

                // Chuẩn hóa dữ liệu
                sanPham.MaSanPham = sanPham.MaSanPham.Trim();
                sanPham.TenSanPham = sanPham.TenSanPham.Trim();
                sanPham.MoTa = sanPham.MoTa?.Trim();

                // Tính giá niêm yết nếu có VAT
                if (sanPham.VAT.HasValue && sanPham.VAT > 0)
                {
                    sanPham.GiaNiemYet = sanPham.GiaGoc * (1 + sanPham.VAT.Value / 100);
                }
                else
                {
                    sanPham.GiaNiemYet = sanPham.GiaGoc;
                }

                // Kiểm tra trùng mã sản phẩm
                var allSanPhams = await _sanPhamService.GetAllSanPhams();
                bool maExists = allSanPhams.Any(p =>
                    p.MaSanPham.Trim().Equals(sanPham.MaSanPham, StringComparison.OrdinalIgnoreCase)
                );

                if (maExists)
                    return Conflict(new { Message = "Mã sản phẩm đã tồn tại trong hệ thống!" });

                // Kiểm tra trùng tên sản phẩm
                bool tenExists = allSanPhams.Any(p =>
                    p.TenSanPham.Trim().Equals(sanPham.TenSanPham, StringComparison.OrdinalIgnoreCase)
                );

                if (tenExists)
                    return Conflict(new { Message = "Tên sản phẩm đã tồn tại trong hệ thống!" });

                // Tạo mới
                await _sanPhamService.Create(sanPham);

                return Ok(new
                {
                    Message = "Thêm sản phẩm thành công!",
                    Success = true,
                    Id = sanPham.IdSanPham
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CREATE ERROR: {ex.Message}");
                return StatusCode(500, new
                {
                    Message = "Lỗi hệ thống khi thêm sản phẩm!",
                    Error = ex.Message
                });
            }
        }

        // ===== EDIT =====
        [HttpPut]
        [Route("Edit/{id:int}")]
        [Consumes("application/json")]
        public async Task<IActionResult> Edit(int id, [FromBody] SanPham sanPham)
        {
            try
            {
                Console.WriteLine($"=== EDIT SAN PHAM ID: {id} ===");
                Console.WriteLine($"Data: {System.Text.Json.JsonSerializer.Serialize(sanPham)}");

                if (sanPham == null)
                    return BadRequest(new { Message = "Dữ liệu sản phẩm không được rỗng!" });

                // Validation
                var errors = new Dictionary<string, string>();
                if (string.IsNullOrWhiteSpace(sanPham.MaSanPham))
                    errors["MaSanPham"] = "Phải nhập mã sản phẩm!";
                if (string.IsNullOrWhiteSpace(sanPham.TenSanPham))
                    errors["TenSanPham"] = "Phải nhập tên sản phẩm!";
                if (sanPham.IdThuongHieu == null || sanPham.IdThuongHieu == 0)
                    errors["IdThuongHieu"] = "Phải chọn thương hiệu!";
                if (sanPham.GiaGoc == null || sanPham.GiaGoc <= 0)
                    errors["GiaGoc"] = "Giá gốc phải lớn hơn 0!";
                if (string.IsNullOrWhiteSpace(sanPham.TrangThaiSP))
                    errors["TrangThaiSP"] = "Phải chọn trạng thái sản phẩm!";

                if (errors.Count > 0)
                    return BadRequest(new { Errors = errors });

                // Validate VAT nếu có
                if (sanPham.VAT.HasValue && (sanPham.VAT < 0 || sanPham.VAT > 100))
                    return BadRequest(new { VAT = "VAT phải từ 0 đến 100%" });

                // Kiểm tra thương hiệu có tồn tại không
                var thuongHieu = await _thuongHieuService.GetThuongHieuById(sanPham.IdThuongHieu ?? 0);
                if (thuongHieu == null)
                    return BadRequest(new { IdThuongHieu = "Thương hiệu không tồn tại!" });

                // Kiểm tra tồn tại
                var existingSanPham = await _sanPhamService.GetSanPhamById(id);
                if (existingSanPham == null)
                    return NotFound(new { Message = $"Không tìm thấy sản phẩm với ID={id}!" });

                // Gán ID và chuẩn hóa
                sanPham.IdSanPham = id;
                sanPham.MaSanPham = sanPham.MaSanPham.Trim();
                sanPham.TenSanPham = sanPham.TenSanPham.Trim();
                sanPham.MoTa = sanPham.MoTa?.Trim();

                // Tính giá niêm yết nếu có VAT
                if (sanPham.VAT.HasValue && sanPham.VAT > 0)
                {
                    sanPham.GiaNiemYet = sanPham.GiaGoc * (1 + sanPham.VAT.Value / 100);
                }
                else
                {
                    sanPham.GiaNiemYet = sanPham.GiaGoc;
                }

                // Kiểm tra trùng mã sản phẩm (trừ chính nó)
                var allSanPhams = await _sanPhamService.GetAllSanPhams();
                bool maExists = allSanPhams.Any(p =>
                    p.IdSanPham != id &&
                    p.MaSanPham.Trim().Equals(sanPham.MaSanPham, StringComparison.OrdinalIgnoreCase)
                );

                if (maExists)
                    return Conflict(new { Message = "Mã sản phẩm đã tồn tại trong hệ thống!" });

                // Kiểm tra trùng tên sản phẩm (trừ chính nó)
                bool tenExists = allSanPhams.Any(p =>
                    p.IdSanPham != id &&
                    p.TenSanPham.Trim().Equals(sanPham.TenSanPham, StringComparison.OrdinalIgnoreCase)
                );

                if (tenExists)
                    return Conflict(new { Message = "Tên sản phẩm đã tồn tại trong hệ thống!" });

                // Cập nhật
                await _sanPhamService.Update(sanPham);

                return Ok(new
                {
                    Message = "Cập nhật sản phẩm thành công!",
                    Success = true,
                    Id = id
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EDIT ERROR ID {id}: {ex.Message}");
                return StatusCode(500, new
                {
                    Message = "Lỗi hệ thống khi cập nhật sản phẩm!",
                    Error = ex.Message
                });
            }
        }

        // ===== DELETE =====
        [HttpDelete]
        [Route("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var existingSanPham = await _sanPhamService.GetSanPhamById(id);
                if (existingSanPham == null)
                    return NotFound(new { Message = $"Không tìm thấy sản phẩm với ID={id}!" });

                await _sanPhamService.Delete(id);

                return Ok(new
                {
                    Message = "Xóa sản phẩm thành công!",
                    Success = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "Lỗi hệ thống khi xóa sản phẩm!",
                    Error = ex.Message
                });
            }
        }

        // ===== GET ALL (API) =====
        [HttpGet]
        [Route("GetAll")]
        [Produces("application/json")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var sanPhams = await _sanPhamService.GetAllSanPhams();
                return Ok(sanPhams);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        // ===== GET BY ID (API) =====
        [HttpGet]
        [Route("GetById/{id:int}")]
        [Produces("application/json")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var sanPham = await _sanPhamService.GetSanPhamById(id);
                if (sanPham == null)
                    return NotFound(new { Message = $"Không tìm thấy sản phẩm với ID={id}" });

                return Ok(sanPham);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        // ===== GET THUONG HIEU =====
        [HttpGet]
        [Route("GetThuongHieu")]
        [Produces("application/json")]
        public async Task<IActionResult> GetThuongHieu()
        {
            try
            {
                Console.WriteLine("🔄 Controller: Đang lấy danh sách thương hiệu...");

                // Sửa tên phương thức để match với Interface
                var thuongHieus = await _thuongHieuService.GetAllThuongHieus();

                Console.WriteLine($"✅ Controller: Tìm thấy {thuongHieus?.Count ?? 0} thương hiệu");

                if (thuongHieus == null || thuongHieus.Count == 0)
                {
                    return Ok(new List<object>());
                }

                // Trả về dữ liệu với format chuẩn
                var result = thuongHieus.Select(th => new
                {
                    IdThuongHieu = th.IdThuongHieu,
                    TenThuongHieu = th.TenThuongHieu,
                    TrangThaiThuongHieu = th.TrangThaiThuongHieu
                }).ToList();

                Console.WriteLine($"✅ Controller: Trả về {result.Count} items");
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Controller Lỗi khi lấy thương hiệu: {ex.Message}");
                Console.WriteLine($"❌ Controller Stack: {ex.StackTrace}");
                return StatusCode(500, new
                {
                    Error = ex.Message,
                    Detail = "Không thể lấy danh sách thương hiệu"
                });
            }
        }

        // ===== GET THUONG HIEU BY ID (API) =====
        [HttpGet]
        [Route("GetThuongHieuById/{id:int}")]
        [Produces("application/json")]
        public async Task<IActionResult> GetThuongHieuById(int id)
        {
            try
            {
                var thuongHieu = await _thuongHieuService.GetThuongHieuById(id);
                if (thuongHieu == null)
                    return NotFound(new { Message = $"Không tìm thấy thương hiệu với ID={id}" });

                return Ok(new
                {
                    IdThuongHieu = thuongHieu.IdThuongHieu,
                    TenThuongHieu = thuongHieu.TenThuongHieu,
                    TrangThaiThuongHieu = thuongHieu.TrangThaiThuongHieu
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }
    }
}