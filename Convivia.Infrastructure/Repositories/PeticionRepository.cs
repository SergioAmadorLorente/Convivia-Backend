using Convivia.Domain.Entities;
using Convivia.Application.Repositories;
using Convivia.Shared.Services;
using Convivia.Infrastructure.Models;
using Microsoft.Extensions.Logging;
using Mapster;

namespace Convivia.Infrastructure.Repositories
{
    public class PeticionRepository : Repository<FireStorePeticion>, IPeticionRepository
    {
        private readonly ILogger<PeticionRepository> _logger;
        private const string Collection = "peticiones";

        public PeticionRepository(IFirebaseService firebase, ILogger<PeticionRepository> logger, ILoggerFactory loggerFactory) 
            : base(firebase, loggerFactory.CreateLogger<Repository<FireStorePeticion>>(), Collection)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> AddAsync(Peticion entity, CancellationToken ct = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var persist = entity.Adapt<FireStorePeticion>();
            return await base.AddAsync(persist, ct);
        }

        public async Task<Peticion?> GetByIdAsync(string id, CancellationToken ct = default)
        {
            var persist = await base.GetByIdAsync(id, ct);
            if (persist == null) return null;
            return persist.Adapt<Peticion>();
        }

        public async Task<IEnumerable<Peticion>> GetAllAsync(CancellationToken ct = default)
        {
            var list = await base.GetAllAsync(ct);
            return list == null ? Array.Empty<Peticion>() : list.Adapt<List<Peticion>>();
        }

        public async Task<IEnumerable<Peticion>> GetByEstadoAsync(string estado, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(estado)) return Array.Empty<Peticion>();
            try
            {
                var list = await _firebase.QueryAsync<FireStorePeticion>(Collection, nameof(FireStorePeticion.Estado), estado, ct);
                if (list == null || !list.Any()) return new List<Peticion>();

                return list.Select(p => p.Adapt<Peticion>()).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error GetByEstadoAsync {Estado}", estado);
                throw;
            }
        }

        public async Task UpdateAsync(string id, Peticion entity, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var persist = entity.Adapt<FireStorePeticion>();
            await base.UpdateAsync(id, persist, ct);
        }

        public async Task UpdateAsync(string id, Peticion entity, bool merge, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var persist = entity.Adapt<FireStorePeticion>();
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
