using DATN_DT.IServices;
using DATN_DT.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace DATN_DT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RAMController : Controller
    {
        private readonly IRAMService _ramService;
        private readonly HttpClient _httpClient;

        public RAMController(IRAMService ramService, IHttpClientFactory httpClientFactory)
        {
            _ramService = ramService;
            _httpClient = httpClientFactory.CreateClient();
        }

        // ===== GET ALL =====
        [HttpGet("list")]
        public async Task<IActionResult> Index()
        {
            var rams = await _ramService.GetAllRAMs();
            return Ok(rams);
        }

        // ===== CREATE =====
        [HttpPost("create")]
        [Consumes("application/json")]
        public async Task<IActionResult> Create([FromBody] RAM? ram)
        {
            var errors = new Dictionary<string, string>();

            // Validation
            if (string.IsNullOrWhiteSpace(ram?.DungLuongRAM))
                errors["DungLuongRAM"] = "Phải điền dung lượng của RAM!";

            if (errors.Count > 0)
                return BadRequest(errors);

            // Check trùng
            var allRAMs = await _ramService.GetAllRAMs();
            bool exists = allRAMs.Any(r =>
                r.DungLuongRAM!.Trim().ToLower() ==
                ram!.DungLuongRAM.Trim().ToLower());

            if (exists)
                return Conflict(new { message = "Dung lượng RAM đã tồn tại!" });

            // Chuẩn hóa dữ liệu
            ram.DungLuongRAM = ram.DungLuongRAM.Trim();
            ram.MoTaRAM = ram.MoTaRAM?.Trim();

            await _ramService.Create(ram);

            return Ok(new { message = "Thêm RAM thành công!" });
        }

        // ===== EDIT =====
        [HttpPut("edit/{id}")]
        [Consumes("application/json")]
        public async Task<IActionResult> Edit(int id, [FromBody] RAM? ram)
        {
            var errors = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(ram?.DungLuongRAM))
                errors["DungLuongRAM"] = "Phải điền dung lượng của RAM!";

            if (errors.Count > 0)
                return BadRequest(errors);

            var existing = await _ramService.GetRAMById(id);
            if (existing == null)
                return NotFound(new { message = "Không tìm thấy RAM!" });

            // Check trùng
            var allRAMs = await _ramService.GetAllRAMs();
            bool exists = allRAMs.Any(r =>
                r.IdRAM != id &&
                r.DungLuongRAM!.Trim().ToLower() ==
                ram.DungLuongRAM!.Trim().ToLower());

            if (exists)
                return Conflict(new { message = "Dung lượng RAM đã tồn tại!" });

            // Cập nhật dữ liệu
            existing.DungLuongRAM = ram.DungLuongRAM.Trim();
            existing.MoTaRAM = ram.MoTaRAM?.Trim();

            await _ramService.Update(existing);

            return Ok(new { message = "Cập nhật RAM thành công!" });
        }

        // ===== TEST HTTPCLIENT =====
        [HttpGet("external-test")]
        public async Task<IActionResult> TestHttpClient()
        {
            var json = await _httpClient.GetStringAsync("https://api.github.com/");
            return Ok(new { data = json });
        }
    }
}
