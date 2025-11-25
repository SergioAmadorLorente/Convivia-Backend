using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Convivia.Shared.DTOs;

namespace Convivia.Shared.Repositories
{
    public interface ISalaRepository
    {
        Task<SalaDto?> GetByIdAsync(string id, CancellationToken ct = default);
        Task<IEnumerable<SalaDto>> GetByEspacioIdAsync(string espacioId, CancellationToken ct = default);
        Task<string> AddAsync(SalaDto sala, CancellationToken ct = default);
        Task UpdateAsync(string id, SalaDto sala, CancellationToken ct = default);
        Task DeleteAsync(string id, CancellationToken ct = default);
    }
}
