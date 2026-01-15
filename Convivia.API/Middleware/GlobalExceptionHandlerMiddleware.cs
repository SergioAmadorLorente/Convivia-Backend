using System.Net;
using System.Text.Json;

namespace Convivia.API.Middleware
{
    /// <summary>
    /// Middleware global que captura excepciones no manejadas y las convierte en respuestas HTTP apropiadas.
    /// - ArgumentNullException ? 400 BadRequest
    /// - ArgumentException ? 400 BadRequest
    /// - InvalidOperationException ? 400 BadRequest
    /// - Otras excepciones ? 500 InternalServerError
    /// </summary>
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = new ErrorResponse();

            // IMPORTANTE: Chequear subclases primero antes que clases base
            switch (exception)
            {
                // ArgumentNullException es un subtipo de ArgumentException, así que debe ir primero
                case ArgumentNullException nullEx:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.StatusCode = 400;
                    response.Message = $"El campo '{nullEx.ParamName}' es obligatorio.";
                    response.ErrorType = "ValidationError";
                    break;

                case ArgumentException argEx:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.StatusCode = 400;
                    response.Message = argEx.Message;
                    response.ErrorType = "ValidationError";
                    break;

                case InvalidOperationException invEx:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.StatusCode = 400;
                    response.Message = invEx.Message;
                    response.ErrorType = "ValidationError";
                    break;

                // Error interno del servidor (500)
                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response.StatusCode = 500;
                    response.Message = "Ha ocurrido un error interno del servidor.";
                    response.ErrorType = "InternalServerError";
                    break;
            }

            var jsonResponse = JsonSerializer.Serialize(response);
            return context.Response.WriteAsync(jsonResponse);
        }
    }

    /// <summary>
    /// Estructura de respuesta de error estandarizada.
    /// </summary>
    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ErrorType { get; set; } = "Error";
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
