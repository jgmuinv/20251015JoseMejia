using Dominio.Productos;
using Microsoft.Extensions.Logging;

namespace Aplicacion.Productos;

public interface IProductosService
{
    Task<IReadOnlyList<ProductoDto>> ListarAsync(string Nombre, string Descripcion, bool Eliminado, CancellationToken ct = default);
    Task<ProductoDto> CrearAsync(CrearProductoDto dto, CancellationToken ct = default);
    Task<ProductoDto?> EditarAsync(int id, EditarProductoDto dto, CancellationToken ct = default);
    Task<ProductoDto?> ActualizarPrecioAsync(int id, ActualizarPrecioDto dto, CancellationToken ct = default);
    Task<bool> EliminarAsync(int id, CancellationToken ct = default);
    Task<ProductoDto?> ObtenerPorIdAsync(int id, CancellationToken ct = default);

}

public class ProductosService : IProductosService
{
    private readonly IProductoRepository _repo;
    private readonly Microsoft.Extensions.Logging.ILogger<ProductosService> _logger;
    public ProductosService(IProductoRepository repo, Microsoft.Extensions.Logging.ILogger<ProductosService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ProductoDto>> ListarAsync(string Nombre, string Descripcion, bool Eliminado, CancellationToken ct = default)
    {
        var items = await _repo.ListarPorStoredProcedureAsync(Nombre, Descripcion, Eliminado, ct);
        return items.Select(Map).ToList();
    }

    public async Task<ProductoDto> CrearAsync(CrearProductoDto dto, CancellationToken ct = default)
    {
        try
        {
            var prod = new Producto(dto.Nombre, dto.Descripcion, dto.PrecioBase, false, dto.ImagenUrl, dto.PrecioConDescuento);
            await _repo.CrearAsync(prod, ct);
            return Map(prod);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear producto {Nombre}", dto.Nombre);
            throw;
        }
    }

    public async Task<ProductoDto?> EditarAsync(int id, EditarProductoDto dto, CancellationToken ct = default)
    {
        try
        {
            var prod = await _repo.ObtenerPorIdAsync(id, ct);
            if (prod == null) return null;
            var revision = prod.EditarDatos(dto.Nombre, dto.Descripcion, dto.ImagenUrl);
            if (!string.Equals(revision, "ok", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(revision);
            }
            await _repo.ActualizarAsync(prod, ct);
            return Map(prod);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al editar producto {ProductoId}", id);
            throw;
        }
    }

    public async Task<ProductoDto?> ActualizarPrecioAsync(int id, ActualizarPrecioDto dto, CancellationToken ct = default)
    {
        try
        {
            var prod = await _repo.ObtenerPorIdAsync(id, ct);
            if (prod == null) return null;
            var revision = prod.ActualizarPrecio(dto.PrecioBase, dto.PrecioConDescuento);
            if (!string.Equals(revision, "ok", StringComparison.OrdinalIgnoreCase))
            {
                // Map message to parameter name for validation response
                var param = revision.Contains("nuevoPrecioConDescuento", StringComparison.OrdinalIgnoreCase)
                    ? nameof(dto.PrecioConDescuento)
                    : nameof(dto.PrecioBase);
                throw new ArgumentOutOfRangeException(param, revision);
            }
            await _repo.ActualizarAsync(prod, ct);
            return Map(prod);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar precio de producto {ProductoId}", id);
            throw;
        }
    }

    public async Task<bool> EliminarAsync(int id, CancellationToken ct = default)
    {
        var prod = await _repo.ObtenerPorIdAsync(id, ct);
        if (prod == null) return false;
        await _repo.EliminarAsync(id, ct);
        return true;
    }
    
    public async Task<ProductoDto?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
    {
        var prod = await _repo.ObtenerPorIdAsync(id, ct);
        return prod is null ? null : Map(prod);
    }

    private static ProductoDto Map(Producto p)
        => new(p.Id, p.Nombre, p.Descripcion, p.PrecioBase, p.PrecioConDescuento, p.ImagenUrl);
}
