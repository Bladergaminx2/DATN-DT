using DATN_DT.IServices;
using DATN_DT.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http.Json;

namespace DATN_DT.Controllers
{
    public class SanPhamController : Controller
    {
        private readonly ISanPhamService _sanPhamService;
        private readonly HttpClient _httpClient;

        public SanPhamController(ISanPhamService sanPhamService, IHttpClientFactory httpClientFactory)
        {
            _sanPhamService = sanPhamService;
            _httpClient = httpClientFactory.CreateClient();
        }

        // ----------------------------
        // GET: Danh sách sản phẩm
        // ----------------------------
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var list = await _sanPhamService.GetAllSanPhams();
            return View(list);
        }
        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> Create([FromBody] SanPham? sp)
        {
            if (sp == null)
                return BadRequest(new { message = "Dữ liệu sản phẩm không hợp lệ!" });

            try
            {
                await _sanPhamService.Create(sp); // ❌ không gán var
                return Ok(new { message = "Thêm sản phẩm thành công!" });
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, new { message = "Lỗi khi gọi API: " + ex.Message });
            }
            catch
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi thêm sản phẩm." });
            }
        }


        // ----------------------------
        // POST: Update sản phẩm
        // ----------------------------
        [HttpPost]
        [Route("SanPham/Edit/{id}")]
        [Consumes("application/json")]
        public async Task<IActionResult> Edit(int id, [FromBody] SanPham? sp)
        {
            if (sp == null)
                return BadRequest(new { message = "Dữ liệu sản phẩm không hợp lệ!" });

            sp.IdSanPham = id; // đảm bảo Id đúng

            try
            {
                // Gọi service, không cần gán var result vì service trả về Task
                await _sanPhamService.Update(sp);

                // Nếu không exception, trả về thành công
                return Ok(new { message = "Cập nhật sản phẩm thành công!" });
            }
            catch (HttpRequestException ex)
            {
                // Nếu gọi API thất bại
                return StatusCode(500, new { message = "Lỗi khi gọi API: " + ex.Message });
            }
            catch
            {
                // Lỗi khác
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi cập nhật sản phẩm." });
            }
        }


        // ----------------------------
        // GET: Load thương hiệu (call API qua HttpClient)
        // ----------------------------
        [HttpGet]
        public async Task<IActionResult> GetThuongHieu()
        {
            try
            {
                // Giả sử API ngoài là: /api/ThuongHieu
                var thuongHieus = await _httpClient.GetFromJsonAsync<List<object>>(
                    "https://localhost:7150/api/ThuongHieu"
                );

                return Ok(thuongHieus);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi load thương hiệu: " + ex.Message });
            }
        }
    }
}
