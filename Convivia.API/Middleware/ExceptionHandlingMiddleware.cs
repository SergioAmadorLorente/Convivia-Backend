using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Convivia.Shared.Contracts; 

namespace Convivia.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                // Obtener correlationId y traceId
                var correlationId = context.Items["CorrelationId"]?.ToString() ?? "none";
                var traceId = Activity.Current?.TraceId.ToString();

                // Log estructurado con correlationId y traceId
                _logger.LogError(ex, "Unhandled exception. CorrelationId: {CorrelationId} TraceId: {TraceId}", correlationId, traceId);

                // Construir respuesta uniforme usando tu DTO
                var error = new ErrorResponse
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Internal Server Error",
                    Detail = _env.IsDevelopment() ? ex.ToString() : "An unexpected error occurred.",
                    CorrelationId = correlationId,
                    Instance = context.Request.Path
                };

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var json = JsonSerializer.Serialize(error, options);
                await context.Response.WriteAsync(json);

                // Opcional: encolar o enviar a Firestore de forma asíncrona (no bloqueante)
            }
        }
    }
}
