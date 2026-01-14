using System;
using Convivia.Infrastructure.Queues;
using Convivia.Infrastructure.Models;
using Convivia.Shared.Contracts;
using Convivia.Shared.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace Convivia.Infrastructure.HostedServices
{
    public class ErrorWriterBackgroundService : BackgroundService
    {
        private readonly IErrorQueue _queue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ErrorWriterBackgroundService> _logger;
        private readonly AsyncRetryPolicy _retryPolicy;
        private readonly string _collection = "api-errors";
        private const int MaxStackLength = 2000; // evita guardar stacks excesivamente largos

        public ErrorWriterBackgroundService(IErrorQueue queue, IServiceScopeFactory scopeFactory, ILogger<ErrorWriterBackgroundService> logger)
        {
            _queue = queue;
            _scopeFactory = scopeFactory;
            _logger = logger;

            _retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(new[]
                {
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(5)
                }, (ex, ts, ctx) =>
                {
                    _logger.LogWarning(ex, "Retrying Firestore write after {Delay}", ts);
                });
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var record in _queue.Reader.ReadAllAsync(stoppingToken))
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var firebase = scope.ServiceProvider.GetRequiredService<IFirebaseService>();
                    var mapper = scope.ServiceProvider.GetRequiredService<MapsterMapper.IMapper>();

                    // Mapear con Mapster (configuración central) a DTO
                    var dto = mapper.Map<ErrorRecordDto>(record);

                    // Truncar stack en el DTO si hace falta (prevención temprana)
                    if (!string.IsNullOrEmpty(dto.Stack) && dto.Stack.Length > MaxStackLength)
                    {
                        dto.Stack = dto.Stack.Substring(0, MaxStackLength);
                    }

                    // Mapear DTO a modelo de persistencia específico de Firestore
                    var firestoreModel = mapper.Map<FireStoreErrorRecord>(dto);

                    // Asegurar timestamp y truncado final en el modelo de persistencia
                    if (firestoreModel.TimestampUtc == default) firestoreModel.TimestampUtc = DateTime.UtcNow;
                    if (!string.IsNullOrEmpty(firestoreModel.Stack) && firestoreModel.Stack.Length > MaxStackLength)
                    {
                        firestoreModel.Stack = firestoreModel.Stack.Substring(0, MaxStackLength);
                    }

                    // Persistir el modelo de Firestore (tipo con [FirestoreData])
                    await _retryPolicy.ExecuteAsync(ct => firebase.AddAsync(_collection, firestoreModel, ct), stoppingToken);

                    _logger.LogInformation("Persisted error to Firestore CorrelationId={CorrelationId}", record.CorrelationId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to persist error to Firestore CorrelationId={CorrelationId}", record.CorrelationId);
                }
            }
        }


        private static ErrorRecordDto MapToDto(ErrorRecord record)
        {
            // Normalizar timestamp y truncar stack si es necesario
            var timestamp = record.TimestampUtc == default ? DateTime.UtcNow : record.TimestampUtc;

            var stack = record.Stack;
            if (!string.IsNullOrEmpty(stack) && stack.Length > MaxStackLength)
            {
                stack = stack.Substring(0, MaxStackLength);
            }

            return new ErrorRecordDto
            {
                CorrelationId = record.CorrelationId ?? string.Empty,
                TraceId = record.TraceId,
                Status = record.Status,
                Message = record.Message,
                Route = record.Route,
                TimestampUtc = timestamp,
                Stack = stack
            };
        }
    }
}
