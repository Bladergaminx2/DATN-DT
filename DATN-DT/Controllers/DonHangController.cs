using DATN_DT.Data;
using DATN_DT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace DATN_DT.Controllers
{
    public class DonHangController : Controller
    {
        private readonly MyDbContext _context;

        public DonHangController(MyDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var donHangs = await _context.DonHangs
                .Include(dh => dh.KhachHang)
                .Include(dh => dh.DonHangChiTiets!)
                    .ThenInclude(dhct => dhct.ModelSanPham) 
                .OrderByDescending(dh => dh.NgayDat)
                .ToListAsync();

            return View(donHangs);
        }
        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> Create([FromBody] DonHang? donHang)
        {
            if (donHang == null)
                return BadRequest(new { message = "Dữ liệu đơn hàng không hợp lệ!" });

            var errors = new Dictionary<string, string>();
            if (donHang.IdKhachHang == null || donHang.IdKhachHang == 0) errors["IdKhachHang"] = "Phải chọn khách hàng!";
            if (string.IsNullOrWhiteSpace(donHang.MaDon)) errors["MaDon"] = "Phải nhập mã đơn hàng!";

            if (errors.Count > 0) return BadRequest(errors);

            // Kiểm tra trùng Mã Đơn
            if (await _context.DonHangs.AnyAsync(dh => dh.MaDon!.Trim().ToLower() == donHang.MaDon!.Trim().ToLower()))
                return Conflict(new { message = "Mã đơn hàng đã tồn tại!" });

            try
            {
                donHang.MaDon = donHang.MaDon.Trim();
                donHang.NgayDat ??= DateTime.Now;
                donHang.TrangThaiHoaDon ??= "Chưa thanh toán";
                donHang.TrangThaiDH ??= "Chờ xử lý";
                donHang.DiaChiGiaoHang = donHang.DiaChiGiaoHang?.Trim();
                donHang.GhiChu = donHang.GhiChu?.Trim();
                donHang.PhuongThucThanhToan = donHang.PhuongThucThanhToan ?? "Tiền mặt";

                _context.DonHangs.Add(donHang);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Thêm Đơn hàng (Header) thành công!", id = donHang.IdDonHang });
            }
            catch
            {
                return StatusCode(500, new { message = "Lỗi khi thêm Đơn hàng. Vui lòng thử lại!" });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetKhachHangs()
        {
            var list = await _context.KhachHangs
                .Select(kh => new { kh.IdKhachHang, DisplayText = kh.HoTenKhachHang + " (" + kh.SdtKhachHang + ")" })
                .ToListAsync();
            return Ok(list);
        }
    }
}