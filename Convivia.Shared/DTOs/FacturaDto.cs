using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convivia.Shared.DTOs
{
    public class FacturaDto
    {
        public FacturaDto() { }

        public string Id { get; set; }

        public string Nombre { get; set; }

        public float Precio { get; set; }

        public float PagoMediano { get; set; }

        public Dictionary<string, bool> Deudores { get; set; } = new Dictionary<string, bool>();

        public bool Pagado { get; set; }

        public bool TieneImagen { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }
}
