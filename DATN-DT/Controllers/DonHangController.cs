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
                    .Where(sp => sp.TrangThaiSP != "Ngừng kinh doanh"); // Chỉ lấy sản phẩm đang kinh doanh

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

            if (ModelState.IsValid)
            {
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

                                    // Tạo bảo hành
                                    var baoHanh = new BaoHanh
                                    {
                                        IdImei = imei.IdImei,
                                        IdKhachHang = orderModel.IdKhachHang,
                                        NgayNhan = DateTime.Now,
                                        NgayTra = DateTime.Now.AddYears(1), // Bảo hành 1 năm
                                        TrangThai = "Đang bảo hành",
                                        MoTaLoi = "Mới mua",
                                        XuLy = "Hoạt động bình thường"
                                    };
                                    _context.BaoHanhs.Add(baoHanh);
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
                                    imeis = await _context.Imeis
                                        .Where(i => item.SelectedImeis.Contains(i.IdImei))
                                        .Select(i => i.MaImei ?? "")
                                        .ToListAsync();
                                }

                                chiTietListForPdf.Add(new InvoiceChiTietViewModel
                                {
                                    ChiTiet = chiTiet,
                                    Imeis = imeis
                                });
                            }
                        }

                        var invoiceViewModel = new InvoiceViewModel
                        {
                            DonHang = donHang,
                            ChiTietList = chiTietListForPdf
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

                var viewModel = new InvoiceViewModel
                {
                    DonHang = donHang,
                    ChiTietList = chiTietList
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

                var viewModel = new InvoiceViewModel
                {
                    DonHang = donHang,
                    ChiTietList = chiTietList
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

                // Trả về file để download
                return File(pdfBytes, "application/pdf", fileName);
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
    }

    public class InvoiceChiTietViewModel
    {
        public DonHangChiTiet ChiTiet { get; set; }
        public List<string> Imeis { get; set; }
    }
}