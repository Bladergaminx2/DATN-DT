using DATN_DT.IServices;
using DATN_DT.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;

namespace DATN_DT.Controllers
{

    public class PinController : Controller
    {
        private readonly IPinService _pinService;
        private readonly HttpClient _httpClient;

        public PinController(IPinService pinService, IHttpClientFactory httpClientFactory)
        {
            _pinService = pinService;
            _httpClient = httpClientFactory.CreateClient();
        }


        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var pins = await _pinService.GetAllPins();
            return View(pins);
        }

        // ===== CREATE =====
        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> Create([FromBody] Pin? pin)
        {
            // 1. Validation cơ bản (kiểm tra rỗng)
            var errors = ValidatePin(pin);
            if (errors.Count > 0)
                return BadRequest(errors);

            // 2. Chuẩn hóa và Validation dung lượng pin là số
            string rawDungLuongPin = pin!.DungLuongPin.Trim();
            if (!Regex.IsMatch(rawDungLuongPin, @"^\d+$"))
                return BadRequest(new { DungLuongPin = "Dung lượng pin chỉ được nhập số!" });

            // 3. Chuẩn hóa dữ liệu và thêm đơn vị
            pin.LoaiPin = pin.LoaiPin!.Trim();
            pin.CongNgheSac = pin.CongNgheSac!.Trim();
            pin.MoTaPin = pin.MoTaPin?.Trim();
            pin.DungLuongPin = rawDungLuongPin + "mAh";

            // 4. Kiểm tra trùng
            var all = await _pinService.GetAllPins();
            bool exists = all.Any(p =>
                p.LoaiPin!.Trim().Equals(pin.LoaiPin, StringComparison.OrdinalIgnoreCase) &&
                p.DungLuongPin!.Trim().Equals(pin.DungLuongPin, StringComparison.OrdinalIgnoreCase) &&
                p.CongNgheSac!.Trim().Equals(pin.CongNgheSac, StringComparison.OrdinalIgnoreCase)
            );

            if (exists)
                return Conflict(new { message = "Pin đã tồn tại!" });

            // 5. Tạo mới
            await _pinService.Create(pin);

            return Ok(new { message = "Thêm pin thành công!" });
        }

        // ===== EDIT =====
        // Giống RAMController, dùng [HttpPut] và nhận ID qua query string hoặc route mặc định
        [HttpPut]
        [Consumes("application/json")]
        public async Task<IActionResult> Edit(int id, [FromBody] Pin? pin)
        {
            // 1. Validation cơ bản (kiểm tra rỗng)
            var errors = ValidatePin(pin);
            if (errors.Count > 0)
                return BadRequest(errors);

            // 2. Tìm kiếm pin hiện tại
            var existing = await _pinService.GetPinById(id);
            if (existing == null)
                return NotFound(new { message = "Không tìm thấy pin!" });

            // 3. Chuẩn hóa và Validation dung lượng pin là số
            string rawDungLuongPin = pin!.DungLuongPin.Trim();
            if (!Regex.IsMatch(rawDungLuongPin, @"^\d+$"))
                return BadRequest(new { DungLuongPin = "Dung lượng pin chỉ được nhập số!" });

            // 4. Chuẩn hóa dữ liệu và thêm đơn vị cho đối tượng mới
            string newLoaiPin = pin.LoaiPin!.Trim();
            string newCongNgheSac = pin.CongNgheSac!.Trim();
            string newDungLuongPin = rawDungLuongPin + "mAh";
            string newMoTaPin = pin.MoTaPin?.Trim();

            // 5. Kiểm tra trùng (trừ chính nó)
            var all = await _pinService.GetAllPins();
            bool exists = all.Any(p =>
                p.IdPin != id &&
                p.LoaiPin!.Trim().Equals(newLoaiPin, StringComparison.OrdinalIgnoreCase) &&
                p.DungLuongPin!.Trim().Equals(newDungLuongPin, StringComparison.OrdinalIgnoreCase) &&
                p.CongNgheSac!.Trim().Equals(newCongNgheSac, StringComparison.OrdinalIgnoreCase)
            );

            if (exists)
                return Conflict(new { message = "Pin đã tồn tại!" });

            // 6. Cập nhật dữ liệu vào đối tượng existing và lưu
            existing.LoaiPin = newLoaiPin;
            existing.DungLuongPin = newDungLuongPin;
            existing.CongNgheSac = newCongNgheSac;
            existing.MoTaPin = newMoTaPin;

            await _pinService.Update(existing);

            return Ok(new { message = "Cập nhật pin thành công!" });
        }

        // ===== DELETE - Dùng HttpDelete (giống RAMController) =====
        // Mặc định là DELETE /Pin/Delete?id=...
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _pinService.GetPinById(id);
            if (existing == null)
                return NotFound(new { message = $"Không tìm thấy Pin với ID = {id} để xóa!" });

            await _pinService.Delete(id);
            return Ok(new { message = "Xoá Pin thành công!" });
        }


        // ===== TEST HTTP CLIENT - Giữ lại ví dụ sử dụng HttpClient =====
        // Mặc định là GET /Pin/ExternalApiDemo
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

            if (pin == null)
            {
                errors["Model"] = "Dữ liệu pin không được rỗng!";
                return errors;
            }

            if (string.IsNullOrWhiteSpace(pin.LoaiPin))
                errors["LoaiPin"] = "Phải nhập loại pin!";
            if (string.IsNullOrWhiteSpace(pin.DungLuongPin))
                errors["DungLuongPin"] = "Phải nhập dung lượng pin!";
            if (string.IsNullOrWhiteSpace(pin.CongNgheSac))
                errors["CongNgheSac"] = "Phải nhập công nghệ sạc!";

            return errors;
        }
    }
}