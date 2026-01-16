using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convivia.Shared.Contracts
{
    // DTO para todas las respuestas de error
    public class ErrorResponse
    {
        public int Status { get; set; }
        public string Title { get; set; }
        public string Detail { get; set; }
        public string CorrelationId { get; set; }
        public string Instance { get; set; }
    }
}
