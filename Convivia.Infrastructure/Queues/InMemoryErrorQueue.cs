using System.Threading.Channels;
using Convivia.Shared.Contracts;

namespace Convivia.Infrastructure.Queues
{
    public class InMemoryErrorQueue : IErrorQueue
    {
        private readonly Channel<ErrorRecord> _channel;

        public InMemoryErrorQueue(int? capacity = null)
        {
            // Si se pasa capacity, usamos bounded channel para backpressure; si no, unbounded.
            _channel = capacity.HasValue
                ? Channel.CreateBounded<ErrorRecord>(new BoundedChannelOptions(capacity.Value)
                {
                    SingleReader = true,
                    SingleWriter = false,
                    FullMode = BoundedChannelFullMode.DropOldest // política; se puede ajustar
                })
                : Channel.CreateUnbounded<ErrorRecord>(new UnboundedChannelOptions
                {
                    SingleReader = true,
                    SingleWriter = false
                });
        }

        public ChannelReader<ErrorRecord> Reader => _channel.Reader;

        public void Enqueue(ErrorRecord record)
        {
            // TryWrite es no bloqueante; si falla (bounded y lleno) se puede decidir política
            if (!_channel.Writer.TryWrite(record))
            {
                // Política por defecto: intentar escribir de forma asíncrona sin bloquear el hilo llamante
                // (esto raramente se ejecutará si usamos unbounded channel).
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _channel.Writer.WriteAsync(record);
                    }
                    catch
                    {
                        // Si falla la escritura, no queremos lanzar desde el middleware.
                        // Aquí podrías incrementar una métrica o contar pérdidas.
                    }
                });
            }
        }
    }
}
