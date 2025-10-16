using RestSharp;
using Web.Models;
using Microsoft.Extensions.Options;

namespace Web.Services;

public class ApiOptions
{
    public string BaseUrl { get; set; } = string.Empty;
}

// Local DTOs para comunicación con la API (evitamos referenciar el proyecto Aplicacion)
internal record CrearProductoDto(string Nombre, string Descripcion, decimal PrecioBase, string? ImagenUrl, decimal? PrecioConDescuento);
internal record EditarProductoDto(string Nombre, string Descripcion, string? ImagenUrl);
internal record ActualizarPrecioDto(decimal PrecioBase, decimal? PrecioConDescuento);
internal record ProductoDto(int Id, string Nombre, string Descripcion, decimal PrecioBase, decimal? PrecioConDescuento, string? ImagenUrl);

public interface IProductosApiClient
{
    Task<IReadOnlyList<ProductoViewModel>> ListarAsync(string? nombre, string? descripcion, bool eliminado, string? token, CancellationToken ct = default);
    Task<ProductoViewModel?> ObtenerPorIdAsync(int id, string? token, CancellationToken ct = default);
    Task<ProductoViewModel> CrearAsync(ProductoViewModel vm, string? token, CancellationToken ct = default);
    Task<ProductoViewModel?> EditarAsync(int id, ProductoViewModel vm, string? token, CancellationToken ct = default);
    Task<ProductoViewModel?> ActualizarPrecioAsync(int id, decimal precioBase, decimal? precioConDescuento, string? token, CancellationToken ct = default);
    Task<bool> EliminarAsync(int id, string? token, CancellationToken ct = default);
    Task<string> SubirImagenAsync(Stream fileStream, string fileName, string? token, CancellationToken ct = default);
}

public class ProductosApiClient : IProductosApiClient
{
    private readonly RestClient _client;
    private readonly string _baseUrl;
    public ProductosApiClient(IOptions<ApiOptions> opts)
    {
        _baseUrl = opts.Value.BaseUrl.TrimEnd('/');
        _client = new RestClient(_baseUrl);
    }

    private static void SetAuth(RestRequest req, string? token)
    {
        if (!string.IsNullOrWhiteSpace(token))
        {
            req.AddOrUpdateHeader("Authorization", $"Bearer {token}");
        }
    }

    public async Task<IReadOnlyList<ProductoViewModel>> ListarAsync(string? nombre, string? descripcion, bool eliminado, string? token, CancellationToken ct = default)
    {
        var req = new RestRequest("/Productos/Listar", Method.Get);
        req.AddQueryParameter("Nombre", nombre ?? string.Empty);
        req.AddQueryParameter("Descripcion", descripcion ?? string.Empty);
        req.AddQueryParameter("Eliminado", eliminado);
        SetAuth(req, token);
        var res = await _client.ExecuteAsync<List<ProductoDto>>(req, ct);
        if (!res.IsSuccessful || res.Data == null) throw new Exception(res.ErrorMessage ?? res.Content);
        return res.Data.Select(MapInstance).ToList();
    }

    public async Task<ProductoViewModel?> ObtenerPorIdAsync(int id, string? token, CancellationToken ct = default)
    {
        var req = new RestRequest($"/Productos/ObtenerPorId/{id}", Method.Get);
        SetAuth(req, token);
        var res = await _client.ExecuteAsync<ProductoDto>(req, ct);
        if (!res.IsSuccessful) return null;
        return res.Data == null ? null : MapInstance(res.Data);
    }

    public async Task<ProductoViewModel> CrearAsync(ProductoViewModel vm, string? token, CancellationToken ct = default)
    {
        var req = new RestRequest("/Productos/Crear", Method.Post);
        SetAuth(req, token);
        var body = new CrearProductoDto(vm.Nombre, vm.Descripcion, vm.PrecioBase, vm.ImagenUrl, vm.PrecioConDescuento);
        req.AddJsonBody(body);
        var res = await _client.ExecuteAsync<ProductoDto>(req, ct);
        if (!res.IsSuccessful || res.Data == null) throw new Exception(res.ErrorMessage ?? res.Content);
        return MapInstance(res.Data);
    }

    public async Task<ProductoViewModel?> EditarAsync(int id, ProductoViewModel vm, string? token, CancellationToken ct = default)
    {
        var req = new RestRequest($"/Productos/Editar/{id}", Method.Put);
        SetAuth(req, token);
        var body = new EditarProductoDto(vm.Nombre, vm.Descripcion, vm.ImagenUrl);
        req.AddJsonBody(body);
        var res = await _client.ExecuteAsync<ProductoDto>(req, ct);
        if (!res.IsSuccessful) return null;
        return res.Data == null ? null : MapInstance(res.Data);
    }

    public async Task<ProductoViewModel?> ActualizarPrecioAsync(int id, decimal precioBase, decimal? precioConDescuento, string? token, CancellationToken ct = default)
    {
        var req = new RestRequest($"/Productos/ActualizarPrecio/{id}/precio", Method.Put);
        SetAuth(req, token);
        var body = new ActualizarPrecioDto(precioBase, precioConDescuento);
        req.AddJsonBody(body);
        var res = await _client.ExecuteAsync(req, ct);
        if (res.IsSuccessful)
        {
            var dto = System.Text.Json.JsonSerializer.Deserialize<ProductoDto>(res.Content ?? "");
            return dto == null ? null : MapInstance(dto);
        }
        if ((int)res.StatusCode == 400 && !string.IsNullOrWhiteSpace(res.Content))
        {
            try
            {
                // Try to parse ValidationProblemDetails: { errors: { key: ["msg"] } }
                using var doc = System.Text.Json.JsonDocument.Parse(res.Content);
                if (doc.RootElement.TryGetProperty("errors", out var errors))
                {
                    foreach (var prop in errors.EnumerateObject())
                    {
                        var arr = prop.Value.EnumerateArray();
                        if (arr.MoveNext())
                        {
                            var msg = arr.Current.GetString();
                            if (!string.IsNullOrWhiteSpace(msg)) throw new InvalidOperationException(msg);
                        }
                    }
                }
            }
            catch
            {
                // ignore parse error
            }
            throw new InvalidOperationException("Los valores de precio no son válidos.");
        }
        return null;
    }

    public async Task<bool> EliminarAsync(int id, string? token, CancellationToken ct = default)
    {
        var req = new RestRequest($"/Productos/Eliminar/{id}", Method.Delete);
        SetAuth(req, token);
        var res = await _client.ExecuteAsync(req, ct);
        return res.IsSuccessful;
    }

    public async Task<string> SubirImagenAsync(Stream fileStream, string fileName, string? token, CancellationToken ct = default)
    {
        var req = new RestRequest("/Productos/SubirImagen/imagen", Method.Post);
        SetAuth(req, token);
        req.AlwaysMultipartFormData = true;
        req.AddFile("archivo", await ReadAllBytesAsync(fileStream, ct), fileName);
        var res = await _client.ExecuteAsync<Dictionary<string,string>>(req, ct);
        if (!res.IsSuccessful || res.Data == null || !res.Data.TryGetValue("ruta", out var ruta))
            throw new Exception(res.ErrorMessage ?? res.Content);
        return ruta;
    }

    private static async Task<byte[]> ReadAllBytesAsync(Stream stream, CancellationToken ct)
    {
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms, ct);
        return ms.ToArray();
    }

    private string? NormalizeUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return url;
        if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return url;
        if (url.StartsWith("/")) return _baseUrl + url;
        return _baseUrl + "/" + url;
    }

    private ProductoViewModel MapInstance(ProductoDto d) => new()
    {
        Id = d.Id,
        Nombre = d.Nombre,
        Descripcion = d.Descripcion,
        PrecioBase = d.PrecioBase,
        PrecioConDescuento = d.PrecioConDescuento,
        ImagenUrl = NormalizeUrl(d.ImagenUrl)
    };

    private static ProductoViewModel Map(ProductoDto d) => new()
    {
        Id = d.Id,
        Nombre = d.Nombre,
        Descripcion = d.Descripcion,
        PrecioBase = d.PrecioBase,
        PrecioConDescuento = d.PrecioConDescuento,
        ImagenUrl = d.ImagenUrl
    };
}