using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Convivia.Shared.DTOs;

namespace Convivia.Shared.Repositories
{
    public interface IPermisoRepository
    {
        Task<IEnumerable<PermisoDto>> GetByRolAsync(TipoRol rol, CancellationToken ct = default);
    }
}
