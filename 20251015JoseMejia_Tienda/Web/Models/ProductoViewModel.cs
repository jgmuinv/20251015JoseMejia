using System.ComponentModel.DataAnnotations;

namespace Web.Models;

public class ProductoViewModel
{
    public int Id { get; set; }

    [Required, StringLength(100, MinimumLength = 2)]
    public string Nombre { get; set; } = string.Empty;

    [Required, StringLength(500, MinimumLength = 2)]
    public string Descripcion { get; set; } = string.Empty;

    [Required, Range(0.01, 99999999)]
    [DataType(DataType.Currency)]
    public decimal PrecioBase { get; set; }

    [Range(0.01, 99999999)]
    [DataType(DataType.Currency)]
    public decimal? PrecioConDescuento { get; set; }

    [Url]
    public string? ImagenUrl { get; set; }
}

public class ProductosFiltroViewModel
{
    [StringLength(100)]
    public string? Nombre { get; set; }
    [StringLength(500)]
    public string? Descripcion { get; set; }
    public bool Eliminado { get; set; }

    public IReadOnlyList<ProductoViewModel> Resultados { get; set; } = new List<ProductoViewModel>();
}