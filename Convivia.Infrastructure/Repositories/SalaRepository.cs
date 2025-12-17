using Convivia.Domain.Entities;
using Convivia.Domain.Repositories;
using Convivia.Infrastructure.Models;
using Convivia.Shared.Services;
using Mapster;
using Microsoft.Extensions.Logging;

namespace Convivia.Infrastructure.Repositories
{
    public class SalaRepository : ISalaRepository
    {
        private readonly IFirebaseService _firebase;
        private readonly ILogger<SalaRepository> _logger;
        private const string Collection = "salas";

        public SalaRepository(IFirebaseService firebase, ILogger<SalaRepository> logger)
        {
            _firebase = firebase ?? throw new ArgumentNullException(nameof(firebase));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Sala?> AddAsync(Sala entity, CancellationToken ct = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var fireStoreSala = entity.Adapt<FireStoreSala>();
            await _firebase.AddAsync(Collection, entity.Id, fireStoreSala, ct);

            return entity;
        }

        public async Task<Sala?> GetByIdAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            try
            {
                var fsala = await _firebase.GetAsync<FireStoreSala>(Collection, id, ct);
                return fsala?.Adapt<Sala>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error GetByIdAsync {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Sala>> GetByEspacioIdAsync(string idEspacio, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(idEspacio)) return Array.Empty<Sala>();
            try
            {
                var list = await _firebase.QueryAsync<FireStoreSala>(Collection, nameof(FireStoreSala.IdEspacio), idEspacio, ct);
                return list?.Select(fs => fs.Adapt<Sala>()) ?? new List<Sala>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error GetByEspacioIdAsync {EspacioId}", idEspacio);
                throw;
            }
        }

        public async Task<Sala?> UpdateAsync(string id, Sala entity, CancellationToken ct = default)
        {
            var fsala = entity.Adapt<FireStoreSala>();
            await _firebase.UpdateAsync(Collection, id, fsala, ct);

            var updated = await _firebase.GetAsync<FireStoreSala>(Collection, id, ct);
            return updated?.Adapt<Sala>();
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

        public async Task<IEnumerable<Sala>> GetAllAsync(CancellationToken ct = default)
        {
            var list = await _firebase.GetAllAsync<FireStoreSala>(Collection, ct);
            return list.Select(fs => fs.Adapt<Sala>());
        }
    }
}