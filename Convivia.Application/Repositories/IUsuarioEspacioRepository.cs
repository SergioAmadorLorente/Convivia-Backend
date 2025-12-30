using Convivia.Application.Repositories;
using Convivia.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Convivia.Application.Repositories
{
    public interface IUsuarioEspacioRepository : IRepository<UsuarioEspacio>
    {
        Task<IEnumerable<UsuarioEspacio>> GetByEspacioIdAsync(string espacioId, CancellationToken ct = default);
        Task<IEnumerable<UsuarioEspacio>> GetByUsuarioIdAsync(string usuarioId, CancellationToken ct = default);
        Task<IEnumerable<UsuarioEspacio>> GetByPermisoIdAsync(string permisoId, CancellationToken ct = default);
    }
}
