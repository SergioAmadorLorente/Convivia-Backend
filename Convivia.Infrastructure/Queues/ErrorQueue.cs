using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Convivia.Shared.Contracts;

namespace Convivia.Infrastructure.Queues
{
    public class ErrorQueue : IErrorQueue
    {
        private readonly Channel<ErrorRecord> _channel;

        public ErrorQueue(int capacity = 1000)
        {
            if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            var options = new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleReader = true,
                SingleWriter = false
            };
            _channel = Channel.CreateBounded<ErrorRecord>(options);
        }

        public ChannelReader<ErrorRecord> Reader => _channel.Reader;

        /// <summary>
        /// Intenta encolar sin bloquear. Devuelve false si la cola está llena.
        /// </summary>
        public bool TryEnqueue(ErrorRecord record)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            return _channel.Writer.TryWrite(record);
        }

        /// <summary>
        /// Encola de forma asíncrona y devuelve un Task que completa cuando se escribe.
        /// Respeta el cancellationToken.
        /// </summary>
        public Task EnqueueAsync(ErrorRecord record, CancellationToken cancellationToken = default)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            // WriteAsync devuelve ValueTask; lo convertimos a Task para cumplir la interfaz
            return _channel.Writer.WriteAsync(record, cancellationToken).AsTask();
        }

        /// <summary>
        /// Marca la cola como completada para nuevos escritores.
        /// </summary>
        public void Complete() => _channel.Writer.Complete();
    }
}
