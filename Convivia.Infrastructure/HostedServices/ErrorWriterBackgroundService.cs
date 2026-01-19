using Convivia.Infrastructure.Models;
using Convivia.Infrastructure.Queues;
using Convivia.Infrastructure.Repositories;
using Convivia.Shared.Contracts;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

public class ErrorWriterBackgroundService : BackgroundService
{
    private readonly IErrorQueue _errorQueue;
    private readonly IMapper _mapper; // Mapster IMapper
    private readonly IErrorRepository _repository; // interfaz para persistir en Firestore
    private readonly ILogger<ErrorWriterBackgroundService> _logger;
    private const int MaxStackLength = 10000;
    private const int MaxValidationEntries = 200; // ejemplo de límite

    public ErrorWriterBackgroundService(
        IErrorQueue errorQueue,
        IMapper mapper,
        IErrorRepository repository,
        ILogger<ErrorWriterBackgroundService> logger)
    {
        _errorQueue = errorQueue ?? throw new ArgumentNullException(nameof(errorQueue));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ErrorWriterBackgroundService started.");

        await foreach (var record in _errorQueue.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                // Mapear a DTO para saneado intermedio
                var dto = _mapper.Map<ErrorRecordDto>(record);

                // Saneado: truncar stack
                if (!string.IsNullOrEmpty(dto.Stack) && dto.Stack.Length > MaxStackLength)
                    dto.Stack = dto.Stack.Substring(0, MaxStackLength);

                // Saneado: ValidationErrors -> Dictionary y limitar número de entradas
                Dictionary<string, string[]>? validation = null;
                if (dto.ValidationErrors != null)
                {
                    var entries = dto.ValidationErrors.Take(MaxValidationEntries);
                    validation = entries.ToDictionary(kv => kv.Key, kv => SanitizeValues(kv.Value));
                    dto.ValidationErrors = validation;
                }

                // Mapear DTO saneado de vuelta al contrato ErrorRecord
                var contractRecord = _mapper.Map<ErrorRecord>(dto);

                // Persistir con reintentos exponenciales simples
                await PersistWithRetryAsync(contractRecord, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // shutdown
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process ErrorRecord from queue. CorrelationId={CorrelationId}", record?.CorrelationId);
            }
        }

        _logger.LogInformation("ErrorWriterBackgroundService stopping.");
    }

    private static string[] SanitizeValues(string[] values)
    {
        if (values == null) return Array.Empty<string>();
        // Truncado simple por valor; aquí puedes añadir enmascarado de PII si procede
        return values.Select(v => string.IsNullOrEmpty(v) ? v : (v.Length > 1000 ? v.Substring(0, 1000) : v)).ToArray();
    }

    private async Task PersistWithRetryAsync(ErrorRecord record, CancellationToken cancellationToken)
    {
        const int maxAttempts = 5;
        var delay = TimeSpan.FromSeconds(1);

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await _repository.SaveAsync(record, cancellationToken);
                _logger.LogInformation("Persisted error record CorrelationId={CorrelationId}", record.CorrelationId);
                return;
            }
            catch (Exception ex) when (IsTransient(ex) && attempt < maxAttempts)
            {
                _logger.LogWarning(ex, "Transient error persisting record. Attempt {Attempt}/{MaxAttempts}. Retrying in {Delay}s. CorrelationId={CorrelationId}",
                    attempt, maxAttempts, delay.TotalSeconds, record.CorrelationId);
                await Task.Delay(delay, cancellationToken);
                delay = delay * 2;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Permanent error persisting record. CorrelationId={CorrelationId}", record.CorrelationId);
                return;
            }
        }
    }

    private bool IsTransient(Exception ex)
    {
        // Implementa heurística para errores transitorios (timeouts, 5xx, Firestore transient exceptions)
        // Ejemplo simplificado: tratar cualquier excepción como transitoria si no es de validación
        return true;
    }
}
