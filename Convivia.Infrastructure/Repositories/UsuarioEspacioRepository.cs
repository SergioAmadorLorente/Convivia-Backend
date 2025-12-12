using Convivia.Domain.Entities;
using Convivia.Domain.Repositories;
using Convivia.Infrastructure.Models;
using Convivia.Shared.Services;
using Mapster;
using Microsoft.Extensions.Logging;

namespace Convivia.Infrastructure.Repositories
{
    public class UsuarioEspacioRepository : IUsuarioEspacioRepository
    {
        private readonly IFirebaseService _firebase;
        private readonly ILogger<UsuarioEspacioRepository> _logger;
        private const string Collection = "usuarioEspacios";

        public UsuarioEspacioRepository(IFirebaseService firebase, ILogger<UsuarioEspacioRepository> logger)
        {
            _firebase = firebase ?? throw new ArgumentNullException(nameof(firebase));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<UsuarioEspacio?> AddAsync(UsuarioEspacio entity, CancellationToken ct = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var fireStoreUsuarioEspacio = entity.Adapt<FireStoreUsuarioEspacio>();
            await _firebase.AddAsync(Collection, entity.Id_UsuarioEspacio, fireStoreUsuarioEspacio, ct);

            return entity;
        }

        public async Task<UsuarioEspacio?> GetByIdAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            try
            {
                var fUsuarioEspacio = await _firebase.GetAsync<FireStoreUsuarioEspacio>(Collection, id, ct);
                return fUsuarioEspacio?.Adapt<UsuarioEspacio>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error GetByIdAsync {Id}", id);
                throw;
            }
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

        public async Task<UsuarioEspacio?> UpdateAsync(string id, UsuarioEspacio entity, CancellationToken ct = default)
        {
            var fUsuarioEspacio = entity.Adapt<FireStoreUsuarioEspacio>();
            await _firebase.UpdateAsync(Collection, id, fUsuarioEspacio, ct);

            var updated = await _firebase.GetAsync<FireStoreUsuarioEspacio>(Collection, id, ct);
            return updated?.Adapt<UsuarioEspacio>();
        }

        public async Task DeleteAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("id requerido", nameof(id));
            try
            {
                await _firebase.DeleteAsync(Collection, id, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error DeleteAsync {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<UsuarioEspacio>> GetAllAsync(CancellationToken ct = default)
        {
            _logger.LogInformation("GetAllAsync llamado para colección: {Collection}", Collection);
            var list = await _firebase.GetAllAsync<FireStoreUsuarioEspacio>(Collection, ct);
            _logger.LogInformation("Firebase devolvió {Count} documentos de {Collection}", list?.Count ?? 0, Collection);
            return list.Select(fs => fs.Adapt<UsuarioEspacio>());
        }
    }
}
