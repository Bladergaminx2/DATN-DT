using DATN_DT.Data;
using DATN_DT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace DATN_DT.Controllers
{
    public class ModelSanPhamController : Controller
    {
        private readonly MyDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ModelSanPhamController(MyDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // Hiển thị danh sách model sản phẩm
        public async Task<IActionResult> Index(int? sanPhamId)
        {
            // Nếu có sanPhamId, chỉ lấy model của sản phẩm đó
            IQueryable<ModelSanPham> query = _context.ModelSanPhams;

            if (sanPhamId.HasValue && sanPhamId.Value > 0)
            {
                query = query.Where(m => m.IdSanPham == sanPhamId.Value);
            }

            var modelSanPhams = await query.ToListAsync();

            var sanPhams = await _context.SanPhams.ToDictionaryAsync(x => x.IdSanPham);
            var manHinhs = await _context.ManHinhs.ToDictionaryAsync(x => x.IdManHinh);
            var cameraTruocs = await _context.CameraTruocs.ToDictionaryAsync(x => x.IdCamTruoc);
            var cameraSaus = await _context.CameraSaus.ToDictionaryAsync(x => x.IdCameraSau);
            var pins = await _context.Pins.ToDictionaryAsync(x => x.IdPin);
            var rams = await _context.RAMs.ToDictionaryAsync(x => x.IdRAM);
            var roms = await _context.ROMs.ToDictionaryAsync(x => x.IdROM);

            // Lấy danh sách ảnh cho từng model
            var anhSanPhams = await _context.AnhSanPhams
                .Where(a => a.IdModelSanPham.HasValue)
                .GroupBy(a => a.IdModelSanPham)
                .ToDictionaryAsync(g => g.Key.Value, g => g.ToList());

            ViewBag.SanPhams = sanPhams;
            ViewBag.ManHinhs = manHinhs;
            ViewBag.CameraTruocs = cameraTruocs;
            ViewBag.CameraSaus = cameraSaus;
            ViewBag.Pins = pins;
            ViewBag.RAMs = rams;
            ViewBag.ROMs = roms;
            ViewBag.AnhSanPhams = anhSanPhams;
            ViewBag.SelectedSanPhamId = sanPhamId;
            ViewBag.AllSanPhams = await _context.SanPhams
                .OrderBy(s => s.TenSanPham)
                .Select(s => new { s.IdSanPham, s.TenSanPham, s.MaSanPham })
                .ToListAsync();

            return View(modelSanPhams);
        }

        // Tạo model sản phẩm mới
        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> Create([FromBody] ModelSanPham? model)
        {
            var errors = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(model?.TenModel))
                errors["TenModel"] = "Phải nhập tên model!";
            if (model?.IdSanPham == null || model.IdSanPham == 0)
                errors["IdSanPham"] = "Phải chọn sản phẩm!";
            if (model?.IdManHinh == null || model.IdManHinh == 0)
                errors["IdManHinh"] = "Phải chọn màn hình!";
            if (model?.IdCameraTruoc == null || model.IdCameraTruoc == 0)
                errors["IdCameraTruoc"] = "Phải chọn camera trước!";
            if (model?.IdCameraSau == null || model.IdCameraSau == 0)
                errors["IdCameraSau"] = "Phải chọn camera sau!";
            if (model?.IdPin == null || model.IdPin == 0)
                errors["IdPin"] = "Phải chọn pin!";
            if (model?.IdRAM == null || model.IdRAM == 0)
                errors["IdRAM"] = "Phải chọn RAM!";
            if (model?.IdROM == null || model.IdROM == 0)
                errors["IdROM"] = "Phải chọn ROM!";
            if (string.IsNullOrWhiteSpace(model?.Mau))
                errors["Mau"] = "Phải nhập màu!";
            if (model?.GiaBanModel == null || model.GiaBanModel <= 0)
                errors["GiaBanModel"] = "Giá bán phải lớn hơn 0!";
            else if (model.GiaBanModel > 1000000000) // 1 tỷ
                errors["GiaBanModel"] = "Giá bán không được vượt quá 1.000.000.000 đ!";
            else if (model.IdSanPham.HasValue)
            {
                // Kiểm tra giá bán model phải hợp lý so với giá gốc sản phẩm
                var sanPham = await _context.SanPhams.FindAsync(model.IdSanPham.Value);
                if (sanPham != null && sanPham.GiaGoc.HasValue)
                {
                    // Giá bán model không được nhỏ hơn 50% giá gốc và không được lớn hơn 300% giá gốc
                    if (model.GiaBanModel < sanPham.GiaGoc.Value * 0.5m)
                        errors["GiaBanModel"] = $"Giá bán model không được nhỏ hơn 50% giá gốc ({sanPham.GiaGoc.Value * 0.5m:N0} đ)!";
                    else if (model.GiaBanModel > sanPham.GiaGoc.Value * 3m)
                        errors["GiaBanModel"] = $"Giá bán model không được lớn hơn 300% giá gốc ({sanPham.GiaGoc.Value * 3m:N0} đ)!";
                }
            }

            if (errors.Count > 0)
                return BadRequest(errors);

            // Check trùng tên model trong cùng sản phẩm
            bool existsTenModel = await _context.ModelSanPhams.AnyAsync(m =>
                m.TenModel!.Trim().ToLower() == model!.TenModel!.Trim().ToLower() &&
                m.IdSanPham == model.IdSanPham
            );
            if (existsTenModel)
                return Conflict(new { message = "Tên model đã tồn tại trong sản phẩm này!" });

            // Check trùng tổ hợp: Pin, RAM, ROM, Màn hình, Camera trước, Camera sau, Màu (không phân biệt hoa thường)
            bool existsCombo = await _context.ModelSanPhams.AnyAsync(m =>
                m.IdSanPham == model.IdSanPham &&
                m.IdPin == model.IdPin &&
                m.IdRAM == model.IdRAM &&
                m.IdROM == model.IdROM &&
                m.IdManHinh == model.IdManHinh &&
                m.IdCameraTruoc == model.IdCameraTruoc &&
                m.IdCameraSau == model.IdCameraSau &&
                m.Mau!.Trim().ToLower() == model.Mau!.Trim().ToLower()
            );
            if (existsCombo)
                return Conflict(new { message = "Sản phẩm chi tiết đã tồn tại trong sản phẩm này!" });

            try
            {
                model.TenModel = model.TenModel.Trim();
                model.Mau = model.Mau.Trim();

                _context.ModelSanPhams.Add(model);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Thêm model sản phẩm thành công!", id = model.IdModelSanPham });
            }
            catch
            {
                return StatusCode(500, new { message = "Lỗi khi thêm model sản phẩm. Vui lòng thử lại!" });
            }
        }

        // Sửa model sản phẩm
        [HttpPost]
        [Route("ModelSanPham/Edit/{id}")]
        [Consumes("application/json")]
        public async Task<IActionResult> Edit(int id, [FromBody] ModelSanPham? model)
        {
            var errors = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(model?.TenModel))
                errors["TenModel"] = "Phải nhập tên model!";
            if (model?.IdSanPham == null || model.IdSanPham == 0)
                errors["IdSanPham"] = "Phải chọn sản phẩm!";
            if (model?.IdManHinh == null || model.IdManHinh == 0)
                errors["IdManHinh"] = "Phải chọn màn hình!";
            if (model?.IdCameraTruoc == null || model.IdCameraTruoc == 0)
                errors["IdCameraTruoc"] = "Phải chọn camera trước!";
            if (model?.IdCameraSau == null || model.IdCameraSau == 0)
                errors["IdCameraSau"] = "Phải chọn camera sau!";
            if (model?.IdPin == null || model.IdPin == 0)
                errors["IdPin"] = "Phải chọn pin!";
            if (model?.IdRAM == null || model.IdRAM == 0)
                errors["IdRAM"] = "Phải chọn RAM!";
            if (model?.IdROM == null || model.IdROM == 0)
                errors["IdROM"] = "Phải chọn ROM!";
            if (string.IsNullOrWhiteSpace(model?.Mau))
                errors["Mau"] = "Phải nhập màu!";
            if (model?.GiaBanModel == null || model.GiaBanModel <= 0)
                errors["GiaBanModel"] = "Giá bán phải lớn hơn 0!";
            else if (model.GiaBanModel > 1000000000) // 1 tỷ
                errors["GiaBanModel"] = "Giá bán không được vượt quá 1.000.000.000 đ!";
            else if (model.IdSanPham.HasValue)
            {
                // Kiểm tra giá bán model phải hợp lý so với giá gốc sản phẩm
                var sanPham = await _context.SanPhams.FindAsync(model.IdSanPham.Value);
                if (sanPham != null && sanPham.GiaGoc.HasValue)
                {
                    // Giá bán model không được nhỏ hơn 50% giá gốc và không được lớn hơn 300% giá gốc
                    if (model.GiaBanModel < sanPham.GiaGoc.Value * 0.5m)
                        errors["GiaBanModel"] = $"Giá bán model không được nhỏ hơn 50% giá gốc ({sanPham.GiaGoc.Value * 0.5m:N0} đ)!";
                    else if (model.GiaBanModel > sanPham.GiaGoc.Value * 3m)
                        errors["GiaBanModel"] = $"Giá bán model không được lớn hơn 300% giá gốc ({sanPham.GiaGoc.Value * 3m:N0} đ)!";
                }
            }

            if (errors.Count > 0)
                return BadRequest(errors);

            var existing = await _context.ModelSanPhams.FindAsync(id);
            if (existing == null)
                return NotFound(new { message = "Không tìm thấy model sản phẩm!" });

            // Check trùng tên model trong cùng sản phẩm (không được thay đổi IdSanPham)
            // Đảm bảo IdSanPham không thay đổi
            if (existing.IdSanPham != model.IdSanPham)
                return BadRequest(new { message = "Không thể thay đổi sản phẩm sau khi đã tạo model!" });

            // Check trùng tên model trong cùng sản phẩm
            bool existsTenModel = await _context.ModelSanPhams.AnyAsync(m =>
                m.TenModel!.Trim().ToLower() == model!.TenModel!.Trim().ToLower() &&
                m.IdSanPham == model.IdSanPham &&
                m.IdModelSanPham != id
            );
            if (existsTenModel)
                return Conflict(new { message = "Tên model đã tồn tại trong sản phẩm này!" });

            // Check trùng tổ hợp: Pin, RAM, ROM, Màn hình, Camera trước, Camera sau, Màu (không phân biệt hoa thường)
            bool existsCombo = await _context.ModelSanPhams.AnyAsync(m =>
                m.IdModelSanPham != id &&
                m.IdSanPham == model.IdSanPham &&
                m.IdPin == model.IdPin &&
                m.IdRAM == model.IdRAM &&
                m.IdROM == model.IdROM &&
                m.IdManHinh == model.IdManHinh &&
                m.IdCameraTruoc == model.IdCameraTruoc &&
                m.IdCameraSau == model.IdCameraSau &&
                m.Mau!.Trim().ToLower() == model.Mau!.Trim().ToLower()
            );
            if (existsCombo)
                return Conflict(new { message = "Sản phẩm chi tiết đã tồn tại trong sản phẩm này!" });

            try
            {
                existing.TenModel = model.TenModel.Trim();
                existing.IdSanPham = model.IdSanPham;
                existing.IdManHinh = model.IdManHinh;
                existing.IdCameraTruoc = model.IdCameraTruoc;
                existing.IdCameraSau = model.IdCameraSau;
                existing.IdPin = model.IdPin;
                existing.IdRAM = model.IdRAM;
                existing.IdROM = model.IdROM;
                existing.Mau = model.Mau.Trim();
                existing.GiaBanModel = model.GiaBanModel;

                _context.ModelSanPhams.Update(existing);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Cập nhật model sản phẩm thành công!" });
            }
            catch
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật model sản phẩm. Vui lòng thử lại!" });
            }
        }

        // ========== QUẢN LÝ ẢNH SẢN PHẨM ==========

        // Upload ảnh cho model sản phẩm
        [HttpPost]
        public async Task<IActionResult> UploadImage(int idModelSanPham, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { message = "Vui lòng chọn file ảnh!" });

                // Kiểm tra model sản phẩm tồn tại
                var modelSanPham = await _context.ModelSanPhams.FindAsync(idModelSanPham);
                if (modelSanPham == null)
                    return NotFound(new { message = "Không tìm thấy model sản phẩm!" });

                // Kiểm tra định dạng file
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var fileExtension = Path.GetExtension(file.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                    return BadRequest(new { message = "Chỉ chấp nhận file ảnh (jpg, jpeg, png, gif, webp)!" });

                // Kiểm tra kích thước file (tối đa 5MB)
                if (file.Length > 5 * 1024 * 1024)
                    return BadRequest(new { message = "Kích thước file không được vượt quá 5MB!" });

                // Kiểm tra số lượng ảnh hiện có (tối đa 8 ảnh)
                const int maxImages = 8;
                var currentImageCount = await _context.AnhSanPhams
                    .CountAsync(a => a.IdModelSanPham == idModelSanPham);
                
                if (currentImageCount >= maxImages)
                    return BadRequest(new { message = $"Đã đạt giới hạn tối đa {maxImages} ảnh. Vui lòng xóa ảnh cũ trước khi thêm ảnh mới!" });

                // Tạo thư mục lưu trữ nếu chưa tồn tại
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "model-images");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // Tạo tên file unique
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                // Lưu file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Tạo đường dẫn tương đối để lưu trong database
                var relativePath = $"/uploads/model-images/{fileName}";

                // Lưu thông tin ảnh vào database
                var anhSanPham = new AnhSanPham
                {
                    IdModelSanPham = idModelSanPham,
                    DuongDan = relativePath
                };

                _context.AnhSanPhams.Add(anhSanPham);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Upload ảnh thành công!",
                    id = anhSanPham.IdAnh,
                    duongDan = relativePath
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi khi upload ảnh: {ex.Message}" });
            }
        }

        // Xóa ảnh
        [HttpPost]
        public async Task<IActionResult> DeleteImage(int idAnh)
        {
            try
            {
                var anhSanPham = await _context.AnhSanPhams.FindAsync(idAnh);
                if (anhSanPham == null)
                    return NotFound(new { message = "Không tìm thấy ảnh!" });

                // Xóa file vật lý
                if (!string.IsNullOrEmpty(anhSanPham.DuongDan))
                {
                    var filePath = Path.Combine(_environment.WebRootPath, anhSanPham.DuongDan.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                // Xóa record trong database
                _context.AnhSanPhams.Remove(anhSanPham);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Xóa ảnh thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi khi xóa ảnh: {ex.Message}" });
            }
        }

        // Lấy danh sách ảnh theo model sản phẩm
        [HttpGet]
        public async Task<IActionResult> GetImagesByModel(int idModelSanPham)
        {
            try
            {
                var images = await _context.AnhSanPhams
                    .Where(a => a.IdModelSanPham == idModelSanPham)
                    .OrderBy(a => a.IdAnh)
                    .Select(a => new
                    {
                        idAnh = a.IdAnh,
                        duongDan = a.DuongDan,
                        tenFile = Path.GetFileName(a.DuongDan)
                    })
                    .ToListAsync();

                return Ok(images);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi khi lấy danh sách ảnh: {ex.Message}" });
            }
        }

        // Đặt ảnh làm ảnh đại diện (nếu cần)
        [HttpPost]
        public async Task<IActionResult> SetDefaultImage(int idModelSanPham, int idAnh)
        {
            try
            {
                // Logic để đặt ảnh đại diện
                // Có thể thêm trường IsDefault trong bảng AnhSanPham nếu cần
                return Ok(new { message = "Đặt ảnh đại diện thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi khi đặt ảnh đại diện: {ex.Message}" });
            }
        }

        // ========== CÁC API LẤY DANH SÁCH CHO DROPDOWN ==========

        [HttpGet]
        public async Task<IActionResult> GetAllSanPham()
        {
            var list = await _context.SanPhams.ToListAsync();
            return Ok(list);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllManHinh()
        {
            var list = await _context.ManHinhs
                .Select(m => new { m.IdManHinh, DisplayText = $"{m.CongNgheManHinh} - {m.KichThuoc}" })
                .ToListAsync();
            return Ok(list);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCameraTruoc()
        {
            var list = await _context.CameraTruocs
                .Select(c => new { c.IdCamTruoc, DisplayText = c.DoPhanGiaiCamTruoc })
                .ToListAsync();
            return Ok(list);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCameraSau()
        {
            var list = await _context.CameraSaus
                .Select(c => new { c.IdCameraSau, DisplayText = c.DoPhanGiaiCamSau })
                .ToListAsync();
            return Ok(list);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPin()
        {
            var list = await _context.Pins
                .Select(p => new { p.IdPin, DisplayText = $"{p.LoaiPin} - {p.DungLuongPin}" })
                .ToListAsync();
            return Ok(list);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRAM()
        {
            var list = await _context.RAMs
                .Select(r => new { r.IdRAM, DisplayText = r.DungLuongRAM })
                .ToListAsync();
            return Ok(list);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllROM()
        {
            var list = await _context.ROMs
                .Select(r => new { r.IdROM, DisplayText = r.DungLuongROM })
                .ToListAsync();
            return Ok(list);
        }

        // ========== QUẢN LÝ IMEI ==========

        // GET: Lấy danh sách IMEI theo model
        [HttpGet("ModelSanPham/GetImeisByModel")]
        public async Task<IActionResult> GetImeisByModel(int modelId)
        {
            try
            {
                var imeis = await _context.Imeis
                    .Where(i => i.IdModelSanPham == modelId)
                    .Select(i => new
                    {
                        idImei = i.IdImei,
                        maImei = i.MaImei,
                        trangThai = i.TrangThai,
                        moTa = i.MoTa,
                        canDelete = !_context.HoaDonChiTiets.Any(h => h.IdImei == i.IdImei)
                    })
                    .OrderBy(i => i.maImei)
                    .ToListAsync();

                return Ok(new { success = true, data = imeis });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Lỗi khi lấy danh sách IMEI: {ex.Message}" });
            }
        }

        // POST: Thêm IMEI mới
        [HttpPost("ModelSanPham/AddImei")]
        [Consumes("application/json")]
        public async Task<IActionResult> AddImei([FromBody] Imei? imei)
        {
            if (imei == null)
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ!" });

            var errors = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(imei.MaImei))
                errors["MaImei"] = "Phải nhập mã IMEI!";
            else
            {
                // Kiểm tra IMEI phải là 15 chữ số
                var imeiRegex = new System.Text.RegularExpressions.Regex(@"^\d{15}$");
                if (!imeiRegex.IsMatch(imei.MaImei.Trim()))
                    errors["MaImei"] = "Mã IMEI phải gồm đúng 15 chữ số!";
            }
            if (imei.IdModelSanPham == null || imei.IdModelSanPham == 0)
                errors["IdModelSanPham"] = "Phải chọn model sản phẩm!";
            if (string.IsNullOrWhiteSpace(imei.TrangThai))
                errors["TrangThai"] = "Phải chọn trạng thái!";

            if (errors.Count > 0)
                return BadRequest(new { success = false, errors });

            // Check trùng mã IMEI
            bool exists = await _context.Imeis.AnyAsync(i =>
                i.MaImei!.Trim().ToLower() == imei.MaImei.Trim().ToLower()
            );
            if (exists)
                return Conflict(new { success = false, message = "Mã IMEI đã tồn tại!" });

            try
            {
                imei.MaImei = imei.MaImei.Trim();
                imei.MoTa = imei.MoTa?.Trim();
                imei.TrangThai = imei.TrangThai.Trim();

                _context.Imeis.Add(imei);
                await _context.SaveChangesAsync();

                // Cập nhật tồn kho
                await UpdateTonKhoForModel(imei.IdModelSanPham.Value);

                return Ok(new { success = true, message = "Thêm IMEI thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Lỗi khi thêm IMEI: {ex.Message}" });
            }
        }

        // POST: Xóa IMEI
        [HttpPost("ModelSanPham/DeleteImei")]
        [Consumes("application/json")]
        public async Task<IActionResult> DeleteImei([FromBody] int idImei)
        {
            try
            {
                var imei = await _context.Imeis.FindAsync(idImei);
                if (imei == null)
                    return NotFound(new { success = false, message = "Không tìm thấy IMEI!" });

                // Kiểm tra xem IMEI có trong hóa đơn không
                bool hasInvoice = await _context.HoaDonChiTiets.AnyAsync(h => h.IdImei == idImei);
                if (hasInvoice)
                    return BadRequest(new { success = false, message = "Không thể xóa IMEI vì đã có trong hóa đơn!" });

                var modelId = imei.IdModelSanPham;

                _context.Imeis.Remove(imei);
                await _context.SaveChangesAsync();

                // Cập nhật tồn kho
                if (modelId.HasValue)
                {
                    await UpdateTonKhoForModel(modelId.Value);
                }

                return Ok(new { success = true, message = "Xóa IMEI thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Lỗi khi xóa IMEI: {ex.Message}" });
            }
        }

        // Helper: Cập nhật tồn kho
        private async Task UpdateTonKhoForModel(int modelSanPhamId)
        {
            var soLuongConHang = await _context.Imeis
                .CountAsync(i => i.IdModelSanPham == modelSanPhamId && i.TrangThai == "Còn hàng");

            var tonKhos = await _context.TonKhos
                .Where(t => t.IdModelSanPham == modelSanPhamId)
                .ToListAsync();

            if (tonKhos.Any())
            {
                foreach (var tonKho in tonKhos)
                {
                    tonKho.SoLuong = soLuongConHang;
                }
                await _context.SaveChangesAsync();
            }
            else
            {
                var khoMacDinh = await _context.Khos.FirstOrDefaultAsync();
                if (khoMacDinh != null)
                {
                    var newTonKho = new TonKho
                    {
                        IdModelSanPham = modelSanPhamId,
                        IdKho = khoMacDinh.IdKho,
                        SoLuong = soLuongConHang
                    };
                    _context.TonKhos.Add(newTonKho);
                    await _context.SaveChangesAsync();
                }
            }
        }

        // Xóa model sản phẩm
        [HttpPost]
        [Route("ModelSanPham/Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
            {
                try
                {
                    var modelSanPham = await _context.ModelSanPhams.FindAsync(id);
                    if (modelSanPham == null)
                        return NotFound(new { success = false, message = "Không tìm thấy model sản phẩm!" });

                    // Kiểm tra xem model có trong giỏ hàng chi tiết không
                    bool hasGioHangChiTiet = await _context.GioHangChiTiets
                        .AnyAsync(g => g.IdModelSanPham == id);
                    if (hasGioHangChiTiet)
                        return BadRequest(new { success = false, message = "Không thể xóa model sản phẩm vì đã có trong giỏ hàng chi tiết!" });

                    // Kiểm tra xem model có trong hóa đơn chi tiết không
                    bool hasHoaDonChiTiet = await _context.HoaDonChiTiets
                        .AnyAsync(h => h.IdModelSanPham == id);
                    if (hasHoaDonChiTiet)
                        return BadRequest(new { success = false, message = "Không thể xóa model sản phẩm vì đã có trong hóa đơn chi tiết!" });

                    // Kiểm tra xem model có trong đơn hàng chi tiết không
                    bool hasDonHangChiTiet = await _context.DonHangChiTiets
                        .AnyAsync(d => d.IdModelSanPham == id);
                    if (hasDonHangChiTiet)
                        return BadRequest(new { success = false, message = "Không thể xóa model sản phẩm vì đã có trong đơn hàng chi tiết!" });

                    // Xóa các ảnh liên quan
                    var anhSanPhams = await _context.AnhSanPhams
                        .Where(a => a.IdModelSanPham == id)
                        .ToListAsync();
                    foreach (var anh in anhSanPhams)
                    {
                        // Xóa file vật lý
                        if (!string.IsNullOrEmpty(anh.DuongDan))
                        {
                            var filePath = Path.Combine(_environment.WebRootPath, anh.DuongDan.TrimStart('/'));
                            if (System.IO.File.Exists(filePath))
                            {
                                System.IO.File.Delete(filePath);
                            }
                        }
                        _context.AnhSanPhams.Remove(anh);
                    }

                    // Xóa các IMEI liên quan (chỉ xóa nếu chưa có trong hóa đơn)
                    var imeis = await _context.Imeis
                        .Where(i => i.IdModelSanPham == id)
                        .ToListAsync();
                    foreach (var imei in imeis)
                    {
                        // Kiểm tra IMEI có trong hóa đơn chi tiết không
                        bool imeiInHoaDon = await _context.HoaDonChiTiets
                            .AnyAsync(h => h.IdImei == imei.IdImei);
                        if (!imeiInHoaDon)
                        {
                            _context.Imeis.Remove(imei);
                        }
                    }

                    // Xóa tồn kho liên quan
                    var tonKhos = await _context.TonKhos
                        .Where(t => t.IdModelSanPham == id)
                        .ToListAsync();
                    foreach (var tonKho in tonKhos)
                    {
                        _context.TonKhos.Remove(tonKho);
                    }

                    // Xóa model sản phẩm
                    _context.ModelSanPhams.Remove(modelSanPham);
                    await _context.SaveChangesAsync();

                    return Ok(new { success = true, message = "Xóa model sản phẩm thành công!" });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { success = false, message = $"Lỗi khi xóa model sản phẩm: {ex.Message}" });
                }
            }
        }
    }