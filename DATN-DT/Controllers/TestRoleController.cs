using DATN_DT.CustomAttribute;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace DATN_DT.Controllers
{
    [AllowAnonymous]
    public class TestRoleController : Controller
    {
        [HttpGet("TestRole")]
        public IActionResult TestRole()
        {
            var token = Request.Cookies["jwt"];
            if (string.IsNullOrEmpty(token))
            {
                return Json(new { error = "Không có token" });
            }

            try
            {
                var jwtHandler = new JwtSecurityTokenHandler();
                var jwtToken = jwtHandler.ReadJwtToken(token);
                
                var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                var nameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                var idNhanVien = jwtToken.Claims.FirstOrDefault(c => c.Type == "IdNhanVien")?.Value;
                
                return Json(new 
                { 
                    token = token.Substring(0, Math.Min(50, token.Length)) + "...",
                    role = roleClaim,
                    roleUpper = roleClaim?.ToUpper(),
                    name = nameClaim,
                    idNhanVien = idNhanVien,
                    allClaims = jwtToken.Claims.Select(c => new { c.Type, c.Value }).ToList()
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }
    }
}

