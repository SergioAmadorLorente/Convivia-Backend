using Convivia.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Convivia.Application.Repositories
{
    public interface IKarmaEstadisticasRepository : IRepository<KarmaEstadisticas>
    {
        /// <summary>
        /// Obtiene las estadísticas de karma por EspacioId y UsuarioEspacioId
        /// </summary>
        Task<KarmaEstadisticas?> GetByUsuarioEspacioIdAsync(string espacioId, string usuarioEspacioId, CancellationToken ct = default);

        /// <summary>
        /// Suma karma a las estadísticas (actualiza total, semanal y mensual)
        /// </summary>
        Task<KarmaEstadisticas> AddKarmaAsync(string espacioId, string usuarioEspacioId, int karmaAmount, CancellationToken ct = default);

        /// <summary>
        /// Resta karma a las estadísticas (sin permitir valores negativos)
        /// </summary>
        Task<KarmaEstadisticas> SubtractKarmaAsync(string espacioId, string usuarioEspacioId, int karmaAmount, CancellationToken ct = default);

        /// <summary>
        /// Actualiza las estadísticas de karma
        /// </summary>
        Task UpdateAsync(string espacioId, string id, KarmaEstadisticas entity, CancellationToken ct = default);
        Task<string> AddAsync(String espacioId, KarmaEstadisticas entity, CancellationToken ct = default);
    }
}
