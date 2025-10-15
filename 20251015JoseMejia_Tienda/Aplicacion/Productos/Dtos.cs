namespace Aplicacion.Productos;

public record CrearProductoDto(string Nombre, string Descripcion, decimal PrecioBase, string? ImagenUrl, decimal? PrecioConDescuento);
public record EditarProductoDto(string Nombre, string Descripcion, string? ImagenUrl);
public record ActualizarPrecioDto(decimal PrecioBase, decimal? PrecioConDescuento);

public record ProductoDto(int Id, string Nombre, string Descripcion, decimal PrecioBase, decimal? PrecioConDescuento, string? ImagenUrl);
