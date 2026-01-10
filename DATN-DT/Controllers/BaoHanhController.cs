using DATN_DT.CustomAttribute;
using DATN_DT.Data;
using DATN_DT.Models;
using DATN_DT.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DATN_DT.Controllers
{
    // Cho phép ADMIN và NHANVIEN truy cập
    [AuthorizeRoleFromToken("ADMIN", "NHANVIEN", "QUANLY", "KYTHUAT")]
    public class BaoHanhController : Controller
    {
        private readonly MyDbContext _context;
        private readonly IBaoHanhStatusService _baoHanhStatusService;

        public BaoHanhController(MyDbContext context, IBaoHanhStatusService baoHanhStatusService)
        {
            _context = context;
            _baoHanhStatusService = baoHanhStatusService;
        }

        // --- Index: Lấy danh sách Phiếu Bảo Hành ---
        public async Task<IActionResult> Index()
        {
            // Tự động cập nhật trạng thái hết bảo hành
            await _baoHanhStatusService.UpdateBaoHanhStatusAsync();

            // Eager loading các đối tượng liên quan để hiển thị thông tin
            var baoHanhs = await _context.BaoHanhs
                .Include(bh => bh.Imei)
                    .ThenInclude(i => i.ModelSanPham)
                        .ThenInclude(m => m.SanPham)
                .Include(bh => bh.KhachHang)
                .Include(bh => bh.NhanVien)
                .OrderByDescending(bh => bh.NgayNhan)
                .ToListAsync();

            return View(baoHanhs);
        }

        // --- Create: Thêm Phiếu Bảo Hành mới ---
        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> Create([FromBody] BaoHanh? baoHanh)
        {
            if (baoHanh == null)
                return BadRequest(new { message = "Dữ liệu không hợp lệ!" });

            // Validation
            var errors = new Dictionary<string, string>();

            // Kiểm tra IMEI bắt buộc
            if (!baoHanh.IdImei.HasValue || baoHanh.IdImei.Value == 0)
            {
                errors["IdImei"] = "Phải chọn IMEI sản phẩm!";
            }
            else
            {
                var imeiExists = await _context.Imeis.AnyAsync(i => i.IdImei == baoHanh.IdImei.Value);
                if (!imeiExists)
                    errors["IdImei"] = "IMEI không tồn tại!";

                // Kiểm tra IMEI đã có bảo hành đang hoạt động chưa
                // Trạng thái không hoạt động: "Đã hoàn thành", "Từ chối", "Hết bảo hành", "Hoàn tất"
                // Các trạng thái còn lại đều được coi là đang hoạt động
                var hasActiveWarranty = await _context.BaoHanhs
                    .AnyAsync(b => b.IdImei == baoHanh.IdImei.Value &&
                                   b.TrangThai != "Đã hoàn thành" &&
                                   b.TrangThai != "Từ chối" &&
                                   b.TrangThai != "Hết bảo hành" &&
                                   b.TrangThai != "Hoàn tất" &&
                                   (b.NgayTra == null || b.NgayTra >= DateTime.Now));
                if (hasActiveWarranty)
                    errors["IdImei"] = "IMEI này đã có bảo hành đang hoạt động!";
            }

            // Kiểm tra Khách hàng
            // Lưu ý: Cho phép null khi tạo tự động từ đơn hàng (khách vãng lai)
            // Nhưng khi tạo thủ công, nếu có giá trị thì phải hợp lệ
            if (baoHanh.IdKhachHang.HasValue && baoHanh.IdKhachHang.Value != 0)
            {
                var khachHangExists = await _context.KhachHangs.AnyAsync(kh => kh.IdKhachHang == baoHanh.IdKhachHang.Value);
                if (!khachHangExists)
                    errors["IdKhachHang"] = "Khách hàng không tồn tại!";
            }
            // Nếu IdKhachHang = 0 (từ form), coi như chưa chọn
            else if (baoHanh.IdKhachHang.HasValue && baoHanh.IdKhachHang.Value == 0)
            {
                errors["IdKhachHang"] = "Phải chọn khách hàng!";
            }

            // Kiểm tra Nhân viên bắt buộc (khi tạo thủ công)
            if (!baoHanh.IdNhanVien.HasValue || baoHanh.IdNhanVien.Value == 0)
            {
                errors["IdNhanVien"] = "Phải chọn nhân viên tiếp nhận!";
            }
            else
            {
                var nhanVienExists = await _context.NhanViens.AnyAsync(nv => nv.IdNhanVien == baoHanh.IdNhanVien.Value);
                if (!nhanVienExists)
                    errors["IdNhanVien"] = "Nhân viên không tồn tại!";
            }

            // Kiểm tra Ngày nhận
            baoHanh.NgayNhan ??= DateTime.Now;

            // Tính Ngày trả tự động dựa trên thời hạn bảo hành sản phẩm
            if (!baoHanh.NgayTra.HasValue && baoHanh.IdImei.HasValue)
            {
                var imei = await _context.Imeis
                    .Include(i => i.ModelSanPham)
                    .FirstOrDefaultAsync(i => i.IdImei == baoHanh.IdImei.Value);
                
                if (imei?.ModelSanPham != null)
                {
                    var thoiHanBaoHanh = imei.ModelSanPham.ThoiHanBaoHanh ?? 12; // Mặc định 12 tháng
                    baoHanh.NgayTra = baoHanh.NgayNhan.Value.AddMonths(thoiHanBaoHanh);
                }
                else
                {
                    // Nếu không tìm thấy model, mặc định 12 tháng
                    baoHanh.NgayTra = baoHanh.NgayNhan.Value.AddMonths(12);
                }
            }

            // Kiểm tra Ngày trả >= Ngày nhận
            if (baoHanh.NgayTra.HasValue && baoHanh.NgayNhan.HasValue)
            {
                if (baoHanh.NgayTra.Value < baoHanh.NgayNhan.Value)
                    errors["NgayTra"] = "Ngày trả phải lớn hơn hoặc bằng ngày nhận!";
            }

            // Kiểm tra Loại bảo hành
            if (string.IsNullOrWhiteSpace(baoHanh.LoaiBaoHanh))
            {
                errors["LoaiBaoHanh"] = "Phải chọn loại bảo hành!";
            }
            else
            {
                var validLoai = new[] { "Mới mua", "Sửa chữa", "Đổi máy" };
                if (!validLoai.Contains(baoHanh.LoaiBaoHanh.Trim()))
                {
                    errors["LoaiBaoHanh"] = "Loại bảo hành không hợp lệ!";
                }
            }

            // Kiểm tra Mô tả lỗi
            if (string.IsNullOrWhiteSpace(baoHanh.MoTaLoi))
                errors["MoTaLoi"] = "Phải nhập mô tả lỗi!";
            else if (baoHanh.MoTaLoi.Trim().Length < 10)
                errors["MoTaLoi"] = "Mô tả lỗi phải có ít nhất 10 ký tự!";
            else if (baoHanh.MoTaLoi.Trim().Length > 500)
                errors["MoTaLoi"] = "Mô tả lỗi không được vượt quá 500 ký tự!";

            // Kiểm tra Chi phí phát sinh
            if (baoHanh.ChiPhiPhatSinh.HasValue && baoHanh.ChiPhiPhatSinh.Value < 0)
                errors["ChiPhiPhatSinh"] = "Chi phí phát sinh không được âm!";
            
            // Tính chi phí phát sinh tự động nếu hết hạn bảo hành
            if (!baoHanh.ChiPhiPhatSinh.HasValue || baoHanh.ChiPhiPhatSinh.Value == 0)
            {
                if (baoHanh.NgayTra.HasValue && baoHanh.NgayTra.Value < DateTime.Now)
                {
                    // Hết hạn bảo hành - mặc định 0, nhân viên sẽ nhập sau khi kiểm tra
                    baoHanh.ChiPhiPhatSinh = 0;
                }
                else
                {
                    // Trong thời hạn bảo hành - chi phí = 0
                    baoHanh.ChiPhiPhatSinh = 0;
                }
            }

            // Kiểm tra Trạng thái
            if (string.IsNullOrWhiteSpace(baoHanh.TrangThai))
                errors["TrangThai"] = "Phải chọn trạng thái!";

            if (errors.Count > 0)
                return BadRequest(errors);

            try
            {
                // Trim các trường text
                baoHanh.MoTaLoi = baoHanh.MoTaLoi?.Trim();
                baoHanh.XuLy = baoHanh.XuLy?.Trim();
                baoHanh.TrangThai = baoHanh.TrangThai?.Trim();
                baoHanh.LoaiBaoHanh = baoHanh.LoaiBaoHanh?.Trim();

                _context.BaoHanhs.Add(baoHanh);
                await _context.SaveChangesAsync();

                // Ghi lịch sử tạo mới
                var lichSu = new BaoHanhLichSu
                {
                    IdBaoHanh = baoHanh.IdBaoHanh,
                    IdNhanVien = baoHanh.IdNhanVien,
                    ThaoTac = "Tạo mới phiếu bảo hành",
                    TrangThaiMoi = baoHanh.TrangThai,
                    MoTa = $"Tạo phiếu bảo hành - Loại: {baoHanh.LoaiBaoHanh}, IMEI: {baoHanh.IdImei}",
                    ThoiGian = DateTime.Now
                };
                _context.BaoHanhLichSus.Add(lichSu);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Thêm Phiếu Bảo Hành thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi thêm Phiếu Bảo Hành. Vui lòng thử lại!" });
            }
        }

        // --- Edit: Cập nhật Phiếu Bảo Hành ---
        // Chỉ ADMIN, QUANLY, KYTHUAT mới được sửa bảo hành
        // NHANVIEN thường chỉ được xem và tạo mới
        [HttpPost]
        [Route("BaoHanh/Edit/{id}")]
        [Consumes("application/json")]
        [AuthorizeRoleFromToken("ADMIN", "QUANLY", "KYTHUAT")]
        public async Task<IActionResult> Edit(int id, [FromBody] BaoHanh? baoHanh)
        {
            if (baoHanh == null)
                return BadRequest(new { message = "Dữ liệu không hợp lệ!" });

            var existing = await _context.BaoHanhs.FindAsync(id);
            if (existing == null)
                return NotFound(new { message = "Không tìm thấy Phiếu Bảo Hành!" });

            // Validation
            var errors = new Dictionary<string, string>();

            // Kiểm tra IMEI bắt buộc
            if (!baoHanh.IdImei.HasValue || baoHanh.IdImei.Value == 0)
            {
                errors["IdImei"] = "Phải chọn IMEI sản phẩm!";
            }
            else
            {
                // Kiểm tra IMEI tồn tại
                var imeiExists = await _context.Imeis.AnyAsync(i => i.IdImei == baoHanh.IdImei.Value);
                if (!imeiExists)
                {
                    errors["IdImei"] = "IMEI không tồn tại!";
                }
                // Kiểm tra IMEI đã có bảo hành đang hoạt động khác (nếu thay đổi IMEI)
                else if (baoHanh.IdImei.Value != existing.IdImei)
                {
                    // Trạng thái không hoạt động: "Đã hoàn thành", "Từ chối", "Hết bảo hành", "Hoàn tất"
                    var hasActiveWarranty = await _context.BaoHanhs
                        .AnyAsync(b => b.IdImei == baoHanh.IdImei.Value &&
                                       b.IdBaoHanh != id &&
                                       b.TrangThai != "Đã hoàn thành" &&
                                       b.TrangThai != "Từ chối" &&
                                       b.TrangThai != "Hết bảo hành" &&
                                       b.TrangThai != "Hoàn tất" &&
                                       (b.NgayTra == null || b.NgayTra >= DateTime.Now));
                    if (hasActiveWarranty)
                        errors["IdImei"] = "IMEI này đã có bảo hành đang hoạt động khác!";
                }
            }

            // Kiểm tra Khách hàng bắt buộc
            if (!baoHanh.IdKhachHang.HasValue || baoHanh.IdKhachHang.Value == 0)
            {
                errors["IdKhachHang"] = "Phải chọn khách hàng!";
            }
            else
            {
                var khachHangExists = await _context.KhachHangs.AnyAsync(kh => kh.IdKhachHang == baoHanh.IdKhachHang.Value);
                if (!khachHangExists)
                    errors["IdKhachHang"] = "Khách hàng không tồn tại!";
            }

            // Kiểm tra Nhân viên bắt buộc
            if (!baoHanh.IdNhanVien.HasValue || baoHanh.IdNhanVien.Value == 0)
            {
                errors["IdNhanVien"] = "Phải chọn nhân viên xử lý!";
            }
            else
            {
                var nhanVienExists = await _context.NhanViens.AnyAsync(nv => nv.IdNhanVien == baoHanh.IdNhanVien.Value);
                if (!nhanVienExists)
                    errors["IdNhanVien"] = "Nhân viên không tồn tại!";
            }

            // Kiểm tra Ngày trả >= Ngày nhận
            var ngayNhan = baoHanh.NgayNhan ?? existing.NgayNhan;
            if (baoHanh.NgayTra.HasValue && ngayNhan.HasValue)
            {
                if (baoHanh.NgayTra.Value < ngayNhan.Value)
                    errors["NgayTra"] = "Ngày trả phải lớn hơn hoặc bằng ngày nhận!";
            }

            // Kiểm tra Mô tả lỗi
            if (string.IsNullOrWhiteSpace(baoHanh.MoTaLoi))
                errors["MoTaLoi"] = "Phải nhập mô tả lỗi!";
            else if (baoHanh.MoTaLoi.Trim().Length < 10)
                errors["MoTaLoi"] = "Mô tả lỗi phải có ít nhất 10 ký tự!";
            else if (baoHanh.MoTaLoi.Trim().Length > 500)
                errors["MoTaLoi"] = "Mô tả lỗi không được vượt quá 500 ký tự!";

            // Kiểm tra Chi phí phát sinh
            if (baoHanh.ChiPhiPhatSinh.HasValue && baoHanh.ChiPhiPhatSinh.Value < 0)
                errors["ChiPhiPhatSinh"] = "Chi phí phát sinh không được âm!";

            // Tính chi phí phát sinh nếu hết hạn bảo hành và chưa có chi phí
            var ngayTra = baoHanh.NgayTra ?? existing.NgayTra;
            if (ngayTra.HasValue && ngayTra.Value < DateTime.Now)
            {
                // Hết hạn bảo hành - nếu chưa có chi phí hoặc = 0, có thể tính phí
                // (Logic tính phí sẽ được nhân viên nhập thủ công hoặc có thể tự động tính dựa trên loại lỗi)
            }

            // Kiểm tra Loại bảo hành
            if (string.IsNullOrWhiteSpace(baoHanh.LoaiBaoHanh))
            {
                errors["LoaiBaoHanh"] = "Phải chọn loại bảo hành!";
            }
            else
            {
                var validLoai = new[] { "Mới mua", "Sửa chữa", "Đổi máy" };
                if (!validLoai.Contains(baoHanh.LoaiBaoHanh.Trim()))
                {
                    errors["LoaiBaoHanh"] = "Loại bảo hành không hợp lệ!";
                }
            }

            // Kiểm tra Trạng thái
            if (string.IsNullOrWhiteSpace(baoHanh.TrangThai))
                errors["TrangThai"] = "Phải chọn trạng thái!";

            // Kiểm tra: Nếu trạng thái là "Hoàn tất" hoặc "Đã hoàn thành" thì phải có Ngày trả
            if ((baoHanh.TrangThai == "Hoàn tất" || baoHanh.TrangThai == "Đã hoàn thành") && !ngayTra.HasValue)
                errors["NgayTra"] = "Khi trạng thái là 'Hoàn tất' hoặc 'Đã hoàn thành', bắt buộc phải có ngày trả!";

            if (errors.Count > 0)
                return BadRequest(errors);

            try
            {
                var trangThaiCu = existing.TrangThai;
                var chiPhiCu = existing.ChiPhiPhatSinh;

                existing.IdImei = baoHanh.IdImei;
                existing.IdKhachHang = baoHanh.IdKhachHang;
                existing.IdNhanVien = baoHanh.IdNhanVien;
                existing.NgayNhan = baoHanh.NgayNhan ?? existing.NgayNhan;
                existing.NgayTra = baoHanh.NgayTra ?? existing.NgayTra;
                existing.TrangThai = baoHanh.TrangThai?.Trim();
                existing.MoTaLoi = baoHanh.MoTaLoi?.Trim();
                existing.XuLy = baoHanh.XuLy?.Trim();
                existing.ChiPhiPhatSinh = baoHanh.ChiPhiPhatSinh;
                existing.LoaiBaoHanh = baoHanh.LoaiBaoHanh?.Trim() ?? existing.LoaiBaoHanh;

                _context.BaoHanhs.Update(existing);
                await _context.SaveChangesAsync();

                // Ghi lịch sử thay đổi
                var lichSuList = new List<BaoHanhLichSu>();

                // Ghi lịch sử thay đổi trạng thái
                if (trangThaiCu != existing.TrangThai)
                {
                    lichSuList.Add(new BaoHanhLichSu
                    {
                        IdBaoHanh = existing.IdBaoHanh,
                        IdNhanVien = existing.IdNhanVien,
                        ThaoTac = "Cập nhật trạng thái",
                        TrangThaiCu = trangThaiCu,
                        TrangThaiMoi = existing.TrangThai,
                        MoTa = $"Thay đổi trạng thái từ '{trangThaiCu}' sang '{existing.TrangThai}'",
                        ThoiGian = DateTime.Now
                    });
                }

                // Ghi lịch sử thay đổi chi phí
                if (chiPhiCu != existing.ChiPhiPhatSinh)
                {
                    lichSuList.Add(new BaoHanhLichSu
                    {
                        IdBaoHanh = existing.IdBaoHanh,
                        IdNhanVien = existing.IdNhanVien,
                        ThaoTac = "Cập nhật chi phí",
                        MoTa = $"Thay đổi chi phí từ {chiPhiCu?.ToString("N0") ?? "0"} ₫ sang {existing.ChiPhiPhatSinh?.ToString("N0") ?? "0"} ₫",
                        ThoiGian = DateTime.Now
                    });
                }

                // Ghi lịch sử thay đổi thông tin khác (nếu có)
                if (lichSuList.Count == 0)
                {
                    lichSuList.Add(new BaoHanhLichSu
                    {
                        IdBaoHanh = existing.IdBaoHanh,
                        IdNhanVien = existing.IdNhanVien,
                        ThaoTac = "Cập nhật thông tin",
                        MoTa = "Cập nhật thông tin phiếu bảo hành",
                        ThoiGian = DateTime.Now
                    });
                }

                _context.BaoHanhLichSus.AddRange(lichSuList);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Cập nhật Phiếu Bảo Hành thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật Phiếu Bảo Hành. Vui lòng thử lại!" });
            }
        }

        // --- API: Lấy danh sách Imei ---
        [HttpGet]
        public async Task<IActionResult> GetImeis()
        {
            var list = await _context.Imeis
                .Select(i => new 
                { 
                    i.IdImei, 
                    DisplayText = i.MaImei
                })
                .OrderBy(i => i.DisplayText)
                .ToListAsync();
            
            return Ok(list);
        }

        // --- API: Lấy danh sách Khách Hàng ---
        [HttpGet]
        public async Task<IActionResult> GetKhachHangs()
        {
            var list = await _context.KhachHangs
                .Select(kh => new { kh.IdKhachHang, DisplayText = kh.HoTenKhachHang + " - " + kh.SdtKhachHang })
                .ToListAsync();
            return Ok(list);
        }

        // --- API: Lấy danh sách Nhân Viên ---
        [HttpGet]
        public async Task<IActionResult> GetNhanViens()
        {
            var list = await _context.NhanViens
                .Select(nv => new { nv.IdNhanVien, DisplayText = nv.HoTenNhanVien + " - " + nv.TenTaiKhoanNV })
                .ToListAsync();
            return Ok(list);
        }

        // --- API: Lấy lịch sử bảo hành ---
        [HttpGet]
        [Route("BaoHanh/GetLichSu/{idBaoHanh}")]
        public async Task<IActionResult> GetLichSu(int idBaoHanh)
        {
            var lichSu = await _context.BaoHanhLichSus
                .Include(l => l.NhanVien)
                .Where(l => l.IdBaoHanh == idBaoHanh)
                .OrderByDescending(l => l.ThoiGian)
                .Select(l => new
                {
                    l.IdBaoHanhLichSu,
                    l.ThaoTac,
                    l.TrangThaiCu,
                    l.TrangThaiMoi,
                    l.MoTa,
                    l.ThoiGian,
                    NhanVien = l.NhanVien != null ? l.NhanVien.HoTenNhanVien : "Hệ thống"
                })
                .ToListAsync();

            return Ok(lichSu);
        }
    }
}