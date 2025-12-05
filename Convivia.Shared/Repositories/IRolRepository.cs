using Convivia.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convivia.Shared.Repositories
{
    public interface IRolRepository
    {
        Task<RolDto?> GetByIdAsync(string id, CancellationToken ct = default);
        Task<IEnumerable<RolDto>> GetAllAsync(CancellationToken ct = default);
        Task<RolDto?> GetByNombreAsync(string nombre, CancellationToken ct = default);
        Task<string> AddAsync(RolDto rol, CancellationToken ct = default);
        Task UpdateAsync(string id, RolDto rol, CancellationToken ct = default);
        Task DeleteAsync(string id, CancellationToken ct = default);
    }
}
