using Convivia.Application.Exceptions;
using Convivia.Infrastructure.Queues;
using Convivia.Shared.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace Convivia.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _env;
        private readonly IErrorQueue _errorQueue;
        private const string HeaderName = "X-Correlation-ID";

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment env, IErrorQueue errorQueue)
        {
            _next = next;
            _logger = logger;
            _env = env;
            _errorQueue = errorQueue ?? throw new ArgumentNullException(nameof(errorQueue));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            _logger.LogDebug("ExceptionHandlingMiddleware: Begin Invoke for {Method} {Path}", context.Request.Method, context.Request.Path);

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExceptionHandlingMiddleware: Caught exception for {Method} {Path}", context.Request.Method, context.Request.Path);

                // Obtener correlation id consistente (prioriza Items, luego header, luego genera uno nuevo)
                var correlationId = GetCorrelationId(context);
                context.Response.Headers[HeaderName] = correlationId;

                int status;
                object problemDetails;

                var stackTrace = _env.IsDevelopment() ? ex.ToString() : null; // ex.ToString() incluye mensaje + stack

                switch (ex)
                {
                    case ValidationException vex:
                        status = StatusCodes.Status400BadRequest;
                        problemDetails = CreateProblemDetails(
                            type: "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                            title: "Validation error",
                            status: status,
                            detail: vex.Message,
                            correlationId: correlationId,
                            errors: vex.Errors,
                            includeStack: _env.IsDevelopment(),
                            stack: stackTrace);
                        await EnqueueErrorRecordAsync(vex, correlationId, status, context, validationErrors: vex.Errors);
                        break;

                    case NotFoundException nfex:
                        status = StatusCodes.Status404NotFound;
                        problemDetails = CreateProblemDetails(
                            type: "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                            title: nfex.Message,
                            status: status,
                            detail: nfex.Message,
                            correlationId: correlationId,
                            entity: nfex.Entity,
                            key: nfex.Key?.ToString(),
                            includeStack: _env.IsDevelopment(),
                            stack: stackTrace);
                        await EnqueueErrorRecordAsync(nfex, correlationId, status, context, entity: nfex.Entity, key: nfex.Key?.ToString());
                        break;

                    case DomainException dex:
                        status = StatusCodes.Status409Conflict;
                        problemDetails = CreateProblemDetails(
                            type: "https://tools.ietf.org/html/rfc9110#section-15.5.8",
                            title: dex.Message ?? "Domain rule violation",
                            status: status,
                            detail: dex.Message,
                            correlationId: correlationId,
                            includeStack: _env.IsDevelopment(),
                            stack: stackTrace);
                        await EnqueueErrorRecordAsync(dex, correlationId, status, context);
                        break;

                    default:
                        status = StatusCodes.Status500InternalServerError;
                        problemDetails = CreateProblemDetails(
                            type: "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                            title: "An unexpected error occurred",
                            status: status,
                            detail: ex.Message,
                            correlationId: correlationId,
                            includeStack: _env.IsDevelopment(),
                            stack: stackTrace);
                        await EnqueueErrorRecordAsync(ex, correlationId, status, context);
                        break;
                }

                // Log antes de escribir la respuesta para confirmar el id usado
                _logger.LogDebug("ExceptionHandlingMiddleware - responding with correlationId: {CorrelationId} Status: {Status}", correlationId, status);

                context.Response.ContentType = "application/problem+json";
                context.Response.StatusCode = status;
                var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var json = JsonSerializer.Serialize(problemDetails, jsonOptions);
                await context.Response.WriteAsync(json);
            }
        }

        private static string GetCorrelationId(HttpContext context)
        {
            // 1. Intentar leer desde Items 
            if (context.Items.TryGetValue("CorrelationId", out var v) && v is string s && !string.IsNullOrWhiteSpace(s))
                return s;

            // 2. Intentar leer desde header
            if (context.Request.Headers.TryGetValue(HeaderName, out var header) && !string.IsNullOrWhiteSpace(header))
                return header.ToString();

            // 3. Generar nuevo si no hay ninguno
            return Guid.NewGuid().ToString();
        }

        private static object CreateProblemDetails(string type, string title, int status, string? detail, string correlationId,
            IReadOnlyDictionary<string, string[]>? errors = null, string? entity = null, string? key = null, bool includeStack = false, string? stack = null)
        {
            var baseObj = new Dictionary<string, object?>
            {
                ["type"] = type,
                ["title"] = title,
                ["status"] = status,
                ["detail"] = detail,
                ["correlationId"] = correlationId
            };

            if (errors != null) baseObj["errors"] = errors;
            if (!string.IsNullOrEmpty(entity)) baseObj["entity"] = entity;
            if (!string.IsNullOrEmpty(key)) baseObj["key"] = key;
            if (includeStack) baseObj["stack"] = stack;

            return baseObj;
        }


        private async Task EnqueueErrorRecordAsync(Exception ex, string correlationId, int status, HttpContext context,
            IReadOnlyDictionary<string, string[]>? validationErrors = null, string? entity = null, string? key = null)
        {
            try
            {
                var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

                var errorRecord = new ErrorRecord
                {
                    CorrelationId = correlationId,
                    TraceId = traceId,
                    Status = status,
                    Message = ex.Message,
                    Route = $"{context.Request.Method} {context.Request.Path}",
                    TimestampUtc = DateTime.UtcNow,
                    Stack = ex.StackTrace,
                    ValidationErrors = validationErrors,
                    Entity = entity,
                    Key = key
                };

                // Intentar encolar de forma no bloqueante para no afectar la latencia de la petición.
                // Si la cola está llena, registramos advertencia y no bloqueamos.
                try
                {
                    if (!_errorQueue.TryEnqueue(errorRecord))
                    {
                        _logger.LogWarning("Error queue full, dropping error CorrelationId={CorrelationId} Status={Status} Route={Route}",
                            correlationId, status, errorRecord.Route);
                    }
                    else
                    {
                        _logger.LogInformation("Enqueued error CorrelationId={CorrelationId} Status={Status} Route={Route}", correlationId, status, errorRecord.Route);
                    }
                }
                catch (Exception qex)
                {
                    // En caso de fallo inesperado al intentar TryEnqueue, intentamos EnqueueAsync como último recurso (no bloqueante si cancellation requested)
                    _logger.LogWarning(qex, "TryEnqueue failed, attempting EnqueueAsync for CorrelationId={CorrelationId}", correlationId);
                    try
                    {
                        await _errorQueue.EnqueueAsync(errorRecord, context.RequestAborted);
                        _logger.LogInformation("Enqueued error (async fallback) CorrelationId={CorrelationId} Status={Status} Route={Route}", correlationId, status, errorRecord.Route);
                    }
                    catch (Exception enqueueEx)
                    {
                        _logger.LogError(enqueueEx, "Failed to enqueue ErrorRecord CorrelationId={CorrelationId}", correlationId);
                    }
                }
            }
            catch (Exception enqueueEx)
            {
                _logger.LogError(enqueueEx, "Failed to build or enqueue ErrorRecord CorrelationId={CorrelationId}", correlationId);
            }
        }
    }
}
