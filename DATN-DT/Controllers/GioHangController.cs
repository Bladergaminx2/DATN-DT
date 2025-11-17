using DATN_DT.Data;
using DATN_DT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace DATN_DT.Controllers
{
    // DTO đơn giản cho việc thêm sản phẩm vào giỏ hàng
    public class GioHangItemAdd
    {
        public int IdKhachHang { get; set; }
        public int IdModelSanPham { get; set; }
        public int SoLuong { get; set; }
    }

    public class GioHangController : Controller
    {
        private readonly MyDbContext _context;

        public GioHangController(MyDbContext context)
        {
            _context = context;
        }

        // --- 1. View & Header: Lấy danh sách Giỏ Hàng (URL: /GioHang/Index) ---
        public async Task<IActionResult> Index()
        {
            var gioHangs = await _context.GioHangs
                .Include(gh => gh.KhachHang)
                .Include(gh => gh.GioHangChiTiets!)
                    .ThenInclude(ghct => ghct.ModelSanPham)
                .ToListAsync();

            return View(gioHangs);
        }

        // --- 2. API: Thêm/Cập nhật sản phẩm vào Giỏ Hàng (URL: /GioHang/AddItem) ---
        [HttpPost]
        public async Task<IActionResult> AddItem([FromBody] GioHangItemAdd item)
        {
            if (item == null || item.IdKhachHang == 0 || item.IdModelSanPham == 0 || item.SoLuong <= 0)
                return BadRequest(new { message = "Dữ liệu không hợp lệ!" });

            // Tìm hoặc Tạo Giỏ Hàng "Hoạt động"
            var gioHang = await _context.GioHangs
                .Include(gh => gh.GioHangChiTiets)
                .FirstOrDefaultAsync(gh => gh.IdKhachHang == item.IdKhachHang);

            if (gioHang == null)
            {
                gioHang = new GioHang
                {
                    IdKhachHang = item.IdKhachHang
                };
                _context.GioHangs.Add(gioHang);
                await _context.SaveChangesAsync();
            }

            // Lấy giá bán
            var modelSanPham = await _context.ModelSanPhams.FindAsync(item.IdModelSanPham);
            if (modelSanPham == null || modelSanPham.GiaBanModel == null)
                return NotFound(new { message = "Không tìm thấy hoặc không có giá cho Model Sản Phẩm này." });

            var donGia = modelSanPham.GiaBanModel.Value;
            var chiTiet = gioHang.GioHangChiTiets?.FirstOrDefault(ct => ct.IdModelSanPham == item.IdModelSanPham);

            if (chiTiet != null)
            {
                chiTiet.SoLuong = (chiTiet.SoLuong ?? 0) + item.SoLuong;
                //chiTiet.DonGia = donGia;
                //chiTiet.ThanhTien = chiTiet.SoLuong * donGia;
            }
            else
            {
                chiTiet = new GioHangChiTiet
                {
                    IdGioHang = gioHang.IdGioHang,
                    IdModelSanPham = item.IdModelSanPham,
                    SoLuong = item.SoLuong,
                    //DonGia = donGia,
                    //ThanhTien = item.SoLuong * donGia
                };
                _context.GioHangChiTiets.Add(chiTiet);
            }

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Cập nhật giỏ hàng thành công!", gioHangId = gioHang.IdGioHang });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lưu giỏ hàng: " + ex.Message });
            }
        }

        // --- 3. API: Xóa một mục Chi Tiết Giỏ Hàng (URL: /GioHang/RemoveItem/{id}) ---
        [HttpPost]
        [Route("GioHang/RemoveItem/{idGioHangChiTiet}")]
        public async Task<IActionResult> RemoveItem(int idGioHangChiTiet)
        {
            var chiTiet = await _context.GioHangChiTiets.FindAsync(idGioHangChiTiet);
            if (chiTiet == null)
                return NotFound(new { message = "Không tìm thấy mục chi tiết cần xóa." });

            _context.GioHangChiTiets.Remove(chiTiet);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã xóa sản phẩm khỏi giỏ hàng." });
        }

        // --- 4. API Hỗ Trợ: Lấy danh sách Khách Hàng (URL: /GioHang/GetKhachHangs) ---
        [HttpGet]
        public async Task<IActionResult> GetKhachHangs()
        {
            var list = await _context.KhachHangs
                .Select(kh => new { kh.IdKhachHang, DisplayText = kh.HoTenKhachHang + " (" + kh.SdtKhachHang + ")" })
                .ToListAsync();
            return Ok(list);
        }

        // --- 5. API Hỗ Trợ: Lấy danh sách Model Sản Phẩm (URL: /GioHang/GetModelSanPhams) ---
        [HttpGet]
        public async Task<IActionResult> GetModelSanPhams()
        {
            var list = await _context.ModelSanPhams
                .Select(m => new { m.IdModelSanPham, DisplayText = m.TenModel + " - " + m.GiaBanModel!.Value.ToString("N0") + " ₫" })
                .ToListAsync();
            return Ok(list);
        }
    }
}