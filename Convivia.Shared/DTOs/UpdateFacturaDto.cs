using Convivia.Shared.DTOs;
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
        public Dictionary<string, float>? Reparto { get; set; } = new Dictionary<string, float>();
        public bool? Pagado { get; set; }
        public byte[]? DocumentoUrl { get; set; }
        public string? TareaId { get; set; }
    }
}