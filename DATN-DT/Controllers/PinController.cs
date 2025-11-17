using DATN_DT.IServices;
using DATN_DT.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace DATN_DT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PinController : Controller
    {
        private readonly IPinService _pinService;
        private readonly HttpClient _httpClient;

        public PinController(IPinService pinService, IHttpClientFactory httpClientFactory)
        {
            _pinService = pinService;
            _httpClient = httpClientFactory.CreateClient(); // dùng khi cần call API khác
        }

        // ===== GET ALL =====
        [HttpGet("list")]
        public async Task<IActionResult> Index()
        {
            var pins = await _pinService.GetAllPins();
            return Ok(pins);
        }

        // ===== CREATE =====
        [HttpPost("create")]
        [Consumes("application/json")]
        public async Task<IActionResult> Create([FromBody] Pin? pin)
        {
            var errors = ValidatePin(pin);
            if (errors.Count > 0)
                return BadRequest(errors);

            // validate dung lượng pin chỉ chứa số
            if (!Regex.IsMatch(pin!.DungLuongPin.Trim(), @"^\d+$"))
                return BadRequest(new { DungLuongPin = "Dung lượng pin chỉ được nhập số!" });

            // thêm mAh
            pin.DungLuongPin = pin.DungLuongPin.Trim() + "mAh";

            // check trùng trong DB qua service
            var all = await _pinService.GetAllPins();
            bool exists = all.Any(p =>
                p.LoaiPin!.Trim().ToLower() == pin.LoaiPin.Trim().ToLower() &&
                p.DungLuongPin!.Trim().ToLower() == pin.DungLuongPin.Trim().ToLower() &&
                p.CongNgheSac!.Trim().ToLower() == pin.CongNgheSac.Trim().ToLower()
            );

            if (exists)
                return Conflict(new { message = "Pin đã tồn tại!" });

            await _pinService.Create(pin);

            return Ok(new { message = "Thêm pin thành công!" });
        }

        // ===== EDIT =====
        [HttpPut("edit/{id}")]
        [Consumes("application/json")]
        public async Task<IActionResult> Edit(int id, [FromBody] Pin? pin)
        {
            var errors = ValidatePin(pin);
            if (errors.Count > 0)
                return BadRequest(errors);

            var existing = await _pinService.GetPinById(id);
            if (existing == null)
                return NotFound(new { message = "Không tìm thấy pin!" });

            // validate dung lượng pin chỉ chứa số
            if (!Regex.IsMatch(pin!.DungLuongPin.Trim(), @"^\d+$"))
                return BadRequest(new { DungLuongPin = "Dung lượng pin chỉ được nhập số!" });

            pin.DungLuongPin = pin.DungLuongPin.Trim() + "mAh";

            // check trùng
            var all = await _pinService.GetAllPins();
            bool exists = all.Any(p =>
                p.IdPin != id &&
                p.LoaiPin!.Trim().ToLower() == pin.LoaiPin.Trim().ToLower() &&
                p.DungLuongPin!.Trim().ToLower() == pin.DungLuongPin.Trim().ToLower() &&
                p.CongNgheSac!.Trim().ToLower() == pin.CongNgheSac.Trim().ToLower()
            );

            if (exists)
                return Conflict(new { message = "Pin đã tồn tại!" });

            // cập nhật
            existing.LoaiPin = pin.LoaiPin.Trim();
            existing.DungLuongPin = pin.DungLuongPin.Trim();
            existing.CongNgheSac = pin.CongNgheSac.Trim();
            existing.MoTaPin = pin.MoTaPin?.Trim();

            await _pinService.Update(existing);

            return Ok(new { message = "Cập nhật pin thành công!" });
        }

        // ===== TEST HTTP CLIENT =====
        [HttpGet("external-test")]
        public async Task<IActionResult> ExternalApiDemo()
        {
            var result = await _httpClient.GetStringAsync("https://api.github.com/");
            return Ok(new { response = result });
        }

        // ===== VALIDATION =====
        private Dictionary<string, string> ValidatePin(Pin? pin)
        {
            var errors = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(pin?.LoaiPin))
                errors["LoaiPin"] = "Phải nhập loại pin!";
            if (string.IsNullOrWhiteSpace(pin?.DungLuongPin))
                errors["DungLuongPin"] = "Phải nhập dung lượng pin!";
            if (string.IsNullOrWhiteSpace(pin?.CongNgheSac))
                errors["CongNgheSac"] = "Phải nhập công nghệ sạc!";

            return errors;
        }
    }
}
