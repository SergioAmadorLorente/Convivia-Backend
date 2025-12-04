using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Convivia.Shared.DTOs;

namespace Convivia.Shared.Repositories
{
    public interface IPeticionRepository
    {
        Task<PeticionDto?> GetByIdAsync(string id, CancellationToken ct = default);
        Task<IEnumerable<PeticionDto>> GetByEstadoAsync(string estado, CancellationToken ct = default);
        Task<IEnumerable<PeticionDto>> GetAllAsync(CancellationToken ct = default);
        Task<string> AddAsync(PeticionDto peticion, CancellationToken ct = default);
        Task UpdateAsync(string id, PeticionDto peticion, CancellationToken ct = default);
        Task DeleteAsync(string id, CancellationToken ct = default);
    }
}
