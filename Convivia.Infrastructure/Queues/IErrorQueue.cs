using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Convivia.Shared.Contracts;

namespace Convivia.Infrastructure.Queues
{
    public interface IErrorQueue
    {
        ChannelReader<ErrorRecord> Reader { get; }
        bool TryEnqueue(ErrorRecord record);
        Task EnqueueAsync(ErrorRecord record, CancellationToken cancellationToken = default);
        void Complete();
    }
}
