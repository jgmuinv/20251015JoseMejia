using System.ComponentModel.DataAnnotations;

namespace Web.Models;

public class LoginViewModel
{
    [Required, StringLength(50)]
    [Display(Name = "Usuario")]
    public string Usuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [DataType(DataType.Password)]
    [StringLength(50, MinimumLength = 4, ErrorMessage = "El campo {0} debe tener entre {2} y {1} caracteres.")]
    [Display(Name = "Clave")]
    public string Clave { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}