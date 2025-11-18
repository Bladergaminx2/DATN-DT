using DATN_DT.IServices;
using DATN_DT.Models;
using Microsoft.AspNetCore.Mvc;

namespace DATN_DT.Controllers
{
    [Route("api/[controller]")]
    public class ManHinhController : Controller
    {
        private readonly IManHinhService _manHinhService;
        private readonly HttpClient _httpClient;

        public ManHinhController(IManHinhService manHinhService, HttpClient httpClient)
        {
            _manHinhService = manHinhService;
            _httpClient = httpClient;
        }

        // ============================
        // GET: List
        // ============================
        public async Task<IActionResult> Index()
        {
            var list = await _manHinhService.GetAllManHinhs();
            return View(list);
        }

        // ============================
        // POST: Create
        // ============================
        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> Create([FromBody] ManHinh? mh)
        {
            var errors = ValidateManHinh(mh);
            if (errors.Count > 0)
                return BadRequest(errors);

            // Check trùng bằng cách lấy tất cả từ service
            var all = await _manHinhService.GetAllManHinhs();
            bool exists = all.Any(m =>
                m.CongNgheManHinh!.Trim().ToLower() == mh!.CongNgheManHinh!.Trim().ToLower() &&
                m.KichThuoc!.Trim().ToLower() == mh.KichThuoc!.Trim().ToLower() &&
                m.DoPhanGiai!.Trim().ToLower() == mh.DoPhanGiai!.Trim().ToLower() &&
                m.TinhNangMan!.Trim().ToLower() == mh.TinhNangMan!.Trim().ToLower()
            );

            if (exists)
                return Conflict(new { message = "Màn hình đã tồn tại!" });


            await _manHinhService.Create(mh!);
            return Ok(new { message = "Thêm màn hình thành công!" });
        }



        // ============================
        // POST: Edit
        // ============================
        [HttpPost]
        [Route("ManHinh/Edit/{id}")]
        [Consumes("application/json")]
        public async Task<IActionResult> Edit(int id, [FromBody] ManHinh? mh)
        {
            var errors = ValidateManHinh(mh);
            if (errors.Count > 0)
                return BadRequest(errors);

            var existing = await _manHinhService.GetManHinhById(id);
            if (existing == null)
                return NotFound(new { message = "Không tìm thấy màn hình!" });

            // Check trùng
            var all = await _manHinhService.GetAllManHinhs();
            bool exists = all.Any(m =>
                m.CongNgheManHinh!.Trim().ToLower() == mh!.CongNgheManHinh!.Trim().ToLower() &&
                m.KichThuoc!.Trim().ToLower() == mh.KichThuoc!.Trim().ToLower() &&
                m.DoPhanGiai!.Trim().ToLower() == mh.DoPhanGiai!.Trim().ToLower() &&
                m.TinhNangMan!.Trim().ToLower() == mh.TinhNangMan!.Trim().ToLower() &&
                m.IdManHinh != id
            );

            if (exists)
                return Conflict(new { message = "Màn hình đã tồn tại!" });


            mh!.IdManHinh = id;
            await _manHinhService.Update(mh);

            return Ok(new { message = "Cập nhật màn hình thành công!" });
        }


        // ============================
        // GET API call via HttpClient
        // ============================
        [HttpGet]
        public async Task<IActionResult> CallExternalApi()
        {
            var response = await _httpClient.GetAsync("https://api.example.com/data");

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Lỗi gọi API ngoài");

            var data = await response.Content.ReadAsStringAsync();
            return Ok(data);
        }

        // ============================
        // Validate input
        // ============================
        private Dictionary<string, string> ValidateManHinh(ManHinh? mh)
        {
            var errors = new Dictionary<string, string>();

            if (mh == null)
            {
                errors["data"] = "Dữ liệu không hợp lệ!";
                return errors;
            }

            if (string.IsNullOrWhiteSpace(mh.CongNgheManHinh))
                errors["CongNgheManHinh"] = "Phải nhập công nghệ màn hình!";
            if (string.IsNullOrWhiteSpace(mh.KichThuoc))
                errors["KichThuoc"] = "Phải nhập kích thước!";
            if (string.IsNullOrWhiteSpace(mh.DoPhanGiai))
                errors["DoPhanGiai"] = "Phải nhập độ phân giải!";
            if (string.IsNullOrWhiteSpace(mh.TinhNangMan))
                errors["TinhNangMan"] = "Phải nhập tính năng màn hình!";

            return errors;
        }
    }
}
