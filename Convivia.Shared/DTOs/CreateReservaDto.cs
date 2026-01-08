using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convivia.Shared.DTOs
{
    public class CreateReservaDto
    {
        public string? description { get; set; }

        public DateTime startDate { get; set; } = DateTime.Now;

        public DateTime? endDate { get; set; }

        public string idSala { get; set; }

        public string idUser { get; set; }
    }
}
