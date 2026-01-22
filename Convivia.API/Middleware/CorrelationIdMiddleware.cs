using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

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

            // Validar GUID si viene, si no generar uno nuevo
            string correlationId;
            if (!string.IsNullOrWhiteSpace(incoming) && Guid.TryParse(incoming, out _))
            {
                correlationId = incoming;
            }
            else
            {
                correlationId = Guid.NewGuid().ToString();
            }

            // Guardarlo en HttpContext.Items para que otros componentes lo lean
            context.Items["CorrelationId"] = correlationId;

            // Asegurar header de respuesta de forma idempotente
            context.Response.OnStarting(() =>
            {
                context.Response.Headers[HeaderName] = correlationId;
                return Task.CompletedTask;
            });

            // Propagar a Activity (OpenTelemetry/diagnostics) si existe
            if (Activity.Current != null)
            {
                Activity.Current.SetTag("correlation_id", correlationId);
                Activity.Current.AddBaggage("correlation_id", correlationId);
            }

            // Abrir scope de logging para que ILogger incluya la propiedad
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
