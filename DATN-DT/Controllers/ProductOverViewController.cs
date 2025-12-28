using DATN_DT.Data;
using DATN_DT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace DATN_DT.Controllers
{
    public class ProductOverViewController : Controller
    {
        private readonly MyDbContext _context;
        private static readonly ConcurrentDictionary<int, DateTime> _productUpdateTimestamps = new();

        public ProductOverViewController(MyDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        // GET: ProductOverview/Details/{id}
        [HttpGet]
        [Route("ProductOverview/Details/{id?}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || id == 0)
            {
                ViewBag.ErrorMessage = "Không tìm thấy sản phẩm";
                return View();
            }

            try
            {
                var productDetail = await (from model in _context.ModelSanPhams
                                           join product in _context.SanPhams on model.IdSanPham equals product.IdSanPham
                                           join brand in _context.ThuongHieus on product.IdThuongHieu equals brand.IdThuongHieu
                                           join ram in _context.RAMs on model.IdRAM equals ram.IdRAM into r
                                           from ram in r.DefaultIfEmpty()
                                           join rom in _context.ROMs on model.IdROM equals rom.IdROM into roms
                                           from rom in roms.DefaultIfEmpty()
                                           join pin in _context.Pins on model.IdPin equals pin.IdPin into p
                                           from pin in p.DefaultIfEmpty()
                                           join manHinh in _context.ManHinhs on model.IdManHinh equals manHinh.IdManHinh into mh
                                           from manHinh in mh.DefaultIfEmpty()
                                           join cameraTruoc in _context.CameraTruocs on model.IdCameraTruoc equals cameraTruoc.IdCamTruoc into ct
                                           from cameraTruoc in ct.DefaultIfEmpty()
                                           join cameraSau in _context.CameraSaus on model.IdCameraSau equals cameraSau.IdCameraSau into cs
                                           from cameraSau in cs.DefaultIfEmpty()
                                           let images = _context.AnhSanPhams
                                               .Where(a => a.IdModelSanPham == model.IdModelSanPham)
                                               .ToList()
                                           let imeiCounts = _context.Imeis
                                               .Where(i => i.IdModelSanPham == model.IdModelSanPham)
                                               .GroupBy(i => i.TrangThai)
                                               .Select(g => new ImeiStatusCount
                                               {
                                                   Status = g.Key,
                                                   Count = g.Count()
                                               })
                                               .ToList()
                                           where model.IdModelSanPham == id && model.TrangThai == 1
                                           select new ProductDetailViewModel
                                           {
                                               IdModelSanPham = model.IdModelSanPham,
                                               TenModel = model.TenModel ?? "Không có tên",
                                               TenSanPham = product.TenSanPham ?? "Không có tên",
                                               TenThuongHieu = brand.TenThuongHieu ?? "Không có thương hiệu",
                                               Mau = model.Mau ?? "Không xác định",
                                               GiaBan = model.GiaBanModel ?? 0,

                                               DungLuongROM = rom != null ? rom.DungLuongROM : "Không xác định",
                                               DungLuongRAM = ram != null ? ram.DungLuongRAM : "Không xác định",
                                               DungLuongPin = pin != null ? pin.DungLuongPin : "Không xác định",
                                               CongNgheManHinh = manHinh != null ? manHinh.CongNgheManHinh : "Không xác định",
                                               KichThuocManHinh = manHinh != null ? manHinh.KichThuoc : "Không xác định",
                                               DoPhanGiaiCamTruoc = cameraTruoc != null ? cameraTruoc.DoPhanGiaiCamTruoc : "Không xác định",
                                               DoPhanGiaiCamSau = cameraSau != null ? cameraSau.DoPhanGiaiCamSau : "Không xác định",

                                               AnhSanPhams = images,

                                               ImeiCounts = imeiCounts,
                                               TongSoImei = _context.Imeis.Count(i => i.IdModelSanPham == model.IdModelSanPham)
                                           }).FirstOrDefaultAsync();

                if (productDetail == null)
                {
                    ViewBag.ErrorMessage = "Không tìm thấy sản phẩm với ID: " + id;
                    return View();
                }

                return View(productDetail);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Lỗi khi tải chi tiết sản phẩm: {ex.Message}";
                return View();
            }
        }

        // API để lấy dữ liệu cho DataTable
        [HttpGet]
        public async Task<IActionResult> GetProductOverviewData()
        {
            try
            {
                var productData = await (from model in _context.ModelSanPhams
                                         join product in _context.SanPhams on model.IdSanPham equals product.IdSanPham
                                         join brand in _context.ThuongHieus on product.IdThuongHieu equals brand.IdThuongHieu
                                         let firstImage = _context.AnhSanPhams
                                             .Where(a => a.IdModelSanPham == model.IdModelSanPham)
                                             .OrderBy(a => a.IdAnh)
                                             .FirstOrDefault()
                                         let stockQuantity = _context.TonKhos
                                             .Where(t => t.IdModelSanPham == model.IdModelSanPham)
                                             .Sum(t => t.SoLuong)
                                         where model.TrangThai == 1
                                         select new ProductOverviewViewModel
                                         {
                                             IdModelSanPham = model.IdModelSanPham,
                                             TenModel = model.TenModel ?? "Không có tên",
                                             TenHang = brand.TenThuongHieu ?? "Không có thương hiệu",
                                             SoLuongTonKho = stockQuantity,
                                             AnhSanPham = firstImage != null ? firstImage.DuongDan : "/images/default-product.png",
                                             GiaBan = model.GiaBanModel ?? 0,
                                             Mau = model.Mau ?? "Không xác định",
                                             IdThuongHieu = brand.IdThuongHieu
                                         }).ToListAsync();

                return Ok(productData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi khi lấy dữ liệu: {ex.Message}" });
            }
        }

        // API tìm kiếm và lọc
        [HttpGet]
        public async Task<IActionResult> SearchProducts(string searchTerm, int? brandId, decimal? minPrice, decimal? maxPrice)
        {
            try
            {
                var query = from model in _context.ModelSanPhams
                            join product in _context.SanPhams on model.IdSanPham equals product.IdSanPham
                            join brand in _context.ThuongHieus on product.IdThuongHieu equals brand.IdThuongHieu
                            let firstImage = _context.AnhSanPhams
                                .Where(a => a.IdModelSanPham == model.IdModelSanPham)
                                .OrderBy(a => a.IdAnh)
                                .FirstOrDefault()
                            let stockQuantity = _context.TonKhos
                                .Where(t => t.IdModelSanPham == model.IdModelSanPham)
                                .Sum(t => t.SoLuong)
                            where model.TrangThai == 1
                            select new ProductOverviewViewModel
                            {
                                IdModelSanPham = model.IdModelSanPham,
                                TenModel = model.TenModel ?? "Không có tên",
                                TenHang = brand.TenThuongHieu ?? "Không có thương hiệu",
                                SoLuongTonKho = stockQuantity,
                                AnhSanPham = firstImage != null ? firstImage.DuongDan : "/images/default-product.png",
                                GiaBan = model.GiaBanModel ?? 0,
                                Mau = model.Mau ?? "Không xác định",
                                IdThuongHieu = brand.IdThuongHieu
                            };

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(x =>
                        (x.TenModel != null && x.TenModel.Contains(searchTerm)) ||
                        (x.TenHang != null && x.TenHang.Contains(searchTerm))
                    );
                }

                if (brandId.HasValue)
                {
                    query = query.Where(x => x.IdThuongHieu == brandId.Value);
                }

                if (minPrice.HasValue)
                {
                    query = query.Where(x => x.GiaBan >= minPrice.Value);
                }

                if (maxPrice.HasValue)
                {
                    query = query.Where(x => x.GiaBan <= maxPrice.Value);
                }

                var result = await query.ToListAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi khi tìm kiếm: {ex.Message}" });
            }
        }

        // Lấy danh sách thương hiệu cho dropdown filter
        [HttpGet]
        public async Task<IActionResult> GetBrands()
        {
            try
            {
                var brands = await _context.ThuongHieus
                    .Where(b => !string.IsNullOrEmpty(b.TrangThaiThuongHieu))
                    .Select(b => new { b.IdThuongHieu, b.TenThuongHieu })
                    .ToListAsync();

                if (!brands.Any())
                {
                    brands = await _context.ThuongHieus
                        .Select(b => new { b.IdThuongHieu, b.TenThuongHieu })
                        .ToListAsync();
                }

                return Ok(brands);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi khi lấy danh sách thương hiệu: {ex.Message}" });
            }
        }

        // API để lấy danh sách IMEI theo sản phẩm
        [HttpGet]
        public async Task<IActionResult> GetImeiByProduct(int productId, string? searchTerm = null, string? status = null)
        {
            try
            {
                var query = _context.Imeis
                    .Where(i => i.IdModelSanPham == productId)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(i => i.MaImei != null && i.MaImei.Contains(searchTerm));
                }

                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(i => i.TrangThai == status);
                }

                var imeis = await query
                    .OrderBy(i => i.IdImei)
                    .Select(i => new
                    {
                        i.IdImei,
                        i.MaImei,
                        i.IdModelSanPham,
                        i.MoTa,
                        i.TrangThai
                    })
                    .ToListAsync();

                return Ok(imeis);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi khi lấy danh sách IMEI: {ex.Message}" });
            }
        }

        // API để lấy tổng quan IMEI (cho trang Details)
        [HttpGet]
        public async Task<IActionResult> GetImeiOverview(int productId)
        {
            try
            {
                var totalImei = await _context.Imeis
                    .CountAsync(i => i.IdModelSanPham == productId);

                var statusCounts = await _context.Imeis
                    .Where(i => i.IdModelSanPham == productId)
                    .GroupBy(i => i.TrangThai)
                    .Select(g => new ImeiStatusCount
                    {
                        Status = g.Key,
                        Count = g.Count()
                    })
                    .ToListAsync();

                return Ok(new
                {
                    TotalImei = totalImei,
                    StatusCounts = statusCounts
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi khi lấy tổng quan IMEI: {ex.Message}" });
            }
        }

        // API để lấy số lượng tồn kho
        [HttpGet]
        public async Task<IActionResult> GetProductStock(int productId)
        {
            try
            {
                var stockQuantity = await _context.TonKhos
                    .Where(t => t.IdModelSanPham == productId)
                    .SumAsync(t => t.SoLuong);

                return Ok(new { StockQuantity = stockQuantity });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi khi lấy số lượng tồn kho: {ex.Message}" });
            }
        }

        // API để thông báo cập nhật IMEI (gọi từ trang Details)
        [HttpPost]
        public async Task<IActionResult> NotifyImeiUpdate([FromBody] ImeiUpdateNotification notification)
        {
            try
            {
                if (notification == null || notification.ProductId <= 0)
                {
                    return BadRequest(new { message = "Thông báo không hợp lệ" });
                }

                // Cập nhật tồn kho cho sản phẩm
                await UpdateProductStock(notification.ProductId);

                // Lưu timestamp cập nhật
                _productUpdateTimestamps[notification.ProductId] = DateTime.UtcNow;

                return Ok(new
                {
                    success = true,
                    message = "Đã cập nhật tồn kho thành công",
                    productId = notification.ProductId,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Lỗi khi cập nhật: {ex.Message}" });
            }
        }

        // API để kiểm tra sản phẩm nào cần cập nhật
        [HttpGet]
        public async Task<IActionResult> CheckForProductUpdates(long lastCheckTimestamp = 0)
        {
            try
            {
                var now = DateTime.UtcNow;
                var updates = new List<ProductUpdateInfo>();

                // Kiểm tra các sản phẩm đã được cập nhật sau thời điểm lastCheckTimestamp
                foreach (var kvp in _productUpdateTimestamps)
                {
                    if (kvp.Value.Ticks > lastCheckTimestamp)
                    {
                        updates.Add(new ProductUpdateInfo
                        {
                            ProductId = kvp.Key,
                            LastUpdateTicks = kvp.Value.Ticks
                        });
                    }
                }

                return Ok(new
                {
                    hasUpdates = updates.Any(),
                    updates = updates,
                    currentTimestamp = now.Ticks
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi khi kiểm tra cập nhật: {ex.Message}" });
            }
        }

        // API để lấy thông tin sản phẩm cần cập nhật
        [HttpGet]
        public async Task<IActionResult> GetUpdatedProducts([FromQuery] List<int> productIds)
        {
            try
            {
                if (productIds == null || !productIds.Any())
                {
                    return Ok(new List<ProductOverviewViewModel>());
                }

                var products = await (from model in _context.ModelSanPhams
                                      join product in _context.SanPhams on model.IdSanPham equals product.IdSanPham
                                      join brand in _context.ThuongHieus on product.IdThuongHieu equals brand.IdThuongHieu
                                      let firstImage = _context.AnhSanPhams
                                          .Where(a => a.IdModelSanPham == model.IdModelSanPham)
                                          .OrderBy(a => a.IdAnh)
                                          .FirstOrDefault()
                                      let stockQuantity = _context.TonKhos
                                          .Where(t => t.IdModelSanPham == model.IdModelSanPham)
                                          .Sum(t => t.SoLuong)
                                      where productIds.Contains(model.IdModelSanPham) && model.TrangThai == 1
                                      select new ProductOverviewViewModel
                                      {
                                          IdModelSanPham = model.IdModelSanPham,
                                          TenModel = model.TenModel ?? "Không có tên",
                                          TenHang = brand.TenThuongHieu ?? "Không có thương hiệu",
                                          SoLuongTonKho = stockQuantity,
                                          AnhSanPham = firstImage != null ? firstImage.DuongDan : "/images/default-product.png",
                                          GiaBan = model.GiaBanModel ?? 0,
                                          Mau = model.Mau ?? "Không xác định",
                                          IdThuongHieu = brand.IdThuongHieu
                                      }).ToListAsync();

                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi khi lấy thông tin sản phẩm: {ex.Message}" });
            }
        }

        // Phương thức cập nhật tồn kho (private)
        private async Task UpdateProductStock(int productId)
        {
            try
            {
                // Tính số lượng IMEI còn hàng
                var soLuongConHang = await _context.Imeis
                    .CountAsync(i => i.IdModelSanPham == productId && i.TrangThai == "Còn hàng");

                // Cập nhật tồn kho
                var tonKhos = await _context.TonKhos
                    .Where(t => t.IdModelSanPham == productId)
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
                    // Nếu không có tồn kho, tạo mới
                    var khoMacDinh = await _context.Khos.FirstOrDefaultAsync();
                    if (khoMacDinh != null)
                    {
                        var newTonKho = new TonKho
                        {
                            IdModelSanPham = productId,
                            IdKho = khoMacDinh.IdKho,
                            SoLuong = soLuongConHang
                        };
                        _context.TonKhos.Add(newTonKho);
                        await _context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating stock for product {productId}: {ex.Message}");
                throw;
            }
        }
    }



    public class ProductOverviewViewModel
    {
        public int IdModelSanPham { get; set; }
        public string TenModel { get; set; } = string.Empty;
        public string TenHang { get; set; } = string.Empty;
        public int SoLuongTonKho { get; set; }
        public string AnhSanPham { get; set; } = string.Empty;
        public decimal GiaBan { get; set; }
        public string Mau { get; set; } = string.Empty;
        public int IdThuongHieu { get; set; }
    }

    public class ProductDetailViewModel
    {
        public int IdModelSanPham { get; set; }
        public string TenModel { get; set; } = string.Empty;
        public string TenSanPham { get; set; } = string.Empty;
        public string TenThuongHieu { get; set; } = string.Empty;
        public string Mau { get; set; } = string.Empty;
        public decimal GiaBan { get; set; }
        public string DungLuongROM { get; set; } = string.Empty;
        public string DungLuongRAM { get; set; } = string.Empty;
        public string DungLuongPin { get; set; } = string.Empty;
        public string CongNgheManHinh { get; set; } = string.Empty;
        public string KichThuocManHinh { get; set; } = string.Empty;
        public string DoPhanGiaiCamTruoc { get; set; } = string.Empty;
        public string DoPhanGiaiCamSau { get; set; } = string.Empty;
        public List<AnhSanPham> AnhSanPhams { get; set; } = new List<AnhSanPham>();
        public List<ImeiStatusCount> ImeiCounts { get; set; } = new List<ImeiStatusCount>();
        public int TongSoImei { get; set; }
    }

    public class ImeiStatusCount
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    // Class cho thông báo cập nhật IMEI
    public class ImeiUpdateNotification
    {
        public int ProductId { get; set; }
        public string Action { get; set; } = "update";
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    // Class cho thông tin cập nhật sản phẩm
    public class ProductUpdateInfo
    {
        public int ProductId { get; set; }
        public long LastUpdateTicks { get; set; }
    }
}