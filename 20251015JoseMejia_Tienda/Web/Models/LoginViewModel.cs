using System.ComponentModel.DataAnnotations;

namespace Web.Models;

public class LoginViewModel
{
    [Required, StringLength(50)]
    public string Usuario { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), StringLength(50, MinimumLength = 4)]
    public string Clave { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}