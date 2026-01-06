using DATN_DT.IServices;
using DATN_DT.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace DATN_DT.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ChucVuController : Controller
    {
        private readonly IChucVuService _chucVuService;
        private readonly HttpClient _httpClient;

        public ChucVuController(IChucVuService chucVuService, IHttpClientFactory httpClientFactory)
        {
            _chucVuService = chucVuService;
            _httpClient = httpClientFactory.CreateClient();
        }

        // ===== GET ALL =====
        [HttpGet]
        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            var chucVus = await _chucVuService.GetAllChucVus();
            return View(chucVus);
        }

        // ===== CREATE =====
        [HttpPost]
        [Route("Create")]
        [Consumes("application/json")]
        public async Task<IActionResult> Create([FromBody] ChucVu? chucVu)
        {
            try
            {
                Console.WriteLine("=== CREATE CHUCVU ===");
                Console.WriteLine($"Received: {JsonSerializer.Serialize(chucVu)}");

                if (chucVu == null)
                {
                    return BadRequest(new { Message = "Dữ liệu chức vụ không được rỗng!" });
                }

                var errors = new Dictionary<string, string>();

                // Validation
                if (string.IsNullOrWhiteSpace(chucVu.TenChucVu))
                    errors["TenChucVu"] = "Phải điền tên chức vụ!";

                if (string.IsNullOrWhiteSpace(chucVu.TenChucVuVietHoa))
                    errors["TenChucVuVietHoa"] = "Phải điền tên chức vụ viết hoa!";

                if (errors.Count > 0)
                    return BadRequest(errors);

                // Check trùng
                var allChucVus = await _chucVuService.GetAllChucVus();
                var tenChucVuLower = chucVu.TenChucVu.Trim().ToLower();
                var tenChucVuVietHoaUpper = chucVu.TenChucVuVietHoa.Trim().ToUpper();
                
                bool exists = allChucVus.Any(cv =>
                    (cv.TenChucVu != null && cv.TenChucVu.Trim().ToLower() == tenChucVuLower) ||
                    (cv.TenChucVuVietHoa != null && cv.TenChucVuVietHoa.Trim().ToUpper() == tenChucVuVietHoaUpper));

                if (exists)
                    return Conflict(new { message = "Chức vụ đã tồn tại!" });

                // Chuẩn hóa dữ liệu
                chucVu.TenChucVu = chucVu.TenChucVu.Trim();
                chucVu.TenChucVuVietHoa = chucVu.TenChucVuVietHoa.Trim().ToUpper();

                await _chucVuService.Create(chucVu);

                return Ok(new { message = "Thêm chức vụ thành công!" });
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"ArgumentException: {ex.Message}");
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating ChucVu: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        // ===== EDIT =====
        [HttpPut]
        [Route("Edit")]
        [Consumes("application/json")]
        public async Task<IActionResult> Edit(int id, [FromBody] ChucVu? chucVu)
        {
            var errors = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(chucVu?.TenChucVu))
                errors["TenChucVu"] = "Phải điền tên chức vụ!";

            if (string.IsNullOrWhiteSpace(chucVu?.TenChucVuVietHoa))
                errors["TenChucVuVietHoa"] = "Phải điền tên chức vụ viết hoa!";

            if (errors.Count > 0)
                return BadRequest(errors);

            var existing = await _chucVuService.GetChucVuById(id);
            if (existing == null)
                return NotFound(new { message = "Không tìm thấy chức vụ!" });

            // Check trùng
            var allChucVus = await _chucVuService.GetAllChucVus();
            var tenChucVuLower = chucVu.TenChucVu.Trim().ToLower();
            var tenChucVuVietHoaUpper = chucVu.TenChucVuVietHoa.Trim().ToUpper();
            
            bool exists = allChucVus.Any(cv =>
                cv.IdChucVu != id &&
                ((cv.TenChucVu != null && cv.TenChucVu.Trim().ToLower() == tenChucVuLower) ||
                (cv.TenChucVuVietHoa != null && cv.TenChucVuVietHoa.Trim().ToUpper() == tenChucVuVietHoaUpper)));

            if (exists)
                return Conflict(new { message = "Chức vụ đã tồn tại!" });

            // Cập nhật dữ liệu
            existing.TenChucVu = chucVu.TenChucVu.Trim();
            existing.TenChucVuVietHoa = chucVu.TenChucVuVietHoa.Trim().ToUpper();

            await _chucVuService.Update(existing);

            return Ok(new { message = "Cập nhật chức vụ thành công!" });
        }

        // ===== DELETE =====
        [HttpDelete]
        [Route("Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            await _chucVuService.Delete(id);
            return Ok(new { message = "Xoá chức vụ thành công!" });
        }
    }
}

