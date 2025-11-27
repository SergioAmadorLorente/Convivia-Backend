using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convivia.Shared.DTOs
{
    public class UpdateUsuarioDto
    {
        public string? Nombre { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Telefono { get; set; }
        public bool? Premium { get; set; }
    }
}
