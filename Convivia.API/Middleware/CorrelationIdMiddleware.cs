using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog.Context; 

namespace Convivia.API.Middleware
{
    public class CorrelationIdMiddleware
    {
        private const string HeaderName = "X-Correlation-ID";
        private readonly RequestDelegate _next;
        private readonly ILogger<CorrelationIdMiddleware> _logger;

        public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            context.Request.Headers.TryGetValue(HeaderName, out var incomingValues);
            var incoming = incomingValues.ToString();

            string correlationId = !string.IsNullOrWhiteSpace(incoming) && Guid.TryParse(incoming, out _)
                ? incoming
                : Guid.NewGuid().ToString();

            // Exponer inmediatamente para que otros middlewares lo lean
            context.Items["CorrelationId"] = correlationId;

            // Establecer TraceIdentifier para que frameworks/SerilogRequestLogging lo puedan usar
            context.TraceIdentifier = correlationId;

            // Poner cabecera de respuesta de forma inmediata (no depender de OnStarting)
            context.Response.Headers[HeaderName] = correlationId;

            if (Activity.Current != null)
            {
                Activity.Current.SetTag("correlation_id", correlationId);
                Activity.Current.AddBaggage("correlation_id", correlationId);
            }

            // Empujar la propiedad al LogContext de Serilog y mantener BeginScope para ILogger
            using (LogContext.PushProperty("CorrelationId", correlationId))
            using (_logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
            {
                await _next(context);
            }
        }

    }

    public static class CorrelationIdMiddlewareExtensions
    {
        public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
        {
            return app.UseMiddleware<CorrelationIdMiddleware>();
        }
    }
}
