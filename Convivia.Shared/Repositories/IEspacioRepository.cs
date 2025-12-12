using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Convivia.Shared.DTOs;

namespace Convivia.Shared.Repositories
{
    public interface IEspacioRepository
    {
        Task<EspacioDto?> GetByIdAsync(string id, CancellationToken ct = default);
        Task<IEnumerable<EspacioDto>> GetByDireccionAsync(string Direccion, CancellationToken ct = default);
        Task<string> AddAsync(EspacioDto espacio, CancellationToken ct = default);
        Task UpdateAsync(string id, EspacioDto espacio, CancellationToken ct = default);
        Task DeleteAsync(string id, CancellationToken ct = default);
        Task AddUsuarioEspacioIdAsync(string espacioId, string usuarioEspacioId, CancellationToken ct = default);
    }
}