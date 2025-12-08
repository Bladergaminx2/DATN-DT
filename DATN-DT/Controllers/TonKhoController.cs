using DATN_DT.IServices;
using DATN_DT.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DATN_DT.Controllers
{
    public class TonKhoController : Controller
    {
        private readonly ITonKhoService _tonKhoService;
        private readonly HttpClient _httpClient;

        public TonKhoController(ITonKhoService tonKhoService, IHttpClientFactory httpClientFactory)
        {
            _tonKhoService = tonKhoService;
            _httpClient = httpClientFactory.CreateClient();
        }

        // ----------------------------
        // GET: Danh sách tồn kho
        // ----------------------------
        public async Task<IActionResult> Index()
        {
            var tonKhos = await _tonKhoService.GetAllTonKhos();
            return View(tonKhos);
        }

        // API endpoints cho dropdown
        [HttpGet("api/ModelSanPham")]
        public async Task<IActionResult> GetModelSanPhams()
        {
            var models = await _tonKhoService.GetModelSanPhams();
            return Ok(models);
        }

        [HttpGet("api/Kho")]
        public async Task<IActionResult> GetKhos()
        {
            var khos = await _tonKhoService.GetKhos();
            return Ok(khos);
        }

        // ----------------------------
        // POST: Tạo tồn kho
        // ----------------------------
        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> Create([FromBody] TonKho? tonKho)
        {
            if (tonKho == null)
                return BadRequest(new { message = "Dữ liệu tồn kho không hợp lệ!" });

            var errors = new Dictionary<string, string>();

            if (tonKho.IdModelSanPham == 0)
                errors["IdModelSanPham"] = "Phải chọn model sản phẩm!";
            if (tonKho.IdKho == 0)
                errors["IdKho"] = "Phải chọn kho!";
            if (tonKho.SoLuong < 0)
                errors["SoLuong"] = "Số lượng không được âm!";
            if (tonKho.SoLuong == 0)
                errors["SoLuong"] = "Số lượng phải lớn hơn 0!";

            if (errors.Count > 0)
                return BadRequest(errors);

            try
            {
                // Kiểm tra trùng (cùng model và cùng kho)
                var isDuplicate = await _tonKhoService.CheckDuplicate(tonKho.IdModelSanPham, tonKho.IdKho, 0);
                if (isDuplicate)
                    return Conflict(new { message = "Đã tồn tại bản ghi tồn kho cho model sản phẩm và kho này!" });

                await _tonKhoService.Create(tonKho);
                return Ok(new { message = "Thêm tồn kho thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi thêm tồn kho: " + ex.Message });
            }
        }

        // ----------------------------
        // POST: Cập nhật tồn kho
        // ----------------------------
        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> Edit(int id, [FromBody] TonKho? tonKho)
        {
            if (tonKho == null)
                return BadRequest(new { message = "Dữ liệu tồn kho không hợp lệ!" });

            var errors = new Dictionary<string, string>();

            if (tonKho.IdModelSanPham == 0)
                errors["IdModelSanPham"] = "Phải chọn model sản phẩm!";
            if (tonKho.IdKho == 0)
                errors["IdKho"] = "Phải chọn kho!";
            if (tonKho.SoLuong < 0)
                errors["SoLuong"] = "Số lượng không được âm!";
            if (tonKho.SoLuong == 0)
                errors["SoLuong"] = "Số lượng phải lớn hơn 0!";

            if (errors.Count > 0)
                return BadRequest(errors);

            try
            {
                // Kiểm tra trùng (cùng model và cùng kho, trừ bản ghi hiện tại)
                var isDuplicate = await _tonKhoService.CheckDuplicate(tonKho.IdModelSanPham, tonKho.IdKho, id);
                if (isDuplicate)
                    return Conflict(new { message = "Đã tồn tại bản ghi tồn kho cho model sản phẩm và kho này!" });

                tonKho.IdTonKho = id;
                await _tonKhoService.Update(tonKho);
                return Ok(new { message = "Cập nhật tồn kho thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật tồn kho: " + ex.Message });
            }
        }

        // ----------------------------
        // DELETE: Xóa tồn kho
        // ----------------------------
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _tonKhoService.Delete(id);
                return Ok(new { message = "Xóa tồn kho thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi xóa tồn kho: " + ex.Message });
            }
        }
    }
}