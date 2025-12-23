using Convivia.Domain.Entities;
using Convivia.Shared.DTOs;
using Convivia.Application.Repositories;
using Convivia.Shared.Services;
using Convivia.Infrastructure.Models;
using Microsoft.Extensions.Logging;
using Mapster;

namespace Convivia.Infrastructure.Repositories
{
    public class EspacioRepository : Repository<FireStoreEspacio>, IEspacioRepository
    {
        private readonly ILogger<EspacioRepository> _logger;
        private const string Collection = "espacios";
             
        public EspacioRepository(IFirebaseService firebase, ILogger<EspacioRepository> logger, ILoggerFactory loggerFactory)
            : base(firebase, loggerFactory.CreateLogger<Repository<FireStoreEspacio>>(), Collection)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> AddAsync(Espacio espacio, CancellationToken ct = default)
        {
            if (espacio == null) throw new ArgumentNullException(nameof(espacio));
            var persist = espacio.Adapt<FireStoreEspacio>();
            return await base.AddAsync(persist,persist.Id, ct);
        }

        public async Task<Espacio?> GetByIdAsync(string id, CancellationToken ct = default)
        {
            var persist = await base.GetByIdAsync(id, ct);
            if (persist == null) return null;
            return persist.Adapt<Espacio>();
        }

        public async Task<IEnumerable<Espacio>> GetAllAsync(CancellationToken ct = default)
        {
            var list = await base.GetAllAsync(ct);
            return list == null ? Array.Empty<Espacio>() : list.Adapt<List<Espacio>>();
        }

        public async Task<IEnumerable<Espacio>> GetByDireccionAsync(string direccion, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(direccion)) return Array.Empty<Espacio>();
            try
            {
                var list = await _firebase.QueryAsync<FireStoreEspacio>(Collection, nameof(FireStoreEspacio.Direccion), direccion, ct);
                if (list == null || !list.Any()) return new List<Espacio>();

                return list.Select(e => e.Adapt<Espacio>().Adapt<Espacio>()).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error GetByDireccionAsync {Direccion}", direccion);
                throw;
            }
        }
      
        public async Task UpdateAsync(string id, Espacio espacio, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));
            if (espacio == null) throw new ArgumentNullException(nameof(espacio));
            
            var persist = espacio.Adapt<FireStoreEspacio>();
            await base.UpdateAsync(id, persist, ct);
        }

        public async Task UpdateAsync(string id, Espacio espacio, bool merge, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (espacio == null) throw new ArgumentNullException(nameof(espacio));

            var persist = espacio.Adapt<FireStoreEspacio>();
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