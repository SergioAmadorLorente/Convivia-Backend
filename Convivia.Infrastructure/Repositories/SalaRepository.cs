using Convivia.Domain.Entities;
using Convivia.Application.Repositories;
using Convivia.Infrastructure.Models;
using Convivia.Shared.Services;
using Mapster;
using Microsoft.Extensions.Logging;

namespace Convivia.Infrastructure.Repositories
{
    public class SalaRepository : Repository<FireStoreSala>, ISalaRepository
    {
        private readonly ILogger<SalaRepository> _logger;
        private const string Collection = "salas";

        public SalaRepository(
            IFirebaseService firebase,
            ILogger<SalaRepository> logger,
            ILoggerFactory loggerFactory)
            : base(firebase, loggerFactory.CreateLogger<Repository<FireStoreSala>>(), Collection)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> AddAsync(Sala entity, CancellationToken ct = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var persist = entity.Adapt<FireStoreSala>();
            return await base.AddAsync(persist, ct);
        }

        public async Task<Sala?> GetByIdAsync(string id, CancellationToken ct = default)
        {
            var persist = await base.GetByIdAsync(id, ct);
            if (persist == null) return null;
            return persist.Adapt<Sala>();
        }

        public async Task<IEnumerable<Sala>> GetAllAsync(CancellationToken ct = default)
        {
            var list = await base.GetAllAsync(ct);
            return list == null ? Array.Empty<Sala>() : list.Adapt<List<Sala>>();
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

        public async Task UpdateAsync(string id, Sala entity, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var persist = entity.Adapt<FireStoreSala>();
            await base.UpdateAsync(id, persist, ct);
        }

        public async Task UpdateAsync(string id, Sala entity, bool merge, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var persist = entity.Adapt<FireStoreSala>();
            await base.UpdateAsync(id, persist, merge, ct);
        }

        public async Task UpdateAsync(string id, IDictionary<string, object> updates, bool useSetMerge = true, CancellationToken ct = default)
        {
            await base.UpdateAsync(id, updates, useSetMerge, ct);
        }

        public async Task DeleteAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            await base.DeleteAsync(id, ct);
        }
    }
}