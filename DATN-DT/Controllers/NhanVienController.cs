using DATN_DT.IServices;
using DATN_DT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;

namespace DATN_DT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NhanVienController : Controller
    {
        private readonly INhanVienService _nhanVienService;
        private readonly HttpClient _httpClient;

        public NhanVienController(INhanVienService nhanVienService,
                                  IHttpClientFactory httpClientFactory)
        {
            _nhanVienService = nhanVienService;
            _httpClient = httpClientFactory.CreateClient(); // dùng khi cần call API ngoài
        }

        // --- GET ALL ---
        [Authorize(Roles = "NhanVien")]
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAll()
        {
            var nhanVienList = await _nhanVienService.GetAllNhanViens();
            return Ok(nhanVienList);
        }

        // --- GET BY ID ---
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var nhanVien = await _nhanVienService.GetNhanVienById(id);
            if (nhanVien == null)
                return NotFound(new { message = "Không tìm thấy nhân viên!" });

            return Ok(nhanVien);
        }

        // --- CREATE ---
        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> Create([FromBody] NhanVien? nhanVien)
        {
            if (nhanVien == null)
                return BadRequest(new { message = "Dữ liệu không hợp lệ!" });

            await _nhanVienService.Create(nhanVien);

            return Ok(new { message = "Thêm Nhân viên thành công!" });
        }

        // --- UPDATE ---
        [HttpPut("{id}")]
        [Consumes("application/json")]
        public async Task<IActionResult> Update(int id, [FromBody] NhanVien? nhanVien)
        {
            if (nhanVien == null || id != nhanVien.IdNhanVien)
                return BadRequest(new { message = "Dữ liệu không hợp lệ!" });

            await _nhanVienService.Update(nhanVien);

            return Ok(new { message = "Cập nhật Nhân viên thành công!" });
        }

        // --- DELETE ---
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _nhanVienService.Delete(id);
            return Ok(new { message = "Xóa nhân viên thành công!" });
        }

        // --- CALL API QUA HTTPCLIENT (OPTIONAL) ---
        [HttpGet("external-test")]
        public async Task<IActionResult> TestHttpClient()
        {
            var json = await _httpClient.GetStringAsync("https://api.github.com/");
            return Ok(new { externalData = json });
        }
    }
}
