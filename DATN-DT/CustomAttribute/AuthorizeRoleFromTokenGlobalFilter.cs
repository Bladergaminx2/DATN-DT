using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System;

namespace DATN_DT.CustomAttribute
{
    // Filter toàn cục
    public class AuthorizeRoleFromTokenGlobalFilter : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;

            // Bỏ qua AllowAnonymous
            if (actionDescriptor.MethodInfo.GetCustomAttributes(typeof(AllowAnonymousAttribute), true).Any() ||
                actionDescriptor.ControllerTypeInfo.GetCustomAttributes(typeof(AllowAnonymousAttribute), true).Any())
                return;

            // Lấy role attribute nếu có
            var roleAttr = actionDescriptor.MethodInfo.GetCustomAttributes(typeof(AuthorizeRoleFromTokenAttribute), true)
                            .FirstOrDefault() as AuthorizeRoleFromTokenAttribute;

            string[] roles = roleAttr?.Roles ?? Array.Empty<string>();

            var token = context.HttpContext.Request.Cookies["jwt"];
            if (string.IsNullOrEmpty(token))
            {
                // Lấy URL hiện tại để làm returnUrl
                var currentUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
                context.Result = new RedirectToActionResult("AccessDenied", "Error", new { message = "Bạn cần đăng nhập.", returnUrl = currentUrl });
                return;
            }

            try
            {
                var jwtHandler = new JwtSecurityTokenHandler();
                
                // Kiểm tra token có thể đọc được không
                if (!jwtHandler.CanReadToken(token))
                {
                    var currentUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
                    context.Result = new RedirectToActionResult("AccessDenied", "Error", new { message = "Token không hợp lệ.", returnUrl = currentUrl });
                    return;
                }
                
                var jwtToken = jwtHandler.ReadJwtToken(token);
                
                // Kiểm tra token hết hạn
                if (jwtToken.ValidTo < DateTime.UtcNow)
                {
                    // Token hết hạn - redirect đến Login với returnUrl
                    var currentUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
                    // Xóa cookie cũ
                    context.HttpContext.Response.Cookies.Delete("jwt");
                    // Redirect trực tiếp đến Login thay vì AccessDenied
                    context.Result = new RedirectResult($"/Login?returnUrl={Uri.EscapeDataString(currentUrl)}");
                    return;
                }
                
                var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value?.ToUpper();

                if (roles.Length > 0 && !roles.Contains(roleClaim))
                {
                    var currentUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
                    context.Result = new RedirectToActionResult("AccessDenied", "Error", new { message = "Bạn không có quyền truy cập.", returnUrl = currentUrl });
                    return;
                }

                // Gắn user
                var claims = jwtToken.Claims.Select(c => new Claim(c.Type, c.Value));
                context.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
            }
            catch (Exception ex)
            {
                // Lấy URL hiện tại để làm returnUrl
                var currentUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
                // Xóa cookie không hợp lệ
                context.HttpContext.Response.Cookies.Delete("jwt");
                // Redirect trực tiếp đến Login thay vì AccessDenied
                context.Result = new RedirectResult($"/Login?returnUrl={Uri.EscapeDataString(currentUrl)}");
            }
        }
    }
}
