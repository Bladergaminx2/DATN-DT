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

        // VẤN ĐỀ 5: Helper: Cập nhật trạng thái khuyến mãi
        private void UpdatePromotionStatus(KhuyenMai km, DateTime now)
        {
            if (km.NgayKetThuc.HasValue && km.NgayBatDau.HasValue)
            {
                var ngayKetThuc = km.NgayKetThuc.Value.Date;
                var ngayBatDau = km.NgayBatDau.Value.Date;

                // Nếu đã quá hạn, chuyển sang "Đã kết thúc"
                if (now > ngayKetThuc)
                {
                    km.TrangThaiKM = "Đã kết thúc";
                }
                // Nếu đang trong thời gian diễn ra
                else if (ngayBatDau <= now && now <= ngayKetThuc)
                {
                    km.TrangThaiKM = "Đang diễn ra";
                }
                // Nếu chưa đến ngày bắt đầu
                else if (now < ngayBatDau)
                {
                    km.TrangThaiKM = "Sắp diễn ra";
                }
            }
        }

        // VẤN ĐỀ 5: Helper: Kiểm tra khuyến mãi có đang active không
        private bool IsPromotionActive(KhuyenMai km, DateTime now)
        {
            if (!km.NgayBatDau.HasValue || !km.NgayKetThuc.HasValue)
                return false;

            var ngayBatDau = km.NgayBatDau.Value.Date;
            var ngayKetThuc = km.NgayKetThuc.Value.Date;

            return ngayBatDau <= now && now <= ngayKetThuc;
        }

        // VẤN ĐỀ 3: Helper: Tính giá sau giảm (có thể tái sử dụng trong controller này)
        private decimal CalculateDiscountedPrice(decimal originalPrice, string? discountType, decimal? discountValue)
        {
            if (discountValue == null || discountValue <= 0 || string.IsNullOrWhiteSpace(discountType))
                return originalPrice;

            decimal discountedPrice = 0;

            if (discountType == "Phần trăm")
            {
                // Giảm theo phần trăm (đảm bảo không vượt quá 100%)
                var percent = Math.Min(100, Math.Max(0, discountValue.Value));
                discountedPrice = originalPrice * (1 - percent / 100);
            }
            else if (discountType == "Số tiền")
            {
                // Giảm theo số tiền (đảm bảo không vượt quá giá gốc)
                var discountAmount = Math.Min(originalPrice, Math.Max(0, discountValue.Value));
                discountedPrice = originalPrice - discountAmount;
            }
            else
            {
                // Mặc định không giảm
                discountedPrice = originalPrice;
            }

            // Làm tròn đến 1000 VNĐ (làm tròn xuống)
            discountedPrice = Math.Floor(discountedPrice / 1000) * 1000;

            // Đảm bảo giá không âm
            return Math.Max(0, discountedPrice);
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
                var oldStatus = km.TrangThaiKM;
                UpdatePromotionStatus(km, now);
                if (km.TrangThaiKM != oldStatus)
                {
                    hasChanges = true;
                }
            }

            // Lưu các thay đổi nếu có
            if (hasChanges)
            {
                await _context.SaveChangesAsync();
            }

            // Lấy danh sách sản phẩm để hiển thị trong form
            var products = await _context.ModelSanPhams
                .Include(m => m.SanPham)
                    .ThenInclude(sp => sp.ThuongHieu)
                .Where(m => m.TrangThai == 1)
                .Select(m => new
                {
                    IdModelSanPham = m.IdModelSanPham,
                    TenModel = m.TenModel ?? "N/A",
                    TenSanPham = m.SanPham != null ? m.SanPham.TenSanPham : "N/A",
                    TenThuongHieu = m.SanPham != null && m.SanPham.ThuongHieu != null ? m.SanPham.ThuongHieu.TenThuongHieu : "N/A",
                    GiaBan = m.GiaBanModel ?? 0
                })
                .OrderBy(p => p.TenThuongHieu)
                .ThenBy(p => p.TenSanPham)
                .ToListAsync();

            ViewBag.Products = products;

            // Lấy số lượng sản phẩm cho mỗi khuyến mãi
            var promotionProductCounts = await _context.ModelSanPhamKhuyenMais
                .GroupBy(mspkm => mspkm.IdKhuyenMai)
                .Select(g => new { IdKhuyenMai = g.Key, Count = g.Count() })
                .ToListAsync();

            ViewBag.PromotionProductCounts = promotionProductCounts.ToDictionary(x => x.IdKhuyenMai, x => x.Count);

            return View(khuyenMais);
        }

        // API: Lấy danh sách sản phẩm (cho AJAX)
        [HttpGet]
        [Route("KhuyenMai/GetAllProducts")]
        public async Task<IActionResult> GetAllProducts()
        {
            try
            {
                var products = await _context.ModelSanPhams
                    .Include(m => m.SanPham)
                        .ThenInclude(sp => sp.ThuongHieu)
                    .Where(m => m.TrangThai == 1)
                    .Select(m => new
                    {
                        idModelSanPham = m.IdModelSanPham,
                        tenModel = m.TenModel ?? "N/A",
                        tenSanPham = m.SanPham != null ? m.SanPham.TenSanPham : "N/A",
                        tenThuongHieu = m.SanPham != null && m.SanPham.ThuongHieu != null ? m.SanPham.ThuongHieu.TenThuongHieu : "N/A",
                        giaBan = m.GiaBanModel ?? 0
                    })
                    .OrderBy(p => p.tenThuongHieu)
                    .ThenBy(p => p.tenSanPham)
                    .ToListAsync();

                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi: " + ex.Message });
            }
        }

        // API: Validate sản phẩm có đang trong khuyến mãi không
        [HttpPost]
        [Route("KhuyenMai/ValidateProducts")]
        [Consumes("application/json")]
        public async Task<IActionResult> ValidateProducts([FromBody] ValidateProductsRequest request)
        {
            try
            {
                if (request == null || request.ProductIds == null || !request.ProductIds.Any())
                {
                    return Ok(new { valid = true, message = "" });
                }

                if (!request.FromDate.HasValue || !request.ToDate.HasValue)
                {
                    return BadRequest(new { valid = false, message = "Phải cung cấp ngày bắt đầu và ngày kết thúc!" });
                }

                var fromDate = request.FromDate.Value.Date;
                var toDate = request.ToDate.Value.Date;

                // Kiểm tra từng sản phẩm có đang trong khuyến mãi nào không
                var productsInPromotion = await _context.ModelSanPhamKhuyenMais
                    .Include(mspkm => mspkm.KhuyenMai)
                    .Include(mspkm => mspkm.ModelSanPham)
                    .Where(mspkm => request.ProductIds.Contains(mspkm.IdModelSanPham.Value)
                        && mspkm.KhuyenMai != null
                        && mspkm.KhuyenMai.NgayBatDau.HasValue
                        && mspkm.KhuyenMai.NgayKetThuc.HasValue)
                    .ToListAsync();

                var conflicts = new List<object>();

                foreach (var link in productsInPromotion)
                {
                    var promoStart = link.KhuyenMai.NgayBatDau.Value.Date;
                    var promoEnd = link.KhuyenMai.NgayKetThuc.Value.Date;

                    // Kiểm tra trùng khoảng thời gian
                    bool timeOverlap = (fromDate >= promoStart && fromDate <= promoEnd) ||
                                      (toDate >= promoStart && toDate <= promoEnd) ||
                                      (fromDate <= promoStart && toDate >= promoEnd) ||
                                      (promoStart <= fromDate && promoEnd >= toDate);

                    if (timeOverlap)
                    {
                        conflicts.Add(new
                        {
                            productId = link.IdModelSanPham,
                            productName = link.ModelSanPham?.TenModel ?? "N/A",
                            promotionCode = link.KhuyenMai.MaKM,
                            promotionPeriod = $"{promoStart:dd/MM/yyyy} - {promoEnd:dd/MM/yyyy}"
                        });
                    }
                }

                if (conflicts.Any())
                {
                    var conflictMessages = conflicts.Select(c => 
                        $"- {((dynamic)c).productName} đang trong khuyến mãi {((dynamic)c).promotionCode} ({((dynamic)c).promotionPeriod})"
                    ).ToList();
                    
                    return Ok(new
                    {
                        valid = false,
                        message = "Các sản phẩm sau đang trong khuyến mãi khác:\n" + string.Join("\n", conflictMessages),
                        conflicts = conflicts
                    });
                }

                Console.WriteLine($"[KhuyenMai] ValidateProducts: {request.ProductIds.Count} sản phẩm, {conflicts.Count} xung đột");
                
                return Ok(new { valid = true, message = "" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[KhuyenMai] Lỗi khi validate sản phẩm: {ex.Message}");
                Console.WriteLine($"[KhuyenMai] Stack trace: {ex.StackTrace}");
                
                return StatusCode(500, new { 
                    valid = false, 
                    message = "Lỗi hệ thống khi kiểm tra sản phẩm. Vui lòng thử lại!",
                    error = ex.Message 
                });
            }
        }

        // --- Create: Thêm Khuyến Mãi mới ---
        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> Create([FromBody] CreateKhuyenMaiRequest? request)
        {
            if (request == null || request.KhuyenMai == null)
                return BadRequest(new { message = "Dữ liệu không hợp lệ!" });

            var khuyenMai = request.KhuyenMai;
            var errors = new Dictionary<string, string>();

            // Validation cơ bản
            if (string.IsNullOrWhiteSpace(khuyenMai.MaKM))
                errors["MaKM"] = "Phải nhập Mã Khuyến Mãi!";
            if (string.IsNullOrWhiteSpace(khuyenMai.LoaiGiam))
                errors["LoaiGiam"] = "Phải chọn Loại Giảm!";
            if (khuyenMai.GiaTri == null || khuyenMai.GiaTri <= 0)
                errors["GiaTri"] = "Giá Trị Khuyến Mãi phải lớn hơn 0!";
            
            // Validation giá trị theo loại giảm - VẤN ĐỀ 2
            if (khuyenMai.GiaTri.HasValue && khuyenMai.GiaTri > 0)
            {
                if (khuyenMai.LoaiGiam == "Phần trăm")
                {
                    // Phần trăm: phải > 0 và <= 100
                    if (khuyenMai.GiaTri <= 0 || khuyenMai.GiaTri > 100)
                        errors["GiaTri"] = "Giá trị phần trăm phải từ 0.01% đến 100%!";
                }
                else if (khuyenMai.LoaiGiam == "Số tiền")
                {
                    // Số tiền: min 1,000 VNĐ, max 100,000,000 VNĐ
                    const decimal MIN_DISCOUNT = 1000;
                    const decimal MAX_DISCOUNT = 100000000;
                    
                    if (khuyenMai.GiaTri < MIN_DISCOUNT)
                        errors["GiaTri"] = $"Giá trị số tiền tối thiểu là {MIN_DISCOUNT:N0} VNĐ!";
                    else if (khuyenMai.GiaTri > MAX_DISCOUNT)
                        errors["GiaTri"] = $"Giá trị số tiền tối đa là {MAX_DISCOUNT:N0} VNĐ!";
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

            // Validate sản phẩm nếu có danh sách sản phẩm được chọn
            if (request.ProductIds != null && request.ProductIds.Any())
            {
                var fromDate = khuyenMai.NgayBatDau.Value.Date;
                var toDate = khuyenMai.NgayKetThuc.Value.Date;

                // Kiểm tra từng sản phẩm có đang trong khuyến mãi nào không
                var productsInPromotion = await _context.ModelSanPhamKhuyenMais
                    .Include(mspkm => mspkm.KhuyenMai)
                    .Include(mspkm => mspkm.ModelSanPham)
                    .Where(mspkm => request.ProductIds.Contains(mspkm.IdModelSanPham.Value)
                        && mspkm.KhuyenMai != null
                        && mspkm.KhuyenMai.NgayBatDau.HasValue
                        && mspkm.KhuyenMai.NgayKetThuc.HasValue)
                    .ToListAsync();

                var conflicts = new List<string>();

                foreach (var link in productsInPromotion)
                {
                    var promoStart = link.KhuyenMai.NgayBatDau.Value.Date;
                    var promoEnd = link.KhuyenMai.NgayKetThuc.Value.Date;

                    // Kiểm tra trùng khoảng thời gian
                    bool timeOverlap = (fromDate >= promoStart && fromDate <= promoEnd) ||
                                      (toDate >= promoStart && toDate <= promoEnd) ||
                                      (fromDate <= promoStart && toDate >= promoEnd) ||
                                      (promoStart <= fromDate && promoEnd >= toDate);

                    if (timeOverlap)
                    {
                        var productName = link.ModelSanPham?.TenModel ?? "N/A";
                        conflicts.Add($"{productName} đang trong khuyến mãi {link.KhuyenMai.MaKM} ({promoStart:dd/MM/yyyy} - {promoEnd:dd/MM/yyyy})");
                    }
                }

                if (conflicts.Any())
                {
                    return BadRequest(new { 
                        message = "Các sản phẩm sau đang trong khuyến mãi khác trong khoảng thời gian này:\n" + string.Join("\n", conflicts),
                        conflicts = conflicts
                    });
                }
            }

            try
            {
                // Chuẩn hóa dữ liệu
                khuyenMai.MaKM = khuyenMai.MaKM.Trim();
                khuyenMai.MoTaKhuyenMai = khuyenMai.MoTaKhuyenMai?.Trim();
                
                // VẤN ĐỀ 5: Cập nhật trạng thái khi tạo mới
                var now = DateTime.Now.Date;
                UpdatePromotionStatus(khuyenMai, now);

                _context.KhuyenMais.Add(khuyenMai);
                await _context.SaveChangesAsync();

                // Nếu có danh sách sản phẩm, gán luôn và cập nhật giá
                if (request.ProductIds != null && request.ProductIds.Any())
                {
                    foreach (var productId in request.ProductIds)
                    {
                        var modelSanPham = await _context.ModelSanPhams
                            .Include(m => m.SanPham)
                            .FirstOrDefaultAsync(m => m.IdModelSanPham == productId);
                        
                        if (modelSanPham == null)
                            continue;

                        var sanPham = modelSanPham.SanPham;
                        
                        // SỬA LỖI: Lấy giá gốc từ SanPham.GiaGoc trước, nếu chưa có thì mới lấy GiaBanModel
                        decimal giaGocHienTai = 0;
                        if (sanPham != null && sanPham.GiaGoc.HasValue && sanPham.GiaGoc > 0)
                        {
                            // Nếu đã có giá gốc, dùng giá gốc
                            giaGocHienTai = sanPham.GiaGoc.Value;
                        }
                        else
                        {
                            // Nếu chưa có giá gốc, lấy từ GiaBanModel hiện tại
                            giaGocHienTai = modelSanPham.GiaBanModel ?? 0;
                            
                            // Lưu giá gốc vào SanPham.GiaGoc nếu chưa có
                            if (sanPham != null && giaGocHienTai > 0)
                            {
                                sanPham.GiaGoc = giaGocHienTai;
                            }
                        }

                        // Kiểm tra sản phẩm có giá hợp lệ không
                        if (giaGocHienTai <= 0)
                        {
                            // Xóa khuyến mãi vừa tạo
                            _context.KhuyenMais.Remove(khuyenMai);
                            await _context.SaveChangesAsync();
                            
                            return BadRequest(new { message = $"Sản phẩm {modelSanPham.TenModel ?? "N/A"} không có giá hợp lệ! Vui lòng kiểm tra lại giá sản phẩm." });
                        }

                        // Kiểm tra giá trị giảm nếu là số tiền
                        if (khuyenMai.LoaiGiam == "Số tiền" && khuyenMai.GiaTri.HasValue)
                        {
                            if (khuyenMai.GiaTri > giaGocHienTai)
                            {
                                // Xóa khuyến mãi vừa tạo
                                _context.KhuyenMais.Remove(khuyenMai);
                                await _context.SaveChangesAsync();
                                
                                return BadRequest(new { message = $"Giá trị giảm ({khuyenMai.GiaTri:N0} VNĐ) vượt quá giá gốc của sản phẩm {modelSanPham.TenModel ?? "N/A"} ({giaGocHienTai:N0} VNĐ)!" });
                            }
                        }

                        // Tính giá sau khuyến mãi và cập nhật vào ModelSanPham
                        var giaSauGiam = CalculateDiscountedPrice(giaGocHienTai, khuyenMai.LoaiGiam, khuyenMai.GiaTri);
                        modelSanPham.GiaBanModel = giaSauGiam;

                        var modelSanPhamKhuyenMai = new ModelSanPhamKhuyenMai
                        {
                            IdModelSanPham = productId,
                            IdKhuyenMai = khuyenMai.IdKhuyenMai,
                            NgayTao = DateTime.Now
                        };
                        _context.ModelSanPhamKhuyenMais.Add(modelSanPhamKhuyenMai);
                    }
                    await _context.SaveChangesAsync();
                }

                return Ok(new { 
                    message = "Thêm Khuyến Mãi thành công!",
                    idKhuyenMai = khuyenMai.IdKhuyenMai
                });
            }
            catch (Exception ex)
            {
                // Log exception chi tiết để debug
                Console.WriteLine($"[KhuyenMai] Lỗi khi tạo khuyến mãi: {ex.Message}");
                Console.WriteLine($"[KhuyenMai] Stack trace: {ex.StackTrace}");
                
                return StatusCode(500, new { 
                    message = "Lỗi hệ thống khi thêm Khuyến Mãi. Vui lòng thử lại!",
                    error = ex.Message 
                });
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
            
            // Validation giá trị theo loại giảm - VẤN ĐỀ 2
            if (khuyenMai.GiaTri.HasValue && khuyenMai.GiaTri > 0)
            {
                if (khuyenMai.LoaiGiam == "Phần trăm")
                {
                    // Phần trăm: phải > 0 và <= 100
                    if (khuyenMai.GiaTri <= 0 || khuyenMai.GiaTri > 100)
                        errors["GiaTri"] = "Giá trị phần trăm phải từ 0.01% đến 100%!";
                }
                else if (khuyenMai.LoaiGiam == "Số tiền")
                {
                    // Số tiền: min 1,000 VNĐ, max 100,000,000 VNĐ
                    const decimal MIN_DISCOUNT = 1000;
                    const decimal MAX_DISCOUNT = 100000000;
                    
                    if (khuyenMai.GiaTri < MIN_DISCOUNT)
                        errors["GiaTri"] = $"Giá trị số tiền tối thiểu là {MIN_DISCOUNT:N0} VNĐ!";
                    else if (khuyenMai.GiaTri > MAX_DISCOUNT)
                        errors["GiaTri"] = $"Giá trị số tiền tối đa là {MAX_DISCOUNT:N0} VNĐ!";
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

            try
            {
                // Cập nhật thông tin
                existing.MaKM = khuyenMai.MaKM.Trim();
                existing.MoTaKhuyenMai = khuyenMai.MoTaKhuyenMai?.Trim();
                existing.LoaiGiam = khuyenMai.LoaiGiam;
                existing.GiaTri = khuyenMai.GiaTri;
                existing.NgayBatDau = khuyenMai.NgayBatDau;
                existing.NgayKetThuc = khuyenMai.NgayKetThuc;
                
                // VẤN ĐỀ 5: Tự động cập nhật trạng thái dựa trên ngày (không dùng giá trị từ client)
                var now = DateTime.Now.Date;
                UpdatePromotionStatus(existing, now);

                _context.KhuyenMais.Update(existing);
                
                // Cập nhật lại giá cho tất cả sản phẩm đã được gán với khuyến mãi này
                var productsWithPromotion = await _context.ModelSanPhamKhuyenMais
                    .Include(mspkm => mspkm.ModelSanPham)
                        .ThenInclude(m => m.SanPham)
                    .Where(mspkm => mspkm.IdKhuyenMai == id)
                    .ToListAsync();

                var skippedProducts = new List<string>();
                var updatedCount = 0;
                
                foreach (var link in productsWithPromotion)
                {
                    var modelSanPham = link.ModelSanPham;
                    if (modelSanPham != null)
                    {
                        var sanPham = modelSanPham.SanPham;
                        
                        // SỬA LỖI: Lấy giá gốc từ SanPham.GiaGoc trước, nếu chưa có thì mới lấy GiaBanModel
                        decimal giaGoc = 0;
                        if (sanPham != null && sanPham.GiaGoc.HasValue && sanPham.GiaGoc > 0)
                        {
                            // Nếu đã có giá gốc, dùng giá gốc
                            giaGoc = sanPham.GiaGoc.Value;
                        }
                        else
                        {
                            // Nếu chưa có giá gốc, lấy từ GiaBanModel hiện tại (có thể đã bị giảm)
                            giaGoc = modelSanPham.GiaBanModel ?? 0;
                            
                            // Lưu giá gốc vào SanPham.GiaGoc nếu chưa có
                            if (sanPham != null && giaGoc > 0)
                            {
                                sanPham.GiaGoc = giaGoc;
                            }
                        }
                        
                        // Kiểm tra sản phẩm có giá hợp lệ không
                        if (giaGoc <= 0)
                        {
                            skippedProducts.Add($"{modelSanPham.TenModel ?? "N/A"} (không có giá hợp lệ)");
                            Console.WriteLine($"[KhuyenMai] Bỏ qua sản phẩm {modelSanPham.TenModel} vì không có giá hợp lệ");
                            continue;
                        }
                        
                        // Kiểm tra giá trị giảm có hợp lý không (nếu là số tiền)
                        if (existing.LoaiGiam == "Số tiền" && existing.GiaTri.HasValue)
                        {
                            if (existing.GiaTri > giaGoc)
                            {
                                skippedProducts.Add($"{modelSanPham.TenModel ?? "N/A"} (giá trị giảm {existing.GiaTri:N0}₫ vượt quá giá gốc {giaGoc:N0}₫)");
                                Console.WriteLine($"[KhuyenMai] Bỏ qua sản phẩm {modelSanPham.TenModel} vì giá trị giảm vượt quá giá gốc");
                                continue;
                            }
                        }
                        
                        // Tính lại giá sau khuyến mãi và cập nhật
                        var giaSauGiam = CalculateDiscountedPrice(giaGoc, existing.LoaiGiam, existing.GiaTri);
                        modelSanPham.GiaBanModel = giaSauGiam;
                        updatedCount++;
                    }
                }

                await _context.SaveChangesAsync();

                var message = $"Cập nhật Khuyến Mãi thành công! Giá của {updatedCount} sản phẩm đã được cập nhật.";
                if (skippedProducts.Any())
                {
                    message += $" {skippedProducts.Count} sản phẩm đã bị bỏ qua do giá trị giảm không hợp lệ.";
                }
                
                Console.WriteLine($"[KhuyenMai] Đã cập nhật khuyến mãi {existing.MaKM}: {updatedCount} sản phẩm được cập nhật, {skippedProducts.Count} sản phẩm bị bỏ qua");
                
                return Ok(new { 
                    message = message,
                    updatedProductsCount = updatedCount,
                    skippedProductsCount = skippedProducts.Count,
                    skippedProducts = skippedProducts.Any() ? skippedProducts : null
                });
            }
            catch (Exception ex)
            {
                // Log lỗi chi tiết để debug
                Console.WriteLine($"[KhuyenMai] Lỗi khi cập nhật khuyến mãi {id}: {ex.Message}");
                Console.WriteLine($"[KhuyenMai] Stack trace: {ex.StackTrace}");
                
                return StatusCode(500, new { 
                    message = "Lỗi hệ thống khi cập nhật Khuyến Mãi. Vui lòng thử lại!",
                    error = ex.Message 
                });
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

            // LƯU Ý: Không cần kiểm tra DonHangChiTiet và HoaDonChiTiet vì giờ chỉ lưu giá khuyến mãi (GiaKhuyenMai), không lưu IdKhuyenMai
            // Việc kiểm tra sử dụng khuyến mãi chỉ cần qua ModelSanPhamKhuyenMai

            // Lấy danh sách sản phẩm đang áp dụng khuyến mãi này
            var productsWithPromotion = await _context.ModelSanPhamKhuyenMais
                .Include(mspkm => mspkm.ModelSanPham)
                    .ThenInclude(m => m.SanPham)
                .Where(mspkm => mspkm.IdKhuyenMai == id)
                .ToListAsync();

            // Kiểm tra khuyến mãi có đang hoạt động không (Đang diễn ra hoặc Sắp diễn ra)
            var now = DateTime.Now.Date;
            bool isActive = false;
            if (existing.NgayBatDau.HasValue && existing.NgayKetThuc.HasValue)
            {
                var ngayKetThuc = existing.NgayKetThuc.Value.Date;
                if (now <= ngayKetThuc && existing.TrangThaiKM != "Đã kết thúc")
                {
                    isActive = true;
                }
            }

            if (isActive && productsWithPromotion.Any())
                return BadRequest(new { message = "Không thể xóa khuyến mãi đang hoạt động và đang được áp dụng cho sản phẩm!" });

            try
            {
                // Khôi phục giá gốc cho tất cả sản phẩm trước khi xóa khuyến mãi
                foreach (var link in productsWithPromotion)
                {
                    var modelSanPham = link.ModelSanPham;
                    if (modelSanPham != null)
                    {
                        var sanPham = modelSanPham.SanPham;
                        if (sanPham != null && sanPham.GiaGoc.HasValue && sanPham.GiaGoc > 0)
                        {
                            modelSanPham.GiaBanModel = sanPham.GiaGoc;
                        }
                    }
                }

                // Xóa tất cả liên kết sản phẩm
                _context.ModelSanPhamKhuyenMais.RemoveRange(productsWithPromotion);
                
                // Xóa khuyến mãi
                _context.KhuyenMais.Remove(existing);
                await _context.SaveChangesAsync();
                var restoredProductsCount = productsWithPromotion.Count;
                Console.WriteLine($"[KhuyenMai] Đã xóa khuyến mãi {existing.MaKM} và khôi phục giá cho {restoredProductsCount} sản phẩm");
                
                return Ok(new { 
                    message = $"Xóa Khuyến Mãi thành công! Giá của {restoredProductsCount} sản phẩm đã được khôi phục.",
                    restoredProductsCount = restoredProductsCount
                });
            }
            catch (Exception ex)
            {
                // Log lỗi chi tiết để debug
                Console.WriteLine($"[KhuyenMai] Lỗi khi xóa khuyến mãi {id}: {ex.Message}");
                Console.WriteLine($"[KhuyenMai] Stack trace: {ex.StackTrace}");
                
                return StatusCode(500, new { 
                    message = "Lỗi hệ thống khi xóa Khuyến Mãi. Vui lòng thử lại!",
                    error = ex.Message 
                });
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

                // VẤN ĐỀ 4: Kiểm tra sản phẩm có đang trong khuyến mãi active khác không
                var now = DateTime.Now.Date;
                
                // Cập nhật trạng thái khuyến mãi trước khi kiểm tra - VẤN ĐỀ 5
                UpdatePromotionStatus(khuyenMai, now);
                
                // Kiểm tra sản phẩm có đang trong khuyến mãi active khác không
                var activePromotionsForProduct = await _context.ModelSanPhamKhuyenMais
                    .Include(mspkm => mspkm.KhuyenMai)
                    .Where(mspkm => mspkm.IdModelSanPham == request.IdModelSanPham 
                        && mspkm.IdKhuyenMai != id
                        && mspkm.KhuyenMai != null
                        && mspkm.KhuyenMai.NgayBatDau.HasValue
                        && mspkm.KhuyenMai.NgayKetThuc.HasValue
                        && now >= mspkm.KhuyenMai.NgayBatDau.Value.Date
                        && now <= mspkm.KhuyenMai.NgayKetThuc.Value.Date)
                    .ToListAsync();

                // Lấy thông tin SanPham trước
                var sanPham = await _context.SanPhams.FindAsync(modelSanPham.IdSanPham);
                
                // SỬA LỖI: Lấy giá gốc từ SanPham.GiaGoc trước, nếu chưa có thì mới lấy GiaBanModel
                decimal giaGocHienTai = 0;
                if (sanPham != null && sanPham.GiaGoc.HasValue && sanPham.GiaGoc > 0)
                {
                    // Nếu đã có giá gốc, dùng giá gốc
                    giaGocHienTai = sanPham.GiaGoc.Value;
                }
                else
                {
                    // Nếu chưa có giá gốc, lấy từ GiaBanModel hiện tại (có thể đã bị giảm)
                    giaGocHienTai = modelSanPham.GiaBanModel ?? 0;
                    
                    // Lưu giá gốc vào SanPham.GiaGoc nếu chưa có
                    if (sanPham != null && giaGocHienTai > 0)
                    {
                        sanPham.GiaGoc = giaGocHienTai;
                    }
                }

                // CẢI THIỆN: Thông báo chi tiết khi sản phẩm đang có khuyến mãi active
                if (activePromotionsForProduct.Any())
                {
                    // SỬA LỖI: Khôi phục giá gốc trước khi remove khuyến mãi cũ
                    if (sanPham != null && sanPham.GiaGoc.HasValue && sanPham.GiaGoc > 0)
                    {
                        modelSanPham.GiaBanModel = sanPham.GiaGoc.Value;
                    }
                    
                    // Thu thập thông tin khuyến mãi cũ để thông báo
                    var oldPromotionInfo = activePromotionsForProduct.Select(op => new
                    {
                        MaKM = op.KhuyenMai?.MaKM ?? "N/A",
                        NgayBatDau = op.KhuyenMai?.NgayBatDau?.ToString("dd/MM/yyyy") ?? "N/A",
                        NgayKetThuc = op.KhuyenMai?.NgayKetThuc?.ToString("dd/MM/yyyy") ?? "N/A"
                    }).ToList();
                    
                    // Tự động remove khuyến mãi cũ và thêm khuyến mãi mới
                    foreach (var oldPromotion in activePromotionsForProduct)
                    {
                        _context.ModelSanPhamKhuyenMais.Remove(oldPromotion);
                    }
                    
                    // Log thông tin để debug
                    Console.WriteLine($"[KhuyenMai] Sản phẩm {modelSanPham.TenModel} đã được chuyển từ {oldPromotionInfo.Count} khuyến mãi cũ sang khuyến mãi mới {khuyenMai.MaKM}");
                }

                // Kiểm tra sản phẩm có giá hợp lệ không
                if (giaGocHienTai <= 0)
                {
                    return BadRequest(new { message = "Sản phẩm không có giá hợp lệ! Vui lòng kiểm tra lại giá sản phẩm." });
                }

                // VẤN ĐỀ 2: Kiểm tra giá trị giảm có hợp lý không (nếu là số tiền)
                if (khuyenMai.LoaiGiam == "Số tiền" && khuyenMai.GiaTri.HasValue)
                {
                    if (khuyenMai.GiaTri > giaGocHienTai)
                    {
                        return BadRequest(new { message = $"Giá trị giảm ({khuyenMai.GiaTri:N0} VNĐ) không được vượt quá giá gốc của sản phẩm ({giaGocHienTai:N0} VNĐ)!" });
                    }
                    if (khuyenMai.GiaTri <= 0)
                    {
                        return BadRequest(new { message = "Giá trị giảm phải lớn hơn 0 VNĐ!" });
                    }
                }

                // Tính giá sau khuyến mãi
                var giaSauGiam = CalculateDiscountedPrice(giaGocHienTai, khuyenMai.LoaiGiam, khuyenMai.GiaTri);
                
                // Cập nhật giá bán của ModelSanPham thành giá sau khuyến mãi (xóa giá gốc)
                modelSanPham.GiaBanModel = giaSauGiam;

                // Tạo mới liên kết
                var modelSanPhamKhuyenMai = new ModelSanPhamKhuyenMai
                {
                    IdModelSanPham = request.IdModelSanPham,
                    IdKhuyenMai = id,
                    NgayTao = DateTime.Now
                };

                _context.ModelSanPhamKhuyenMais.Add(modelSanPhamKhuyenMai);
                await _context.SaveChangesAsync();

                var message = activePromotionsForProduct.Any() 
                    ? $"Gán sản phẩm cho khuyến mãi thành công! Đã tự động gỡ {activePromotionsForProduct.Count} khuyến mãi cũ và áp dụng khuyến mãi mới. Giá đã được cập nhật."
                    : "Gán sản phẩm cho khuyến mãi thành công! Giá đã được cập nhật.";
                
                return Ok(new { 
                    message = message,
                    replacedPromotions = activePromotionsForProduct.Any() ? activePromotionsForProduct.Count : 0
                });
            }
            catch (Exception ex)
            {
                // Log lỗi chi tiết để debug
                Console.WriteLine($"[KhuyenMai] Lỗi khi gán sản phẩm {request.IdModelSanPham} cho khuyến mãi {id}: {ex.Message}");
                Console.WriteLine($"[KhuyenMai] Stack trace: {ex.StackTrace}");
                
                return StatusCode(500, new { 
                    message = "Lỗi hệ thống khi gán sản phẩm cho khuyến mãi. Vui lòng thử lại!",
                    error = ex.Message 
                });
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
                    .Include(mspkm => mspkm.ModelSanPham)
                        .ThenInclude(m => m.SanPham)
                    .FirstOrDefaultAsync(mspkm => mspkm.IdKhuyenMai == id && mspkm.IdModelSanPham == request.IdModelSanPham);

                if (link == null)
                {
                    return NotFound(new { message = "Không tìm thấy liên kết!" });
                }

                var modelSanPham = link.ModelSanPham;
                if (modelSanPham != null)
                {
                    // Khôi phục giá gốc từ SanPham.GiaGoc về GiaBanModel
                    var sanPham = modelSanPham.SanPham;
                    if (sanPham != null && sanPham.GiaGoc.HasValue && sanPham.GiaGoc > 0)
                    {
                        modelSanPham.GiaBanModel = sanPham.GiaGoc;
                    }
                    // Nếu không có giá gốc, giữ nguyên giá hiện tại
                }

                _context.ModelSanPhamKhuyenMais.Remove(link);
                await _context.SaveChangesAsync();

                Console.WriteLine($"[KhuyenMai] Đã xóa sản phẩm {request.IdModelSanPham} khỏi khuyến mãi {id} và khôi phục giá gốc");
                
                return Ok(new { 
                    message = "Xóa sản phẩm khỏi khuyến mãi thành công! Giá đã được khôi phục.",
                    productId = request.IdModelSanPham
                });
            }
            catch (Exception ex)
            {
                // Log lỗi chi tiết để debug
                Console.WriteLine($"[KhuyenMai] Lỗi khi xóa sản phẩm {request.IdModelSanPham} khỏi khuyến mãi {id}: {ex.Message}");
                Console.WriteLine($"[KhuyenMai] Stack trace: {ex.StackTrace}");
                
                return StatusCode(500, new { 
                    message = "Lỗi hệ thống khi xóa sản phẩm khỏi khuyến mãi. Vui lòng thử lại!",
                    error = ex.Message 
                });
            }
        }

        // --- Quản lý sản phẩm cho khuyến mãi ---
        [HttpGet]
        [Route("KhuyenMai/ManageProducts/{id}")]
        public async Task<IActionResult> ManageProducts(int id)
        {
            try
            {
                var khuyenMai = await _context.KhuyenMais.FindAsync(id);
                if (khuyenMai == null)
                {
                    return NotFound("Không tìm thấy khuyến mãi!");
                }

                return View(khuyenMai);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi: " + ex.Message);
            }
        }

        // --- Lấy danh sách sản phẩm có thể thêm vào khuyến mãi ---
        [HttpGet]
        [Route("KhuyenMai/GetAvailableProducts/{id}")]
        public async Task<IActionResult> GetAvailableProducts(int id)
        {
            try
            {
                var khuyenMai = await _context.KhuyenMais.FindAsync(id);
                if (khuyenMai == null)
                {
                    return NotFound(new { message = "Không tìm thấy khuyến mãi!" });
                }

                // Lấy danh sách sản phẩm đã được gán cho khuyến mãi này
                var assignedProductIds = await _context.ModelSanPhamKhuyenMais
                    .Where(mspkm => mspkm.IdKhuyenMai == id)
                    .Select(mspkm => mspkm.IdModelSanPham)
                    .ToListAsync();

                // Lấy tất cả sản phẩm chưa được gán (hoặc có thể gán lại)
                var availableProducts = await _context.ModelSanPhams
                    .Include(m => m.SanPham)
                        .ThenInclude(sp => sp.ThuongHieu)
                    .Include(m => m.AnhSanPhams)
                    .Where(m => m.TrangThai == 1)
                    .Select(m => new
                    {
                        IdModelSanPham = m.IdModelSanPham,
                        TenModel = m.TenModel ?? "N/A",
                        TenSanPham = m.SanPham != null ? m.SanPham.TenSanPham : "N/A",
                        TenThuongHieu = m.SanPham != null && m.SanPham.ThuongHieu != null ? m.SanPham.ThuongHieu.TenThuongHieu : "N/A",
                        GiaBan = m.GiaBanModel ?? 0,
                        HinhAnh = m.AnhSanPhams != null && m.AnhSanPhams.Any() ? m.AnhSanPhams.First().DuongDan : "/images/default-product.jpg",
                        IsAssigned = assignedProductIds.Contains(m.IdModelSanPham)
                    })
                    .ToListAsync();

                return Ok(availableProducts);
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
                // Lấy khuyến mãi để tính giá
                var khuyenMai = await _context.KhuyenMais.FindAsync(id);
                
                var products = await _context.ModelSanPhamKhuyenMais
                    .Include(mspkm => mspkm.ModelSanPham)
                        .ThenInclude(m => m.SanPham)
                            .ThenInclude(sp => sp.ThuongHieu)
                    .Include(mspkm => mspkm.ModelSanPham)
                        .ThenInclude(m => m.AnhSanPhams)
                    .Where(mspkm => mspkm.IdKhuyenMai == id)
                    .ToListAsync();
                
                // VẤN ĐỀ 3: Tính giá sau giảm khi lấy danh sách sản phẩm
                var productsWithDiscount = products.Select(mspkm => new
                    {
                        IdModelSanPham = mspkm.ModelSanPham.IdModelSanPham,
                        TenModel = mspkm.ModelSanPham.TenModel ?? "N/A",
                        TenSanPham = mspkm.ModelSanPham.SanPham?.TenSanPham ?? "N/A",
                        TenThuongHieu = mspkm.ModelSanPham.SanPham?.ThuongHieu?.TenThuongHieu ?? "N/A",
                        GiaBan = mspkm.ModelSanPham.GiaBanModel ?? 0,
                        GiaSauGiam = khuyenMai != null ? CalculateDiscountedPrice(
                            mspkm.ModelSanPham.GiaBanModel ?? 0,
                            khuyenMai.LoaiGiam,
                            khuyenMai.GiaTri) : (mspkm.ModelSanPham.GiaBanModel ?? 0),
                        HinhAnh = mspkm.ModelSanPham.AnhSanPhams?.FirstOrDefault()?.DuongDan ?? "/images/default-product.jpg"
                    })
                    .ToList();

                return Ok(productsWithDiscount);
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

    public class CreateKhuyenMaiRequest
    {
        public KhuyenMai? KhuyenMai { get; set; }
        public List<int>? ProductIds { get; set; }
    }

    public class ValidateProductsRequest
    {
        public List<int>? ProductIds { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}