namespace Contratos.DTOs.Producto;

public class CrearProductoDto
{
    public required string Nombre { get; set; }
    public required string Descripcion { get; set; }
    public required decimal PrecioBase { get; set; }
    public required string ImagenUrl { get; set; }
    public decimal PrecioConDescuento { get; set; }
}