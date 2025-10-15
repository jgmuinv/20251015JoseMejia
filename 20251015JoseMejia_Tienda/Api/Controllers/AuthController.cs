using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Api.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest req)
        {
            // Validación simple de demo (reemplace por su lógica)
            if (req.Username != "admin" || req.Password != "123456")
                return Unauthorized();

            var issuer = "JoseMejia";
            var audience = "TiendaWeb";
            var keyBytes = Encoding.UTF8.GetBytes("dk5tn[8i[LJbcU`rC9$jJ0/6f@u9O$J-BzZDR4-D~!+mg*]J5;"); // misma clave

            var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, req.Username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.Role, "Admin") // para probar la política AdminOnly
        };

            var creds = new SigningCredentials(new SymmetricSecurityKey(keyBytes),
                                               SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return Ok(new { token = jwt });
        }
        public record LoginRequest(string Username, string Password);
    }
}
