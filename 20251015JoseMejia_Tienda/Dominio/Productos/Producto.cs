namespace Dominio.Productos;

public class Producto
{
    public int Id { get; set; }
    public string Nombre { get; private set; }
    public string Descripcion { get; private set; }
    public decimal PrecioBase { get; private set; }
    public decimal? PrecioConDescuento { get; private set; }
    public string? ImagenUrl { get; private set; }
    public bool Activo { get; private set; } = true;

    public Producto(string nombre, string descripcion, decimal precioBase, bool eliminado, string? imagenUrl = null, decimal? precioConDescuento = null)
    {
        if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("Nombre es requerido", nameof(nombre));
        if (precioBase <= 0) throw new ArgumentOutOfRangeException(nameof(precioBase), "El precio base debe ser mayor que cero");
        if (precioConDescuento.HasValue && (precioConDescuento.Value <= 0 || precioConDescuento.Value > precioBase))
            throw new ArgumentOutOfRangeException(nameof(precioConDescuento), "El descuento debe ser mayor a 0 y menor o igual al precio base");

        Nombre = nombre.Trim();
        Descripcion = descripcion?.Trim() ?? string.Empty;
        PrecioBase = precioBase;
        PrecioConDescuento = precioConDescuento;
        ImagenUrl = imagenUrl;
    }

    public string EditarDatos(string nombre, string descripcion, string? imagenUrl)
    {
        if (string.IsNullOrWhiteSpace(nombre)) return $"{nameof(nombre)}: Nombre es requerido";
        Nombre = nombre.Trim();
        Descripcion = descripcion?.Trim() ?? string.Empty;
        ImagenUrl = imagenUrl;
        return "ok";
    }

    public string ActualizarPrecio(decimal nuevoPrecioBase, decimal? nuevoPrecioConDescuento)
    {
        if (nuevoPrecioBase <= 0)
            return ($"{nameof(nuevoPrecioBase)}: El precio base debe ser mayor que cero");
        if (nuevoPrecioConDescuento.HasValue && (nuevoPrecioConDescuento.Value <= 0 || nuevoPrecioConDescuento.Value > nuevoPrecioBase))
            return ($"{nameof(nuevoPrecioConDescuento)}: El descuento debe ser mayor a 0 y menor o igual al precio base");

        PrecioBase = nuevoPrecioBase;
        PrecioConDescuento = nuevoPrecioConDescuento;
        return "ok";
    }
}
