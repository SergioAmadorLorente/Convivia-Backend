using Convivia.Application.Repositories;
using Convivia.Domain.Entities;
using Convivia.Infrastructure.Models;
using Convivia.Shared.DTOs;
using Convivia.Shared.Services;
using Mapster;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Convivia.Infrastructure.Repositories
{
    public class RolRepository : Repository<FireStoreRol>, IRolRepository
    {
        private readonly ILogger<RolRepository> _logger;
        private const string Collection = "roles";

        public RolRepository(IFirebaseService firebase, ILogger<RolRepository> logger, ILoggerFactory loggerFactory)
            : base(firebase, loggerFactory.CreateLogger<Repository<FireStoreRol>>(), Collection)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> AddAsync(Rol rol, CancellationToken ct = default)
        {
            if (rol == null) throw new ArgumentNullException(nameof(rol));
            var persist = rol.Adapt<FireStoreRol>();
            return await base.AddAsync(persist, persist.Id, ct);
        }

        public async Task<Rol?> GetByIdAsync(string id, CancellationToken ct = default)
        {
            var persist = await base.GetByIdAsync(id, ct);
            if (persist == null) return null;
            return persist.Adapt<Rol>();
        }

        public async Task<IEnumerable<Rol>> GetAllAsync(CancellationToken ct = default)
        {
            var list = await base.GetAllAsync(ct);
            return list == null ? Array.Empty<Rol>() : list.Adapt<List<Rol>>();
        }

        public async Task<Rol?> GetByNombreAsync(TipoRol nombre, CancellationToken ct = default)
        {
            try
            {
                // Convertir enum a string para consultar en Firestore
                var nombreStr = nombre.ToString();
                
                // Consultar FireStoreRol desde Firestore por nombre
                var list = await _firebase.QueryAsync<FireStoreRol>(Collection, nameof(FireStoreRol.Nombre), nombreStr, ct);
                if (list == null || !list.Any()) return null;
                
                var rolPersist = list.FirstOrDefault();
                if (rolPersist == null) return null;
                
                return rolPersist.Adapt<Rol>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error GetByNombreAsync {Nombre}", nombre);
                throw;
            }
        }

        public async Task UpdateAsync(string id, Rol rol, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (rol == null) throw new ArgumentNullException(nameof(rol));

            var persist = rol.Adapt<FireStoreRol>();
            await base.UpdateAsync(id, persist, ct);
        }

        public async Task UpdateAsync(string id, Rol rol, bool merge, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (rol == null) throw new ArgumentNullException(nameof(rol));

            var persist = rol.Adapt<FireStoreRol>();
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
