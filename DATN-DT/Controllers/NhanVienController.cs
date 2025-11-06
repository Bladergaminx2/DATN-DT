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
    public class NhanVienController : Controller
    {
        private readonly MyDbContext _context;

        public NhanVienController(MyDbContext context)
        {
            _context = context;
        }

        // --- Index: Lấy danh sách Nhân viên ---
        public async Task<IActionResult> Index()
        {
            // Eager loading ChucVu để hiển thị tên chức vụ trong View
            var nhanViens = await _context.NhanViens
                .Include(nv => nv.ChucVu)
                .ToListAsync();

            // Lấy danh sách Chức vụ để dùng trong ViewBag (cho Modal Thêm/Sửa)
            var chucVus = await _context.ChucVus.ToDictionaryAsync(x => x.IdChucVu);
            ViewBag.ChucVus = chucVus;

            return View(nhanViens);
        }

        // --- Create: Thêm Nhân viên mới ---
        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> Create([FromBody] NhanVien? nhanVien)
        {
            if (nhanVien == null)
                return BadRequest(new { message = "Dữ liệu không hợp lệ!" });

            var errors = new Dictionary<string, string>();

            // Validation cơ bản
            if (string.IsNullOrWhiteSpace(nhanVien.TenTaiKhoanNV))
                errors["TenTaiKhoanNV"] = "Phải nhập tên tài khoản!";
            if (string.IsNullOrWhiteSpace(nhanVien.Password))
                errors["Password"] = "Phải nhập mật khẩu!";
            if (string.IsNullOrWhiteSpace(nhanVien.HoTenNhanVien))
                errors["HoTenNhanVien"] = "Phải nhập họ tên nhân viên!";
            if (nhanVien.IdChucVu == null || nhanVien.IdChucVu == 0)
                errors["IdChucVu"] = "Phải chọn chức vụ!";
            if (string.IsNullOrWhiteSpace(nhanVien.SdtNhanVien))
                errors["SdtNhanVien"] = "Phải nhập số điện thoại!";

            // Ngày vào làm mặc định là hôm nay nếu không có
            nhanVien.NgayVaoLam ??= DateTime.Now;
            nhanVien.TrangThaiNV ??= "Đang làm việc";


            if (errors.Count > 0)
                return BadRequest(errors);

            // Check trùng Tên tài khoản
            bool tkExists = await _context.NhanViens.AnyAsync(nv =>
                nv.TenTaiKhoanNV!.Trim().ToLower() == nhanVien.TenTaiKhoanNV!.Trim().ToLower()
            );
            if (tkExists)
                return Conflict(new { message = "Tên tài khoản đã tồn tại!" });

            // Check trùng SĐT
            bool sdtExists = await _context.NhanViens.AnyAsync(nv =>
                nv.SdtNhanVien != null && nv.SdtNhanVien.Trim().ToLower() == nhanVien.SdtNhanVien!.Trim().ToLower()
            );
            if (sdtExists)
                return Conflict(new { message = "Số điện thoại đã được sử dụng!" });

            try
            {
                // Chuẩn hóa dữ liệu
                nhanVien.TenTaiKhoanNV = nhanVien.TenTaiKhoanNV.Trim();
                nhanVien.HoTenNhanVien = nhanVien.HoTenNhanVien.Trim();
                // Lưu ý: Mật khẩu nên được Hash trước khi lưu! (Phần này tôi bỏ qua để giữ đơn giản)
                nhanVien.SdtNhanVien = nhanVien.SdtNhanVien.Trim();
                nhanVien.EmailNhanVien = nhanVien.EmailNhanVien?.Trim();
                nhanVien.DiaChiNV = nhanVien.DiaChiNV?.Trim();
                nhanVien.TrangThaiNV = nhanVien.TrangThaiNV.Trim();

                _context.NhanViens.Add(nhanVien);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Thêm Nhân viên thành công!" });
            }
            catch (Exception ex)
            {
                // Log exception (ex)
                return StatusCode(500, new { message = "Lỗi khi thêm Nhân viên. Vui lòng thử lại!" });
            }
        }

        // --- Edit: Cập nhật thông tin Nhân viên ---
        [HttpPost]
        [Route("NhanVien/Edit/{id}")]
        [Consumes("application/json")]
        public async Task<IActionResult> Edit(int id, [FromBody] NhanVien? nhanVien)
        {
            if (nhanVien == null)
                return BadRequest(new { message = "Dữ liệu không hợp lệ!" });

            var errors = new Dictionary<string, string>();

            // Validation cơ bản (Mật khẩu có thể không cần thiết khi Edit nếu không thay đổi)
            if (string.IsNullOrWhiteSpace(nhanVien.TenTaiKhoanNV))
                errors["TenTaiKhoanNV"] = "Phải nhập tên tài khoản!";
            if (string.IsNullOrWhiteSpace(nhanVien.HoTenNhanVien))
                errors["HoTenNhanVien"] = "Phải nhập họ tên nhân viên!";
            if (nhanVien.IdChucVu == null || nhanVien.IdChucVu == 0)
                errors["IdChucVu"] = "Phải chọn chức vụ!";
            if (string.IsNullOrWhiteSpace(nhanVien.SdtNhanVien))
                errors["SdtNhanVien"] = "Phải nhập số điện thoại!";

            if (errors.Count > 0)
                return BadRequest(errors);

            var existing = await _context.NhanViens.FindAsync(id);
            if (existing == null)
                return NotFound(new { message = "Không tìm thấy Nhân viên!" });

            // Check trùng Tên tài khoản (ngoại trừ chính nó)
            bool tkExists = await _context.NhanViens.AnyAsync(nv =>
                nv.TenTaiKhoanNV!.Trim().ToLower() == nhanVien.TenTaiKhoanNV!.Trim().ToLower() &&
                nv.IdNhanVien != id
            );
            if (tkExists)
                return Conflict(new { message = "Tên tài khoản đã tồn tại cho nhân viên khác!" });

            // Check trùng SĐT (ngoại trừ chính nó)
            bool sdtExists = await _context.NhanViens.AnyAsync(nv =>
                nv.SdtNhanVien != null && nv.SdtNhanVien.Trim().ToLower() == nhanVien.SdtNhanVien!.Trim().ToLower() &&
                nv.IdNhanVien != id
            );
            if (sdtExists)
                return Conflict(new { message = "Số điện thoại đã được sử dụng bởi nhân viên khác!" });

            try
            {
                // Cập nhật thông tin
                existing.TenTaiKhoanNV = nhanVien.TenTaiKhoanNV.Trim();
                if (!string.IsNullOrWhiteSpace(nhanVien.Password))
                {
                    // Lưu ý: Hash mật khẩu mới nếu người dùng nhập!
                    existing.Password = nhanVien.Password;
                }
                existing.HoTenNhanVien = nhanVien.HoTenNhanVien.Trim();
                existing.IdChucVu = nhanVien.IdChucVu;
                existing.SdtNhanVien = nhanVien.SdtNhanVien.Trim();
                existing.EmailNhanVien = nhanVien.EmailNhanVien?.Trim();
                existing.DiaChiNV = nhanVien.DiaChiNV?.Trim();
                existing.NgayVaoLam = nhanVien.NgayVaoLam;
                existing.TrangThaiNV = nhanVien.TrangThaiNV?.Trim();

                _context.NhanViens.Update(existing);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Cập nhật Nhân viên thành công!" });
            }
            catch
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật Nhân viên. Vui lòng thử lại!" });
            }
        }

        // --- Xóa Nhân viên ---
        [HttpPost]
        [Route("NhanVien/Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _context.NhanViens.FindAsync(id);
            if (existing == null)
                return NotFound(new { message = "Không tìm thấy Nhân viên để xóa!" });

            try
            {
                _context.NhanViens.Remove(existing);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Xóa Nhân viên thành công!" });
            }
            catch
            {
                return StatusCode(500, new { message = "Lỗi khi xóa Nhân viên. Có thể do Nhân viên này đã phát sinh giao dịch." });
            }
        }

        // --- Lấy danh sách Chức vụ (Dùng cho dropdown) ---
        [HttpGet]
        public async Task<IActionResult> GetChucVus()
        {
            // Giả định bạn có DbSet<ChucVu> và class ChucVu có IdChucVu và TenChucVu
            var list = await _context.ChucVus
                .Select(m => new { m.IdChucVu, m.TenChucVu })
                .ToListAsync();
            return Ok(list);
        }
    }
}