using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

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
                context.Result = new RedirectToActionResult("AccessDenied", "Error", new { message = "Bạn cần đăng nhập." });
                return;
            }

            try
            {
                var jwtHandler = new JwtSecurityTokenHandler();
                var jwtToken = jwtHandler.ReadJwtToken(token);
                var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value?.ToUpper();

                if (roles.Length > 0 && !roles.Contains(roleClaim))
                {
                    context.Result = new RedirectToActionResult("AccessDenied", "Error", new { message = "Bạn không có quyền truy cập." });
                    return;
                }

                // Gắn user
                var claims = jwtToken.Claims.Select(c => new Claim(c.Type, c.Value));
                context.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
            }
            catch
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Error", new { message = "Token không hợp lệ." });
            }
        }
    }
}
