using DATN_DT.CustomAttribute;
using DATN_DT.Data;
using DATN_DT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DATN_DT.Controllers
{
    public class DonHangController : Controller
    {
        private readonly MyDbContext _context;

        public DonHangController(MyDbContext context)
        {
            _context = context;
        }
        [AuthorizeRoleFromToken("ADMIN")]
        // ================================
        // 1. Danh sách đơn hàng
        // ================================
        public async Task<IActionResult> Index()
        {
            var donHangs = await _context.DonHangs
                .Include(x => x.KhachHang)
                .Include(x => x.DonHangChiTiets)!
                    .ThenInclude(x => x.ModelSanPham)
                .ToListAsync();

            return View(donHangs);
        }

        // ================================
        // 2. Xem chi tiết đơn hàng
        // ================================
        public async Task<IActionResult> Details(int id)
        {
            var donHang = await _context.DonHangs
                .Include(x => x.KhachHang)
                .Include(x => x.DonHangChiTiets)!
                    .ThenInclude(ct => ct.ModelSanPham)
                .Include(x => x.DonHangChiTiets)!
                    .ThenInclude(ct => ct.KhuyenMai)
                .FirstOrDefaultAsync();

            if (donHang == null)
                return NotFound();

            return View(donHang);
        }

        // ================================
        // 3. Tạo đơn hàng (online / offline)
        // ================================
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.ListKhachHang = _context.KhachHangs.ToList();
            ViewBag.ListSanPham = _context.ModelSanPhams.ToList();
            ViewBag.ListKhuyenMai = _context.KhuyenMais.ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(DonHang model, List<DonHangChiTiet> chitiets)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Ngày đặt đơn
            model.NgayDat = DateTime.Now;

            // Trạng thái đơn hàng
            // 0: Chờ xác nhận | 1: Đang giao | 2: Hoàn thành | 3: Hủy
            model.TrangThaiDH = 0;

            // Trạng thái thanh toán
            // COD = chưa thanh toán
            model.TrangThaiHoaDon = model.PhuongThucThanhToan == "COD"
                ? "Chưa thanh toán"
                : "Đã thanh toán";

            // Tạo mã đơn
            model.MaDon = "DH" + DateTime.Now.Ticks;

            // ========== TÍNH TỔNG TIỀN – GIẢM GIÁ ==========
            decimal tongTien = 0;
            decimal tongGiam = 0;

            foreach (var ct in chitiets)
            {
                ct.ThanhTien = (ct.SoLuong ?? 1) * (ct.DonGia ?? 0);

                tongTien += ct.ThanhTien.Value;

                if (ct.IdKhuyenMai != null)
                {
                    var km = await _context.KhuyenMais.FindAsync(ct.IdKhuyenMai);

                    if (km != null)
                    {
                        if (km.LoaiGiam == "PT") // giảm %
                        {
                            tongGiam += ct.ThanhTien.Value * (km.GiaTri ?? 0) / 100;
                        }
                        else if (km.LoaiGiam == "Tien") // giảm tiền trực tiếp
                        {
                            tongGiam += km.GiaTri ?? 0;
                        }
                    }
                }
            }

            ViewBag.TongTien = tongTien;
            ViewBag.TongGiam = tongGiam;
            ViewBag.TongThanhToan = tongTien - tongGiam;

            // Lưu đơn hàng
            _context.DonHangs.Add(model);
            await _context.SaveChangesAsync();

            // Gán lại IdDonHang
            foreach (var ct in chitiets)
            {
                _context.DonHangChiTiets.Add(ct);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ================================
        // 4. Cập nhật trạng thái đơn hàng
        // ================================
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, int trangthai)
        {
            var dh = await _context.DonHangs.FindAsync(id);
            if (dh == null) return NotFound();

            dh.TrangThaiDH = trangthai;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id });
        }

        // ================================
        // 5. Cập nhật trạng thái thanh toán
        // ================================
        [HttpPost]
        public async Task<IActionResult> UpdatePayment(int id)
        {
            var dh = await _context.DonHangs.FindAsync(id);
            if (dh == null) return NotFound();

            dh.TrangThaiHoaDon = "Đã thanh toán";
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}