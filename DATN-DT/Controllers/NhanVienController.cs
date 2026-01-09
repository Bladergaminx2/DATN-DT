using DATN_DT.CustomAttribute;
using DATN_DT.Form;
using DATN_DT.IServices;
using Microsoft.AspNetCore.Mvc;

namespace DATN_DT.Controllers
{
    [Route("NhanVien")]
    [AuthorizeRoleFromToken("ADMIN")] // Chỉ ADMIN mới được quản lý nhân viên
    public class NhanVienController : Controller
    {
        private readonly INhanVienService _nhanVienService;

        public NhanVienController(INhanVienService nhanVienService)
        {
            _nhanVienService = nhanVienService;
        }

        // --- GET ALL ---
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var nhanVienList = await _nhanVienService.GetAllNhanViens();
            return View(nhanVienList);
        }

        // --- GET ALL CHUC VU ---
        [HttpGet("ChucVu")]
        public async Task<IActionResult> GetChucVus()
        {
            var chucVus = await _nhanVienService.GetChucVus();
            return Ok(chucVus);
        }

        // --- GET CHUC VU EXCEPT ADMIN ---
        [HttpGet("ChucVu/ExceptAdmin")]
        public async Task<IActionResult> GetChucVusExceptAdmin()
        {
            var chucVus = await _nhanVienService.GetChucVusExceptAdmin();
            return Ok(chucVus);
        }

        // --- CREATE ---
        [HttpPost("Create")]
        [Consumes("application/json")]
        public async Task<IActionResult> Create([FromBody] NhanVienFormSystemCreate? nhanVien)
        {
            if (nhanVien == null)
                return BadRequest(new { message = "Dữ liệu không hợp lệ!" });

            try
            {
                await _nhanVienService.Create(nhanVien);
                return Ok(new { message = "Thêm Nhân viên thành công!" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        // --- UPDATE ---
        [HttpPost("Update/{id}")]
        [Consumes("application/json")]
        public async Task<IActionResult> Update(int id, [FromBody] NhanVienFormSystem? nhanVien)
        {
            try
            {
                if (nhanVien == null)
                    return BadRequest(new { message = "Dữ liệu không hợp lệ!" });

                // Nếu có password mới, dùng UpdateWithPassword
                if (!string.IsNullOrEmpty(nhanVien.Password))
                {
                    await _nhanVienService.UpdateWithPassword(id, nhanVien, nhanVien.Password);
                }
                else
                {
                    await _nhanVienService.Update(id, nhanVien);
                }

                return Ok(new { message = "Cập nhật Nhân viên thành công!" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Lỗi server khi cập nhật nhân viên",
                    detail = ex.Message
                });
            }
        }

        // --- DELETE ---
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _nhanVienService.Delete(id);
                return Ok(new { message = "Xóa nhân viên thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }
    }
}
