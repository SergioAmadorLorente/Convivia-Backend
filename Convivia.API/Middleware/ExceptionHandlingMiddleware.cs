using Convivia.Shared.Contracts; 
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Convivia.Infrastructure.Queues;
using System.Diagnostics;
using System.Text.Json;

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

                _logger.LogDebug("Enqueuing error for CorrelationId {CorrelationId}", correlationId);

                // Construir ErrorRecord para persistencia asíncrona
                var record = new ErrorRecord
                {
                    CorrelationId = correlationId,
                    TraceId = traceId,
                    Status = StatusCodes.Status500InternalServerError,
                    Message = ex.Message,
                    Route = context.Request.Path,
                    TimestampUtc = DateTime.UtcNow,
                    Stack = _env.IsDevelopment() ? ex.ToString() : null,
                };

                // Obtener la cola desde DI y encolar sin bloquear
                try
                {
                    var queue = context.RequestServices.GetService(typeof(IErrorQueue)) as IErrorQueue;
                    if (queue != null)
                    {
                        queue.Enqueue(record);
                        _logger.LogDebug("Error enqueued for CorrelationId {CorrelationId}", correlationId);
                    }
                    else
                    {
                        _logger.LogWarning("IErrorQueue not registered; error not enqueued. CorrelationId {CorrelationId}", correlationId);
                    }
                }
                catch (Exception qex)
                {
                    // No queremos que la encolación rompa la respuesta al cliente
                    _logger.LogWarning(qex, "Failed to enqueue error for CorrelationId {CorrelationId}", correlationId);
                }

            }
        }
    }
}
