using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convivia.Shared.DTOs
{
    public class CreateFacturaDto
    {
        public string Nombre { get; set; }
        public decimal Precio { get; set; }
        public float PagoMediano { get; set; }
        public Dictionary<string, bool> Deudores { get; set; } = new Dictionary<string, bool>();
        public bool Pagado { get; set; }
    }
}
