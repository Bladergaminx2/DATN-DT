using DATN_DT.IServices;
using DATN_DT.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace DATN_DT.Controllers
{
<<<<<<< HEAD
    [Route("api/[controller]")]
=======
    [Route("[controller]")]
>>>>>>> origin/Update-SP/Quanly/MuaHang
    [ApiController]
    public class TonKhoController : Controller
    {
        private readonly ITonKhoService _tonKhoService;

        public TonKhoController(ITonKhoService tonKhoService, IHttpClientFactory httpClientFactory)
        public TonKhoController(ITonKhoService tonKhoService)
        {
            _tonKhoService = tonKhoService;
        }

        // ===== GET: TonKho/Index =====
        [HttpGet]
        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var tonKhos = await _tonKhoService.GetAllTonKhos();
                return View(tonKhos);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading ton kho: {ex.Message}");
                return View(new List<TonKho>());
            }
        }

        // ===== CREATE =====
        [HttpPost]
        [Route("Create")]
        [Consumes("application/json")]
        public async Task<IActionResult> Create([FromBody] TonKho tonKho)
        {
            try
            {
                Console.WriteLine("=== CREATE TON KHO ===");
                Console.WriteLine($"Received: {System.Text.Json.JsonSerializer.Serialize(tonKho)}");

                if (tonKho == null)
                {
                    return BadRequest(new { Message = "Dữ liệu tồn kho không được rỗng!" });
                }

                // Validation
                var errors = new Dictionary<string, string>();

                if (tonKho.IdModelSanPham == null || tonKho.IdModelSanPham == 0)
                    errors["IdModelSanPham"] = "Phải chọn model sản phẩm!";

                if (tonKho.IdKho == null || tonKho.IdKho == 0)
                    errors["IdKho"] = "Phải chọn kho!";

                if (errors.Count > 0)
                    return BadRequest(new { Errors = errors });

                // Lấy danh sách kho và model từ TonKhoService
                var allKho = await _tonKhoService.GetAllKho();
                var allModel = await _tonKhoService.GetAllModelSanPham();

                // Kiểm tra kho có tồn tại không
                var khoExists = allKho.Any(k => k.IdKho == tonKho.IdKho);
                if (!khoExists)
                    return BadRequest(new { IdKho = "Kho không tồn tại!" });

                // Kiểm tra model sản phẩm có tồn tại không
                var modelExists = allModel.Any(m => m.IdModelSanPham == tonKho.IdModelSanPham);
                if (!modelExists)
                    return BadRequest(new { IdModelSanPham = "Model sản phẩm không tồn tại!" });

                // Kiểm tra model sản phẩm đã có trong kho khác chưa
                var modelInAnyKho = await _tonKhoService.IsModelSanPhamInAnyKho(tonKho.IdModelSanPham.Value);
                if (modelInAnyKho)
                {
                    var existingTonKho = await _tonKhoService.GetTonKhoByModelSanPham(tonKho.IdModelSanPham.Value);
                    var khoName = existingTonKho?.Kho?.TenKho ?? $"ID: {existingTonKho?.IdKho}";
                    return Conflict(new
                    {
                        Message = $"Model sản phẩm đã được lưu giữ ở kho '{khoName}'. Một model chỉ có thể ở một kho."
                    });
                }

                try
                {
                    // Tạo mới - sẽ tự động tính số lượng từ IMEI
                    await _tonKhoService.Create(tonKho);

                    return Ok(new
                    {
                        Message = "Thêm tồn kho thành công!",
                        Success = true,
                        Id = tonKho.IdTonKho,
                        SoLuong = tonKho.SoLuong // Số lượng tự động tính từ IMEI
                    });
                }
                catch (InvalidOperationException ex)
                {
                    // Bắt lỗi từ Repository về model đã có trong kho khác
                    return Conflict(new { Message = ex.Message });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CREATE ERROR: {ex.Message}");
                return StatusCode(500, new
                {
                    Message = "Lỗi hệ thống khi thêm tồn kho!",
                    Error = ex.Message
                });
            }
        }

        // ===== EDIT =====
        [HttpPut]
        [Route("Edit/{id:int}")]
        [Consumes("application/json")]
        public async Task<IActionResult> Edit(int id, [FromBody] TonKho tonKho)
        {
            try
            {
                Console.WriteLine($"=== EDIT TON KHO ID: {id} ===");
                Console.WriteLine($"Data: {System.Text.Json.JsonSerializer.Serialize(tonKho)}");

                if (tonKho == null)
                    return BadRequest(new { Message = "Dữ liệu tồn kho không được rỗng!" });

                // Validation
                var errors = new Dictionary<string, string>();

                if (tonKho.IdModelSanPham == null || tonKho.IdModelSanPham == 0)
                    errors["IdModelSanPham"] = "Phải chọn model sản phẩm!";

                if (tonKho.IdKho == null || tonKho.IdKho == 0)
                    errors["IdKho"] = "Phải chọn kho!";

                if (errors.Count > 0)
                    return BadRequest(new { Errors = errors });

                // Gán ID
                tonKho.IdTonKho = id;

                // Lấy danh sách kho và model từ TonKhoService
                var allKho = await _tonKhoService.GetAllKho();
                var allModel = await _tonKhoService.GetAllModelSanPham();

                // Kiểm tra kho có tồn tại không
                var khoExists = allKho.Any(k => k.IdKho == tonKho.IdKho);
                if (!khoExists)
                    return BadRequest(new { IdKho = "Kho không tồn tại!" });

                // Kiểm tra model sản phẩm có tồn tại không
                var modelExists = allModel.Any(m => m.IdModelSanPham == tonKho.IdModelSanPham);
                if (!modelExists)
                    return BadRequest(new { IdModelSanPham = "Model sản phẩm không tồn tại!" });

                // Kiểm tra tồn tại
                var existingTonKho = await _tonKhoService.GetTonKhoById(id);
                if (existingTonKho == null)
                    return NotFound(new { Message = $"Không tìm thấy tồn kho với ID={id}!" });

                try
                {
                    // Cập nhật - sẽ tự động tính số lượng từ IMEI
                    await _tonKhoService.Update(tonKho);

                    return Ok(new
                    {
                        Message = "Cập nhật tồn kho thành công!",
                        Success = true,
                        Id = id,
                        SoLuong = tonKho.SoLuong // Số lượng tự động tính từ IMEI
                    });
                }
                catch (InvalidOperationException ex)
                {
                    // Bắt lỗi từ Repository về model đã có trong kho khác
                    return Conflict(new { Message = ex.Message });
                }
                catch (KeyNotFoundException ex)
                {
                    return NotFound(new { Message = ex.Message });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EDIT ERROR ID {id}: {ex.Message}");
                return StatusCode(500, new
                {
                    Message = "Lỗi hệ thống khi cập nhật tồn kho!",
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
                var existingTonKho = await _tonKhoService.GetTonKhoById(id);
                if (existingTonKho == null)
                    return NotFound(new { Message = $"Không tìm thấy tồn kho với ID={id}!" });

                await _tonKhoService.Delete(id);

                return Ok(new
                {
                    Message = "Xóa tồn kho thành công!",
                    Success = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "Lỗi hệ thống khi xóa tồn kho!",
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
                var tonKhos = await _tonKhoService.GetAllTonKhos();

                // Format dữ liệu trả về - sửa theo model thực tế
                var result = tonKhos.Select(tk => new
                {
                    tk.IdTonKho,
                    tk.IdModelSanPham,
                    tk.IdKho,
                    tk.SoLuong,
                    ModelSanPham = tk.ModelSanPham != null ? new
                    {
                        tk.ModelSanPham.IdModelSanPham,
                        tk.ModelSanPham.TenModel,
                        tk.ModelSanPham.Mau,
                        tk.ModelSanPham.GiaBanModel,
                        tk.ModelSanPham.TrangThai,
                        SanPham = tk.ModelSanPham.SanPham != null ? new
                        {
                            tk.ModelSanPham.SanPham.IdSanPham,
                            tk.ModelSanPham.SanPham.TenSanPham,
                            tk.ModelSanPham.SanPham.MaSanPham
                        } : null
                    } : null,
                    Kho = tk.Kho != null ? new
                    {
                        tk.Kho.IdKho,
                        tk.Kho.TenKho
                    } : null,
                    SoLuongImeiConHang = tk.ModelSanPham?.Imeis?.Count(i => i.TrangThai == "Còn hàng") ?? 0
                }).ToList();

                return Ok(result);
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
                var tonKho = await _tonKhoService.GetTonKhoById(id);
                if (tonKho == null)
                    return NotFound(new { Message = $"Không tìm thấy tồn kho với ID={id}" });

                return Ok(new
                {
                    tonKho.IdTonKho,
                    tonKho.IdModelSanPham,
                    tonKho.IdKho,
                    tonKho.SoLuong,
                    ModelSanPham = tonKho.ModelSanPham != null ? new
                    {
                        tonKho.ModelSanPham.IdModelSanPham,
                        tonKho.ModelSanPham.TenModel,
                        tonKho.ModelSanPham.Mau,
                        tonKho.ModelSanPham.GiaBanModel,
                        SanPham = tonKho.ModelSanPham.SanPham != null ? new
                        {
                            tonKho.ModelSanPham.SanPham.IdSanPham,
                            tonKho.ModelSanPham.SanPham.TenSanPham
                        } : null
                    } : null,
                    Kho = tonKho.Kho != null ? new
                    {
                        tonKho.Kho.IdKho,
                        tonKho.Kho.TenKho
                    } : null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        // ===== GET KHO =====
        [HttpGet]
        [Route("GetKho")]
        [Produces("application/json")]
        public async Task<IActionResult> GetKho()
        {
            try
            {
                Console.WriteLine("🔄 Controller: Đang lấy danh sách kho...");

                var khoList = await _tonKhoService.GetAllKho();

                Console.WriteLine($"✅ Controller: Tìm thấy {khoList?.Count ?? 0} kho");

                if (khoList == null || khoList.Count == 0)
                {
                    return Ok(new List<object>());
                }

                // Trả về dữ liệu với format chuẩn - sửa theo model thực tế
                var result = khoList.Select(k => new
                {
                    IdKho = k.IdKho,
                    TenKho = k.TenKho
                }).ToList();

                Console.WriteLine($"✅ Controller: Trả về {result.Count} items");
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Controller Lỗi khi lấy kho: {ex.Message}");
                Console.WriteLine($"❌ Controller Stack: {ex.StackTrace}");
                return StatusCode(500, new
                {
                    Error = ex.Message,
                    Detail = "Không thể lấy danh sách kho"
                });
            }
        }

        // ===== GET MODEL SAN PHAM =====
        [HttpGet]
        [Route("GetModelSanPham")]
        [Produces("application/json")]
        public async Task<IActionResult> GetModelSanPham()
        {
            try
            {
                Console.WriteLine("🔄 Controller: Đang lấy danh sách model sản phẩm...");

                var modelList = await _tonKhoService.GetAllModelSanPham();

                Console.WriteLine($"✅ Controller: Tìm thấy {modelList?.Count ?? 0} model");

                if (modelList == null || modelList.Count == 0)
                {
                    return Ok(new List<object>());
                }

                // Tạo danh sách kết quả
                var result = new List<object>();

                foreach (var m in modelList)
                {
                    // Kiểm tra model đã có trong kho chưa
                    var isInKho = await _tonKhoService.IsModelSanPhamInAnyKho(m.IdModelSanPham);

                    result.Add(new
                    {
                        IdModelSanPham = m.IdModelSanPham,
                        TenModel = m.TenModel,
                        Mau = m.Mau,
                        GiaBanModel = m.GiaBanModel,
                        TrangThai = m.TrangThai,
                        SanPham = m.SanPham != null ? new
                        {
                            m.SanPham.IdSanPham,
                            m.SanPham.TenSanPham,
                            m.SanPham.MaSanPham
                        } : null,
                        SoLuongImeiConHang = m.Imeis?.Count(i => i.TrangThai == "Còn hàng") ?? 0,
                        IsInKho = isInKho
                    });
                }

                Console.WriteLine($"✅ Controller: Trả về {result.Count} items");
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Controller Lỗi khi lấy model sản phẩm: {ex.Message}");
                Console.WriteLine($"❌ Controller Stack: {ex.StackTrace}");
                return StatusCode(500, new
                {
                    Error = ex.Message,
                    Detail = "Không thể lấy danh sách model sản phẩm"
                });
            }
        }

        // ===== GET SO LUONG IMEI CON HANG =====
        [HttpGet]
        [Route("GetSoLuongImeiConHang/{idModelSanPham:int}")]
        [Produces("application/json")]
        public async Task<IActionResult> GetSoLuongImeiConHang(int idModelSanPham)
        {
            try
            {
                var soLuong = await _tonKhoService.GetSoLuongImeiConHang(idModelSanPham);

                return Ok(new
                {
                    IdModelSanPham = idModelSanPham,
                    SoLuongImeiConHang = soLuong
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        // ===== REFRESH TON KHO CHO MODEL =====
        [HttpPost]
        [Route("RefreshTonKho/{idModelSanPham:int}")]
        public async Task<IActionResult> RefreshTonKho(int idModelSanPham)
        {
            try
            {
                await _tonKhoService.RefreshTonKhoForModel(idModelSanPham);

                return Ok(new
                {
                    Message = "Refresh tồn kho thành công!",
                    Success = true,
                    IdModelSanPham = idModelSanPham
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "Lỗi khi refresh tồn kho!",
                    Error = ex.Message
                });
            }
        }

        // ===== CHECK MODEL IN KHO =====
        [HttpGet]
        [Route("CheckModelInKho/{idModelSanPham:int}")]
        [Produces("application/json")]
        public async Task<IActionResult> CheckModelInKho(int idModelSanPham)
        {
            try
            {
                var isInKho = await _tonKhoService.IsModelSanPhamInAnyKho(idModelSanPham);
                var tonKho = await _tonKhoService.GetTonKhoByModelSanPham(idModelSanPham);

                return Ok(new
                {
                    IdModelSanPham = idModelSanPham,
                    IsInKho = isInKho,
                    TonKhoInfo = tonKho != null ? new
                    {
                        tonKho.IdTonKho,
                        tonKho.IdKho,
                        KhoName = tonKho.Kho?.TenKho,
                        tonKho.SoLuong
                    } : null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        // ===== GET TON KHO BY MODEL AND KHO =====
        [HttpGet]
        [Route("GetTonKhoByModelAndKho/{idModelSanPham:int}/{idKho:int}")]
        [Produces("application/json")]
        public async Task<IActionResult> GetTonKhoByModelAndKho(int idModelSanPham, int idKho)
        {
            try
            {
                var tonKho = await _tonKhoService.GetTonKhoByModelAndKho(idModelSanPham, idKho);

                if (tonKho == null)
                {
                    return Ok(new
                    {
                        Exists = false,
                        Message = "Không tìm thấy tồn kho cho model và kho này"
                    });
                }

                return Ok(new
                {
                    Exists = true,
                    IdTonKho = tonKho.IdTonKho,
                    SoLuong = tonKho.SoLuong,
                    KhoName = tonKho.Kho?.TenKho
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }
    }
}