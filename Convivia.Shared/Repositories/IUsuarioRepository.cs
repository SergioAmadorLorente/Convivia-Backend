using Convivia.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convivia.Shared.Repositories
{
    public interface IUsuarioRepository
    {
        Task<UsuarioDto?> GetByIdAsync(string id, CancellationToken ct = default);
        Task<IEnumerable<UsuarioDto>> GetByFullNameInvitadoAsync(string Nombre, CancellationToken ct = default);
        Task<string> AddAsync(UsuarioDto usuario, CancellationToken ct = default);
        Task UpdateAsync(string id, UsuarioDto usuario, CancellationToken ct = default);
        Task DeleteAsync(string id, CancellationToken ct = default);
        Task AddUsuarioEspacioIdAsync(string usuarioId, string usuarioEspacioId, CancellationToken ct = default);
    }
}
