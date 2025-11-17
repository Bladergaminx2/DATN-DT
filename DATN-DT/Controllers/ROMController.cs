using DATN_DT.IServices;
using DATN_DT.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DATN_DT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ROMController : Controller
    {
        private readonly IROMService _romService;
        private readonly HttpClient _httpClient;

        public ROMController(IROMService romService, IHttpClientFactory httpClientFactory)
        {
            _romService = romService;
            _httpClient = httpClientFactory.CreateClient();
        }

        // ===== GET ALL =====
        [HttpGet("list")]
        public async Task<IActionResult> Index()
        {
            var roms = await _romService.GetAllROMs();
            return Ok(roms);
        }

        // ===== CREATE =====
        [HttpPost("create")]
        [Consumes("application/json")]
        public async Task<IActionResult> Create([FromBody] ROM? rom)
        {
            var errors = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(rom?.DungLuongROM))
                errors["DungLuongROM"] = "Phải điền dung lượng của ROM!";

            if (errors.Count > 0)
                return BadRequest(errors);

            // Check trùng
            var allROMs = await _romService.GetAllROMs();
            bool exists = allROMs.Any(r =>
                r.DungLuongROM!.Trim().ToLower() ==
                rom!.DungLuongROM.Trim().ToLower());

            if (exists)
                return Conflict(new { message = "Dung lượng ROM đã tồn tại!" });

            // Chuẩn hóa trước khi lưu
            rom.DungLuongROM = rom.DungLuongROM.Trim();
            rom.MoTaROM = rom.MoTaROM?.Trim();

            await _romService.Create(rom);

            return Ok(new { message = "Thêm ROM thành công!" });
        }

        // ===== EDIT =====
        [HttpPut("edit/{id}")]
        [Consumes("application/json")]
        public async Task<IActionResult> Edit(int id, [FromBody] ROM? rom)
        {
            var errors = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(rom?.DungLuongROM))
                errors["DungLuongROM"] = "Phải điền dung lượng của ROM!";

            if (errors.Count > 0)
                return BadRequest(errors);

            var existing = await _romService.GetROMById(id);
            if (existing == null)
                return NotFound(new { message = "Không tìm thấy ROM!" });

            // Check trùng dung lượng ROM
            var allROMs = await _romService.GetAllROMs();
            bool exists = allROMs.Any(r =>
                r.IdROM != id &&
                r.DungLuongROM!.Trim().ToLower() ==
                rom.DungLuongROM!.Trim().ToLower());

            if (exists)
                return Conflict(new { message = "Dung lượng ROM đã tồn tại!" });

            // Cập nhật dữ liệu
            existing.DungLuongROM = rom.DungLuongROM.Trim();
            existing.MoTaROM = rom.MoTaROM?.Trim();

            await _romService.Update(existing);

            return Ok(new { message = "Cập nhật ROM thành công!" });
        }

        // ===== HTTPCLIENT TEST =====
        [HttpGet("test-http")]
        public async Task<IActionResult> TestHttpClient()
        {
            var data = await _httpClient.GetStringAsync("https://api.github.com/");
            return Ok(new { response = data });
        }
    }
}
