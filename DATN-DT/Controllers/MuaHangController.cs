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
                                          .Sum(t => t.SoLuong)
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
                                           .Sum(t => t.SoLuong)
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
                                               .Sum(t => t.SoLuong)
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
            var now = DateTime.Now;
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
                                  // Lấy khuyến mãi đang active cho sản phẩm (bỏ kiểm tra ngày kết thúc để cho phép khuyến mãi hết hạn vẫn hoạt động)
                                  let activePromotion = (from mskm in _context.ModelSanPhamKhuyenMais
                                                        join km in _context.KhuyenMais on mskm.IdKhuyenMai equals km.IdKhuyenMai
                                                        where mskm.IdModelSanPham == model.IdModelSanPham
                                                              && (km.TrangThaiKM == "Đang diễn ra" || km.TrangThaiKM == "Đã kết thúc")
                                                              && km.NgayBatDau.HasValue
                                                              && km.NgayBatDau.Value <= now
                                                        orderby km.NgayKetThuc descending
                                                        select km).FirstOrDefault()
                                  where model.TrangThai == 1 && stockQuantity > 0
                                  select new
                                  {
                                      IdModelSanPham = model.IdModelSanPham,
                                      TenModel = model.TenModel ?? "Không có tên",
                                      TenSanPham = product.TenSanPham ?? "Không có tên",
                                      TenThuongHieu = brand.TenThuongHieu ?? "Không có thương hiệu",
                                      GiaBan = model.GiaBanModel ?? 0,
                                      GiaGoc = product.GiaGoc ?? model.GiaBanModel ?? 0,
                                      HinhAnh = firstImage != null ? firstImage.DuongDan : "/ImgResource/default-product.jpg",
                                      SoLuongTon = stockQuantity,
                                      Mau = model.Mau ?? "Không xác định",
                                      // Xác định sản phẩm hot (nếu đã bán hơn 3 sản phẩm)
                                      SoldCount = _context.Imeis
                                          .Count(i => i.IdModelSanPham == model.IdModelSanPham &&
                                                     i.TrangThai == "Đã bán"),
                                      IdThuongHieu = brand.IdThuongHieu,
                                      // Thông tin khuyến mãi
                                      KhuyenMai = activePromotion != null ? new
                                      {
                                          IdKhuyenMai = activePromotion.IdKhuyenMai,
                                          LoaiGiam = activePromotion.LoaiGiam,
                                          GiaTri = activePromotion.GiaTri ?? 0
                                      } : null
                                  })
                                 .ToListAsync();

            // Tính giá sau khuyến mãi và phần trăm giảm
            var productsWithPromotion = products.Select(p =>
            {
                decimal giaGoc = p.GiaGoc > 0 ? p.GiaGoc : p.GiaBan;
                decimal giaSauGiam = p.GiaBan;
                decimal phanTramGiam = 0;
                bool hasPromotion = p.KhuyenMai != null;

                if (hasPromotion && p.KhuyenMai.GiaTri > 0)
                {
                    // SỬA LỖI: Sử dụng đúng giá trị từ database ("Phần trăm" và "Số tiền")
                    if (p.KhuyenMai.LoaiGiam == "Phần trăm")
                    {
                        // Giảm theo phần trăm
                        phanTramGiam = (decimal)p.KhuyenMai.GiaTri;
                        giaSauGiam = giaGoc * (1 - phanTramGiam / 100);
                    }
                    else if (p.KhuyenMai.LoaiGiam == "Số tiền")
                    {
                        // Giảm theo số tiền
                        decimal soTienGiam = (decimal)p.KhuyenMai.GiaTri;
                        giaSauGiam = giaGoc - soTienGiam;
                        if (giaSauGiam < 0) giaSauGiam = 0;
                        // Tính phần trăm giảm để hiển thị
                        if (giaGoc > 0)
                        {
                            phanTramGiam = (soTienGiam / giaGoc) * 100;
                        }
                    }
                }

                return new
                {
                    p.IdModelSanPham,
                    p.TenModel,
                    p.TenSanPham,
                    p.TenThuongHieu,
                    GiaBan = giaSauGiam, // Giá sau khuyến mãi (hoặc giá gốc nếu không có khuyến mãi)
                    GiaGoc = hasPromotion ? (decimal?)null : giaGoc, // Chỉ trả về giá gốc nếu không có khuyến mãi
                    p.HinhAnh,
                    p.SoLuongTon,
                    p.Mau,
                    p.SoldCount,
                    IsHot = p.SoldCount > 3,
                    p.IdThuongHieu,
                    HasPromotion = hasPromotion,
                    PhanTramGiam = Math.Round(phanTramGiam, 0),
                    SoTienGiam = hasPromotion ? (giaGoc - giaSauGiam) : 0
                };
            }).ToList();

            return productsWithPromotion.Cast<dynamic>().ToList();
        }

        // Phương thức lấy ModelSanPham hot nhất (6 sản phẩm bán chạy nhất)
        private async Task<List<dynamic>> GetHotModelSanPhamBasedOnSoldImei()
        {
            var now = DateTime.Now;
            var hotProducts = await (from model in _context.ModelSanPhams
                                     join product in _context.SanPhams on model.IdSanPham equals product.IdSanPham
                                     join brand in _context.ThuongHieus on product.IdThuongHieu equals brand.IdThuongHieu
                                     let soldCount = _context.Imeis
                                         .Count(i => i.IdModelSanPham == model.IdModelSanPham &&
                                                    i.TrangThai == "Đã bán")
                                     let stockQuantity = _context.TonKhos
                                         .Where(t => t.IdModelSanPham == model.IdModelSanPham)
                                         .Sum(t => t.SoLuong)
                                     let firstImage = _context.AnhSanPhams
                                         .Where(a => a.IdModelSanPham == model.IdModelSanPham)
                                         .OrderBy(a => a.IdAnh)
                                         .FirstOrDefault()
                                     // Lấy khuyến mãi đang active
                                     let activePromotion = (from mskm in _context.ModelSanPhamKhuyenMais
                                                           join km in _context.KhuyenMais on mskm.IdKhuyenMai equals km.IdKhuyenMai
                                                           where mskm.IdModelSanPham == model.IdModelSanPham
                                                                 && (km.TrangThaiKM == "Đang diễn ra" || km.TrangThaiKM == "Đã kết thúc")
                                                                 && km.NgayBatDau.HasValue
                                                                 && km.NgayBatDau.Value <= now
                                                           orderby km.NgayKetThuc descending
                                                           select km).FirstOrDefault()
                                     where model.TrangThai == 1 && stockQuantity > 0 && soldCount > 3
                                     orderby soldCount descending
                                     select new
                                     {
                                         IdModelSanPham = model.IdModelSanPham,
                                         TenModel = model.TenModel ?? product.TenSanPham ?? "Không có tên",
                                         TenSanPham = product.TenSanPham ?? "Không có tên",
                                         TenThuongHieu = brand.TenThuongHieu ?? "Không có thương hiệu",
                                         GiaBan = model.GiaBanModel ?? 0,
                                         GiaGoc = product.GiaGoc ?? model.GiaBanModel ?? 0,
                                         HinhAnh = firstImage != null ? firstImage.DuongDan : "/ImgResource/default-product.jpg",
                                         Mau = model.Mau ?? "Không xác định",
                                         SoldCount = soldCount,
                                         IsHot = true,
                                         KhuyenMai = activePromotion != null ? new
                                         {
                                             IdKhuyenMai = activePromotion.IdKhuyenMai,
                                             LoaiGiam = activePromotion.LoaiGiam,
                                             GiaTri = activePromotion.GiaTri ?? 0
                                         } : null
                                     })
                                    .Take(6) // Lấy 6 sản phẩm hot nhất (bán chạy nhất)
                                    .ToListAsync();

            // Tính giá sau khuyến mãi
            var hotProductsWithPromotion = hotProducts.Select(p =>
            {
                decimal giaGoc = p.GiaGoc > 0 ? p.GiaGoc : p.GiaBan;
                decimal giaSauGiam = p.GiaBan;
                decimal phanTramGiam = 0;
                bool hasPromotion = p.KhuyenMai != null;

                if (hasPromotion && p.KhuyenMai.GiaTri > 0)
                {
                    
                    if (p.KhuyenMai.LoaiGiam == "Phần trăm")
                    {
                        phanTramGiam = (decimal)p.KhuyenMai.GiaTri;
                        giaSauGiam = giaGoc * (1 - phanTramGiam / 100);
                    }
                    else if (p.KhuyenMai.LoaiGiam == "Số tiền")
                    {
                        decimal soTienGiam = (decimal)p.KhuyenMai.GiaTri;
                        giaSauGiam = giaGoc - soTienGiam;
                        if (giaSauGiam < 0) giaSauGiam = 0;
                        if (giaGoc > 0)
                        {
                            phanTramGiam = (soTienGiam / giaGoc) * 100;
                        }
                    }
                }

                return new
                {
                    p.IdModelSanPham,
                    p.TenModel,
                    p.TenSanPham,
                    p.TenThuongHieu,
                    GiaBan = giaSauGiam,
                    GiaGoc = giaGoc,
                    p.HinhAnh,
                    p.Mau,
                    p.SoldCount,
                    p.IsHot,
                    HasPromotion = hasPromotion,
                    PhanTramGiam = Math.Round(phanTramGiam, 0),
                    SoTienGiam = giaGoc - giaSauGiam
                };
            }).ToList();

            return hotProductsWithPromotion.Cast<dynamic>().ToList();
        }

        // THÊM MỚI: API lấy tất cả sản phẩm (cho bộ lọc)
        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            try
            {
                var products = await GetAllModelSanPhamFromDatabase();
                // Đảm bảo luôn trả về mảng, không bao giờ null
                if (products == null)
                {
                    products = new List<dynamic>();
                }
                return Json(products);
            }
            catch (Exception ex)
            {
                // Log lỗi để debug
                Console.WriteLine($"❌ Lỗi GetAllProducts: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return Json(new { error = ex.Message });
            }
        }

        // API: Lọc và sắp xếp sản phẩm
        [HttpGet]
        public async Task<IActionResult> FilterProducts(
            decimal? minPrice = null,
            decimal? maxPrice = null,
            int? brandId = null,
            int? ramId = null,
            int? romId = null,
            string? sortBy = null) // "price_asc", "price_desc"
        {
            try
            {
                var now = DateTime.Now.Date;
                
                var query = from model in _context.ModelSanPhams
                           join product in _context.SanPhams on model.IdSanPham equals product.IdSanPham
                           join brand in _context.ThuongHieus on product.IdThuongHieu equals brand.IdThuongHieu
                           join ram in _context.RAMs on model.IdRAM equals ram.IdRAM into r
                           from ram in r.DefaultIfEmpty()
                           join rom in _context.ROMs on model.IdROM equals rom.IdROM into roms
                           from rom in roms.DefaultIfEmpty()
                           let firstImage = _context.AnhSanPhams
                               .Where(a => a.IdModelSanPham == model.IdModelSanPham)
                               .OrderBy(a => a.IdAnh)
                               .FirstOrDefault()
                           let stockQuantity = _context.TonKhos
                               .Where(t => t.IdModelSanPham == model.IdModelSanPham)
                               .Sum(t => t.SoLuong)
                           let soldCount = _context.Imeis
                               .Count(i => i.IdModelSanPham == model.IdModelSanPham && i.TrangThai == "Đã bán")
                           // Lấy khuyến mãi active
                           let activePromotion = _context.ModelSanPhamKhuyenMais
                               .Where(mspkm => mspkm.IdModelSanPham == model.IdModelSanPham)
                               .Select(mspkm => mspkm.KhuyenMai)
                               .Where(km => km != null 
                                   && km.NgayBatDau.HasValue 
                                   && now >= km.NgayBatDau.Value.Date
                                   && (km.TrangThaiKM == "Đang diễn ra" || km.TrangThaiKM == "Đã kết thúc"))
                               .FirstOrDefault()
                           where model.TrangThai == 1 && stockQuantity > 0
                           // Áp dụng bộ lọc
                           where (!minPrice.HasValue || model.GiaBanModel >= minPrice.Value)
                           where (!maxPrice.HasValue || model.GiaBanModel <= maxPrice.Value)
                           where (!brandId.HasValue || brand.IdThuongHieu == brandId.Value)
                           where (!ramId.HasValue || model.IdRAM == ramId.Value)
                           where (!romId.HasValue || model.IdROM == romId.Value)
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
                               SoldCount = soldCount,
                               IsHot = soldCount > 3,
                               IdThuongHieu = brand.IdThuongHieu,
                               IdRAM = model.IdRAM,
                               IdROM = model.IdROM,
                               DungLuongRAM = ram != null ? ram.DungLuongRAM : (string?)null,
                               DungLuongROM = rom != null ? rom.DungLuongROM : (string?)null,
                               // Thông tin khuyến mãi
                               KhuyenMai = activePromotion != null ? new
                               {
                                   LoaiGiam = activePromotion.LoaiGiam,
                                   GiaTri = activePromotion.GiaTri
                               } : null
                           };

                // Sắp xếp
                if (sortBy == "price_asc")
                {
                    query = query.OrderBy(p => p.GiaBan);
                }
                else if (sortBy == "price_desc")
                {
                    query = query.OrderByDescending(p => p.GiaBan);
                }
                else
                {
                    // Mặc định: sắp xếp theo Id
                    query = query.OrderByDescending(p => p.IdModelSanPham);
                }

                var products = await query.ToListAsync();

                // Tính giá sau giảm và đảm bảo >= 0
                var productsWithPrice = products.Select(p =>
                {
                    decimal giaSauGiam = p.GiaBan;
                    bool hasPromotion = false;

                    if (p.KhuyenMai != null)
                    {
                        giaSauGiam = CalculateDiscountedPrice(
                            p.GiaBan,
                            p.KhuyenMai.LoaiGiam,
                            p.KhuyenMai.GiaTri);
                        hasPromotion = true;
                    }

                    // Đảm bảo giá không âm (quan trọng!)
                    giaSauGiam = Math.Max(0, giaSauGiam);

                    return new
                    {
                        p.IdModelSanPham,
                        p.TenModel,
                        p.TenSanPham,
                        p.TenThuongHieu,
                        GiaBan = p.GiaBan,
                        GiaSauGiam = giaSauGiam,
                        DisplayPrice = hasPromotion ? giaSauGiam : p.GiaBan, // Giá để sắp xếp
                        HasPromotion = hasPromotion,
                        p.HinhAnh,
                        p.SoLuongTon,
                        p.Mau,
                        p.SoldCount,
                        p.IsHot,
                        p.IdThuongHieu,
                        p.IdRAM,
                        p.IdROM,
                        p.DungLuongRAM,
                        p.DungLuongROM
                    };
                }).ToList();

                // Sắp xếp lại theo DisplayPrice nếu có sortBy
                if (sortBy == "price_asc")
                {
                    productsWithPrice = productsWithPrice.OrderBy(p => p.DisplayPrice).ToList();
                }
                else if (sortBy == "price_desc")
                {
                    productsWithPrice = productsWithPrice.OrderByDescending(p => p.DisplayPrice).ToList();
                }

                // Loại bỏ DisplayPrice trước khi trả về
                var result = productsWithPrice.Select(p => new
                {
                    p.IdModelSanPham,
                    p.TenModel,
                    p.TenSanPham,
                    p.TenThuongHieu,
                    p.GiaBan,
                    p.GiaSauGiam,
                    p.HasPromotion,
                    p.HinhAnh,
                    p.SoLuongTon,
                    p.Mau,
                    p.SoldCount,
                    p.IsHot,
                    p.IdThuongHieu,
                    p.IdRAM,
                    p.IdROM,
                    p.DungLuongRAM,
                    p.DungLuongROM
                }).ToList();

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // Helper: Tính giá sau giảm (đảm bảo >= 0)
        private decimal CalculateDiscountedPrice(decimal originalPrice, string? discountType, decimal? discountValue)
        {
            if (discountValue == null || discountValue <= 0 || string.IsNullOrWhiteSpace(discountType))
                return Math.Max(0, originalPrice);

            decimal discountedPrice = 0;

            if (discountType == "Phần trăm")
            {
                var percent = Math.Min(100, Math.Max(0, discountValue.Value));
                discountedPrice = originalPrice * (1 - percent / 100);
            }
            else if (discountType == "Số tiền")
            {
                var discountAmount = Math.Min(originalPrice, Math.Max(0, discountValue.Value));
                discountedPrice = originalPrice - discountAmount;
            }
            else
            {
                discountedPrice = originalPrice;
            }

            // Làm tròn đến 1000 VNĐ
            discountedPrice = Math.Floor(discountedPrice / 1000) * 1000;

            // Đảm bảo giá không âm
            return Math.Max(0, discountedPrice);
        }

        // API: Lấy danh sách RAM để lọc
        [HttpGet]
        public async Task<IActionResult> GetRAMs()
        {
            try
            {
                var rams = await _context.RAMs
                    .ToListAsync();
                
                var sortedRams = rams.OrderBy(r => {
                    if (string.IsNullOrEmpty(r.DungLuongRAM)) return int.MaxValue;
                    var match = System.Text.RegularExpressions.Regex.Match(r.DungLuongRAM, @"\d+");
                    return match.Success ? int.Parse(match.Value) : int.MaxValue;
                }).Select(r => new { 
                    IdRAM = r.IdRAM, 
                    DungLuongRAM = r.DungLuongRAM ?? "N/A"
                }).ToList();
                return Json(sortedRams);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // API: Lấy danh sách ROM để lọc
        [HttpGet]
        public async Task<IActionResult> GetROMs()
        {
            try
            {
                var roms = await _context.ROMs
                    .ToListAsync();
                
                var sortedROMs = roms.OrderBy(r => {
                    if (string.IsNullOrEmpty(r.DungLuongROM)) return int.MaxValue;
                    var match = System.Text.RegularExpressions.Regex.Match(r.DungLuongROM, @"\d+");
                    return match.Success ? int.Parse(match.Value) : int.MaxValue;
                }).Select(r => new { 
                    IdROM = r.IdROM, 
                    DungLuongROM = r.DungLuongROM ?? "N/A"
                }).ToList();
                return Json(sortedROMs);
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
                    .SumAsync(t => t.SoLuong);

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

        // THÊM MỚI: API lấy sản phẩm có khuyến mãi đang hoạt động
        [HttpGet]
        public async Task<IActionResult> GetDiscountedProducts()
        {
            try
            {
                var now = DateTime.Now.Date;

                // Lấy các khuyến mãi đang hoạt động
                var activePromotions = await _context.KhuyenMais
                    .Where(km => km.NgayBatDau.HasValue &&
                                 km.NgayKetThuc.HasValue &&
                                 km.NgayBatDau.Value.Date <= now &&
                                 km.NgayKetThuc.Value.Date >= now &&
                                 km.TrangThaiKM == "Đang diễn ra")
                    .ToListAsync();

                if (!activePromotions.Any())
                {
                    return Json(new List<object>());
                }

                var promotionIds = activePromotions.Select(km => km.IdKhuyenMai).ToList();

                // Lấy các sản phẩm có khuyến mãi đang hoạt động
                var discountedProducts = await (from mspkm in _context.ModelSanPhamKhuyenMais
                                               where promotionIds.Contains(mspkm.IdKhuyenMai.Value)
                                               join model in _context.ModelSanPhams on mspkm.IdModelSanPham equals model.IdModelSanPham
                                               join product in _context.SanPhams on model.IdSanPham equals product.IdSanPham
                                               join brand in _context.ThuongHieus on product.IdThuongHieu equals brand.IdThuongHieu
                                               join promotion in _context.KhuyenMais on mspkm.IdKhuyenMai equals promotion.IdKhuyenMai
                                               let firstImage = _context.AnhSanPhams
                                                   .Where(a => a.IdModelSanPham == model.IdModelSanPham)
                                                   .OrderBy(a => a.IdAnh)
                                                   .FirstOrDefault()
                                               let stockQuantity = _context.TonKhos
                                                   .Where(t => t.IdModelSanPham == model.IdModelSanPham)
                                                   .Sum(t => t.SoLuong)
                                               where model.TrangThai == 1 && stockQuantity > 0
                                               select new
                                               {
                                                   IdModelSanPham = model.IdModelSanPham,
                                                   TenModel = model.TenModel ?? product.TenSanPham ?? "Không có tên",
                                                   TenSanPham = product.TenSanPham ?? "Không có tên",
                                                   TenThuongHieu = brand.TenThuongHieu ?? "Không có thương hiệu",
                                                   GiaBan = model.GiaBanModel ?? 0,
                                                   GiaGoc = model.GiaBanModel ?? 0, // Giá gốc trước giảm
                                                   HinhAnh = firstImage != null ? firstImage.DuongDan : "/ImgResource/default-product.jpg",
                                                   SoLuongTon = stockQuantity,
                                                   Mau = model.Mau ?? "Không xác định",
                                                   IdThuongHieu = brand.IdThuongHieu,
                                                   // Thông tin khuyến mãi
                                                   IdKhuyenMai = promotion.IdKhuyenMai,
                                                   MaKM = promotion.MaKM,
                                                   LoaiGiam = promotion.LoaiGiam,
                                                   GiaTriKhuyenMai = promotion.GiaTri ?? 0,
                                                   // Tính giá sau giảm (làm tròn đến 1000 VNĐ)
                                                   GiaSauGiam = CalculateDiscountedPrice(
                                                       model.GiaBanModel ?? 0,
                                                       promotion.LoaiGiam ?? "Phần trăm",
                                                       promotion.GiaTri ?? 0)
                                               })
                                              .ToListAsync();

                // Tính giá sau giảm cho mỗi sản phẩm
                var productsWithDiscount = discountedProducts.Select(p => new
                {
                    p.IdModelSanPham,
                    p.TenModel,
                    p.TenSanPham,
                    p.TenThuongHieu,
                    p.GiaBan,
                    p.GiaGoc,
                    p.HinhAnh,
                    p.SoLuongTon,
                    p.Mau,
                    p.IdThuongHieu,
                    p.IdKhuyenMai,
                    p.MaKM,
                    p.LoaiGiam,
                    p.GiaTriKhuyenMai,
                    GiaSauGiam = p.GiaSauGiam,
                    PhanTramGiam = p.LoaiGiam == "Phần trăm"
                        ? p.GiaTriKhuyenMai
                        : Math.Round((p.GiaTriKhuyenMai / p.GiaBan) * 100, 1)
                }).ToList();

                return Json(productsWithDiscount);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // VẤN ĐỀ 3: Helper: Tính giá sau giảm (làm tròn đến 1000 VNĐ, đảm bảo >= 0)
        private decimal CalculateDiscountedPrice(decimal originalPrice, string discountType, decimal discountValue)
        {
            if (discountValue <= 0 || string.IsNullOrWhiteSpace(discountType))
                return originalPrice;

            decimal discountedPrice = 0;

            if (discountType == "Phần trăm")
            {
                // Giảm theo phần trăm (đảm bảo không vượt quá 100%)
                var percent = Math.Min(100, Math.Max(0, discountValue));
                discountedPrice = originalPrice * (1 - percent / 100);
            }
            else if (discountType == "Số tiền")
            {
                // Giảm theo số tiền (đảm bảo không vượt quá giá gốc)
                var discountAmount = Math.Min(originalPrice, Math.Max(0, discountValue));
                discountedPrice = originalPrice - discountAmount;
            }
            else
            {
                // Mặc định không giảm
                discountedPrice = originalPrice;
            }

            // Làm tròn đến 1000 VNĐ (làm tròn xuống)
            discountedPrice = Math.Floor(discountedPrice / 1000) * 1000;

            // Đảm bảo giá không âm
            return Math.Max(0, discountedPrice);
        }
    }


    public class CartItemRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}