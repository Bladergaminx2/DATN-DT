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
        public async Task<IActionResult> Index()
        {
            var modelSanPhams = await _context.ModelSanPhams.ToListAsync();

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

            if (errors.Count > 0)
                return BadRequest(errors);

            // Check trùng
            bool exists = await _context.ModelSanPhams.AnyAsync(m =>
                m.TenModel!.Trim().ToLower() == model!.TenModel!.Trim().ToLower() &&
                m.Mau!.Trim().ToLower() == model.Mau!.Trim().ToLower() &&
                m.IdSanPham == model.IdSanPham
            );
            if (exists)
                return Conflict(new { message = "Model sản phẩm đã tồn tại!" });

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

            if (errors.Count > 0)
                return BadRequest(errors);

            var existing = await _context.ModelSanPhams.FindAsync(id);
            if (existing == null)
                return NotFound(new { message = "Không tìm thấy model sản phẩm!" });

            // Check trùng
            bool exists = await _context.ModelSanPhams.AnyAsync(m =>
                m.TenModel!.Trim().ToLower() == model!.TenModel!.Trim().ToLower() &&
                m.Mau!.Trim().ToLower() == model.Mau!.Trim().ToLower() &&
                m.IdSanPham == model.IdSanPham &&
                m.IdModelSanPham != id
            );
            if (exists)
                return Conflict(new { message = "Model sản phẩm đã tồn tại!" });

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

                // Kiểm tra số lượng ảnh hiện tại (tối đa 6 ảnh)
                var currentImageCount = await _context.AnhSanPhams
                    .CountAsync(a => a.IdModelSanPham == idModelSanPham);

                if (currentImageCount >= 6)
                    return BadRequest(new { message = "Đã đạt giới hạn tối đa 6 ảnh. Vui lòng xóa ảnh cũ trước khi thêm mới!" });

                // Kiểm tra định dạng file
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var fileExtension = Path.GetExtension(file.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                    return BadRequest(new { message = "Chỉ chấp nhận file ảnh (jpg, jpeg, png, gif, webp)!" });

                // Kiểm tra kích thước file (tối đa 5MB)
                if (file.Length > 5 * 1024 * 1024)
                    return BadRequest(new { message = "Kích thước file không được vượt quá 5MB!" });

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

                // Kiểm tra số lượng ảnh hiện tại (phải giữ lại ít nhất 1 ảnh)
                var currentImageCount = await _context.AnhSanPhams
                    .CountAsync(a => a.IdModelSanPham == anhSanPham.IdModelSanPham);

                if (currentImageCount <= 1)
                    return BadRequest(new { message = "Phải giữ lại ít nhất 1 ảnh làm ảnh mặc định. Không thể xóa ảnh cuối cùng!" });

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
    }
}