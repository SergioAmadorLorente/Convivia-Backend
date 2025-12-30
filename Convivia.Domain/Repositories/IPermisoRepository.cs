using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Convivia.Shared.DTOs;
using Convivia.Domain.Entities;
namespace Convivia.Shared.Repositories
{
    public interface IPermisoRepository
    {
        Task<IEnumerable<Permiso>> GetByRolAsync(TipoRol rol, CancellationToken ct = default);
    }
}
