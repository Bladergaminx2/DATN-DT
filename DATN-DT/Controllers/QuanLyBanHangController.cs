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

                // Lấy các hóa đơn online (bán hàng online)
                var hoaDons = await _context.HoaDons
                    .Include(h => h.KhachHang)
                    .Include(h => h.NhanVien)
                    .Include(h => h.HoaDonChiTiets)
                        .ThenInclude(hdct => hdct.ModelSanPham)
                            .ThenInclude(m => m.SanPham)
                    .Include(h => h.HoaDonChiTiets)
                        .ThenInclude(hdct => hdct.ModelSanPham)
                            .ThenInclude(m => m.RAM)
                    .Include(h => h.HoaDonChiTiets)
                        .ThenInclude(hdct => hdct.ModelSanPham)
                            .ThenInclude(m => m.ROM)
                    .Include(h => h.HoaDonChiTiets)
                        .ThenInclude(hdct => hdct.ModelSanPham)
                            .ThenInclude(m => m.AnhSanPhams)
                    .Include(h => h.HoaDonChiTiets)
                        .ThenInclude(hdct => hdct.Imei)
                    .Where(h => h.NgayLapHoaDon >= fromDate.Value &&
                                h.NgayLapHoaDon <= toDate.Value)
                    .OrderByDescending(h => h.NgayLapHoaDon)
                    .ToListAsync();

                // Tính tổng doanh thu offline
                decimal tongDoanhThuOff = 0;
                var donHangList = new List<object>();

                foreach (var donHang in donHangs)
                {
                    var tongTienDon = donHang.DonHangChiTiets?.Sum(c => c.ThanhTien ?? 0) ?? 0;
                    tongDoanhThuOff += tongTienDon;

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

                // Tính tổng doanh thu online
                decimal tongDoanhThuOn = 0;
                var hoaDonList = new List<object>();

                foreach (var hoaDon in hoaDons)
                {
                    var tongTienHoaDon = hoaDon.TongTien ?? 0;
                    tongDoanhThuOn += tongTienHoaDon;

                    var chiTietList = new List<object>();
                    if (hoaDon.HoaDonChiTiets != null)
                    {
                        foreach (var chiTiet in hoaDon.HoaDonChiTiets)
                        {
                            var model = chiTiet.ModelSanPham;
                            var sanPham = model?.SanPham;

                            // Lấy IMEI
                            var imeis = new List<string>();
                            if (chiTiet.IdImei.HasValue)
                            {
                                var imei = await _context.Imeis
                                    .FirstOrDefaultAsync(i => i.IdImei == chiTiet.IdImei.Value);
                                if (imei != null && !string.IsNullOrEmpty(imei.MaImei))
                                {
                                    imeis.Add(imei.MaImei);
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

                    hoaDonList.Add(new
                    {
                        IdHoaDon = hoaDon.IdHoaDon,
                        MaDon = $"HD{hoaDon.IdHoaDon:D6}",
                        NgayDat = hoaDon.NgayLapHoaDon?.ToString("dd/MM/yyyy HH:mm:ss") ?? "N/A",
                        TenKhachHang = !string.IsNullOrWhiteSpace(
                            hoaDon.KhachHang?.HoTenKhachHang ?? hoaDon.HoTenNguoiNhan)
                            ? (hoaDon.KhachHang?.HoTenKhachHang ?? hoaDon.HoTenNguoiNhan)
                            : "Khách vãng lai",
                        SdtKhachHang = !string.IsNullOrWhiteSpace(
                            hoaDon.KhachHang?.SdtKhachHang ?? hoaDon.SdtKhachHang)
                            ? (hoaDon.KhachHang?.SdtKhachHang ?? hoaDon.SdtKhachHang)
                            : "N/A",
                        PhuongThucThanhToan = !string.IsNullOrWhiteSpace(hoaDon.PhuongThucThanhToan)
                            ? hoaDon.PhuongThucThanhToan
                            : "Chưa xác định",
                        TenNhanVien = !string.IsNullOrWhiteSpace(hoaDon.NhanVien?.HoTenNhanVien)
                            ? hoaDon.NhanVien.HoTenNhanVien
                            : "N/A",
                        TongTien = tongTienHoaDon,
                        ChiTiet = chiTietList
                    });
                }

                // Tổng doanh thu (online + offline)
                decimal tongDoanhThu = tongDoanhThuOn + tongDoanhThuOff;

                return Json(new
                {
                    success = true,
                    fromDate = fromDate.Value.ToString("dd/MM/yyyy"),
                    toDate = toDate.Value.ToString("dd/MM/yyyy"),
                    tongDoanhThu = tongDoanhThu,
                    tongDoanhThuOn = tongDoanhThuOn,
                    tongDoanhThuOff = tongDoanhThuOff,
                    soLuongDonHangOn = hoaDons.Count,
                    soLuongDonHangOff = donHangs.Count,
                    donHangs = donHangList,
                    hoaDons = hoaDonList
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

                // Lấy dữ liệu offline
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

                // Lấy dữ liệu online
                var hoaDons = await _context.HoaDons
                    .Include(h => h.KhachHang)
                    .Include(h => h.HoaDonChiTiets)
                        .ThenInclude(hdct => hdct.ModelSanPham)
                            .ThenInclude(m => m.SanPham)
                    .Where(h => h.NgayLapHoaDon >= fromDate.Value &&
                                h.NgayLapHoaDon <= toDate.Value)
                    .OrderByDescending(h => h.NgayLapHoaDon)
                    .ToListAsync();

                // Tạo Excel
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new ExcelPackage())
                {
                    // Sheet 1: Tổng quan
                    var summarySheet = package.Workbook.Worksheets.Add("Tổng quan");
                    summarySheet.Cells[1, 1].Value = "BÁO CÁO BÁN HÀNG TỔNG HỢP";
                    summarySheet.Cells[1, 1, 1, 4].Merge = true;
                    summarySheet.Cells[1, 1].Style.Font.Size = 16;
                    summarySheet.Cells[1, 1].Style.Font.Bold = true;

                    summarySheet.Cells[3, 1].Value = "Từ ngày:";
                    summarySheet.Cells[3, 2].Value = fromDate.Value.ToString("dd/MM/yyyy");
                    summarySheet.Cells[4, 1].Value = "Đến ngày:";
                    summarySheet.Cells[4, 2].Value = toDate.Value.ToString("dd/MM/yyyy");

                    decimal tongDoanhThuOff = 0;
                    foreach (var donHang in donHangs)
                    {
                        tongDoanhThuOff += donHang.DonHangChiTiets?.Sum(c => c.ThanhTien ?? 0) ?? 0;
                    }

                    decimal tongDoanhThuOn = 0;
                    foreach (var hoaDon in hoaDons)
                    {
                        tongDoanhThuOn += hoaDon.TongTien ?? 0;
                    }

                    decimal tongDoanhThu = tongDoanhThuOn + tongDoanhThuOff;

                    summarySheet.Cells[6, 1].Value = "Tổng doanh thu:";
                    summarySheet.Cells[6, 2].Value = tongDoanhThu;
                    summarySheet.Cells[6, 2].Style.Numberformat.Format = "#,##0";
                    summarySheet.Cells[6, 1].Style.Font.Bold = true;
                    summarySheet.Cells[6, 2].Style.Font.Bold = true;

                    summarySheet.Cells[7, 1].Value = "Doanh thu Online:";
                    summarySheet.Cells[7, 2].Value = tongDoanhThuOn;
                    summarySheet.Cells[7, 2].Style.Numberformat.Format = "#,##0";
                    summarySheet.Cells[7, 1].Style.Font.Bold = true;
                    summarySheet.Cells[7, 2].Style.Font.Bold = true;

                    summarySheet.Cells[8, 1].Value = "Doanh thu Offline:";
                    summarySheet.Cells[8, 2].Value = tongDoanhThuOff;
                    summarySheet.Cells[8, 2].Style.Numberformat.Format = "#,##0";
                    summarySheet.Cells[8, 1].Style.Font.Bold = true;
                    summarySheet.Cells[8, 2].Style.Font.Bold = true;

                    summarySheet.Cells[9, 1].Value = "Số lượng đơn hàng Online:";
                    summarySheet.Cells[9, 2].Value = hoaDons.Count;
                    summarySheet.Cells[9, 1].Style.Font.Bold = true;
                    summarySheet.Cells[9, 2].Style.Font.Bold = true;

                    summarySheet.Cells[10, 1].Value = "Số lượng đơn hàng Offline:";
                    summarySheet.Cells[10, 2].Value = donHangs.Count;
                    summarySheet.Cells[10, 1].Style.Font.Bold = true;
                    summarySheet.Cells[10, 2].Style.Font.Bold = true;

                    // Sheet 2: Danh sách hóa đơn Online
                    var invoiceSheetOn = package.Workbook.Worksheets.Add("Hóa đơn Online");
                    invoiceSheetOn.Cells[1, 1].Value = "Mã đơn";
                    invoiceSheetOn.Cells[1, 2].Value = "Ngày đặt";
                    invoiceSheetOn.Cells[1, 3].Value = "Tên khách hàng";
                    invoiceSheetOn.Cells[1, 4].Value = "Số điện thoại";
                    invoiceSheetOn.Cells[1, 5].Value = "Phương thức thanh toán";
                    invoiceSheetOn.Cells[1, 6].Value = "Tổng tiền";
                    
                    var headerRangeOn = invoiceSheetOn.Cells[1, 1, 1, 6];
                    headerRangeOn.Style.Font.Bold = true;
                    headerRangeOn.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    headerRangeOn.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);

                    int row = 2;
                    foreach (var hoaDon in hoaDons)
                    {
                        invoiceSheetOn.Cells[row, 1].Value = $"HD{hoaDon.IdHoaDon:D6}";
                        invoiceSheetOn.Cells[row, 2].Value = hoaDon.NgayLapHoaDon?.ToString("dd/MM/yyyy HH:mm:ss");
                        invoiceSheetOn.Cells[row, 3].Value = hoaDon.KhachHang?.HoTenKhachHang ?? hoaDon.HoTenNguoiNhan;
                        invoiceSheetOn.Cells[row, 4].Value = hoaDon.KhachHang?.SdtKhachHang ?? hoaDon.SdtKhachHang;
                        invoiceSheetOn.Cells[row, 5].Value = hoaDon.PhuongThucThanhToan;
                        invoiceSheetOn.Cells[row, 6].Value = hoaDon.TongTien ?? 0;
                        invoiceSheetOn.Cells[row, 6].Style.Numberformat.Format = "#,##0";
                        row++;
                    }

                    invoiceSheetOn.Cells.AutoFitColumns();

                    // Sheet 3: Danh sách hóa đơn Offline
                    var invoiceSheetOff = package.Workbook.Worksheets.Add("Hóa đơn Offline");
                    invoiceSheetOff.Cells[1, 1].Value = "Mã đơn";
                    invoiceSheetOff.Cells[1, 2].Value = "Ngày đặt";
                    invoiceSheetOff.Cells[1, 3].Value = "Tên khách hàng";
                    invoiceSheetOff.Cells[1, 4].Value = "Số điện thoại";
                    invoiceSheetOff.Cells[1, 5].Value = "Phương thức thanh toán";
                    invoiceSheetOff.Cells[1, 6].Value = "Tổng tiền";
                    
                    var headerRangeOff = invoiceSheetOff.Cells[1, 1, 1, 6];
                    headerRangeOff.Style.Font.Bold = true;
                    headerRangeOff.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    headerRangeOff.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightPink);

                    row = 2;
                    foreach (var donHang in donHangs)
                    {
                        var tongTien = donHang.DonHangChiTiets?.Sum(c => c.ThanhTien ?? 0) ?? 0;
                        invoiceSheetOff.Cells[row, 1].Value = donHang.MaDon;
                        invoiceSheetOff.Cells[row, 2].Value = donHang.NgayDat?.ToString("dd/MM/yyyy HH:mm:ss");
                        invoiceSheetOff.Cells[row, 3].Value = donHang.KhachHang?.HoTenKhachHang ?? donHang.HoTenNguoiNhan;
                        invoiceSheetOff.Cells[row, 4].Value = donHang.KhachHang?.SdtKhachHang ?? donHang.SdtNguoiNhan;
                        invoiceSheetOff.Cells[row, 5].Value = donHang.PhuongThucThanhToan;
                        invoiceSheetOff.Cells[row, 6].Value = tongTien;
                        invoiceSheetOff.Cells[row, 6].Style.Numberformat.Format = "#,##0";
                        row++;
                    }

                    invoiceSheetOff.Cells.AutoFitColumns();

                    // Sheet 4: Chi tiết hóa đơn Online
                    var detailSheetOn = package.Workbook.Worksheets.Add("Chi tiết Online");
                    detailSheetOn.Cells[1, 1].Value = "Mã đơn";
                    detailSheetOn.Cells[1, 2].Value = "Ngày đặt";
                    detailSheetOn.Cells[1, 3].Value = "Tên khách hàng";
                    detailSheetOn.Cells[1, 4].Value = "Số điện thoại";
                    detailSheetOn.Cells[1, 5].Value = "Tên sản phẩm";
                    detailSheetOn.Cells[1, 6].Value = "Tên model";
                    detailSheetOn.Cells[1, 7].Value = "IMEI";
                    detailSheetOn.Cells[1, 8].Value = "Số lượng";
                    detailSheetOn.Cells[1, 9].Value = "Đơn giá";
                    detailSheetOn.Cells[1, 10].Value = "Thành tiền";

                    var detailHeaderRangeOn = detailSheetOn.Cells[1, 1, 1, 10];
                    detailHeaderRangeOn.Style.Font.Bold = true;
                    detailHeaderRangeOn.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    detailHeaderRangeOn.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);

                    row = 2;
                    foreach (var hoaDon in hoaDons)
                    {
                        if (hoaDon.HoaDonChiTiets != null)
                        {
                            foreach (var chiTiet in hoaDon.HoaDonChiTiets)
                            {
                                var model = chiTiet.ModelSanPham;
                                var sanPham = model?.SanPham;

                                // Lấy IMEI
                                var imeiStr = "N/A";
                                if (chiTiet.IdImei.HasValue)
                                {
                                    var imei = await _context.Imeis.FirstOrDefaultAsync(i => i.IdImei == chiTiet.IdImei.Value);
                                    if (imei != null && !string.IsNullOrEmpty(imei.MaImei))
                                    {
                                        imeiStr = imei.MaImei;
                                    }
                                }

                                detailSheetOn.Cells[row, 1].Value = $"HD{hoaDon.IdHoaDon:D6}";
                                detailSheetOn.Cells[row, 2].Value = hoaDon.NgayLapHoaDon?.ToString("dd/MM/yyyy HH:mm:ss");
                                detailSheetOn.Cells[row, 3].Value = hoaDon.KhachHang?.HoTenKhachHang ?? hoaDon.HoTenNguoiNhan;
                                detailSheetOn.Cells[row, 4].Value = hoaDon.KhachHang?.SdtKhachHang ?? hoaDon.SdtKhachHang;
                                detailSheetOn.Cells[row, 5].Value = sanPham?.TenSanPham ?? "N/A";
                                detailSheetOn.Cells[row, 6].Value = model?.TenModel ?? "N/A";
                                detailSheetOn.Cells[row, 7].Value = imeiStr;
                                detailSheetOn.Cells[row, 8].Value = chiTiet.SoLuong ?? 0;
                                detailSheetOn.Cells[row, 9].Value = chiTiet.DonGia ?? 0;
                                detailSheetOn.Cells[row, 9].Style.Numberformat.Format = "#,##0";
                                detailSheetOn.Cells[row, 10].Value = chiTiet.ThanhTien ?? 0;
                                detailSheetOn.Cells[row, 10].Style.Numberformat.Format = "#,##0";
                                row++;
                            }
                        }
                    }

                    detailSheetOn.Cells.AutoFitColumns();

                    // Sheet 5: Chi tiết hóa đơn Offline
                    var detailSheetOff = package.Workbook.Worksheets.Add("Chi tiết Offline");
                    detailSheetOff.Cells[1, 1].Value = "Mã đơn";
                    detailSheetOff.Cells[1, 2].Value = "Ngày đặt";
                    detailSheetOff.Cells[1, 3].Value = "Tên khách hàng";
                    detailSheetOff.Cells[1, 4].Value = "Số điện thoại";
                    detailSheetOff.Cells[1, 5].Value = "Tên sản phẩm";
                    detailSheetOff.Cells[1, 6].Value = "Tên model";
                    detailSheetOff.Cells[1, 7].Value = "IMEI";
                    detailSheetOff.Cells[1, 8].Value = "Số lượng";
                    detailSheetOff.Cells[1, 9].Value = "Đơn giá";
                    detailSheetOff.Cells[1, 10].Value = "Thành tiền";

                    var detailHeaderRangeOff = detailSheetOff.Cells[1, 1, 1, 10];
                    detailHeaderRangeOff.Style.Font.Bold = true;
                    detailHeaderRangeOff.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    detailHeaderRangeOff.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightPink);

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

                                detailSheetOff.Cells[row, 1].Value = donHang.MaDon;
                                detailSheetOff.Cells[row, 2].Value = donHang.NgayDat?.ToString("dd/MM/yyyy HH:mm:ss");
                                detailSheetOff.Cells[row, 3].Value = donHang.KhachHang?.HoTenKhachHang ?? donHang.HoTenNguoiNhan;
                                detailSheetOff.Cells[row, 4].Value = donHang.KhachHang?.SdtKhachHang ?? donHang.SdtNguoiNhan;
                                detailSheetOff.Cells[row, 5].Value = sanPham?.TenSanPham ?? "N/A";
                                detailSheetOff.Cells[row, 6].Value = model?.TenModel ?? "N/A";
                                detailSheetOff.Cells[row, 7].Value = string.Join(", ", imeis);
                                detailSheetOff.Cells[row, 8].Value = chiTiet.SoLuong ?? 0;
                                detailSheetOff.Cells[row, 9].Value = chiTiet.DonGia ?? 0;
                                detailSheetOff.Cells[row, 9].Style.Numberformat.Format = "#,##0";
                                detailSheetOff.Cells[row, 10].Value = chiTiet.ThanhTien ?? 0;
                                detailSheetOff.Cells[row, 10].Style.Numberformat.Format = "#,##0";
                                row++;
                            }
                        }
                    }

                    detailSheetOff.Cells.AutoFitColumns();

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
