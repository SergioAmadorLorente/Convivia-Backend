using Convivia.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Convivia.Application.Repositories
{
    public interface ISalaRepository : IRepository<Sala>
    {
        Task<IEnumerable<Sala>> GetByEspacioIdAsync(string espacioId, CancellationToken ct = default);
    }
}