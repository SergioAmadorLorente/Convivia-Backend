using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convivia.Infrastructure.Correlation
{
    public interface ICorrelationProvider
    {
        string CorrelationId { get; }
        string TraceId { get; }
    }

}
