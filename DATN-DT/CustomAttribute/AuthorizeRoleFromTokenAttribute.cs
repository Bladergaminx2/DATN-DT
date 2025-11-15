using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace DATN_DT.CustomAttribute
{
    public class AuthorizeRoleFromTokenAttribute : Attribute, IAuthorizationFilter
    {
        public string[] Roles { get; }  // 🌟 property công khai

        public AuthorizeRoleFromTokenAttribute(params string[] roles)
        {
            Roles = roles.Select(r => r.ToUpper()).ToArray();
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
            if (actionDescriptor != null)
            {
                bool hasAllowAnonymous = actionDescriptor.MethodInfo
                    .GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute), true)
                    .Any()
                    || actionDescriptor.ControllerTypeInfo
                    .GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute), true)
                    .Any();

                if (hasAllowAnonymous)
                {
                    return;
                }
            }

            var httpContext = context.HttpContext;
            var token = httpContext.Request.Cookies["jwt"];
            if (string.IsNullOrEmpty(token))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Error", new { message = "Bạn cần đăng nhập." });
                return;
            }

            try
            {
                var jwtHandler = new JwtSecurityTokenHandler();
                var jwtToken = jwtHandler.ReadJwtToken(token);

                // ✅ Dùng property Roles thay cho _roles
                var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value?.ToUpper();
                if (string.IsNullOrEmpty(roleClaim) || !Roles.Contains(roleClaim))
                {
                    context.Result = new RedirectToActionResult("AccessDenied", "Error", new { message = "Bạn không có quyền truy cập." });
                    return;
                }

                var claims = jwtToken.Claims.Select(c => new Claim(c.Type, c.Value));
                var identity = new ClaimsIdentity(claims, "jwt");
                httpContext.User = new ClaimsPrincipal(identity);
            }
            catch
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Error", new { message = "Token không hợp lệ." });
            }
        }
    }
}
