using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using DATN_DT.Data;
using DATN_DT.Models;
using System.Collections.Generic;

namespace DATN_DT.Controllers
{
    public class MuaHangController : Controller
    {
        private readonly MyDbContext _context;

        public MuaHangController(MyDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Lấy danh sách thương hiệu từ database
                var brands = await _context.ThuongHieus
                    .Where(t => t.TrangThaiThuongHieu == "Còn hoạt động")
                    .OrderBy(t => t.TenThuongHieu)
                    .ToListAsync();

                // Truyền thương hiệu vào View
                ViewBag.Brands = brands;

                // Lấy danh sách ModelSanPham từ database 
                var products = await GetAllModelSanPhamFromDatabase();
                ViewBag.Products = products;

                // Lấy danh sách ModelSanPham hot (dựa trên IMEI đã bán)
                var hotProducts = await GetHotModelSanPhamBasedOnSoldImei();
                ViewBag.HotProducts = hotProducts;

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Brands = new List<ThuongHieu>();
                ViewBag.Products = new List<dynamic>();
                ViewBag.HotProducts = new List<dynamic>();
                return View();
            }
        }

        // THÊM MỚI: Action xem chi tiết sản phẩm
        [HttpGet]
        [Route("MuaHang/Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                // Lấy thông tin chi tiết sản phẩm
                var productDetail = await GetProductDetail(id);

                if (productDetail == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy sản phẩm";
                    return RedirectToAction("Index");
                }

                // Lấy các sản phẩm cùng thương hiệu (gợi ý)
                var suggestedProducts = await GetSuggestedProducts(productDetail.IdThuongHieu ?? 0, id);

                // Sử dụng dynamic object để truyền dữ liệu
                ViewBag.SuggestedProducts = suggestedProducts;

                // Truyền dữ liệu qua ViewBag hoặc ViewData
                ViewBag.ProductDetail = productDetail;

                // Hoặc sử dụng ViewData
                ViewData["ProductDetail"] = productDetail;

                return View(productDetail);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi tải chi tiết sản phẩm: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // THÊM MỚI: API tìm kiếm sản phẩm
        [HttpGet]
        public async Task<IActionResult> SearchModelSanPham(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return Json(new List<object>());
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
                                          .Sum(t => t.SoLuong) ?? 0
                                      let soldCount = _context.Imeis
                                          .Count(i => i.IdModelSanPham == model.IdModelSanPham &&
                                                     i.TrangThai == "Đã bán")
                                      where model.TrangThai == 1 && stockQuantity > 0
                                      where model.TenModel != null && model.TenModel.Contains(searchTerm) ||
                                            product.TenSanPham != null && product.TenSanPham.Contains(searchTerm) ||
                                            brand.TenThuongHieu != null && brand.TenThuongHieu.Contains(searchTerm)
                                      select new
                                      {
                                          IdModelSanPham = model.IdModelSanPham,
                                          TenModel = model.TenModel ?? product.TenSanPham ?? "Không có tên",
                                          TenSanPham = product.TenSanPham ?? "Không có tên",
                                          TenThuongHieu = brand.TenThuongHieu ?? "Không có thương hiệu",
                                          GiaBan = model.GiaBanModel ?? 0,
                                          HinhAnh = firstImage != null ? firstImage.DuongDan : "/ImgResource/default-product.jpg",
                                          SoLuongTon = stockQuantity,
                                          Mau = model.Mau ?? "Không xác định",
                                          IsHot = soldCount > 3,
                                          SoldCount = soldCount,
                                          IdThuongHieu = brand.IdThuongHieu
                                      })
                                 .Take(20) // Giới hạn 20 kết quả
                                 .ToListAsync();

                return Json(products);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // THÊM MỚI: Lấy thông tin chi tiết sản phẩm - DÙNG DYNAMIC OBJECT
        private async Task<dynamic> GetProductDetail(int productId)
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
                                       let stockQuantity = _context.TonKhos
                                           .Where(t => t.IdModelSanPham == model.IdModelSanPham)
                                           .Sum(t => t.SoLuong) ?? 0
                                       let soldCount = _context.Imeis
                                           .Count(i => i.IdModelSanPham == model.IdModelSanPham &&
                                                      i.TrangThai == "Đã bán")
                                       let availableCount = _context.Imeis
                                           .Count(i => i.IdModelSanPham == model.IdModelSanPham &&
                                                      i.TrangThai == "Còn hàng")
                                       where model.IdModelSanPham == productId && model.TrangThai == 1
                                       select new
                                       {
                                           IdModelSanPham = model.IdModelSanPham,
                                           TenModel = model.TenModel ?? product.TenSanPham ?? "Không có tên",
                                           TenSanPham = product.TenSanPham ?? "Không có tên",
                                           TenThuongHieu = brand.TenThuongHieu ?? "Không có thương hiệu",
                                           Mau = model.Mau ?? "Không xác định",
                                           GiaBan = model.GiaBanModel ?? 0,
                                           GiaGoc = product.GiaGoc ?? 0,
                                           VAT = product.VAT ?? 0,

                                           // Thông số kỹ thuật
                                           DungLuongROM = rom != null ? rom.DungLuongROM : "Không xác định",
                                           DungLuongRAM = ram != null ? ram.DungLuongRAM : "Không xác định",
                                           DungLuongPin = pin != null ? pin.DungLuongPin : "Không xác định",
                                           CongNgheManHinh = manHinh != null ? manHinh.CongNgheManHinh : "Không xác định",
                                           KichThuocManHinh = manHinh != null ? manHinh.KichThuoc : "Không xác định",
                                           DoPhanGiaiCamTruoc = cameraTruoc != null ? cameraTruoc.DoPhanGiaiCamTruoc : "Không xác định",
                                           DoPhanGiaiCamSau = cameraSau != null ? cameraSau.DoPhanGiaiCamSau : "Không xác định",

                                           // Thông tin bổ sung
                                           AnhSanPhams = images,
                                           SoLuongTon = stockQuantity,
                                           SoLuongDaBan = soldCount,
                                           SoLuongConLai = availableCount,
                                           IdThuongHieu = brand.IdThuongHieu,
                                           IsHot = soldCount > 3,

                                           // Mô tả chi tiết
                                           MoTaSanPham = product.MoTa ?? "Không có mô tả",
                                           LoaiPin = pin != null ? pin.LoaiPin : "Không xác định",
                                           CongNgheSac = pin != null ? pin.CongNgheSac : "Không xác định",
                                           TinhNangManHinh = manHinh != null ? manHinh.TinhNangMan : "Không có",
                                           SoLuongCameraSau = cameraSau != null ? cameraSau.SoLuongOngKinh : "Không xác định"
                                       }).FirstOrDefaultAsync();

            return productDetail;
        }

        // THÊM MỚI: Lấy sản phẩm gợi ý (cùng thương hiệu) - DÙNG DYNAMIC OBJECT
        private async Task<List<dynamic>> GetSuggestedProducts(int brandId, int excludeProductId)
        {
            var suggestedProducts = await (from model in _context.ModelSanPhams
                                           join product in _context.SanPhams on model.IdSanPham equals product.IdSanPham
                                           join brand in _context.ThuongHieus on product.IdThuongHieu equals brand.IdThuongHieu
                                           let firstImage = _context.AnhSanPhams
                                               .Where(a => a.IdModelSanPham == model.IdModelSanPham)
                                               .OrderBy(a => a.IdAnh)
                                               .FirstOrDefault()
                                           let stockQuantity = _context.TonKhos
                                               .Where(t => t.IdModelSanPham == model.IdModelSanPham)
                                               .Sum(t => t.SoLuong) ?? 0
                                           where model.TrangThai == 1 &&
                                                 stockQuantity > 0 &&
                                                 brand.IdThuongHieu == brandId &&
                                                 model.IdModelSanPham != excludeProductId
                                           orderby Guid.NewGuid() // Random order
                                           select new
                                           {
                                               IdModelSanPham = model.IdModelSanPham,
                                               TenModel = model.TenModel ?? product.TenSanPham ?? "Không có tên",
                                               TenSanPham = product.TenSanPham ?? "Không có tên",
                                               TenThuongHieu = brand.TenThuongHieu ?? "Không có thương hiệu",
                                               GiaBan = model.GiaBanModel ?? 0,
                                               AnhSanPham = firstImage != null ? firstImage.DuongDan : "/ImgResource/default-product.jpg",
                                               SoLuongTon = stockQuantity,
                                               Mau = model.Mau ?? "Không xác định",
                                               IdThuongHieu = brand.IdThuongHieu
                                           })
                                         .Take(4) // Lấy 4 sản phẩm gợi ý
                                         .ToListAsync();

            return suggestedProducts.Cast<dynamic>().ToList();
        }

        // Phương thức lấy TẤT CẢ ModelSanPham từ database (sản phẩm thịnh hành)
        private async Task<List<dynamic>> GetAllModelSanPhamFromDatabase()
        {
            var products = await (from model in _context.ModelSanPhams
                                  join product in _context.SanPhams on model.IdSanPham equals product.IdSanPham
                                  join brand in _context.ThuongHieus on product.IdThuongHieu equals brand.IdThuongHieu
                                  let firstImage = _context.AnhSanPhams
                                      .Where(a => a.IdModelSanPham == model.IdModelSanPham)
                                      .OrderBy(a => a.IdAnh)
                                      .FirstOrDefault()
                                  let stockQuantity = _context.TonKhos
                                      .Where(t => t.IdModelSanPham == model.IdModelSanPham)
                                      .Sum(t => t.SoLuong) ?? 0
                                  where model.TrangThai == 1 && stockQuantity > 0
                                  select new
                                  {
                                      IdModelSanPham = model.IdModelSanPham,
                                      TenModel = model.TenModel ?? "Không có tên",
                                      TenSanPham = product.TenSanPham ?? "Không có tên",
                                      TenThuongHieu = brand.TenThuongHieu ?? "Không có thương hiệu",
                                      GiaBan = model.GiaBanModel ?? 0,
                                      HinhAnh = firstImage != null ? firstImage.DuongDan : "/ImgResource/default-product.jpg",
                                      SoLuongTon = stockQuantity,
                                      Mau = model.Mau ?? "Không xác định",
                                      // Xác định sản phẩm hot (nếu đã bán hơn 3 sản phẩm)
                                      SoldCount = _context.Imeis
                                          .Count(i => i.IdModelSanPham == model.IdModelSanPham &&
                                                     i.TrangThai == "Đã bán"),
                                      IdThuongHieu = brand.IdThuongHieu
                                  })
                                 .ToListAsync();

            // Xác định sản phẩm hot: nếu đã bán hơn 3 sản phẩm
            var productsWithHot = products.Select(p => new
            {
                p.IdModelSanPham,
                p.TenModel,
                p.TenSanPham,
                p.TenThuongHieu,
                p.GiaBan,
                p.HinhAnh,
                p.SoLuongTon,
                p.Mau,
                p.SoldCount,
                IsHot = p.SoldCount > 3, // Nếu đã bán hơn 3 sản phẩm thì là HOT
                p.IdThuongHieu
            }).ToList();

            return productsWithHot.Cast<dynamic>().ToList();
        }

        // Phương thức lấy ModelSanPham hot nhất (6 sản phẩm bán chạy nhất)
        private async Task<List<dynamic>> GetHotModelSanPhamBasedOnSoldImei()
        {
            var hotProducts = await (from model in _context.ModelSanPhams
                                     join product in _context.SanPhams on model.IdSanPham equals product.IdSanPham
                                     join brand in _context.ThuongHieus on product.IdThuongHieu equals brand.IdThuongHieu
                                     let soldCount = _context.Imeis
                                         .Count(i => i.IdModelSanPham == model.IdModelSanPham &&
                                                    i.TrangThai == "Đã bán")
                                     let stockQuantity = _context.TonKhos
                                         .Where(t => t.IdModelSanPham == model.IdModelSanPham)
                                         .Sum(t => t.SoLuong) ?? 0
                                     where model.TrangThai == 1 && stockQuantity > 0 && soldCount > 3
                                     orderby soldCount descending
                                     select new
                                     {
                                         IdModelSanPham = model.IdModelSanPham,
                                         TenModel = model.TenModel ?? product.TenSanPham ?? "Không có tên",
                                         TenThuongHieu = brand.TenThuongHieu ?? "Không có thương hiệu",
                                         GiaBan = model.GiaBanModel ?? 0,
                                         SoldCount = soldCount,
                                         IsHot = true // Đây là sản phẩm hot
                                     })
                                    .Take(6) // Lấy 6 sản phẩm hot nhất (bán chạy nhất)
                                    .ToListAsync();

            return hotProducts.Cast<dynamic>().ToList();
        }

        // THÊM MỚI: API lấy tất cả sản phẩm (cho bộ lọc)
        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            try
            {
                var products = await GetAllModelSanPhamFromDatabase();
                return Json(products);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // THÊM MỚI: API thêm vào giỏ hàng
        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] CartItemRequest request)
        {
            try
            {
                if (request == null || request.ProductId <= 0 || request.Quantity <= 0)
                {
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
                }

                // Kiểm tra sản phẩm tồn tại
                var product = await _context.ModelSanPhams
                    .Include(m => m.SanPham)
                    .FirstOrDefaultAsync(m => m.IdModelSanPham == request.ProductId && m.TrangThai == 1);

                if (product == null)
                {
                    return Json(new { success = false, message = "Sản phẩm không tồn tại" });
                }

                // Kiểm tra tồn kho
                var stockQuantity = await _context.TonKhos
                    .Where(t => t.IdModelSanPham == request.ProductId)
                    .SumAsync(t => t.SoLuong) ?? 0;

                if (stockQuantity < request.Quantity)
                {
                    return Json(new { success = false, message = $"Chỉ còn {stockQuantity} sản phẩm trong kho" });
                }

                return Json(new
                {
                    success = true,
                    message = "Đã thêm vào giỏ hàng thành công",
                    productName = product.TenModel ?? product.SanPham?.TenSanPham ?? "Sản phẩm",
                    price = product.GiaBanModel ?? 0,
                    quantity = request.Quantity
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }
    }


    public class CartItemRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}