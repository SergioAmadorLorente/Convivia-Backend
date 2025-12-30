using Convivia.Domain.Entities;
using Convivia.Application.Repositories;
using Convivia.Infrastructure.Models;
using Convivia.Shared.Services;
using Mapster;
using Microsoft.Extensions.Logging;

namespace Convivia.Infrastructure.Repositories
{
    public class UsuarioEspacioRepository : Repository<FireStoreUsuarioEspacio>, IUsuarioEspacioRepository
    {
        private readonly ILogger<UsuarioEspacioRepository> _logger;
        private const string Collection = "usuarioEspacios";

        public UsuarioEspacioRepository(
            IFirebaseService firebase,
            ILogger<UsuarioEspacioRepository> logger,
            ILoggerFactory loggerFactory)
            : base(firebase, loggerFactory.CreateLogger<Repository<FireStoreUsuarioEspacio>>(), Collection)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> AddAsync(UsuarioEspacio entity, CancellationToken ct = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var persist = entity.Adapt<FireStoreUsuarioEspacio>();
            return await base.AddAsync(persist, persist.Id, ct);
        }

        public async Task<UsuarioEspacio?> GetByIdAsync(string id, CancellationToken ct = default)
        {
            var persist = await base.GetByIdAsync(id, ct);
            if (persist == null) return null;
            return persist.Adapt<UsuarioEspacio>();
        }

        public async Task<IEnumerable<UsuarioEspacio>> GetAllAsync(CancellationToken ct = default)
        {
            var list = await base.GetAllAsync(ct);
            return list == null ? Array.Empty<UsuarioEspacio>() : list.Adapt<List<UsuarioEspacio>>();
        }

        public async Task<IEnumerable<UsuarioEspacio>> GetByEspacioIdAsync(string espacioId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(espacioId)) return Array.Empty<UsuarioEspacio>();
            try
            {
                _logger.LogInformation("GetByEspacioIdAsync llamado para espacioId: {EspacioId}", espacioId);
                var list = await _firebase.QueryAsync<FireStoreUsuarioEspacio>(Collection, "EspacioRef", espacioId, ct);
                _logger.LogInformation("GetByEspacioIdAsync devolvió {Count} documentos para espacioId: {EspacioId}", list?.Count ?? 0, espacioId);
                return list?.Select(fs => fs.Adapt<UsuarioEspacio>()) ?? new List<UsuarioEspacio>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error GetByEspacioIdAsync {EspacioId}", espacioId);
                throw;
            }
        }

        public async Task<IEnumerable<UsuarioEspacio>> GetByUsuarioIdAsync(string usuarioId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(usuarioId)) return Array.Empty<UsuarioEspacio>();
            try
            {
                _logger.LogInformation("GetByUsuarioIdAsync llamado para usuarioId: {UsuarioId}", usuarioId);
                var list = await _firebase.QueryAsync<FireStoreUsuarioEspacio>(Collection, "UsuarioRef", usuarioId, ct);
                _logger.LogInformation("GetByUsuarioIdAsync devolvió {Count} documentos para usuarioId: {UsuarioId}", list?.Count ?? 0, usuarioId);
                return list?.Select(fs => fs.Adapt<UsuarioEspacio>()) ?? new List<UsuarioEspacio>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error GetByUsuarioIdAsync {UsuarioId}", usuarioId);
                throw;
            }
        }

        public async Task<IEnumerable<UsuarioEspacio>> GetByPermisoIdAsync(string permisoId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(permisoId)) return Array.Empty<UsuarioEspacio>();
            try
            {
                var list = await _firebase.QueryAsync<FireStoreUsuarioEspacio>(Collection, "PermisoRef", permisoId, ct);
                return list?.Select(fs => fs.Adapt<UsuarioEspacio>()) ?? new List<UsuarioEspacio>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error GetByPermisoIdAsync {PermisoId}", permisoId);
                throw;
            }
        }

        public async Task UpdateAsync(string id, UsuarioEspacio entity, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var persist = entity.Adapt<FireStoreUsuarioEspacio>();
            await base.UpdateAsync(id, persist, ct);
        }

        public async Task UpdateAsync(string id, UsuarioEspacio entity, bool merge, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var persist = entity.Adapt<FireStoreUsuarioEspacio>();
            await base.UpdateAsync(id, persist, merge, ct);
        }

        public async Task UpdateAsync(string id, IDictionary<string, object> updates, bool useSetMerge = true, CancellationToken ct = default)
        {
            await base.UpdateAsync(id, updates, useSetMerge, ct);
        }

        public new async Task DeleteAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            await base.DeleteAsync(id, ct);
        }
    }
}
