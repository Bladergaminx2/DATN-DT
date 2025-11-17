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

            if (errors.Count > 0)
                return BadRequest(errors);

            try
            {
                // Gọi service (không gán var vì Update/Create trả về Task void)
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
        [Route("TonKho/Edit/{id}")]
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

            if (errors.Count > 0)
                return BadRequest(errors);

            try
            {
                tonKho.IdTonKho = id; // đảm bảo Id đúng
                await _tonKhoService.Update(tonKho); // ✅ gọi service, không gán var
                return Ok(new { message = "Cập nhật tồn kho thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật tồn kho: " + ex.Message });
            }
        }

        // ----------------------------
        // POST: Đồng bộ tồn kho từ IMEI
        // ----------------------------
        //[HttpPost]
        //[Route("TonKho/SyncInventory")]
        //public async Task<IActionResult> SyncInventory()
        //{
        //    try
        //    {
        //        // Gọi service để xử lý đồng bộ
        //        await _tonKhoService.SyncInventory(); // Service thực hiện logic sync
        //        return Ok(new { message = "Đồng bộ tồn kho thành công!" });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = "Lỗi khi đồng bộ tồn kho: " + ex.Message });
        //    }
        //}

        // ----------------------------
        // GET: Danh sách ModelSanPham
        // ----------------------------
        //[HttpGet]
        //public async Task<IActionResult> GetAllModelSanPham()
        //{
        //    try
        //    {
        //        var list = await _tonKhoService.GetAllModelSanPhams(); // Service trả về List<ModelSanPham>
        //        return Ok(list);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = "Lỗi khi lấy danh sách model: " + ex.Message });
        //    }
        //}

        // ----------------------------
        // GET: Danh sách Kho
        // ----------------------------
        //[HttpGet]
        //public async Task<IActionResult> GetAllKho()
        //{
        //    try
        //    {
        //        var list = await _tonKhoService.GetAllKhos(); // Service trả về List<Kho>
        //        return Ok(list);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = "Lỗi khi lấy danh sách kho: " + ex.Message });
        //    }
        //}
    }
}
