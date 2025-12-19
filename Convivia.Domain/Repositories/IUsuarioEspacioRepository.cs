using Convivia.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Convivia.Domain.Repositories
{
    /// <summary>
    /// Repositorio para operaciones CRUD de UsuarioEspacio
    /// </summary>
    public interface IUsuarioEspacioRepository
    {
        /// <summary>
        /// Obtener un UsuarioEspacio por su ID
        /// </summary>
        Task<UsuarioEspacio?> GetByIdAsync(string usuarioEspacioId, CancellationToken ct = default);

        /// <summary>
        /// Actualizar un UsuarioEspacio (aumentar karma, cambiar estado, etc.)
        /// </summary>
        Task UpdateAsync(string usuarioEspacioId, UsuarioEspacio usuarioEspacio, CancellationToken ct = default);

        /// <summary>
        /// Aumentar karma de un UsuarioEspacio
        /// </summary>
        Task<int> UpdateKarmaAsync(string usuarioEspacioId, int karmaAmount, CancellationToken ct = default);
    }
}
