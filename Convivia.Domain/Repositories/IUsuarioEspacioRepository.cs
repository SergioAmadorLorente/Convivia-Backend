using Convivia.Infrastructure.Repositories;
using Convivia.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Convivia.Domain.Repositories
{
    public interface IUsuarioEspacioRepository : IRepository<UsuarioEspacio>
    {
        Task<IEnumerable<UsuarioEspacio>> GetByEspacioIdAsync(string espacioId, CancellationToken ct = default);
        Task<IEnumerable<UsuarioEspacio>> GetByUsuarioIdAsync(string usuarioId, CancellationToken ct = default);
    }
}
