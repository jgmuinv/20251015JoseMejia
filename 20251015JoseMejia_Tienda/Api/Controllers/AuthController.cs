using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _cfg;
    public AuthController(IConfiguration cfg)
    {
        _cfg = cfg;
    }

    public record LoginRequest(string Payload);
    public record LoginResponse(bool Ok, string? Token, string? Usuario, string? Error);

    [HttpPost]
    [AllowAnonymous]
    public ActionResult<LoginResponse> Login([FromBody] LoginRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Payload))
            return BadRequest(new LoginResponse(false, null, null, "Payload vacío"));

        try
        {
            var secret = _cfg.GetSection("Jwt").GetValue<string>("Secret") ?? string.Empty;
            var (user, pass) = DecryptCredentials(req.Payload, secret);

            // Validación de credenciales en el backEnd
            if (user == "admin" && pass == "123456")
            {
                var token = CreateAdminToken(user);
                return Ok(new LoginResponse(true, token, user, null));
            }
            return Unauthorized(new LoginResponse(false, null, null, "Credenciales inválidas"));
        }
        catch (FormatException)
        {
            // Payload no es Base64 válido
            return Unauthorized(new LoginResponse(false, null, null, "Credenciales inválidas"));
        }
        catch (CryptographicException)
        {
            // Error de padding/clave/IV: tratar como credenciales inválidas para no filtrar detalles
            return Unauthorized(new LoginResponse(false, null, null, "Credenciales inválidas"));
        }
        catch (InvalidOperationException)
        {
            return Unauthorized(new LoginResponse(false, null, null, "Credenciales inválidas"));
        }
        catch (Exception ex)
        {
            return BadRequest(new LoginResponse(false, null, null, ex.Message));
        }
    }

    private static (string user, string pass) DecryptCredentials(string payloadBase64, string secret)
    {
        var cipher = Convert.FromBase64String(payloadBase64);
        using var aes = Aes.Create();
        aes.Key = DeriveKey(secret);
        aes.IV = new byte[16]; // IV fijo para simplicidad demo
        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream(cipher);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs, Encoding.UTF8);
        var plain = sr.ReadToEnd();
        var parts = plain.Split(':', 2);
        if (parts.Length != 2) throw new InvalidOperationException("Formato de credenciales inválido");
        return (parts[0], parts[1]);
    }

    private static byte[] DeriveKey(string secret)
    {
        // Deriva 32 bytes de la secret (HMACSHA256)
        using var sha = SHA256.Create();
        return sha.ComputeHash(Encoding.UTF8.GetBytes(secret));
    }

    private string CreateAdminToken(string username)
    {
        var issuer = _cfg.GetSection("Jwt").GetValue<string>("Issuer") ?? string.Empty;
        var audience = _cfg.GetSection("Jwt").GetValue<string>("Audience") ?? string.Empty;
        var secret = _cfg.GetSection("Jwt").GetValue<string>("Secret") ?? string.Empty;
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, username),
            new(ClaimTypes.Role, "Admin")
        };
        var now = DateTime.UtcNow;
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: now,
            expires: now.AddHours(1),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
