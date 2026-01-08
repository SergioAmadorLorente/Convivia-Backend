using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convivia.Shared.DTOs
{
    public class ReservaDto
    {
        public string Id { get; set; } = string.Empty;
        public string? description { get; set; }
        public DateTime startTime { get; set; } = DateTime.Now;
        public DateTime? endTime { get; set; }
        public string idSala { get; set; } = string.Empty;
        public string idUser { get; set; } = string.Empty;
    }
}
