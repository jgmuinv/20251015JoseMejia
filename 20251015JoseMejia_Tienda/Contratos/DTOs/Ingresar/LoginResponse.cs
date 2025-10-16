using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contratos.DTOs.Ingresar
{
    public sealed record LoginResponse(
        bool   Ok,
        string? Token,
        string? Usuario,
        string? Error
    );
}
