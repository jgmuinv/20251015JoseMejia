using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contratos.DTOs.Ingresar
{
    public class LoginResponse
    {
        public bool ok { get; set; }
        public object token { get; set; }
        public object usuario { get; set; }
        public string error { get; set; }
    }
}
