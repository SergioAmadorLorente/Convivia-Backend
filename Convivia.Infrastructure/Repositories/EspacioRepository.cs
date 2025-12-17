using Convivia.Domain.Entities;
using Convivia.Shared.DTOs;
using Convivia.Shared.Repositories;
using Convivia.Shared.Services;
using Convivia.Infrastructure.Models;
using Microsoft.Extensions.Logging;
using Mapster;

namespace Convivia.Infrastructure.Repositories
{
    public class EspacioRepository : IEspacioRepository
    {
        private readonly IFirebaseService _firebase;
        private readonly ILogger<EspacioRepository> _logger;
        private const string Collection = "espacios";

        public EspacioRepository(IFirebaseService firebase, ILogger<EspacioRepository> logger)
        {
            _firebase = firebase ?? throw new ArgumentNullException(nameof(firebase));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> AddAsync(EspacioDto espacio, CancellationToken ct = default)
        {
            if (espacio == null) throw new ArgumentNullException(nameof(espacio));

            // Convertir EspacioDto → Espacio (Domain) → FireStoreEspacio (Firestore)
            var espacioDomain = espacio.Adapt<Espacio>();
            var espacioPersist = espacioDomain.Adapt<FireStoreEspacio>();

            if (string.IsNullOrWhiteSpace(espacioPersist.Id))
            {
                var generatedId = await _firebase.AddAsync(Collection, espacioPersist, ct);
                return generatedId;
            }

            await _firebase.AddAsync(Collection, espacioPersist.Id, espacioPersist, ct);
            return espacioPersist.Id;
        }

        public async Task<EspacioDto?> GetByIdAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            try
            {
                var espacioPersist = await _firebase.GetAsync<FireStoreEspacio>(Collection, id, ct);
                if (espacioPersist == null) return null;

                // Convertir FireStoreEspacio → Espacio (Domain) → EspacioDto
                var espacioDomain = espacioPersist.Adapt<Espacio>();
                return espacioDomain.Adapt<EspacioDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error GetByIdAsync {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<EspacioDto>> GetByDireccionAsync(string direccion, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(direccion)) return Array.Empty<EspacioDto>();
            try
            {
                var list = await _firebase.QueryAsync<FireStoreEspacio>(Collection, nameof(FireStoreEspacio.Direccion), direccion, ct);
                if (list == null || !list.Any()) return new List<EspacioDto>();

                return list.Select(e => e.Adapt<Espacio>().Adapt<EspacioDto>()).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error GetByDireccionAsync {Direccion}", direccion);
                throw;
            }
        }
      
        public async Task UpdateAsync(string id, EspacioDto espacio, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("id requerido", nameof(id));
            if (espacio == null) throw new ArgumentNullException(nameof(espacio));

            try
            {
                // Convertir EspacioDto → Espacio (Domain) → FireStoreEspacio
                var espacioDomain = espacio.Adapt<Espacio>();
                var espacioPersist = espacioDomain.Adapt<FireStoreEspacio>();

                await _firebase.UpdateAsync(Collection, id, espacioPersist, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error UpdateAsync {Id}", id);
                throw;
            }
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
    }
}