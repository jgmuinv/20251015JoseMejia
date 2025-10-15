using Dominio.Productos;

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
    public ProductosService(IProductoRepository repo)
    {
        _repo = repo;
    }

    public async Task<IReadOnlyList<ProductoDto>> ListarAsync(string Nombre, string Descripcion, bool Eliminado, CancellationToken ct = default)
    {
        var items = await _repo.ListarPorStoredProcedureAsync(Nombre, Descripcion, Eliminado, ct);
        return items.Select(Map).ToList();
    }

    public async Task<ProductoDto> CrearAsync(CrearProductoDto dto, CancellationToken ct = default)
    {
        var prod = new Producto(dto.Nombre, dto.Descripcion, dto.PrecioBase, false, dto.ImagenUrl);
        await _repo.CrearAsync(prod, ct);
        return Map(prod);
    }

    public async Task<ProductoDto?> EditarAsync(int id, EditarProductoDto dto, CancellationToken ct = default)
    {
        var prod = await _repo.ObtenerPorIdAsync(id, ct);
        if (prod == null) return null;
        prod.EditarDatos(dto.Nombre, dto.Descripcion, dto.ImagenUrl);
        await _repo.ActualizarAsync(prod, ct);
        return Map(prod);
    }

    public async Task<ProductoDto?> ActualizarPrecioAsync(int id, ActualizarPrecioDto dto, CancellationToken ct = default)
    {
        var prod = await _repo.ObtenerPorIdAsync(id, ct);
        if (prod == null) return null;
        prod.ActualizarPrecio(dto.PrecioBase, dto.PrecioConDescuento);
        await _repo.ActualizarAsync(prod, ct);
        return Map(prod);
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
