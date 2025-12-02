using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convivia.Shared.DTOs
{
    public class UpdateFacturaDto
    {
        public string? Nombre { get; set; }
        public decimal? Precio { get; set; }
        public Dictionary<string, decimal>? Reparto { get; private set; } = new Dictionary<string, decimal>();
        public bool? Pagado { get; private set; }
        public byte[]? DocumentoUrl { get; private set; }
        public string? TareaId { get; private set; }
    }
}
