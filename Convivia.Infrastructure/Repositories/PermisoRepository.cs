using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Convivia.Application.Repositories;
using Convivia.Shared.Services;
using Convivia.Infrastructure.Models;
using Convivia.Domain.Entities;
using Microsoft.Extensions.Logging;
using Mapster;

namespace Convivia.Infrastructure.Repositories
{
    public class PermisoRepository : Repository<FireStorePermiso>, IPermisoRepository
    {
        private readonly ILogger<PermisoRepository> _logger;
        private const string Collection = "permisos";

        public PermisoRepository(
            IFirebaseService firebase,
            ILogger<PermisoRepository> logger,
            ILoggerFactory loggerFactory)
            : base(firebase, loggerFactory.CreateLogger<Repository<FireStorePermiso>>(), Collection)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> AddAsync(Permiso entity, CancellationToken ct = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var persist = entity.Adapt<FireStorePermiso>();
            return await base.AddAsync(persist, ct);
        }

        public async Task<Permiso?> GetByIdAsync(string id, CancellationToken ct = default)
        {
            var persist = await base.GetByIdAsync(id, ct);
            if (persist == null) return null;
            return persist.Adapt<Permiso>();
        }

        public async Task<IEnumerable<Permiso>> GetAllAsync(CancellationToken ct = default)
        {
            var list = await base.GetAllAsync(ct);
            return list == null ? Array.Empty<Permiso>() : list.Adapt<List<Permiso>>();
        }

        public async Task<IEnumerable<Permiso>> GetByRolAsync(string rol, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(rol)) return Array.Empty<Permiso>();
            try
            {
                var list = await _firebase.QueryAsync<FireStorePermiso>(Collection, nameof(FireStorePermiso.Rol), rol, ct);
                if (list == null || !list.Any()) return new List<Permiso>();
                
                return list.Select(pp => pp.Adapt<Permiso>()).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error GetByRolAsync {Rol}", rol);
                throw;
            }
        }

        public async Task UpdateAsync(string id, Permiso entity, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var persist = entity.Adapt<FireStorePermiso>();
            await base.UpdateAsync(id, persist, ct);
        }

        public async Task UpdateAsync(string id, Permiso entity, bool merge, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var persist = entity.Adapt<FireStorePermiso>();
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
