using Microsoft.AspNetCore.Http;
using System;
using System.Diagnostics;
using Convivia.Shared.Correlation;

namespace Convivia.Infrastructure.Correlation
{
    public class CorrelationProvider : ICorrelationProvider
    {
        private readonly IHttpContextAccessor _http;

        public CorrelationProvider(IHttpContextAccessor http)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
        }

        public string CorrelationId
        {
            get
            {
                var ctx = _http?.HttpContext;
                if (ctx == null) return Guid.NewGuid().ToString();

                if (ctx.Items.TryGetValue("CorrelationId", out var v))
                {
                    var s = v as string;
                    if (!string.IsNullOrWhiteSpace(s)) return s;
                }

                if (ctx.Request?.Headers != null && ctx.Request.Headers.TryGetValue("X-Correlation-ID", out var h))
                {
                    var headerValue = h.ToString();
                    if (!string.IsNullOrWhiteSpace(headerValue)) return headerValue;
                }

                return Guid.NewGuid().ToString();
            }
        }

        public string TraceId => Activity.Current?.Id ?? _http?.HttpContext?.TraceIdentifier ?? string.Empty;
    }
}
