using DATN_DT.IServices;
using DATN_DT.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http.Json;

namespace DATN_DT.Controllers
{
    public class ThuongHieuController : Controller
    {
        private readonly IThuongHieuService _thuongHieuService;
        private readonly HttpClient _httpClient;

        public ThuongHieuController(IThuongHieuService thuongHieuService, IHttpClientFactory httpClientFactory)
        {
            _thuongHieuService = thuongHieuService;
            _httpClient = httpClientFactory.CreateClient();
        }

        // ----------------------------
        // GET: danh sách thương hiệu
        // ----------------------------
        public async Task<IActionResult> Index()
        {
            var list = await _thuongHieuService.GetAllThuongHieus();
            return View(list);
        }

        // ----------------------------
        // POST: tạo thương hiệu
        // ----------------------------
        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> Create([FromBody] ThuongHieu? th)
        {
            if (th == null)
                return BadRequest(new { message = "Dữ liệu thương hiệu không hợp lệ!" });

            // Validation
            var errors = new Dictionary<string, string>();
            if (string.IsNullOrWhiteSpace(th.TenThuongHieu))
                errors["TenThuongHieu"] = "Phải nhập tên thương hiệu!";
            if (string.IsNullOrWhiteSpace(th.TrangThaiThuongHieu))
                errors["TrangThaiThuongHieu"] = "Phải nhập trạng thái thương hiệu!";

            if (errors.Count > 0)
                return BadRequest(errors);

            try
            {
                // Gọi service
                await _thuongHieuService.CreateThuongHieu(th);
                return Ok(new { message = "Thêm thương hiệu thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi thêm thương hiệu: " + ex.Message });
            }
        }

        // ----------------------------
        // POST: cập nhật thương hiệu
        // ----------------------------
        [HttpPost]
        [Route("ThuongHieu/Edit/{id}")]
        [Consumes("application/json")]
        public async Task<IActionResult> Edit(int id, [FromBody] ThuongHieu? th)
        {
            if (th == null)
                return BadRequest(new { message = "Dữ liệu thương hiệu không hợp lệ!" });

            // Validation
            var errors = new Dictionary<string, string>();
            if (string.IsNullOrWhiteSpace(th.TenThuongHieu))
                errors["TenThuongHieu"] = "Phải nhập tên thương hiệu!";
            if (string.IsNullOrWhiteSpace(th.TrangThaiThuongHieu))
                errors["TrangThaiThuongHieu"] = "Phải nhập trạng thái thương hiệu!";

            if (errors.Count > 0)
                return BadRequest(errors);

            try
            {
                // Gán Id để service biết bản ghi nào update
                var data = await _thuongHieuService.GetThuongHieuById(id);
                data.TenThuongHieu = th.TenThuongHieu;
                data.TrangThaiThuongHieu = th.TrangThaiThuongHieu;
                // Gọi service
                await _thuongHieuService.UpdateThuongHieu(data);

                return Ok(new { message = "Cập nhật thương hiệu thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật thương hiệu: " + ex.Message });
            }
        }

        // ===== TEST HTTPCLIENT =====
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            await _thuongHieuService.DeleteThuongHieu(id);
            return Ok(new { message = "Xoá RAM thành công!" });
        }

        // ----------------------------
        // GET: lấy thương hiệu theo API HttpClient (nếu cần)
        // ----------------------------
        [HttpGet]
        public async Task<IActionResult> GetThuongHieusFromApi()
        {
            try
            {
                // Ví dụ gọi API ngoài
                var thuongHieus = await _httpClient.GetFromJsonAsync<List<ThuongHieu>>("https://localhost:7150/api/ThuongHieus");
                return Ok(thuongHieus);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi gọi API: " + ex.Message });
            }
        }
        // ----------------------------
        // API: lấy danh sách thương hiệu đang hoạt động cho trang Mua hàng
        // ----------------------------
        [HttpGet]
        [Route("api/thuonghieu/active")]  // URL: /api/thuonghieu/active
        public async Task<IActionResult> GetActiveBrands()
        {
            try
            {
                var list = await _thuongHieuService.GetAllThuongHieus();

                // Chỉ lấy thương hiệu có trạng thái "Còn hoạt động" (có dấu)
                var activeBrands = list.Where(t =>
                    t.TrangThaiThuongHieu == "Còn hoạt động")
                    .OrderBy(t => t.TenThuongHieu)
                    .ToList();

                return Ok(activeBrands);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tải thương hiệu: " + ex.Message });
            }
        }
    }
}
