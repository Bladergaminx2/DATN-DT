using DATN_DT.CustomAttribute;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DATN_DT.Controllers
{
    public class DemoController : Controller
    {
        [AllowAnonymous]
        public IActionResult PublicPage()
        {
            return View();
        }

        [AuthorizeRoleFromToken("KHACHHANG")]
        public IActionResult CustomerPage()
        {
            // Trả View để render giao diện
            return View();
        }

        [AuthorizeRoleFromToken("ADMIN")]
        public IActionResult StaffPage()
        {
            // Trả View để render giao diện
            return View();
        }

        [AuthorizeRoleFromToken("KHACHHANG", "ADMIN")]
        public IActionResult MultiRolePage()
        {
            // Trả View để render giao diện
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult NoRoleAciton([FromBody] string info)
        {
            // Cả Khách hàng và ADMIN đều có thể thao tác
            return Json(new { success = true, message = $"Xử lý không yêu cầu đăng nhập: {info}" });
        }

        // ✅ Ví dụ action xử lý CRUD (POST) cho khách hàng
        [HttpPost]
        [AuthorizeRoleFromToken("KHACHHANG")]
        public IActionResult CustomerDoSomething([FromBody] string data)
        {
            // Xử lý logic chỉ dành cho Khách hàng
            return Json(new { success = true, message = $"Khách hàng xử lý thành công: {data}" });
        }

        // ✅ Ví dụ action xử lý CRUD (POST) cho ADMIN
        [HttpPost]
        [AuthorizeRoleFromToken("ADMIN")]
        public IActionResult StaffDoSomething([FromBody] string task)
        {
            // Xử lý logic chỉ dành cho ADMIN
            return Json(new { success = true, message = $"ADMIN thực hiện task: {task}" });
        }

        // ✅ Ví dụ action xử lý cho nhiều role
        [HttpPost]
        [AuthorizeRoleFromToken("KHACHHANG", "ADMIN")]
        public IActionResult MultiRoleDoSomething([FromBody] string info)
        {
            // Cả Khách hàng và ADMIN đều có thể thao tác
            return Json(new { success = true, message = $"Xử lý chung: {info}" });
        }
    }
}
