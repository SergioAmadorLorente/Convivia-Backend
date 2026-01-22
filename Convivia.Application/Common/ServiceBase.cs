using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Convivia.Shared.Correlation;

namespace Convivia.Application.Common
{
    public abstract class ServiceBase
    {
        protected readonly ILogger _logger;
        protected readonly ICorrelationProvider _correlation;

        protected ServiceBase(ILogger logger, ICorrelationProvider correlation)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _correlation = correlation ?? throw new ArgumentNullException(nameof(correlation));
        }

        protected IDisposable BeginCorrelationScope()
        {
            var props = new Dictionary<string, object>
            {
                ["CorrelationId"] = _correlation.CorrelationId,
                ["TraceId"] = _correlation.TraceId
            };
            return _logger.BeginScope(props);
        }
    }
}
