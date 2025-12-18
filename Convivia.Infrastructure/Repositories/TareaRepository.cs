using Convivia.Domain.Entities;
using Convivia.Application.Repositories;
using Convivia.Infrastructure.Models;
using Convivia.Shared.Services;
using Mapster;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Convivia.Infrastructure.Repositories
{
    public class TareaRepository : Repository<FirestoreTarea>, ITareaRepository
    {
        private const string COLLECTION = "tareas";
        private readonly ILogger<TareaRepository> _logger;

        public TareaRepository(
            IFirebaseService firebase,
            ILogger<TareaRepository> logger,
            ILoggerFactory loggerFactory)
            : base(firebase, loggerFactory.CreateLogger<Repository<FirestoreTarea>>(), COLLECTION)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> AddAsync(Tarea tarea, CancellationToken ct = default)
        {
            if (tarea == null) throw new ArgumentNullException(nameof(tarea));
            var firestoreEntity = tarea.Adapt<FirestoreTarea>();
            return await base.AddAsync(firestoreEntity, ct);
        }

        public async Task<Tarea?> GetByIdAsync(string id, CancellationToken ct = default)
        {
            var firestoreEntity = await base.GetByIdAsync(id, ct);
            return firestoreEntity?.Adapt<Tarea>();
        }

        public async Task<IEnumerable<Tarea>> GetAllAsync(CancellationToken ct = default)
        {
            var firestoreEntities = await base.GetAllAsync(ct);
            return firestoreEntities == null ? new List<Tarea>() : firestoreEntities.Select(e => e.Adapt<Tarea>()).ToList();
        }

        public async Task<List<Tarea>> GetAllByEspacioIdAsync(string espacioid, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(espacioid)) throw new ArgumentNullException(nameof(espacioid));
            
            var entidadesEspacio = await _firebase.QueryAsync<FirestoreTarea>(COLLECTION, nameof(FirestoreTarea.EspacioId), espacioid, ct);
            
            if (!entidadesEspacio.Any())
                return new List<Tarea>();
            
            return entidadesEspacio.Select(pte => pte.Adapt<Tarea>()).ToList();
        }

        public async Task UpdateAsync(string id, Tarea tarea, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (tarea == null) throw new ArgumentNullException(nameof(tarea));

            var firestoreEntity = tarea.Adapt<FirestoreTarea>();
            await base.UpdateAsync(id, firestoreEntity, ct);
        }

        public async Task UpdateAsync(string id, Tarea tarea, bool merge, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (tarea == null) throw new ArgumentNullException(nameof(tarea));

            var firestoreEntity = tarea.Adapt<FirestoreTarea>();
            await base.UpdateAsync(id, firestoreEntity, merge, ct);
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