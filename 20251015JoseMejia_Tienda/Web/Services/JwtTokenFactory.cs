using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Web.Services;

public class JwtOptions
{
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;
}

public interface IJwtTokenFactory
{
    string CreateAdminToken(string username, TimeSpan? lifetime = null);
}

public class JwtTokenFactory : IJwtTokenFactory
{
    private readonly JwtOptions _opts;
    public JwtTokenFactory(IOptions<JwtOptions> opts)
    {
        _opts = opts.Value;
    }

    public string CreateAdminToken(string username, TimeSpan? lifetime = null)
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opts.Secret));
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, username),
            new(ClaimTypes.Role, "Admin")
        };
        var now = DateTime.UtcNow;
        var token = new JwtSecurityToken(
            issuer: _opts.Issuer,
            audience: _opts.Audience,
            claims: claims,
            notBefore: now,
            expires: now.Add(lifetime ?? TimeSpan.FromHours(1)),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}