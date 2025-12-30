using Convivia.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Convivia.Domain.Entities;
namespace Convivia.Shared.Repositories
{
    public interface IRolRepository
    {
        Task<IEnumerable<Rol>> GetAllAsync(CancellationToken ct = default);
        Task<Rol?> GetByNombreAsync(TipoRol nombre, CancellationToken ct = default);
    }
}
