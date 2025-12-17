using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Convivia.Domain.Entities;
using Convivia.Shared.DTOs;

namespace Convivia.Application.Repositories
{
    public interface IPermisoRepository : IRepository<Permiso>
    {
        Task<IEnumerable<Permiso>> GetByRolAsync(string rol, CancellationToken ct = default);
    }
}
