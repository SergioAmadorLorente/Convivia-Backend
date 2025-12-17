using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Convivia.Domain.Entities;
using Convivia.Shared.DTOs;

namespace Convivia.Application.Repositories
{
    public interface IPeticionRepository : IRepository<Peticion>
    {
        Task<IEnumerable<Peticion>> GetByEstadoAsync(string estado, CancellationToken ct = default);
    }
}
