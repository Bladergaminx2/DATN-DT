using DATN_DT.Data;
using DATN_DT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DATN_DT.Controllers
{
    [AllowAnonymous]
    public class QuanLyBanHangController : Controller
    {
        private readonly MyDbContext _context;

        public QuanLyBanHangController(MyDbContext context)
        {
            _context = context;
        }

        // GET: QuanLyBanHang
        public IActionResult Index()
        {
            return View();
        }

        // GET: QuanLyBanHang/GetData
        [HttpGet]
        public async Task<IActionResult> GetData(DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                // Mặc định là hôm nay nếu không có tham số
                if (!fromDate.HasValue && !toDate.HasValue)
                {
                    fromDate = DateTime.Today;
                    toDate = DateTime.Today.AddDays(1).AddSeconds(-1);
                }
                else if (fromDate.HasValue && !toDate.HasValue)
                {
                    toDate = fromDate.Value.AddDays(1).AddSeconds(-1);
                }
                else if (!fromDate.HasValue && toDate.HasValue)
                {
                    fromDate = toDate.Value.Date;
                }
                else
                {
                    // Nếu có cả 2, đảm bảo toDate bao gồm cả ngày cuối
                    toDate = toDate.Value.AddDays(1).AddSeconds(-1);
                }

                // Lấy các đơn hàng đã thanh toán (bán hàng offline)
                var donHangs = await _context.DonHangs
                    .Include(d => d.KhachHang)
                    .Include(d => d.NhanVien)
                    .Include(d => d.DonHangChiTiets)
                        .ThenInclude(c => c.ModelSanPham)
                            .ThenInclude(m => m.SanPham)
                    .Include(d => d.DonHangChiTiets)
                        .ThenInclude(c => c.ModelSanPham)
                            .ThenInclude(m => m.RAM)
                    .Include(d => d.DonHangChiTiets)
                        .ThenInclude(c => c.ModelSanPham)
                            .ThenInclude(m => m.ROM)
                    .Include(d => d.DonHangChiTiets)
                        .ThenInclude(c => c.ModelSanPham)
                            .ThenInclude(m => m.AnhSanPhams)
                    .Where(d => d.TrangThaiHoaDon == "Đã thanh toán" &&
                                d.NgayDat >= fromDate.Value &&
                                d.NgayDat <= toDate.Value)
                    .OrderByDescending(d => d.NgayDat)
                    .ToListAsync();

                // Tính tổng doanh thu
                decimal tongDoanhThu = 0;
                var donHangList = new List<object>();

                foreach (var donHang in donHangs)
                {
                    var tongTienDon = donHang.DonHangChiTiets?.Sum(c => c.ThanhTien ?? 0) ?? 0;
                    tongDoanhThu += tongTienDon;

                    var chiTietList = new List<object>();
                    if (donHang.DonHangChiTiets != null)
                    {
                        foreach (var chiTiet in donHang.DonHangChiTiets)
                        {
                            var model = chiTiet.ModelSanPham;
                            var sanPham = model?.SanPham;

                            // Lấy IMEI thông qua BaoHanh (IMEI được bán trong khoảng thời gian của đơn hàng)
                            var imeis = new List<string>();
                            if (chiTiet.IdModelSanPham.HasValue)
                            {
                                // Lấy IMEI thông qua BaoHanh được tạo trong ngày và giờ của đơn hàng (trong vòng 1 phút)
                                var ngayDat = donHang.NgayDat.Value;
                                var imeiIds = await _context.BaoHanhs
                                    .Where(b => ((donHang.IdKhachHang.HasValue && b.IdKhachHang == donHang.IdKhachHang) ||
                                                 (!donHang.IdKhachHang.HasValue && b.NgayNhan >= ngayDat.AddMinutes(-1) && b.NgayNhan <= ngayDat.AddMinutes(1))) &&
                                                b.NgayNhan >= ngayDat.Date &&
                                                b.NgayNhan < ngayDat.Date.AddDays(1) &&
                                                b.IdImei.HasValue)
                                    .Select(b => b.IdImei.Value)
                                    .ToListAsync();

                                if (imeiIds.Any())
                                {
                                    imeis = await _context.Imeis
                                        .Where(i => imeiIds.Contains(i.IdImei) && 
                                                    i.IdModelSanPham == chiTiet.IdModelSanPham)
                                        .Select(i => i.MaImei ?? "")
                                        .ToListAsync();
                                }
                                
                                // Nếu không tìm thấy qua BaoHanh, lấy IMEI "Đã bán" của model theo số lượng (fallback)
                                if (!imeis.Any() || imeis.Count < chiTiet.SoLuong)
                                {
                                    var soLuongCanLay = (chiTiet.SoLuong ?? 1) - imeis.Count;
                                    var themImeis = await _context.Imeis
                                        .Where(i => i.IdModelSanPham == chiTiet.IdModelSanPham && 
                                                    i.TrangThai == "Đã bán" &&
                                                    !imeis.Contains(i.MaImei ?? ""))
                                        .Take(soLuongCanLay)
                                        .Select(i => i.MaImei ?? "")
                                        .ToListAsync();
                                    imeis.AddRange(themImeis);
                                }
                            }

                            // Lấy ảnh đầu tiên của model
                            var anhDauTien = await _context.AnhSanPhams
                                .Where(a => a.IdModelSanPham == chiTiet.IdModelSanPham)
                                .Select(a => a.DuongDan)
                                .FirstOrDefaultAsync();

                            chiTietList.Add(new
                            {
                                TenSanPham = sanPham?.TenSanPham ?? "N/A",
                                TenModel = model?.TenModel ?? "N/A",
                                SoLuong = chiTiet.SoLuong ?? 0,
                                DonGia = chiTiet.DonGia ?? 0,
                                ThanhTien = chiTiet.ThanhTien ?? 0,
                                Imeis = imeis,
                                RAM = model?.RAM?.DungLuongRAM ?? "N/A",
                                ROM = model?.ROM?.DungLuongROM ?? "N/A",
                                Mau = model?.Mau ?? "N/A",
                                AnhSanPham = anhDauTien ?? ""
                            });
                        }
                    }

                    donHangList.Add(new
                    {
                        IdDonHang = donHang.IdDonHang,
                        MaDon = !string.IsNullOrWhiteSpace(donHang.MaDon) 
                            ? donHang.MaDon 
                            : $"DH{donHang.IdDonHang}",
                        NgayDat = donHang.NgayDat?.ToString("dd/MM/yyyy HH:mm:ss") ?? "N/A",
                        TenKhachHang = !string.IsNullOrWhiteSpace(
                            donHang.KhachHang?.HoTenKhachHang ?? donHang.HoTenNguoiNhan)
                            ? (donHang.KhachHang?.HoTenKhachHang ?? donHang.HoTenNguoiNhan)
                            : "Khách vãng lai",
                        SdtKhachHang = !string.IsNullOrWhiteSpace(
                            donHang.KhachHang?.SdtKhachHang ?? donHang.SdtNguoiNhan)
                            ? (donHang.KhachHang?.SdtKhachHang ?? donHang.SdtNguoiNhan)
                            : "N/A",
                        PhuongThucThanhToan = !string.IsNullOrWhiteSpace(donHang.PhuongThucThanhToan)
                            ? donHang.PhuongThucThanhToan
                            : "Chưa xác định",
                        TenNhanVien = !string.IsNullOrWhiteSpace(donHang.NhanVien?.HoTenNhanVien)
                            ? donHang.NhanVien.HoTenNhanVien
                            : "N/A",
                        TongTien = tongTienDon,
                        ChiTiet = chiTietList
                    });
                }

                return Json(new
                {
                    success = true,
                    fromDate = fromDate.Value.ToString("dd/MM/yyyy"),
                    toDate = toDate.Value.ToString("dd/MM/yyyy"),
                    tongDoanhThu = tongDoanhThu,
                    soLuongDonHang = donHangs.Count,
                    donHangs = donHangList
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // GET: QuanLyBanHang/ExportExcel
        [HttpGet]
        public async Task<IActionResult> ExportExcel(DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                // Mặc định là hôm nay nếu không có tham số
                if (!fromDate.HasValue && !toDate.HasValue)
                {
                    fromDate = DateTime.Today;
                    toDate = DateTime.Today.AddDays(1).AddSeconds(-1);
                }
                else if (fromDate.HasValue && !toDate.HasValue)
                {
                    toDate = fromDate.Value.AddDays(1).AddSeconds(-1);
                }
                else if (!fromDate.HasValue && toDate.HasValue)
                {
                    fromDate = toDate.Value.Date;
                }
                else
                {
                    toDate = toDate.Value.AddDays(1).AddSeconds(-1);
                }

                // Lấy dữ liệu
                var donHangs = await _context.DonHangs
                    .Include(d => d.KhachHang)
                    .Include(d => d.DonHangChiTiets)
                        .ThenInclude(c => c.ModelSanPham)
                            .ThenInclude(m => m.SanPham)
                    .Where(d => d.TrangThaiHoaDon == "Đã thanh toán" &&
                                d.NgayDat >= fromDate.Value &&
                                d.NgayDat <= toDate.Value)
                    .OrderByDescending(d => d.NgayDat)
                    .ToListAsync();

                // Tạo Excel
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new ExcelPackage())
                {
                    // Sheet 1: Tổng quan
                    var summarySheet = package.Workbook.Worksheets.Add("Tổng quan");
                    summarySheet.Cells[1, 1].Value = "BÁO CÁO BÁN HÀNG OFFLINE";
                    summarySheet.Cells[1, 1, 1, 4].Merge = true;
                    summarySheet.Cells[1, 1].Style.Font.Size = 16;
                    summarySheet.Cells[1, 1].Style.Font.Bold = true;

                    summarySheet.Cells[3, 1].Value = "Từ ngày:";
                    summarySheet.Cells[3, 2].Value = fromDate.Value.ToString("dd/MM/yyyy");
                    summarySheet.Cells[4, 1].Value = "Đến ngày:";
                    summarySheet.Cells[4, 2].Value = toDate.Value.ToString("dd/MM/yyyy");

                    decimal tongDoanhThu = 0;
                    foreach (var donHang in donHangs)
                    {
                        tongDoanhThu += donHang.DonHangChiTiets?.Sum(c => c.ThanhTien ?? 0) ?? 0;
                    }

                    summarySheet.Cells[6, 1].Value = "Tổng doanh thu:";
                    summarySheet.Cells[6, 2].Value = tongDoanhThu;
                    summarySheet.Cells[6, 2].Style.Numberformat.Format = "#,##0";
                    summarySheet.Cells[6, 1].Style.Font.Bold = true;
                    summarySheet.Cells[6, 2].Style.Font.Bold = true;

                    summarySheet.Cells[7, 1].Value = "Số lượng đơn hàng:";
                    summarySheet.Cells[7, 2].Value = donHangs.Count;
                    summarySheet.Cells[7, 1].Style.Font.Bold = true;
                    summarySheet.Cells[7, 2].Style.Font.Bold = true;

                    // Sheet 2: Danh sách hóa đơn
                    var invoiceSheet = package.Workbook.Worksheets.Add("Danh sách hóa đơn");
                    invoiceSheet.Cells[1, 1].Value = "Mã đơn";
                    invoiceSheet.Cells[1, 2].Value = "Ngày đặt";
                    invoiceSheet.Cells[1, 3].Value = "Tên khách hàng";
                    invoiceSheet.Cells[1, 4].Value = "Số điện thoại";
                    invoiceSheet.Cells[1, 5].Value = "Phương thức thanh toán";
                    invoiceSheet.Cells[1, 6].Value = "Tổng tiền";
                    
                    var headerRange = invoiceSheet.Cells[1, 1, 1, 6];
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                    int row = 2;
                    foreach (var donHang in donHangs)
                    {
                        var tongTien = donHang.DonHangChiTiets?.Sum(c => c.ThanhTien ?? 0) ?? 0;
                        invoiceSheet.Cells[row, 1].Value = donHang.MaDon;
                        invoiceSheet.Cells[row, 2].Value = donHang.NgayDat?.ToString("dd/MM/yyyy HH:mm:ss");
                        invoiceSheet.Cells[row, 3].Value = donHang.KhachHang?.HoTenKhachHang ?? donHang.HoTenNguoiNhan;
                        invoiceSheet.Cells[row, 4].Value = donHang.KhachHang?.SdtKhachHang ?? donHang.SdtNguoiNhan;
                        invoiceSheet.Cells[row, 5].Value = donHang.PhuongThucThanhToan;
                        invoiceSheet.Cells[row, 6].Value = tongTien;
                        invoiceSheet.Cells[row, 6].Style.Numberformat.Format = "#,##0";
                        row++;
                    }

                    invoiceSheet.Cells.AutoFitColumns();

                    // Sheet 3: Chi tiết hóa đơn
                    var detailSheet = package.Workbook.Worksheets.Add("Chi tiết hóa đơn");
                    detailSheet.Cells[1, 1].Value = "Mã đơn";
                    detailSheet.Cells[1, 2].Value = "Ngày đặt";
                    detailSheet.Cells[1, 3].Value = "Tên khách hàng";
                    detailSheet.Cells[1, 4].Value = "Số điện thoại";
                    detailSheet.Cells[1, 5].Value = "Tên sản phẩm";
                    detailSheet.Cells[1, 6].Value = "Tên model";
                    detailSheet.Cells[1, 7].Value = "IMEI";
                    detailSheet.Cells[1, 8].Value = "Số lượng";
                    detailSheet.Cells[1, 9].Value = "Đơn giá";
                    detailSheet.Cells[1, 10].Value = "Thành tiền";

                    var detailHeaderRange = detailSheet.Cells[1, 1, 1, 10];
                    detailHeaderRange.Style.Font.Bold = true;
                    detailHeaderRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    detailHeaderRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                    row = 2;
                    foreach (var donHang in donHangs)
                    {
                        if (donHang.DonHangChiTiets != null)
                        {
                            foreach (var chiTiet in donHang.DonHangChiTiets)
                            {
                                var model = chiTiet.ModelSanPham;
                                var sanPham = model?.SanPham;

                                // Lấy IMEI thông qua BaoHanh
                                var imeis = new List<string>();
                                if (chiTiet.IdModelSanPham.HasValue)
                                {
                                    var ngayDat = donHang.NgayDat.Value;
                                    var imeiIds = await _context.BaoHanhs
                                        .Where(b => ((donHang.IdKhachHang.HasValue && b.IdKhachHang == donHang.IdKhachHang) ||
                                                     (!donHang.IdKhachHang.HasValue && b.NgayNhan >= ngayDat.AddMinutes(-1) && b.NgayNhan <= ngayDat.AddMinutes(1))) &&
                                                    b.NgayNhan >= ngayDat.Date &&
                                                    b.NgayNhan < ngayDat.Date.AddDays(1) &&
                                                    b.IdImei.HasValue)
                                        .Select(b => b.IdImei.Value)
                                        .ToListAsync();

                                    if (imeiIds.Any())
                                    {
                                        imeis = await _context.Imeis
                                            .Where(i => imeiIds.Contains(i.IdImei) && 
                                                        i.IdModelSanPham == chiTiet.IdModelSanPham)
                                            .Select(i => i.MaImei ?? "")
                                            .ToListAsync();
                                    }
                                    
                                    if (!imeis.Any() || imeis.Count < chiTiet.SoLuong)
                                    {
                                        var soLuongCanLay = (chiTiet.SoLuong ?? 1) - imeis.Count;
                                        var themImeis = await _context.Imeis
                                            .Where(i => i.IdModelSanPham == chiTiet.IdModelSanPham && 
                                                        i.TrangThai == "Đã bán" &&
                                                        !imeis.Contains(i.MaImei ?? ""))
                                            .Take(soLuongCanLay)
                                            .Select(i => i.MaImei ?? "")
                                            .ToListAsync();
                                        imeis.AddRange(themImeis);
                                    }
                                }

                                detailSheet.Cells[row, 1].Value = donHang.MaDon;
                                detailSheet.Cells[row, 2].Value = donHang.NgayDat?.ToString("dd/MM/yyyy HH:mm:ss");
                                detailSheet.Cells[row, 3].Value = donHang.KhachHang?.HoTenKhachHang ?? donHang.HoTenNguoiNhan;
                                detailSheet.Cells[row, 4].Value = donHang.KhachHang?.SdtKhachHang ?? donHang.SdtNguoiNhan;
                                detailSheet.Cells[row, 5].Value = sanPham?.TenSanPham ?? "N/A";
                                detailSheet.Cells[row, 6].Value = model?.TenModel ?? "N/A";
                                detailSheet.Cells[row, 7].Value = string.Join(", ", imeis);
                                detailSheet.Cells[row, 8].Value = chiTiet.SoLuong ?? 0;
                                detailSheet.Cells[row, 9].Value = chiTiet.DonGia ?? 0;
                                detailSheet.Cells[row, 9].Style.Numberformat.Format = "#,##0";
                                detailSheet.Cells[row, 10].Value = chiTiet.ThanhTien ?? 0;
                                detailSheet.Cells[row, 10].Style.Numberformat.Format = "#,##0";
                                row++;
                            }
                        }
                    }

                    detailSheet.Cells.AutoFitColumns();

                    // Trả về file
                    var stream = new MemoryStream();
                    package.SaveAs(stream);
                    stream.Position = 0;

                    var fileName = $"BaoCaoBanHang_{fromDate.Value:yyyyMMdd}_{toDate.Value:yyyyMMdd}.xlsx";
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi khi xuất Excel: {ex.Message}");
            }
        }
    }
}
