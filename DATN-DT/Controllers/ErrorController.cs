using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DATN_DT.Controllers
{
    [AllowAnonymous]
    public class ErrorController : Controller
    {
        public IActionResult AccessDenied(string message, string returnUrl = null)
        {
            ViewBag.Message = message;
            ViewBag.ReturnUrl = returnUrl;
            
            // Nếu có returnUrl trong query string, lưu lại
            var queryReturnUrl = Request.Query["returnUrl"].FirstOrDefault();
            if (!string.IsNullOrEmpty(queryReturnUrl))
            {
                ViewBag.ReturnUrl = queryReturnUrl;
            }
            
            // Nếu không có returnUrl, thử lấy từ referer
            if (string.IsNullOrEmpty(ViewBag.ReturnUrl))
            {
                var referer = Request.Headers["Referer"].ToString();
                if (!string.IsNullOrEmpty(referer) && referer.Contains("/UserProfile"))
                {
                    ViewBag.ReturnUrl = referer;
                }
            }
            
            return View();
        }
    }
}
