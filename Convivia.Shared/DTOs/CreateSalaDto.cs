using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convivia.Shared.DTOs
{
    public class CreateSalaDto
    {
        public string Nombre { get; set; }
        public string? Descripcion { get; set; }
        public string IdEspacio { get; set; }
    }
}
