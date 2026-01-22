using System.Threading;
using System.Threading.Tasks;
using Convivia.Shared.Contracts;

namespace Convivia.Infrastructure.Repositories
{
    public interface IErrorRepository
    {
        Task SaveAsync(ErrorRecord record, CancellationToken cancellationToken = default);
    }
}
