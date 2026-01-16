using System.Diagnostics;
using System.Threading.Tasks;
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
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Obtener o generar correlation id
            var correlationId = context.Request.Headers.ContainsKey(HeaderName) &&
                                !string.IsNullOrWhiteSpace(context.Request.Headers[HeaderName])
                ? context.Request.Headers[HeaderName].ToString()
                : Guid.NewGuid().ToString();

            // Guardarlo en HttpContext.Items para que otros componentes lo lean
            context.Items["CorrelationId"] = correlationId;

            // Añadir cabecera en la respuesta
            context.Response.OnStarting(() =>
            {
                if (!context.Response.Headers.ContainsKey(HeaderName))
                    context.Response.Headers.Add(HeaderName, correlationId);
                return Task.CompletedTask;
            });

            // Incluir en Activity (trace) si existe
            if (Activity.Current != null && !string.IsNullOrEmpty(Activity.Current.TraceId.ToString()))
            {
                Activity.Current.SetParentId(Activity.Current.Id);
            }

            // Abrir scope de logging para que ILogger incluya la propiedad
            using (_logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
            {
                await _next(context);
            }
        }
    }

    // Extensión para registrar el middleware de forma fluida
    public static class CorrelationIdMiddlewareExtensions
    {
        public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
        {
            return app.UseMiddleware<CorrelationIdMiddleware>();
        }
    }
}
