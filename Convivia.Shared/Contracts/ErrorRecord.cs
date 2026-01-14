using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convivia.Shared.Contracts
{
    public class ErrorRecord
    {
        public string CorrelationId { get; set; }
        public string TraceId { get; set; }
        public int Status { get; set; }
        public string Message { get; set; }
        public string Route { get; set; }
        public DateTime TimestampUtc { get; set; }
        public string Stack { get; set; }
    }
}
