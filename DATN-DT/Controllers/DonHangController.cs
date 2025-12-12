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

namespace DATN_DT.Controllers
{
    [AllowAnonymous]
    public class DonHangController : Controller
    {
        private readonly MyDbContext _context;

        public DonHangController(MyDbContext context)
        {
            _context = context;
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
                var products = await _context.SanPhams
                    .Where(sp =>
                        (sp.TrangThaiSP == "Còn hàng") &&
                        (string.IsNullOrEmpty(keyword) ||
                         sp.TenSanPham.Contains(keyword)))
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
                .Where(i => i.IdModelSanPham == modelId && i.TrangThai == "Available")
                .Select(i => new { i.IdImei, i.MaImei })
                .ToListAsync();

            return Json(imeis);
        }

        // POST: Order/Create
        [HttpPost]
        public async Task<IActionResult> Create(OrderCreateModel orderModel)
        {
            if (ModelState.IsValid)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // 1. Tạo đơn hàng
                    var donHang = new DonHang
                    {
                        IdKhachHang = orderModel.IdKhachHang,
                        MaDon = "DH" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                        NgayDat = DateTime.Now,
                        HoTenNguoiNhan = string.IsNullOrWhiteSpace(orderModel.TenKhachHang) ? "Khách" : orderModel.TenKhachHang,
                        SdtNguoiNhan = orderModel.SdtKhachHang,
                        DiaChiGiaoHang = orderModel.DiaChiGiaoHang,
                        TrangThaiHoaDon = "Đã thanh toán",
                        PhuongThucThanhToan = orderModel.PhuongThucThanhToan,
                        TrangThaiDH = 1
                    };

                    _context.DonHangs.Add(donHang);
                    await _context.SaveChangesAsync();

                    decimal tongTien = 0;

                    // 2. Tạo chi tiết đơn hàng và xử lý IMEI
                    foreach (var item in orderModel.ChiTietDonHang)
                    {
                        // Lấy giá từ ModelSanPham
                        var model = await _context.ModelSanPhams
                            .FirstOrDefaultAsync(m => m.IdModelSanPham == item.IdModelSanPham);

                        if (model == null) continue;

                        var donGia = model.GiaBanModel ?? 0;
                        var thanhTien = donGia * item.SoLuong;

                        tongTien += thanhTien;

                        var chiTiet = new DonHangChiTiet
                        {
                            IdDonHang = donHang.IdDonHang,
                            IdModelSanPham = item.IdModelSanPham,
                            SoLuong = item.SoLuong,
                            DonGia = donGia,
                            ThanhTien = thanhTien
                        };

                        _context.DonHangChiTiets.Add(chiTiet);

                        // 3. Trừ tồn kho và cập nhật trạng thái IMEI
                        if (item.SelectedImeis != null && item.SelectedImeis.Any())
                        {
                            foreach (var imeiId in item.SelectedImeis)
                            {
                                var imei = await _context.Imeis.FindAsync(imeiId);
                                if (imei != null)
                                {
                                    imei.TrangThai = "Sold";

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

                    // Trả về kết quả
                    return Json(new
                    {
                        success = true,
                        message = "Thanh toán thành công!",
                        maDon = donHang.MaDon,
                        ngayBaoHanh = DateTime.Now.AddYears(1).ToString("dd/MM/yyyy"),
                        tongTien = tongTien
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
    }
}