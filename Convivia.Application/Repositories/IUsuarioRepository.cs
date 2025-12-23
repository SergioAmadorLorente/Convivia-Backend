using Convivia.Domain.Entities;
using Convivia.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convivia.Application.Repositories
{
    public interface IUsuarioRepository : IRepository<Usuario>
    {
        Task<UsuarioDto?> GetByEmailAsync(string email, CancellationToken ct = default);
        Task AddUsuarioEspacioIdAsync(string espacioId, string usuarioEspacioId, CancellationToken ct = default);

    }
}
