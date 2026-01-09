using DATN_DT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using DATN_DT.Data;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using SelectPdf;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace DATN_DT.Controllers
{
    [AllowAnonymous]
    public class DonHangController : Controller
    {
        private readonly MyDbContext _context;
        private readonly IWebHostEnvironment _env;

        public DonHangController(MyDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: Order
        public async Task<IActionResult> Index()
        {
            var model = new OrderViewModel
            {
                KhachHangList = await _context.KhachHangs.ToListAsync(),
                SanPhamList = await _context.ModelSanPhams
                    .Include(m => m.SanPham)
                    .Where(m => m.TrangThai == 1)
                    .ToListAsync()
            };

            return View(model);
        }

        // Tìm kiếm sản phẩm
        [HttpGet]
        public async Task<IActionResult> SearchProducts(string keyword)
        {
            try
            {
                var query = _context.SanPhams
                    .Include(sp => sp.ThuongHieu)
                    .Where(sp => sp.TrangThaiSP != "Ngừng kinh doanh" && // Chỉ lấy sản phẩm đang kinh doanh
                                _context.ModelSanPhams.Any(m => m.IdSanPham == sp.IdSanPham && m.TrangThai == 1)); // Chỉ lấy sản phẩm có model

                // Tìm kiếm theo keyword (không phân biệt hoa thường)
                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    keyword = keyword.Trim().ToLower();
                    query = query.Where(sp =>
                        (sp.MaSanPham != null && sp.MaSanPham.ToLower().Contains(keyword)) ||
                        (sp.TenSanPham != null && sp.TenSanPham.ToLower().Contains(keyword)) ||
                        (sp.ThuongHieu != null && sp.ThuongHieu.TenThuongHieu != null &&
                         sp.ThuongHieu.TenThuongHieu.ToLower().Contains(keyword))
                    );
                }

                var products = await query
                    .Select(m => new
                    {
                        idSanPham = m.IdSanPham,
                        tenSanPham = m.TenSanPham,
                        thuongHieu = m.ThuongHieu != null ? m.ThuongHieu.TenThuongHieu : string.Empty
                    })
                    .OrderBy(m => m.tenSanPham)
                    .Take(20)
                    .ToListAsync();

                return Json(new { success = true, data = products });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Lấy chi tiết sản phẩm với IMEI
        [HttpGet]
        public async Task<IActionResult> GetProductDetail(int id)
        {
            try
            {
                var product = await _context.ModelSanPhams
                    .Include(m => m.SanPham)
                    .Include(m => m.RAM)
                    .Include(m => m.ROM)
                    .Include(m => m.AnhSanPhams)
                    .Where(m => m.IdModelSanPham == id && m.TrangThai == 1)
                    .Select(m => new
                    {
                        idModelSanPham = m.IdModelSanPham,
                        idSanPham = m.IdSanPham,
                        tenModel = m.TenModel,
                        tenSanPham = m.SanPham != null ? m.SanPham.TenSanPham : string.Empty,
                        giaBanModel = m.GiaBanModel ?? 0,
                        tenMau = string.IsNullOrEmpty(m.Mau) ? "Không xác định" : m.Mau,
                        ram = m.RAM != null ? m.RAM.DungLuongRAM : string.Empty,
                        rom = m.ROM != null ? m.ROM.DungLuongROM : string.Empty,
                        anh = m.AnhSanPhams.Select(a => a.DuongDan).FirstOrDefault(),
                        soLuongTon = _context.TonKhos
                            .Where(t => t.IdModelSanPham == m.IdModelSanPham)
                            .Select(t => t.SoLuong)
                            .FirstOrDefault(),
                        imeis = _context.Imeis
                            .Where(i => i.IdModelSanPham == m.IdModelSanPham && i.TrangThai == "Còn hàng")
                            .Select(i => new { i.IdImei, i.MaImei })
                            .ToList()
                    })
                    .FirstOrDefaultAsync();

                if (product == null)
                    return Json(new { success = false, message = "Không tìm thấy sản phẩm" });

                return Json(new { success = true, data = product });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Lấy danh sách model theo sản phẩm
        [HttpGet]
        public async Task<IActionResult> GetModelsByProduct(int productId)
        {
            try
            {
                var models = await _context.ModelSanPhams
                    .Include(m => m.RAM)
                    .Include(m => m.ROM)
                    .Include(m => m.AnhSanPhams)
                    .Where(m => m.IdSanPham == productId && m.TrangThai == 1)
                    .Select(m => new
                    {
                        idModelSanPham = m.IdModelSanPham,
                        tenModel = m.TenModel,
                        mau = string.IsNullOrEmpty(m.Mau) ? "Không xác định" : m.Mau,
                        ram = m.RAM != null ? m.RAM.DungLuongRAM : string.Empty,
                        rom = m.ROM != null ? m.ROM.DungLuongROM : string.Empty,
                        giaBanModel = m.GiaBanModel ?? 0,
                        anh = m.AnhSanPhams.Select(a => a.DuongDan).FirstOrDefault(),
                        soLuongTon = _context.TonKhos
                            .Where(t => t.IdModelSanPham == m.IdModelSanPham)
                            .Select(t => t.SoLuong)
                            .FirstOrDefault()
                    })
                    .OrderBy(m => m.tenModel)
                    .ToListAsync();

                return Json(new { success = true, data = models });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Order/GetImeiByModel
        [HttpGet]
        public async Task<IActionResult> GetImeiByModel(int modelId)
        {
            var imeis = await _context.Imeis
                .Where(i => i.IdModelSanPham == modelId && i.TrangThai == "Còn hàng")
                .Select(i => new { i.IdImei, i.MaImei })
                .ToListAsync();

            return Json(imeis);
        }

        // Helper method để lấy IdNhanVien từ JWT token
        private int? GetCurrentNhanVienId()
        {
            try
            {
                var token = Request.Cookies["jwt"];
                if (string.IsNullOrEmpty(token)) return null;

                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadJwtToken(token);
                var nhanVienIdClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "IdNhanVien");

                if (nhanVienIdClaim != null && int.TryParse(nhanVienIdClaim.Value, out int nhanVienId))
                {
                    return nhanVienId;
                }
            }
            catch { }
            return null;
        }

        // Helper method: Tính giá khuyến mãi cho sản phẩm
        private async Task<decimal?> CalculatePromotionPrice(int idModelSanPham, decimal originalPrice)
        {
            try
            {
                var now = DateTime.Now.Date;

                // Tìm khuyến mãi đang hoạt động cho sản phẩm này
                var activePromotion = await (from mspkm in _context.ModelSanPhamKhuyenMais
                                             join km in _context.KhuyenMais on mspkm.IdKhuyenMai equals km.IdKhuyenMai
                                             where mspkm.IdModelSanPham == idModelSanPham
                                                && km.NgayBatDau.HasValue
                                                && km.NgayKetThuc.HasValue
                                                && km.NgayBatDau.Value.Date <= now
                                                && km.NgayKetThuc.Value.Date >= now
                                                && km.TrangThaiKM == "Đang diễn ra"
                                             orderby km.NgayKetThuc descending // Lấy khuyến mãi gần nhất
                                             select km)
                                          .FirstOrDefaultAsync();

                if (activePromotion == null)
                    return null;

                // Tính giá sau giảm
                decimal discountedPrice = 0;

                if (activePromotion.LoaiGiam == "Phần trăm")
                {
                    var percent = Math.Min(100, Math.Max(0, activePromotion.GiaTri ?? 0));
                    discountedPrice = originalPrice * (1 - percent / 100);
                }
                else if (activePromotion.LoaiGiam == "Số tiền")
                {
                    var discountAmount = Math.Min(originalPrice, Math.Max(0, activePromotion.GiaTri ?? 0));
                    discountedPrice = originalPrice - discountAmount;
                }
                else
                {
                    return null;
                }

                // Làm tròn đến 1000 VNĐ (làm tròn xuống)
                discountedPrice = Math.Floor(discountedPrice / 1000) * 1000;

                // Đảm bảo giá không âm
                discountedPrice = Math.Max(0, discountedPrice);

                return discountedPrice;
            }
            catch
            {
                return null;
            }
        }

        // POST: Order/Create
        [HttpPost]
        public async Task<IActionResult> Create(OrderCreateModel orderModel)
        {
            // Validation thông tin khách hàng
            if (!orderModel.IdKhachHang.HasValue)
            {
                // Nếu không chọn khách hàng thân thiết, phải có tên và số điện thoại
                if (string.IsNullOrWhiteSpace(orderModel.TenKhachHang))
                {
                    return Json(new { success = false, message = "Vui lòng nhập tên khách hàng" });
                }
                if (orderModel.TenKhachHang.Trim().Length < 2 || orderModel.TenKhachHang.Trim().Length > 100)
                {
                    return Json(new { success = false, message = "Tên khách hàng phải có từ 2 đến 100 ký tự" });
                }
                if (string.IsNullOrWhiteSpace(orderModel.SdtKhachHang))
                {
                    return Json(new { success = false, message = "Vui lòng nhập số điện thoại" });
                }
                // Validate số điện thoại (10-11 số, bắt đầu bằng 0 hoặc +84)
                var phoneRegex = new System.Text.RegularExpressions.Regex(@"^(0|\+84)[0-9]{9,10}$");
                if (!phoneRegex.IsMatch(orderModel.SdtKhachHang.Trim().Replace(" ", "")))
                {
                    return Json(new { success = false, message = "Số điện thoại không hợp lệ (VD: 0123456789 hoặc +84123456789)" });
                }
            }

            // Validation phương thức thanh toán
            if (string.IsNullOrWhiteSpace(orderModel.PhuongThucThanhToan))
            {
                return Json(new { success = false, message = "Vui lòng chọn phương thức thanh toán" });
            }

            // Validation số tiền khách đưa (nếu thanh toán bằng tiền mặt)
            if (orderModel.PhuongThucThanhToan == "Tiền mặt")
            {
                if (!orderModel.TienKhachDua.HasValue || orderModel.TienKhachDua.Value <= 0)
                {
                    return Json(new { success = false, message = "Vui lòng nhập số tiền khách đưa" });
                }

                // Validate min/max số tiền khách đưa
                decimal minTien = 0;
                decimal maxTien = 10000000000; // 10 tỷ VNĐ

                if (orderModel.TienKhachDua.Value < minTien)
                {
                    return Json(new { success = false, message = $"Số tiền khách đưa không được nhỏ hơn {minTien:N0} VNĐ" });
                }

                if (orderModel.TienKhachDua.Value > maxTien)
                {
                    return Json(new { success = false, message = $"Số tiền khách đưa không được vượt quá {maxTien:N0} VNĐ (10 tỷ VNĐ)" });
                }

                // Tính tổng tiền tạm thời để validate
                decimal tongTienTam = 0;
                if (orderModel.ChiTietDonHang != null && orderModel.ChiTietDonHang.Any())
                {
                    foreach (var item in orderModel.ChiTietDonHang)
                    {
                        var model = await _context.ModelSanPhams.FirstOrDefaultAsync(m => m.IdModelSanPham == item.IdModelSanPham);
                        if (model != null)
                        {
                            var donGia = model.GiaBanModel ?? 0;
                            var giaKhuyenMai = await CalculatePromotionPrice(item.IdModelSanPham, donGia);
                            var finalPrice = giaKhuyenMai ?? donGia;
                            tongTienTam += finalPrice * item.SoLuong;
                        }
                    }
                }

                if (orderModel.TienKhachDua.Value < tongTienTam)
                {
                    var thieu = tongTienTam - orderModel.TienKhachDua.Value;
                    return Json(new { success = false, message = $"Số tiền khách đưa không đủ! Còn thiếu: {thieu:N0} đ" });
                }

                // Tính tiền thừa
                orderModel.TienThua = orderModel.TienKhachDua.Value - tongTienTam;
            }
            else
            {
                // Chuyển khoản: không cần tiền khách đưa
                orderModel.TienKhachDua = null;
                orderModel.TienThua = null;
            }

            // Validation chi tiết đơn hàng
            if (orderModel.ChiTietDonHang == null || !orderModel.ChiTietDonHang.Any())
            {
                return Json(new { success = false, message = "Vui lòng chọn ít nhất một sản phẩm" });
            }

            // Validation IMEI cho từng sản phẩm
            var imeiErrors = new List<string>();
            foreach (var item in orderModel.ChiTietDonHang)
            {
                if (item.SelectedImeis == null || !item.SelectedImeis.Any())
                {
                    var model = await _context.ModelSanPhams
                        .Include(m => m.SanPham)
                        .FirstOrDefaultAsync(m => m.IdModelSanPham == item.IdModelSanPham);
                    var tenSanPham = model?.SanPham?.TenSanPham ?? "N/A";
                    var tenModel = model?.TenModel ?? "N/A";
                    imeiErrors.Add($"Sản phẩm {tenSanPham} - {tenModel} chưa chọn IMEI");
                }
                else
                {
                    // Kiểm tra IMEI có tồn tại và có thể bán được
                    foreach (var imeiId in item.SelectedImeis)
                    {
                        var imei = await _context.Imeis
                            .Include(i => i.ModelSanPham)
                            .ThenInclude(m => m.SanPham)
                            .FirstOrDefaultAsync(i => i.IdImei == imeiId);

                        if (imei == null)
                        {
                            imeiErrors.Add($"IMEI ID {imeiId} không tồn tại");
                        }
                        else
                        {
                            // Kiểm tra trạng thái IMEI - chỉ cho phép bán nếu trạng thái là "Còn hàng"
                            var trangThaiHienTai = imei.TrangThai ?? "";
                            var trangThaiHopLe = trangThaiHienTai == "Còn hàng";

                            // Kiểm tra IMEI có đúng model không
                            var imeiDungModel = imei.IdModelSanPham == item.IdModelSanPham;

                            if (!trangThaiHopLe)
                            {
                                var tenSanPham = imei.ModelSanPham?.SanPham?.TenSanPham ?? "N/A";
                                var tenModel = imei.ModelSanPham?.TenModel ?? "N/A";
                                imeiErrors.Add($"IMEI {imei.MaImei} của sản phẩm {tenSanPham} - {tenModel} không thể bán (Trạng thái: {trangThaiHienTai}). Chỉ có thể bán IMEI có trạng thái 'Còn hàng'");
                            }
                            else if (!imeiDungModel)
                            {
                                var tenSanPham = imei.ModelSanPham?.SanPham?.TenSanPham ?? "N/A";
                                var tenModel = imei.ModelSanPham?.TenModel ?? "N/A";
                                imeiErrors.Add($"IMEI {imei.MaImei} không thuộc về model sản phẩm đã chọn. IMEI này thuộc: {tenSanPham} - {tenModel}");
                            }
                        }
                    }
                }
            }

            if (imeiErrors.Any())
            {
                return Json(new { success = false, message = "Lỗi IMEI:", errors = imeiErrors });
            }

            if (ModelState.IsValid)
            {
                // Kiểm tra số lượng sản phẩm trong kho trước khi tạo hóa đơn
                var stockErrors = new List<string>();
                foreach (var item in orderModel.ChiTietDonHang)
                {
                    var tonKho = await _context.TonKhos
                        .Where(t => t.IdModelSanPham == item.IdModelSanPham)
                        .SumAsync(t => t.SoLuong);

                    if (tonKho <= 0)
                    {
                        var model = await _context.ModelSanPhams
                            .Include(m => m.SanPham)
                            .FirstOrDefaultAsync(m => m.IdModelSanPham == item.IdModelSanPham);
                        var tenSanPham = model?.SanPham?.TenSanPham ?? "N/A";
                        var tenModel = model?.TenModel ?? "N/A";
                        stockErrors.Add($"Sản phẩm {tenSanPham} - {tenModel} đã hết hàng trong kho (tồn kho: {tonKho})");
                    }
                    else if (tonKho < item.SoLuong)
                    {
                        var model = await _context.ModelSanPhams
                            .Include(m => m.SanPham)
                            .FirstOrDefaultAsync(m => m.IdModelSanPham == item.IdModelSanPham);
                        var tenSanPham = model?.SanPham?.TenSanPham ?? "N/A";
                        var tenModel = model?.TenModel ?? "N/A";
                        stockErrors.Add($"Sản phẩm {tenSanPham} - {tenModel} không đủ số lượng (tồn kho: {tonKho}, yêu cầu: {item.SoLuong})");
                    }
                }

                if (stockErrors.Any())
                {
                    return Json(new { success = false, message = "Không thể tạo đơn hàng:", errors = stockErrors });
                }

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // 1. Tạo đơn hàng
                    // Xác định thông tin khách hàng: ưu tiên từ input, nếu không có thì lấy từ database
                    string hoTenNguoiNhan = string.Empty;
                    string sdtNguoiNhan = string.Empty;

                    if (!string.IsNullOrWhiteSpace(orderModel.TenKhachHang))
                    {
                        hoTenNguoiNhan = orderModel.TenKhachHang.Trim();
                    }
                    else if (orderModel.IdKhachHang.HasValue)
                    {
                        var khachHang = await _context.KhachHangs.FindAsync(orderModel.IdKhachHang);
                        hoTenNguoiNhan = khachHang?.HoTenKhachHang ?? string.Empty;
                    }

                    if (!string.IsNullOrWhiteSpace(orderModel.SdtKhachHang))
                    {
                        sdtNguoiNhan = orderModel.SdtKhachHang.Trim();
                    }
                    else if (orderModel.IdKhachHang.HasValue)
                    {
                        var khachHang = await _context.KhachHangs.FindAsync(orderModel.IdKhachHang);
                        sdtNguoiNhan = khachHang?.SdtKhachHang ?? string.Empty;
                    }

                    // Tính tổng tiền trước khi tạo đơn hàng
                    decimal tongTienTinh = 0;
                    foreach (var item in orderModel.ChiTietDonHang)
                    {
                        var model = await _context.ModelSanPhams
                            .FirstOrDefaultAsync(m => m.IdModelSanPham == item.IdModelSanPham);
                        if (model != null)
                        {
                            var donGia = model.GiaBanModel ?? 0;
                            var giaKhuyenMai = await CalculatePromotionPrice(item.IdModelSanPham, donGia);
                            var finalPrice = giaKhuyenMai ?? donGia;
                            tongTienTinh += finalPrice * item.SoLuong;
                        }
                    }

                    // Lưu thông tin thanh toán vào GhiChu (JSON format)
                    var ghiChuThanhToan = string.Empty;
                    if (orderModel.PhuongThucThanhToan == "Tiền mặt" && orderModel.TienKhachDua.HasValue && orderModel.TienThua.HasValue)
                    {
                        ghiChuThanhToan = $"TIEN_KHACH_DUA:{orderModel.TienKhachDua.Value:N0}|TIEN_THUA:{orderModel.TienThua.Value:N0}|TONG_TIEN:{tongTienTinh:N0}";
                    }

                    var donHang = new DonHang
                    {
                        IdKhachHang = orderModel.IdKhachHang,
                        HoTenNguoiNhan = hoTenNguoiNhan,
                        SdtNguoiNhan = sdtNguoiNhan,
                        MaDon = "DH" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                        NgayDat = DateTime.Now,
                        DiaChiGiaoHang = orderModel.DiaChiGiaoHang,
                        TrangThaiHoaDon = "Đã thanh toán",
                        PhuongThucThanhToan = orderModel.PhuongThucThanhToan,
                        GhiChu = ghiChuThanhToan,
                        TrangThaiDH = 1
                    };

                    _context.DonHangs.Add(donHang);
                    await _context.SaveChangesAsync();

                    decimal tongTien = 0;

                    // 2. Tạo chi tiết đơn hàng và xử lý IMEI (Bán hàng tại quầy - chỉ tạo DonHang và DonHangChiTiet)
                    foreach (var item in orderModel.ChiTietDonHang)
                    {
                        // Lấy giá từ ModelSanPham
                        var model = await _context.ModelSanPhams
                            .FirstOrDefaultAsync(m => m.IdModelSanPham == item.IdModelSanPham);

                        if (model == null) continue;

                        var donGia = model.GiaBanModel ?? 0;

                        // Tính giá khuyến mãi nếu có
                        var idModelSanPham = item.IdModelSanPham;
                        var giaKhuyenMai = await CalculatePromotionPrice(idModelSanPham, donGia);
                        var finalPrice = giaKhuyenMai ?? donGia; // Nếu có khuyến mãi thì dùng giá khuyến mãi, không thì dùng giá gốc
                        var thanhTien = finalPrice * item.SoLuong;

                        tongTien += thanhTien;

                        // Tạo chi tiết đơn hàng (cho bán hàng tại quầy)
                        var chiTiet = new DonHangChiTiet
                        {
                            IdDonHang = donHang.IdDonHang,
                            IdModelSanPham = item.IdModelSanPham,
                            GiaKhuyenMai = giaKhuyenMai, // Lưu giá khuyến mãi (null nếu không có)
                            DonGia = finalPrice, // Lưu giá cuối cùng để tính thành tiền
                            ThanhTien = thanhTien
                        };

                        _context.DonHangChiTiets.Add(chiTiet);

                        // 3. Trừ tồn kho và cập nhật trạng thái IMEI
                        if (item.SelectedImeis != null && item.SelectedImeis.Any())
                        {
                            foreach (var imeiIdItem in item.SelectedImeis)
                            {
                                var imei = await _context.Imeis.FindAsync(imeiIdItem);
                                if (imei != null)
                                {
                                    imei.TrangThai = "Đã bán";

                                    // Kiểm tra IMEI đã có bảo hành đang hoạt động chưa (tránh trùng lặp)
                                    // Trạng thái hoạt động: "Đang tiếp nhận", "Đang xử lý", "Đang bảo hành"
                                    // Trạng thái không hoạt động: "Đã hoàn thành", "Từ chối bảo hành"
                                    var hasActiveWarranty = await _context.BaoHanhs
                                        .AnyAsync(b => b.IdImei == imei.IdImei &&
                                                       b.TrangThai != "Đã hoàn thành" &&
                                                       b.TrangThai != "Từ chối bảo hành" &&
                                                       (b.NgayTra == null || b.NgayTra >= DateTime.Now));

                                    // Chỉ tạo bảo hành nếu chưa có bảo hành đang hoạt động
                                    if (!hasActiveWarranty)
                                    {
                                        // Tạo bảo hành tự động khi bán hàng
                                        // Lưu ý: IdKhachHang có thể null nếu là khách vãng lai (được phép)
                                        var baoHanh = new BaoHanh
                                        {
                                            IdImei = imei.IdImei,
                                            IdKhachHang = orderModel.IdKhachHang, // Có thể null cho khách vãng lai
                                            NgayNhan = DateTime.Now,
                                            NgayTra = DateTime.Now.AddYears(1), // Bảo hành 1 năm
                                            TrangThai = "Đang bảo hành", // Trạng thái khi mới mua
                                            MoTaLoi = "Mới mua - Sản phẩm hoạt động bình thường",
                                            XuLy = "Hoạt động bình thường"
                                        };
                                        _context.BaoHanhs.Add(baoHanh);
                                    }
                                }
                            }
                        }

                        // Trừ tồn kho
                        var tonKho = await _context.TonKhos
                            .FirstOrDefaultAsync(t => t.IdModelSanPham == item.IdModelSanPham);
                        if (tonKho != null)
                        {
                            tonKho.SoLuong -= item.SoLuong;
                        }
                    }

                    // 4. Cập nhật điểm tích lũy cho khách hàng
                    if (orderModel.IdKhachHang.HasValue)
                    {
                        var khachHang = await _context.KhachHangs.FindAsync(orderModel.IdKhachHang);
                        if (khachHang != null)
                        {
                            // Tích điểm: 1 điểm / 10.000đ
                            int diemTichLuy = (int)(tongTien / 10000);
                            khachHang.DiemTichLuy = (khachHang.DiemTichLuy ?? 0) + diemTichLuy;
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // Tạo và lưu PDF hóa đơn
                    string pdfUrl = "";
                    try
                    {
                        var chiTietListForPdf = new List<InvoiceChiTietViewModel>();
                        foreach (var item in orderModel.ChiTietDonHang)
                        {
                            var chiTiet = donHang.DonHangChiTiets?.FirstOrDefault(c => c.IdModelSanPham == item.IdModelSanPham);
                            if (chiTiet != null)
                            {
                                var imeis = new List<string>();
                                if (item.SelectedImeis != null && item.SelectedImeis.Any())
                                {
                                    // Lấy IMEI từ danh sách đã chọn
                                    imeis = await _context.Imeis
                                        .Where(i => item.SelectedImeis.Contains(i.IdImei))
                                        .Select(i => i.MaImei ?? "")
                                        .OrderBy(i => i)
                                        .ToListAsync();
                                }
                                else
                                {
                                    // Nếu không có SelectedImeis, lấy từ BaoHanh được tạo cùng ngày với đơn hàng
                                    var ngayDat = donHang.NgayDat ?? DateTime.Now;
                                    var baoHanhImeis = await _context.BaoHanhs
                                        .Where(b => b.IdKhachHang == donHang.IdKhachHang &&
                                                   b.NgayNhan.HasValue &&
                                                   b.NgayNhan.Value.Date == ngayDat.Date &&
                                                   b.IdImei.HasValue &&
                                                   _context.Imeis.Any(i => i.IdImei == b.IdImei.Value && i.IdModelSanPham == item.IdModelSanPham))
                                        .Select(b => b.IdImei.Value)
                                        .ToListAsync();

                                    if (baoHanhImeis.Any())
                                    {
                                        imeis = await _context.Imeis
                                            .Where(i => baoHanhImeis.Contains(i.IdImei))
                                            .Select(i => i.MaImei ?? "")
                                            .OrderBy(i => i)
                                            .ToListAsync();
                                    }
                                }

                                chiTietListForPdf.Add(new InvoiceChiTietViewModel
                                {
                                    ChiTiet = chiTiet,
                                    Imeis = imeis
                                });
                            }
                        }

                        // Parse thông tin thanh toán từ GhiChu
                        decimal? tienKhachDua = null;
                        decimal? tienThua = null;
                        if (!string.IsNullOrWhiteSpace(ghiChuThanhToan))
                        {
                            var parts = ghiChuThanhToan.Split('|');
                            foreach (var part in parts)
                            {
                                if (part.StartsWith("TIEN_KHACH_DUA:"))
                                {
                                    var value = part.Replace("TIEN_KHACH_DUA:", "").Replace(",", "");
                                    if (decimal.TryParse(value, out decimal tienDua))
                                        tienKhachDua = tienDua;
                                }
                                else if (part.StartsWith("TIEN_THUA:"))
                                {
                                    var value = part.Replace("TIEN_THUA:", "").Replace(",", "");
                                    if (decimal.TryParse(value, out decimal tienThuaValue))
                                        tienThua = tienThuaValue;
                                }
                            }
                        }

                        var invoiceViewModel = new InvoiceViewModel
                        {
                            DonHang = donHang,
                            ChiTietList = chiTietListForPdf,
                            TienKhachDua = tienKhachDua,
                            TienThua = tienThua,
                            TongTien = tongTien
                        };

                        var html = await RenderViewToStringAsync("Invoice", invoiceViewModel);

                        // Tạo PDF từ HTML sử dụng SelectPdf
                        HtmlToPdf converter = new HtmlToPdf();
                        converter.Options.PdfPageSize = PdfPageSize.A4;
                        converter.Options.PdfPageOrientation = PdfPageOrientation.Portrait;
                        converter.Options.MarginTop = 10;
                        converter.Options.MarginBottom = 10;
                        converter.Options.MarginLeft = 10;
                        converter.Options.MarginRight = 10;
                        converter.Options.AutoFitWidth = HtmlToPdfPageFitMode.ShrinkOnly;

                        PdfDocument doc = converter.ConvertHtmlString(html);
                        byte[] pdfBytes = doc.Save();
                        doc.Close();

                        // Lưu file PDF vào wwwroot/invoices
                        var invoicesFolder = Path.Combine(_env.WebRootPath, "invoices");
                        if (!Directory.Exists(invoicesFolder))
                        {
                            Directory.CreateDirectory(invoicesFolder);
                        }

                        var fileName = $"HoaDon_{donHang.MaDon}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                        var filePath = Path.Combine(invoicesFolder, fileName);
                        await System.IO.File.WriteAllBytesAsync(filePath, pdfBytes);

                        pdfUrl = $"/DonHang/ExportPdf?maDon={donHang.MaDon}";
                    }
                    catch (Exception pdfEx)
                    {
                        // Log lỗi nhưng không làm gián đoạn quá trình thanh toán
                        Console.WriteLine($"Lỗi tạo PDF: {pdfEx.Message}");
                        Console.WriteLine($"Stack trace: {pdfEx.StackTrace}");
                        // Vẫn trả về pdfUrl để có thể thử lại sau
                        pdfUrl = $"/DonHang/ExportPdf?maDon={donHang.MaDon}";
                    }

                    // Trả về kết quả
                    return Json(new
                    {
                        success = true,
                        message = "Thanh toán thành công!",
                        maDon = donHang.MaDon,
                        idDonHang = donHang.IdDonHang,
                        ngayBaoHanh = DateTime.Now.AddYears(1).ToString("dd/MM/yyyy"),
                        tongTien = tongTien,
                        pdfUrl = pdfUrl
                    });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = "Lỗi: " + ex.Message });
                }
            }

            return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
        }

        // GET: DonHang/Invoice/{id}
        [HttpGet]
        public async Task<IActionResult> Invoice(int id)
        {
            try
            {
                var donHang = await _context.DonHangs
                    .Include(d => d.KhachHang)
                    .Include(d => d.DonHangChiTiets)
                        .ThenInclude(c => c.ModelSanPham)
                            .ThenInclude(m => m.SanPham)
                    .FirstOrDefaultAsync(d => d.IdDonHang == id);

                if (donHang == null)
                {
                    return NotFound("Không tìm thấy đơn hàng");
                }

                // Lấy IMEI cho từng chi tiết
                var chiTietList = new List<InvoiceChiTietViewModel>();
                foreach (var chiTiet in donHang.DonHangChiTiets ?? new List<DonHangChiTiet>())
                {
                    var imeis = new List<string>();
                    if (chiTiet.IdModelSanPham.HasValue)
                    {
                        imeis = await _context.Imeis
                            .Where(i => i.IdModelSanPham == chiTiet.IdModelSanPham && i.TrangThai == "Đã bán")
                            .Select(i => i.MaImei ?? "")
                            .ToListAsync();
                    }

                    chiTietList.Add(new InvoiceChiTietViewModel
                    {
                        ChiTiet = chiTiet,
                        Imeis = imeis
                    });
                }

                // Parse thông tin thanh toán từ GhiChu
                decimal? tienKhachDua = null;
                decimal? tienThua = null;
                decimal tongTien = chiTietList.Sum(c => c.ChiTiet.ThanhTien ?? 0);

                if (!string.IsNullOrWhiteSpace(donHang.GhiChu) && donHang.GhiChu.Contains("TIEN_KHACH_DUA:"))
                {
                    var parts = donHang.GhiChu.Split('|');
                    foreach (var part in parts)
                    {
                        if (part.StartsWith("TIEN_KHACH_DUA:"))
                        {
                            var value = part.Replace("TIEN_KHACH_DUA:", "").Replace(",", "");
                            if (decimal.TryParse(value, out decimal tienDua))
                                tienKhachDua = tienDua;
                        }
                        else if (part.StartsWith("TIEN_THUA:"))
                        {
                            var value = part.Replace("TIEN_THUA:", "").Replace(",", "");
                            if (decimal.TryParse(value, out decimal tienThuaValue))
                                tienThua = tienThuaValue;
                        }
                    }
                }

                var viewModel = new InvoiceViewModel
                {
                    DonHang = donHang,
                    ChiTietList = chiTietList,
                    TienKhachDua = tienKhachDua,
                    TienThua = tienThua,
                    TongTien = tongTien
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                return BadRequest("Lỗi: " + ex.Message);
            }
        }

        // GET: DonHang/ExportPdf?maDon={maDon}
        [HttpGet]
        public async Task<IActionResult> ExportPdf(string maDon)
        {
            try
            {
                if (string.IsNullOrEmpty(maDon))
                {
                    return BadRequest("Mã đơn không được để trống");
                }

                var donHang = await _context.DonHangs
                    .Include(d => d.KhachHang)
                    .Include(d => d.DonHangChiTiets)
                        .ThenInclude(c => c.ModelSanPham)
                            .ThenInclude(m => m.SanPham)
                    .FirstOrDefaultAsync(d => d.MaDon == maDon);

                if (donHang == null)
                {
                    return NotFound("Không tìm thấy đơn hàng");
                }

                // Lấy IMEI cho từng chi tiết
                var chiTietList = new List<InvoiceChiTietViewModel>();
                foreach (var chiTiet in donHang.DonHangChiTiets ?? new List<DonHangChiTiet>())
                {
                    var imeis = new List<string>();
                    if (chiTiet.IdModelSanPham.HasValue)
                    {
                        imeis = await _context.Imeis
                            .Where(i => i.IdModelSanPham == chiTiet.IdModelSanPham && i.TrangThai == "Đã bán")
                            .Select(i => i.MaImei ?? "")
                            .ToListAsync();
                    }

                    chiTietList.Add(new InvoiceChiTietViewModel
                    {
                        ChiTiet = chiTiet,
                        Imeis = imeis
                    });
                }

                // Parse thông tin thanh toán từ GhiChu
                decimal? tienKhachDua = null;
                decimal? tienThua = null;
                decimal tongTien = chiTietList.Sum(c => c.ChiTiet.ThanhTien ?? 0);

                if (!string.IsNullOrWhiteSpace(donHang.GhiChu) && donHang.GhiChu.Contains("TIEN_KHACH_DUA:"))
                {
                    var parts = donHang.GhiChu.Split('|');
                    foreach (var part in parts)
                    {
                        if (part.StartsWith("TIEN_KHACH_DUA:"))
                        {
                            var value = part.Replace("TIEN_KHACH_DUA:", "").Replace(",", "");
                            if (decimal.TryParse(value, out decimal tienDua))
                                tienKhachDua = tienDua;
                        }
                        else if (part.StartsWith("TIEN_THUA:"))
                        {
                            var value = part.Replace("TIEN_THUA:", "").Replace(",", "");
                            if (decimal.TryParse(value, out decimal tienThuaValue))
                                tienThua = tienThuaValue;
                        }
                    }
                }

                var viewModel = new InvoiceViewModel
                {
                    DonHang = donHang,
                    ChiTietList = chiTietList,
                    TienKhachDua = tienKhachDua,
                    TienThua = tienThua,
                    TongTien = tongTien
                };

                // Render HTML
                var html = await RenderViewToStringAsync("Invoice", viewModel);

                // Tạo PDF từ HTML sử dụng SelectPdf
                HtmlToPdf converter = new HtmlToPdf();
                converter.Options.PdfPageSize = PdfPageSize.A4;
                converter.Options.PdfPageOrientation = PdfPageOrientation.Portrait;
                converter.Options.MarginTop = 10;
                converter.Options.MarginBottom = 10;
                converter.Options.MarginLeft = 10;
                converter.Options.MarginRight = 10;
                converter.Options.AutoFitWidth = HtmlToPdfPageFitMode.ShrinkOnly;

                PdfDocument doc = converter.ConvertHtmlString(html);
                byte[] pdfBytes = doc.Save();
                doc.Close();

                // Tạo thư mục invoices nếu chưa có
                var invoicesFolder = Path.Combine(_env.WebRootPath, "invoices");
                if (!Directory.Exists(invoicesFolder))
                {
                    Directory.CreateDirectory(invoicesFolder);
                }

                // Lưu file PDF vào wwwroot/invoices
                var fileName = $"HoaDon_{donHang.MaDon}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                var filePath = Path.Combine(invoicesFolder, fileName);
                await System.IO.File.WriteAllBytesAsync(filePath, pdfBytes);

                // Trả về file để download với Content-Disposition header để force download
                var downloadFileName = $"HoaDon_{donHang.MaDon}.pdf";
                Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{downloadFileName}\"");
                return File(pdfBytes, "application/pdf", downloadFileName);
            }
            catch (Exception ex)
            {
                return BadRequest("Lỗi: " + ex.Message);
            }
        }

        // Helper method để render view thành string
        private async Task<string> RenderViewToStringAsync(string viewName, object model)
        {
            ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewEngine = HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;
                var viewResult = viewEngine?.FindView(ControllerContext, viewName, false);

                if (viewResult?.View == null)
                {
                    throw new Exception($"View '{viewName}' not found");
                }

                var viewContext = new ViewContext(
                    ControllerContext,
                    viewResult.View,
                    ViewData,
                    TempData,
                    sw,
                    new HtmlHelperOptions()
                );

                await viewResult.View.RenderAsync(viewContext);
                return sw.GetStringBuilder().ToString();
            }
        }

        // GET: DonHang/Manage - Trang quản lý hóa đơn offline
        public IActionResult Manage()
        {
            return View();
        }

        // GET: API - Lấy danh sách đơn hàng offline
        [HttpGet]
        public async Task<IActionResult> GetOfflineOrders(int? status, string? search, DateTime? fromDate, DateTime? toDate, string? paymentMethod)
        {
            try
            {
                var query = _context.DonHangs
                    .Include(d => d.KhachHang)
                    .Include(d => d.NhanVien)
                    .Include(d => d.DonHangChiTiets)
                        .ThenInclude(c => c.ModelSanPham)
                            .ThenInclude(m => m.SanPham)
                                .ThenInclude(sp => sp.ThuongHieu)
                    .Include(d => d.DonHangChiTiets)
                        .ThenInclude(c => c.ModelSanPham)
                            .ThenInclude(m => m.RAM)
                    .Include(d => d.DonHangChiTiets)
                        .ThenInclude(c => c.ModelSanPham)
                            .ThenInclude(m => m.ROM)
                    .Include(d => d.DonHangChiTiets)
                        .ThenInclude(c => c.ModelSanPham)
                            .ThenInclude(m => m.AnhSanPhams)
                    .Where(d => d.TrangThaiHoaDon == "Đã thanh toán") // Chỉ lấy đơn đã thanh toán
                    .AsQueryable();

                // Filter theo trạng thái (nếu có)
                if (status.HasValue)
                {
                    query = query.Where(d => d.TrangThaiDH == status.Value);
                }

                // Tìm kiếm
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var searchLower = search.ToLower();
                    query = query.Where(d =>
                        (d.MaDon != null && d.MaDon.ToLower().Contains(searchLower)) ||
                        (d.KhachHang != null && d.KhachHang.HoTenKhachHang != null && d.KhachHang.HoTenKhachHang.ToLower().Contains(searchLower)) ||
                        (d.HoTenNguoiNhan != null && d.HoTenNguoiNhan.ToLower().Contains(searchLower)) ||
                        (d.SdtNguoiNhan != null && d.SdtNguoiNhan.Contains(search))
                    );
                }

                // Filter theo ngày
                if (fromDate.HasValue)
                {
                    query = query.Where(d => d.NgayDat >= fromDate.Value);
                }
                if (toDate.HasValue)
                {
                    query = query.Where(d => d.NgayDat <= toDate.Value.AddDays(1).AddSeconds(-1));
                }

                // Filter theo phương thức thanh toán
                if (!string.IsNullOrWhiteSpace(paymentMethod))
                {
                    query = query.Where(d => d.PhuongThucThanhToan == paymentMethod);
                }

                var donHangs = await query
                    .OrderByDescending(d => d.NgayDat)
                    .ToListAsync();

                var orders = donHangs.Select(d => new
                {
                    idDonHang = d.IdDonHang,
                    maDon = d.MaDon ?? $"DH{d.IdDonHang:D6}",
                    ngayDat = d.NgayDat?.ToString("dd/MM/yyyy HH:mm"),
                    trangThai = d.TrangThaiHoaDon ?? "Đã thanh toán",
                    trangThaiSo = d.TrangThaiDH ?? 1,
                    tongTien = d.DonHangChiTiets?.Sum(c => c.ThanhTien ?? 0) ?? 0,
                    phuongThucThanhToan = d.PhuongThucThanhToan ?? "Tiền mặt",
                    hoTenKhachHang = d.KhachHang?.HoTenKhachHang ?? d.HoTenNguoiNhan ?? "N/A",
                    sdtKhachHang = d.KhachHang?.SdtKhachHang ?? d.SdtNguoiNhan ?? "N/A",
                    hoTenNhanVien = d.NhanVien?.HoTenNhanVien ?? "N/A",
                    soLuongSanPham = d.DonHangChiTiets?.Count ?? 0,
                    chiTiet = d.DonHangChiTiets?.Select(c => new
                    {
                        tenSanPham = c.ModelSanPham?.SanPham?.TenSanPham ?? "N/A",
                        tenModel = c.ModelSanPham?.TenModel ?? "N/A",
                        tenThuongHieu = c.ModelSanPham?.SanPham?.ThuongHieu?.TenThuongHieu ?? "N/A",
                        mau = c.ModelSanPham?.Mau ?? "N/A",
                        ram = c.ModelSanPham?.RAM?.DungLuongRAM ?? "N/A",
                        rom = c.ModelSanPham?.ROM?.DungLuongROM ?? "N/A",
                        soLuong = 1, // Mỗi chi tiết là 1 sản phẩm với IMEI
                        donGia = c.DonGia ?? 0,
                        giaKhuyenMai = c.GiaKhuyenMai,
                        thanhTien = c.ThanhTien ?? 0,
                        hinhAnh = c.ModelSanPham?.AnhSanPhams?.FirstOrDefault()?.DuongDan ?? "/images/default-product.jpg"
                    }).ToList()
                }).ToList();

                return Json(new { success = true, data = orders });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: API - Lấy chi tiết đơn hàng offline
        [HttpGet]
        public async Task<IActionResult> GetOfflineOrderDetail(int id)
        {
            try
            {
                var donHang = await _context.DonHangs
                    .Include(d => d.KhachHang)
                    .Include(d => d.NhanVien)
                    .Include(d => d.DonHangChiTiets)
                        .ThenInclude(c => c.ModelSanPham)
                            .ThenInclude(m => m.SanPham)
                    .Include(d => d.DonHangChiTiets)
                        .ThenInclude(c => c.ModelSanPham)
                            .ThenInclude(m => m.AnhSanPhams)
                    .FirstOrDefaultAsync(d => d.IdDonHang == id);

                if (donHang == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
                }

                // Lấy IMEI cho từng chi tiết
                var chiTietList = new List<object>();
                foreach (var chiTiet in donHang.DonHangChiTiets ?? new List<DonHangChiTiet>())
                {
                    var imeis = new List<string>();
                    if (chiTiet.IdModelSanPham.HasValue)
                    {
                        imeis = await _context.Imeis
                            .Where(i => i.IdModelSanPham == chiTiet.IdModelSanPham && i.TrangThai == "Đã bán")
                            .Select(i => i.MaImei ?? "")
                            .ToListAsync();
                    }

                    chiTietList.Add(new
                    {
                        tenSanPham = chiTiet.ModelSanPham?.SanPham?.TenSanPham ?? "N/A",
                        tenModel = chiTiet.ModelSanPham?.TenModel ?? "N/A",
                        mau = chiTiet.ModelSanPham?.Mau ?? "N/A",
                        ram = chiTiet.ModelSanPham?.RAM?.DungLuongRAM ?? "N/A",
                        rom = chiTiet.ModelSanPham?.ROM?.DungLuongROM ?? "N/A",
                        soLuong = 1,
                        donGia = chiTiet.DonGia ?? 0,
                        giaKhuyenMai = chiTiet.GiaKhuyenMai,
                        thanhTien = chiTiet.ThanhTien ?? 0,
                        hinhAnh = chiTiet.ModelSanPham?.AnhSanPhams?.FirstOrDefault()?.DuongDan ?? "/images/default-product.jpg",
                        imeis = imeis
                    });
                }

                // Parse thông tin thanh toán từ GhiChu
                decimal? tienKhachDua = null;
                decimal? tienThua = null;
                if (!string.IsNullOrWhiteSpace(donHang.GhiChu) && donHang.GhiChu.Contains("TIEN_KHACH_DUA:"))
                {
                    var parts = donHang.GhiChu.Split('|');
                    foreach (var part in parts)
                    {
                        if (part.StartsWith("TIEN_KHACH_DUA:"))
                        {
                            var value = part.Replace("TIEN_KHACH_DUA:", "").Replace(",", "");
                            if (decimal.TryParse(value, out decimal tienDua))
                                tienKhachDua = tienDua;
                        }
                        else if (part.StartsWith("TIEN_THUA:"))
                        {
                            var value = part.Replace("TIEN_THUA:", "").Replace(",", "");
                            if (decimal.TryParse(value, out decimal tienThuaValue))
                                tienThua = tienThuaValue;
                        }
                    }
                }

                var result = new
                {
                    idDonHang = donHang.IdDonHang,
                    maDon = donHang.MaDon ?? $"DH{donHang.IdDonHang:D6}",
                    ngayDat = donHang.NgayDat?.ToString("dd/MM/yyyy HH:mm"),
                    trangThai = donHang.TrangThaiHoaDon ?? "Đã thanh toán",
                    trangThaiSo = donHang.TrangThaiDH ?? 1,
                    tongTien = donHang.DonHangChiTiets?.Sum(c => c.ThanhTien ?? 0) ?? 0,
                    phuongThucThanhToan = donHang.PhuongThucThanhToan ?? "Tiền mặt",
                    hoTenKhachHang = donHang.KhachHang?.HoTenKhachHang ?? donHang.HoTenNguoiNhan ?? "N/A",
                    sdtKhachHang = donHang.KhachHang?.SdtKhachHang ?? donHang.SdtNguoiNhan ?? "N/A",
                    hoTenNhanVien = donHang.NhanVien?.HoTenNhanVien ?? "N/A",
                    tienKhachDua = tienKhachDua,
                    tienThua = tienThua,
                    chiTiet = chiTietList
                };

                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

    }

    // ViewModel cho trang Index
    public class OrderViewModel
    {
        public List<KhachHang> KhachHangList { get; set; }
        public List<ModelSanPham> SanPhamList { get; set; }
    }

    // Model cho tạo đơn hàng
    public class OrderCreateModel
    {
        public int? IdKhachHang { get; set; }
        public string TenKhachHang { get; set; }
        public string SdtKhachHang { get; set; }
        public string DiaChiGiaoHang { get; set; }
        public string PhuongThucThanhToan { get; set; }
        public decimal? TienKhachDua { get; set; }
        public decimal? TienThua { get; set; }
        public List<OrderItemModel> ChiTietDonHang { get; set; }
    }

    public class OrderItemModel
    {
        public int IdModelSanPham { get; set; }
        public int SoLuong { get; set; }
        public List<int> SelectedImeis { get; set; }
        public int? IdKhuyenMai { get; set; }
    }

    // ViewModel cho hóa đơn
    public class InvoiceViewModel
    {
        public DonHang DonHang { get; set; }
        public List<InvoiceChiTietViewModel> ChiTietList { get; set; }
        public decimal? TienKhachDua { get; set; }
        public decimal? TienThua { get; set; }
        public decimal TongTien { get; set; }
    }

    public class InvoiceChiTietViewModel
    {
        public DonHangChiTiet ChiTiet { get; set; }
        public List<string> Imeis { get; set; }
    }

    public class QrThanhToanViewModel
    {
        public string TenNganHang { get; set; }
        public string SoTaiKhoan { get; set; }
        public string ChuTaiKhoan { get; set; }
        public decimal SoTien { get; set; }
        public string NoiDung { get; set; }
        public string QrImageUrl { get; set; }
        public string MaDon { get; set; }
    }

}