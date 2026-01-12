using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting; 
using Convivia.Infrastructure.Queues;
using Convivia.Shared.Contracts;

namespace Convivia.Infrastructure.HostedServices
{
    public class TestErrorConsumer : BackgroundService
    {
        private readonly IErrorQueue _queue;
        private readonly ILogger<TestErrorConsumer> _logger;

        public TestErrorConsumer(IErrorQueue queue, ILogger<TestErrorConsumer> logger)
        {
            _queue = queue;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var record in _queue.Reader.ReadAllAsync(stoppingToken))
            {
                // Aquí solo imprimimos para comprobar; luego lo reemplazaremos por escritura en Firestore
                _logger.LogInformation("TestErrorConsumer received error: CorrelationId={CorrelationId} Route={Route} Message={Message}",
                    record.CorrelationId, record.Route, record.Message);
                // Simular trabajo breve
                await Task.Delay(50, stoppingToken);
            }
        }
    }
}
