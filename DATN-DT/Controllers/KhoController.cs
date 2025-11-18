using DATN_DT.IServices;
using DATN_DT.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DATN_DT.Controllers
{
    public class KhoController : Controller
    {
        private readonly IKhoService _khoService;

        public KhoController(IKhoService khoService)
        {
            _khoService = khoService;
        }

        // ----------------------------
        // GET: Danh sách kho
        // ----------------------------
        public async Task<IActionResult> Index()
        {
            var khos = await _khoService.GetAllKhos();
            return View(khos);
        }

        // ----------------------------
        // POST: Tạo kho
        // ----------------------------
        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> Create([FromBody] Kho? kho)
        {
            if (kho == null)
                return BadRequest(new { message = "Dữ liệu kho không hợp lệ!" });

            var errors = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(kho.TenKho))
                errors["TenKho"] = "Phải nhập tên kho!";
            if (string.IsNullOrWhiteSpace(kho.DiaChiKho))
                errors["DiaChi"] = "Phải nhập địa chỉ kho!";

            if (errors.Count > 0)
                return BadRequest(errors);

            try
            {
                // Kiểm tra trùng tên kho
                var isDuplicate = await _khoService.CheckDuplicate(kho.TenKho, 0);
                if (isDuplicate)
                    return Conflict(new { message = "Tên kho đã tồn tại!" });

                await _khoService.Create(kho);
                return Ok(new { message = "Thêm kho thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi thêm kho: " + ex.Message });
            }
        }

        // ----------------------------
        // POST: Cập nhật kho
        // ----------------------------
        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> Edit(int id, [FromBody] Kho? kho)
        {
            if (kho == null)
                return BadRequest(new { message = "Dữ liệu kho không hợp lệ!" });

            var errors = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(kho.TenKho))
                errors["TenKho"] = "Phải nhập tên kho!";
            if (string.IsNullOrWhiteSpace(kho.DiaChiKho))
                errors["DiaChi"] = "Phải nhập địa chỉ kho!";

            if (errors.Count > 0)
                return BadRequest(errors);

            try
            {
                // Kiểm tra trùng tên kho (trừ bản ghi hiện tại)
                var isDuplicate = await _khoService.CheckDuplicate(kho.TenKho, id);
                if (isDuplicate)
                    return Conflict(new { message = "Tên kho đã tồn tại!" });

                kho.IdKho = id;
                await _khoService.Update(kho);
                return Ok(new { message = "Cập nhật kho thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật kho: " + ex.Message });
            }
        }

        // ----------------------------
        // DELETE: Xóa kho
        // ----------------------------
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _khoService.Delete(id);
                return Ok(new { message = "Xóa kho thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi xóa kho: " + ex.Message });
            }
        }
    }
}