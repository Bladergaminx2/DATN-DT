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
    public class KhuyenMaiController : Controller
    {
        private readonly MyDbContext _context;

        public KhuyenMaiController(MyDbContext context)
        {
            _context = context;
        }

        // --- Index: Lấy danh sách Khuyến Mãi ---
        public async Task<IActionResult> Index()
        {
            var khuyenMais = await _context.KhuyenMais
                .OrderByDescending(km => km.NgayBatDau)
                .ToListAsync();

            // Tự động cập nhật trạng thái dựa trên ngày hiện tại
            var now = DateTime.Now.Date;
            bool hasChanges = false;

            foreach (var km in khuyenMais)
            {
                if (km.NgayKetThuc.HasValue && km.NgayBatDau.HasValue)
                {
                    var ngayKetThuc = km.NgayKetThuc.Value.Date;
                    var ngayBatDau = km.NgayBatDau.Value.Date;

                    // Nếu đã quá hạn, chuyển sang "Đã kết thúc"
                    if (now > ngayKetThuc)
                    {
                        if (km.TrangThaiKM != "Đã kết thúc")
                        {
                            km.TrangThaiKM = "Đã kết thúc";
                            hasChanges = true;
                        }
                    }
                    // Nếu đang trong thời gian diễn ra
                    else if (ngayBatDau <= now && now <= ngayKetThuc)
                    {
                        if (km.TrangThaiKM != "Đang diễn ra")
                        {
                            km.TrangThaiKM = "Đang diễn ra";
                            hasChanges = true;
                        }
                    }
                    // Nếu chưa đến ngày bắt đầu
                    else if (now < ngayBatDau)
                    {
                        if (km.TrangThaiKM != "Sắp diễn ra")
                        {
                            km.TrangThaiKM = "Sắp diễn ra";
                            hasChanges = true;
                        }
                    }
                }
            }

            // Lưu các thay đổi nếu có
            if (hasChanges)
            {
                await _context.SaveChangesAsync();
            }

            return View(khuyenMais);
        }

        // --- Create: Thêm Khuyến Mãi mới ---
        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> Create([FromBody] KhuyenMai? khuyenMai)
        {
            if (khuyenMai == null)
                return BadRequest(new { message = "Dữ liệu không hợp lệ!" });

            var errors = new Dictionary<string, string>();

            // Validation cơ bản
            if (string.IsNullOrWhiteSpace(khuyenMai.MaKM))
                errors["MaKM"] = "Phải nhập Mã Khuyến Mãi!";
            if (string.IsNullOrWhiteSpace(khuyenMai.LoaiGiam))
                errors["LoaiGiam"] = "Phải chọn Loại Giảm!";
            if (khuyenMai.GiaTri == null || khuyenMai.GiaTri <= 0)
                errors["GiaTri"] = "Giá Trị Khuyến Mãi phải lớn hơn 0!";
            
            // Validation giá trị theo loại giảm
            if (khuyenMai.GiaTri.HasValue && khuyenMai.GiaTri > 0)
            {
                if (khuyenMai.LoaiGiam == "Phần trăm")
                {
                    if (khuyenMai.GiaTri > 100)
                        errors["GiaTri"] = "Giá trị phần trăm không được vượt quá 100%!";
                }
                else if (khuyenMai.LoaiGiam == "Số tiền")
                {
                    if (khuyenMai.GiaTri <= 0)
                        errors["GiaTri"] = "Giá trị số tiền phải lớn hơn 0!";
                }
            }
            
            if (khuyenMai.NgayBatDau == null)
                errors["NgayBatDau"] = "Phải chọn Ngày Bắt Đầu!";
            if (khuyenMai.NgayKetThuc == null)
                errors["NgayKetThuc"] = "Phải chọn Ngày Kết Thúc!";

            if (khuyenMai.NgayBatDau.HasValue && khuyenMai.NgayKetThuc.HasValue && khuyenMai.NgayBatDau >= khuyenMai.NgayKetThuc)
                errors["NgayKetThuc"] = "Ngày Kết Thúc phải sau Ngày Bắt Đầu!";

            if (errors.Count > 0)
                return BadRequest(errors);

            // Check trùng Mã Khuyến Mãi
            bool exists = await _context.KhuyenMais.AnyAsync(km =>
                km.MaKM!.Trim().ToLower() == khuyenMai.MaKM!.Trim().ToLower()
            );
            if (exists)
                return Conflict(new { message = "Mã Khuyến Mãi đã tồn tại!" });

            // Check trùng thời gian khuyến mãi
            var ngayBatDau = khuyenMai.NgayBatDau.Value.Date;
            var ngayKetThuc = khuyenMai.NgayKetThuc.Value.Date;

            bool timeOverlap = await _context.KhuyenMais.AnyAsync(km =>
                km.NgayBatDau.HasValue &&
                km.NgayKetThuc.HasValue &&
                (
                    // Case 1: Khuyến mãi mới bắt đầu trong khoảng thời gian của khuyến mãi cũ
                    (ngayBatDau >= km.NgayBatDau.Value.Date && ngayBatDau <= km.NgayKetThuc.Value.Date) ||
                    // Case 2: Khuyến mãi mới kết thúc trong khoảng thời gian của khuyến mãi cũ
                    (ngayKetThuc >= km.NgayBatDau.Value.Date && ngayKetThuc <= km.NgayKetThuc.Value.Date) ||
                    // Case 3: Khuyến mãi mới bao trùm hoàn toàn khuyến mãi cũ
                    (ngayBatDau <= km.NgayBatDau.Value.Date && ngayKetThuc >= km.NgayKetThuc.Value.Date) ||
                    // Case 4: Khuyến mãi cũ bao trùm hoàn toàn khuyến mãi mới
                    (km.NgayBatDau.Value.Date <= ngayBatDau && km.NgayKetThuc.Value.Date >= ngayKetThuc)
                )
            );

            if (timeOverlap)
                return Conflict(new { message = "Thời gian khuyến mãi bị trùng với một khuyến mãi khác! Vui lòng chọn khoảng thời gian khác." });

            try
            {
                // Chuẩn hóa dữ liệu
                khuyenMai.MaKM = khuyenMai.MaKM.Trim();
                khuyenMai.MoTaKhuyenMai = khuyenMai.MoTaKhuyenMai?.Trim();
                khuyenMai.ApDungVoi ??= "Tất cả";
                khuyenMai.TrangThaiKM ??= "Sắp diễn ra";

                _context.KhuyenMais.Add(khuyenMai);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Thêm Khuyến Mãi thành công!" });
            }
            catch (Exception ex)
            {
                // Log exception (ex)
                return StatusCode(500, new { message = "Lỗi khi thêm Khuyến Mãi. Vui lòng thử lại!" });
            }
        }

        // --- Edit: Cập nhật Khuyến Mãi ---
        [HttpPost]
        [Route("KhuyenMai/Edit/{id}")]
        [Consumes("application/json")]
        public async Task<IActionResult> Edit(int id, [FromBody] KhuyenMai? khuyenMai)
        {
            if (khuyenMai == null)
                return BadRequest(new { message = "Dữ liệu không hợp lệ!" });

            var errors = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(khuyenMai.MaKM))
                errors["MaKM"] = "Phải nhập Mã Khuyến Mãi!";
            if (string.IsNullOrWhiteSpace(khuyenMai.LoaiGiam))
                errors["LoaiGiam"] = "Phải chọn Loại Giảm!";
            if (khuyenMai.GiaTri == null || khuyenMai.GiaTri <= 0)
                errors["GiaTri"] = "Giá Trị Khuyến Mãi phải lớn hơn 0!";
            
            // Validation giá trị theo loại giảm
            if (khuyenMai.GiaTri.HasValue && khuyenMai.GiaTri > 0)
            {
                if (khuyenMai.LoaiGiam == "Phần trăm")
                {
                    if (khuyenMai.GiaTri > 100)
                        errors["GiaTri"] = "Giá trị phần trăm không được vượt quá 100%!";
                }
                else if (khuyenMai.LoaiGiam == "Số tiền")
                {
                    if (khuyenMai.GiaTri <= 0)
                        errors["GiaTri"] = "Giá trị số tiền phải lớn hơn 0!";
                }
            }
            
            if (khuyenMai.NgayBatDau == null)
                errors["NgayBatDau"] = "Phải chọn Ngày Bắt Đầu!";
            if (khuyenMai.NgayKetThuc == null)
                errors["NgayKetThuc"] = "Phải chọn Ngày Kết Thúc!";

            if (khuyenMai.NgayBatDau.HasValue && khuyenMai.NgayKetThuc.HasValue && khuyenMai.NgayBatDau >= khuyenMai.NgayKetThuc)
                errors["NgayKetThuc"] = "Ngày Kết Thúc phải sau Ngày Bắt Đầu!";

            if (errors.Count > 0)
                return BadRequest(errors);

            var existing = await _context.KhuyenMais.FindAsync(id);
            if (existing == null)
                return NotFound(new { message = "Không tìm thấy Khuyến Mãi!" });

            // Check trùng Mã Khuyến Mãi (ngoại trừ chính nó)
            bool exists = await _context.KhuyenMais.AnyAsync(km =>
                km.MaKM!.Trim().ToLower() == khuyenMai.MaKM!.Trim().ToLower() &&
                km.IdKhuyenMai != id
            );
            if (exists)
                return Conflict(new { message = "Mã Khuyến Mãi đã tồn tại cho chương trình khác!" });

            // Check trùng thời gian khuyến mãi (ngoại trừ chính nó)
            var ngayBatDau = khuyenMai.NgayBatDau.Value.Date;
            var ngayKetThuc = khuyenMai.NgayKetThuc.Value.Date;

            bool timeOverlap = await _context.KhuyenMais.AnyAsync(km =>
                km.IdKhuyenMai != id &&
                km.NgayBatDau.HasValue &&
                km.NgayKetThuc.HasValue &&
                (
                    // Case 1: Khuyến mãi mới bắt đầu trong khoảng thời gian của khuyến mãi cũ
                    (ngayBatDau >= km.NgayBatDau.Value.Date && ngayBatDau <= km.NgayKetThuc.Value.Date) ||
                    // Case 2: Khuyến mãi mới kết thúc trong khoảng thời gian của khuyến mãi cũ
                    (ngayKetThuc >= km.NgayBatDau.Value.Date && ngayKetThuc <= km.NgayKetThuc.Value.Date) ||
                    // Case 3: Khuyến mãi mới bao trùm hoàn toàn khuyến mãi cũ
                    (ngayBatDau <= km.NgayBatDau.Value.Date && ngayKetThuc >= km.NgayKetThuc.Value.Date) ||
                    // Case 4: Khuyến mãi cũ bao trùm hoàn toàn khuyến mãi mới
                    (km.NgayBatDau.Value.Date <= ngayBatDau && km.NgayKetThuc.Value.Date >= ngayKetThuc)
                )
            );

            if (timeOverlap)
                return Conflict(new { message = "Thời gian khuyến mãi bị trùng với một khuyến mãi khác! Vui lòng chọn khoảng thời gian khác." });

            try
            {
                // Cập nhật thông tin
                existing.MaKM = khuyenMai.MaKM.Trim();
                existing.MoTaKhuyenMai = khuyenMai.MoTaKhuyenMai?.Trim();
                existing.LoaiGiam = khuyenMai.LoaiGiam;
                existing.ApDungVoi = khuyenMai.ApDungVoi;
                existing.GiaTri = khuyenMai.GiaTri;
                existing.NgayBatDau = khuyenMai.NgayBatDau;
                existing.NgayKetThuc = khuyenMai.NgayKetThuc;
                existing.TrangThaiKM = khuyenMai.TrangThaiKM?.Trim();

                _context.KhuyenMais.Update(existing);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Cập nhật Khuyến Mãi thành công!" });
            }
            catch
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật Khuyến Mãi. Vui lòng thử lại!" });
            }
        }

        // --- Xóa Khuyến Mãi ---
        [HttpPost]
        [Route("KhuyenMai/Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _context.KhuyenMais.FindAsync(id);
            if (existing == null)
                return NotFound(new { message = "Không tìm thấy Khuyến Mãi để xóa!" });

            // Kiểm tra khuyến mãi có đang được sử dụng trong đơn hàng không
            bool usedInDonHang = await _context.DonHangChiTiets
                .AnyAsync(dhct => dhct.IdKhuyenMai == id);

            if (usedInDonHang)
                return BadRequest(new { message = "Không thể xóa khuyến mãi này vì đã được sử dụng trong đơn hàng!" });

            // Kiểm tra khuyến mãi có đang được sử dụng trong hóa đơn không
            bool usedInHoaDon = await _context.HoaDonChiTiets
                .AnyAsync(hdct => hdct.IdKhuyenMai == id);

            if (usedInHoaDon)
                return BadRequest(new { message = "Không thể xóa khuyến mãi này vì đã được sử dụng trong hóa đơn!" });

            // Kiểm tra khuyến mãi có đang được áp dụng cho sản phẩm không
            bool usedInProduct = await _context.ModelSanPhamKhuyenMais
                .AnyAsync(mspkm => mspkm.IdKhuyenMai == id);

            if (usedInProduct)
                return BadRequest(new { message = "Không thể xóa khuyến mãi này vì đang được áp dụng cho sản phẩm!" });

            // Kiểm tra khuyến mãi có đang hoạt động không (Đang diễn ra hoặc Sắp diễn ra)
            var now = DateTime.Now.Date;
            if (existing.NgayBatDau.HasValue && existing.NgayKetThuc.HasValue)
            {
                var ngayKetThuc = existing.NgayKetThuc.Value.Date;
                if (now <= ngayKetThuc && existing.TrangThaiKM != "Đã kết thúc")
                    return BadRequest(new { message = "Không thể xóa khuyến mãi đang hoạt động hoặc sắp diễn ra!" });
            }

            try
            {
                _context.KhuyenMais.Remove(existing);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Xóa Khuyến Mãi thành công!" });
            }
            catch
            {
                return StatusCode(500, new { message = "Lỗi khi xóa Khuyến Mãi. Vui lòng thử lại!" });
            }
        }

        // --- Gán sản phẩm cho khuyến mãi ---
        [HttpPost]
        [Route("KhuyenMai/AddProduct/{id}")]
        [Consumes("application/json")]
        public async Task<IActionResult> AddProduct(int id, [FromBody] AddProductRequest request)
        {
            try
            {
                if (request == null || request.IdModelSanPham == null || request.IdModelSanPham <= 0)
                {
                    return BadRequest(new { message = "Dữ liệu không hợp lệ!" });
                }

                var khuyenMai = await _context.KhuyenMais.FindAsync(id);
                if (khuyenMai == null)
                {
                    return NotFound(new { message = "Không tìm thấy khuyến mãi!" });
                }

                // Kiểm tra sản phẩm tồn tại
                var modelSanPham = await _context.ModelSanPhams.FindAsync(request.IdModelSanPham);
                if (modelSanPham == null)
                {
                    return NotFound(new { message = "Không tìm thấy sản phẩm!" });
                }

                // Kiểm tra đã được gán chưa
                bool alreadyAssigned = await _context.ModelSanPhamKhuyenMais
                    .AnyAsync(mspkm => mspkm.IdModelSanPham == request.IdModelSanPham && mspkm.IdKhuyenMai == id);

                if (alreadyAssigned)
                {
                    return Conflict(new { message = "Sản phẩm này đã được gán cho khuyến mãi này!" });
                }

                // Tạo mới liên kết
                var modelSanPhamKhuyenMai = new ModelSanPhamKhuyenMai
                {
                    IdModelSanPham = request.IdModelSanPham,
                    IdKhuyenMai = id,
                    NgayTao = DateTime.Now
                };

                _context.ModelSanPhamKhuyenMais.Add(modelSanPhamKhuyenMai);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Gán sản phẩm cho khuyến mãi thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi: " + ex.Message });
            }
        }

        // --- Xóa sản phẩm khỏi khuyến mãi ---
        [HttpPost]
        [Route("KhuyenMai/RemoveProduct/{id}")]
        [Consumes("application/json")]
        public async Task<IActionResult> RemoveProduct(int id, [FromBody] RemoveProductRequest request)
        {
            try
            {
                if (request == null || request.IdModelSanPham == null || request.IdModelSanPham <= 0)
                {
                    return BadRequest(new { message = "Dữ liệu không hợp lệ!" });
                }

                var link = await _context.ModelSanPhamKhuyenMais
                    .FirstOrDefaultAsync(mspkm => mspkm.IdKhuyenMai == id && mspkm.IdModelSanPham == request.IdModelSanPham);

                if (link == null)
                {
                    return NotFound(new { message = "Không tìm thấy liên kết!" });
                }

                _context.ModelSanPhamKhuyenMais.Remove(link);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Xóa sản phẩm khỏi khuyến mãi thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi: " + ex.Message });
            }
        }

        // --- Lấy danh sách sản phẩm của khuyến mãi ---
        [HttpGet]
        [Route("KhuyenMai/GetProducts/{id}")]
        public async Task<IActionResult> GetProducts(int id)
        {
            try
            {
                var products = await _context.ModelSanPhamKhuyenMais
                    .Include(mspkm => mspkm.ModelSanPham)
                        .ThenInclude(m => m.SanPham)
                            .ThenInclude(sp => sp.ThuongHieu)
                    .Include(mspkm => mspkm.ModelSanPham)
                        .ThenInclude(m => m.AnhSanPhams)
                    .Where(mspkm => mspkm.IdKhuyenMai == id)
                    .Select(mspkm => new
                    {
                        IdModelSanPham = mspkm.ModelSanPham.IdModelSanPham,
                        TenModel = mspkm.ModelSanPham.TenModel ?? "N/A",
                        TenSanPham = mspkm.ModelSanPham.SanPham.TenSanPham ?? "N/A",
                        TenThuongHieu = mspkm.ModelSanPham.SanPham.ThuongHieu.TenThuongHieu ?? "N/A",
                        GiaBan = mspkm.ModelSanPham.GiaBanModel ?? 0,
                        HinhAnh = mspkm.ModelSanPham.AnhSanPhams.FirstOrDefault().DuongDan ?? "/images/default-product.jpg"
                    })
                    .ToListAsync();

                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi: " + ex.Message });
            }
        }
    }

    // Request models
    public class AddProductRequest
    {
        public int? IdModelSanPham { get; set; }
    }

    public class RemoveProductRequest
    {
        public int? IdModelSanPham { get; set; }
    }
}