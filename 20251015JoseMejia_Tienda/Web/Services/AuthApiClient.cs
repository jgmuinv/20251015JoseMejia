using Contratos.DTOs.Ingresar;
using Microsoft.Extensions.Options;
using RestSharp;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Web.Services;

public interface IAuthApiClient
{
    Task<(bool ok, string? token, string? usuario, string? error)> LoginAsync(string usuario, string clave, CancellationToken ct = default);
}

public class AuthApiClient : IAuthApiClient
{
    private readonly RestClient _client;
    private readonly string _secret;
    public AuthApiClient(IOptions<ApiOptions> api, IOptions<JwtOptions> jwt)
    {
        _client = new RestClient(api.Value.BaseUrl.TrimEnd('/'));
        _secret = jwt.Value.Secret;
    }

    private static readonly JsonSerializerOptions _json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public async Task<(bool ok, string? token, string? usuario, string? error)> LoginAsync(string usuario, string clave, CancellationToken ct = default)
    {
        var payload = EncryptCredentials(usuario + ":" + clave, _secret);
        var req = new RestRequest("/Auth/Login", Method.Post).AddJsonBody(new LoginRequest(payload));
        var res = await _client.ExecuteAsync<LoginResponse>(req, ct);
        if (!res.IsSuccessful || res.Data == null)
        {
            return (false, null, null, res.ErrorMessage ?? res.Content);
        }
        return (res.Data.Ok, res.Data.Token, res.Data.Usuario, res.Data.Error);
    }

    private static string EncryptCredentials(string text, string secret)
    {
        using var aes = Aes.Create();
        aes.Key = DeriveKey(secret);
        aes.IV = new byte[16];
        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (var sw = new StreamWriter(cs, Encoding.UTF8))
        {
            sw.Write(text);
        }
        return Convert.ToBase64String(ms.ToArray());
    }

    private static byte[] DeriveKey(string secret)
    {
        using var sha = SHA256.Create();
        return sha.ComputeHash(Encoding.UTF8.GetBytes(secret));
    }

    private record LoginResponse(bool Ok, string? Token, string? Usuario, string? Error);
}
