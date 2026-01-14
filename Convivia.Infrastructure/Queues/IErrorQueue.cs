using System.Threading.Channels;
using Convivia.Shared.Contracts;

namespace Convivia.Infrastructure.Queues
{
    public interface IErrorQueue
    {
        /// <summary>
        /// Encola un ErrorRecord de forma no bloqueante.
        /// </summary>
        void Enqueue(ErrorRecord record);

        /// <summary>
        /// Exponer el ChannelReader para que el BackgroundService consuma los registros.
        /// </summary>
        ChannelReader<ErrorRecord> Reader { get; }
    }
}
