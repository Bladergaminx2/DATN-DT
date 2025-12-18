using DATN_DT.IServices;
using DATN_DT.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace DATN_DT.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PinController : Controller
    {
        private readonly IPinService _pinService;

        public PinController(IPinService pinService)
        {
            _pinService = pinService;
        }

        // ===== GET: Pin/Index =====
        [HttpGet]
        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var pins = await _pinService.GetAllPins();
                return View(pins);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading pins: {ex.Message}");
                return View(new List<Pin>());
            }
        }

        // ===== CREATE =====
        [HttpPost]
        [Route("Create")]
        [Consumes("application/json")]
        public async Task<IActionResult> Create([FromBody] Pin pin)
        {
            try
            {
                Console.WriteLine("=== CREATE PIN ===");
                Console.WriteLine($"Received: {System.Text.Json.JsonSerializer.Serialize(pin)}");

                if (pin == null)
                {
                    return BadRequest(new { Message = "Dữ liệu pin không được rỗng!" });
                }

                // Validation
                var errors = new Dictionary<string, string>();
                if (string.IsNullOrWhiteSpace(pin.LoaiPin))
                    errors["LoaiPin"] = "Phải nhập loại pin!";
                if (string.IsNullOrWhiteSpace(pin.DungLuongPin))
                    errors["DungLuongPin"] = "Phải nhập dung lượng pin!";
                if (string.IsNullOrWhiteSpace(pin.CongNgheSac))
                    errors["CongNgheSac"] = "Phải nhập công nghệ sạc!";

                if (errors.Count > 0)
                    return BadRequest(new { Errors = errors });

                // Validate dung lượng là số
                string rawDungLuongPin = pin.DungLuongPin.Trim();
                if (!Regex.IsMatch(rawDungLuongPin, @"^\d+$"))
                    return BadRequest(new { DungLuongPin = "Dung lượng pin chỉ được nhập số!" });

                // Chuẩn hóa dữ liệu
                pin.LoaiPin = pin.LoaiPin.Trim();
                pin.CongNgheSac = pin.CongNgheSac.Trim();
                pin.MoTaPin = pin.MoTaPin?.Trim();
                pin.DungLuongPin = rawDungLuongPin + "mAh";

                // Kiểm tra trùng
                var allPins = await _pinService.GetAllPins();
                bool exists = allPins.Any(p =>
                    p.LoaiPin.Trim().Equals(pin.LoaiPin, StringComparison.OrdinalIgnoreCase) &&
                    p.DungLuongPin.Trim().Equals(pin.DungLuongPin, StringComparison.OrdinalIgnoreCase) &&
                    p.CongNgheSac.Trim().Equals(pin.CongNgheSac, StringComparison.OrdinalIgnoreCase)
                );

                if (exists)
                    return Conflict(new { Message = "Pin đã tồn tại trong hệ thống!" });

                // Tạo mới
                await _pinService.Create(pin);

                return Ok(new
                {
                    Message = "Thêm pin thành công!",
                    Success = true,
                    Id = pin.IdPin
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CREATE ERROR: {ex.Message}");
                return StatusCode(500, new
                {
                    Message = "Lỗi hệ thống khi thêm pin!",
                    Error = ex.Message
                });
            }
        }

        // ===== EDIT =====
        [HttpPut]
        [Route("Edit/{id:int}")]
        [Consumes("application/json")]
        public async Task<IActionResult> Edit(int id, [FromBody] Pin pin)
        {
            try
            {
                Console.WriteLine($"=== EDIT PIN ID: {id} ===");
                Console.WriteLine($"Data: {System.Text.Json.JsonSerializer.Serialize(pin)}");

                if (pin == null)
                    return BadRequest(new { Message = "Dữ liệu pin không được rỗng!" });

                // Validation
                var errors = new Dictionary<string, string>();
                if (string.IsNullOrWhiteSpace(pin.LoaiPin))
                    errors["LoaiPin"] = "Phải nhập loại pin!";
                if (string.IsNullOrWhiteSpace(pin.DungLuongPin))
                    errors["DungLuongPin"] = "Phải nhập dung lượng pin!";
                if (string.IsNullOrWhiteSpace(pin.CongNgheSac))
                    errors["CongNgheSac"] = "Phải nhập công nghệ sạc!";

                if (errors.Count > 0)
                    return BadRequest(new { Errors = errors });

                // Validate dung lượng là số
                string rawDungLuongPin = pin.DungLuongPin.Trim();
                if (!Regex.IsMatch(rawDungLuongPin, @"^\d+$"))
                    return BadRequest(new { DungLuongPin = "Dung lượng pin chỉ được nhập số!" });

                // Gán ID và chuẩn hóa
                pin.IdPin = id;
                pin.LoaiPin = pin.LoaiPin.Trim();
                pin.CongNgheSac = pin.CongNgheSac.Trim();
                pin.MoTaPin = pin.MoTaPin?.Trim();
                pin.DungLuongPin = rawDungLuongPin + "mAh";

                // Kiểm tra tồn tại
                var existingPin = await _pinService.GetPinById(id);
                if (existingPin == null)
                    return NotFound(new { Message = $"Không tìm thấy pin với ID={id}!" });

                // Kiểm tra trùng (trừ chính nó)
                var allPins = await _pinService.GetAllPins();
                bool exists = allPins.Any(p =>
                    p.IdPin != id &&
                    p.LoaiPin.Trim().Equals(pin.LoaiPin, StringComparison.OrdinalIgnoreCase) &&
                    p.DungLuongPin.Trim().Equals(pin.DungLuongPin, StringComparison.OrdinalIgnoreCase) &&
                    p.CongNgheSac.Trim().Equals(pin.CongNgheSac, StringComparison.OrdinalIgnoreCase)
                );

                if (exists)
                    return Conflict(new { Message = "Pin đã tồn tại trong hệ thống!" });

                // Cập nhật
                await _pinService.Update(pin);

                return Ok(new
                {
                    Message = "Cập nhật pin thành công!",
                    Success = true,
                    Id = id
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EDIT ERROR ID {id}: {ex.Message}");
                return StatusCode(500, new
                {
                    Message = "Lỗi hệ thống khi cập nhật pin!",
                    Error = ex.Message
                });
            }
        }

        // ===== DELETE =====
        [HttpDelete]
        [Route("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var existingPin = await _pinService.GetPinById(id);
                if (existingPin == null)
                    return NotFound(new { Message = $"Không tìm thấy pin với ID={id}!" });

                await _pinService.Delete(id);

                return Ok(new
                {
                    Message = "Xóa pin thành công!",
                    Success = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "Lỗi hệ thống khi xóa pin!",
                    Error = ex.Message
                });
            }
        }

        // ===== GET ALL (API) =====
        [HttpGet]
        [Route("GetAll")]
        [Produces("application/json")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var pins = await _pinService.GetAllPins();
                return Ok(pins);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        // ===== GET BY ID (API) =====
        [HttpGet]
        [Route("GetById/{id:int}")]
        [Produces("application/json")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var pin = await _pinService.GetPinById(id);
                if (pin == null)
                    return NotFound(new { Message = $"Không tìm thấy pin với ID={id}" });

                return Ok(pin);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }
    }
}