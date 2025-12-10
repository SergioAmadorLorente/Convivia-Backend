using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convivia.Shared.DTOs
{
    public class UpdateReservaDto
    {
        public string? description { get; set; }
        public DateTime? startTime { get; set; }
        public DateTime? endTime { get; set; }
        public string? idSala { get; set; }
        public string? idUser { get; set; }

    }
}
