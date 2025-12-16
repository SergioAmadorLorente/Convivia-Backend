using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Convivia.Shared.DTOs;

namespace Convivia.Shared.Repositories
{
    public interface IPermisoRepository
    {
        Task<PermisoDto?> GetByIdAsync(string id, CancellationToken ct = default);
        Task<IEnumerable<PermisoDto>> GetByRolAsync(TipoRol rol, CancellationToken ct = default);
        Task<IEnumerable<PermisoDto>> GetAllAsync(CancellationToken ct = default);
        Task<string> AddAsync(PermisoDto permiso, CancellationToken ct = default);
        Task UpdateAsync(string id, PermisoDto permiso, CancellationToken ct = default);
        Task DeleteAsync(string id, CancellationToken ct = default);
    }
}
