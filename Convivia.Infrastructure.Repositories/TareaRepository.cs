using Convivia.Domain.Entities;
using Convivia.Domain.Repositories;
using Convivia.Infrastructure.Models;
using Convivia.Shared.Services;
using Mapster;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace Convivia.Infrastructure.Repositories
{
    public class TareaRepository : Repository<FirestoreTarea>, ITareaRepository
    {
        private readonly IFirebaseService _firebase;
        private readonly ILogger<TareaRepository> _logger;

        public TareaRepository(IFirebaseService firebase, ILogger<TareaRepository> logger)
            : base(firebase, logger: logger as ILogger<Repository<FirestoreTarea>> ?? throw new ArgumentNullException(nameof(logger)), collection: "plantillatareas/*/tareas")
        {
            _firebase = firebase ?? throw new ArgumentNullException(nameof(firebase));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private string GetSubcollectionPath(string plantillaId)
        {
            if (string.IsNullOrWhiteSpace(plantillaId)) throw new ArgumentException("plantillaId requerido", nameof(plantillaId));
            return $"plantillatareas/{plantillaId}/tareas";
        }

        public async Task<string> AddAsync(Tarea tarea, CancellationToken ct = default)
        {
            if (tarea == null) throw new ArgumentNullException(nameof(tarea));
            if (string.IsNullOrWhiteSpace(tarea.PlantillaId)) throw new ArgumentException("PlantillaId requerido en Tarea", nameof(tarea));

            var firestoreEntity = tarea.Adapt<FirestoreTarea>();
            var subcollectionPath = GetSubcollectionPath(tarea.PlantillaId);
            await _firebase.AddAsync(subcollectionPath, tarea.Id, firestoreEntity, ct);
            return tarea.Id;
        }

        public async Task<Tarea> GetByIdAsync(string id, CancellationToken ct = default)
        {
            throw new NotImplementedException("Usar GetAsync.");
        }

        public async Task<List<string>> AddAsyncList(List<Tarea> tareas, CancellationToken ct = default)
        {
            if (tareas == null) throw new ArgumentNullException(nameof(tareas));
            var ids = new List<string>();
            foreach (var tarea in tareas)
            {
                if (string.IsNullOrWhiteSpace(tarea.PlantillaId)) throw new ArgumentException("PlantillaId requerido en Tarea", nameof(tarea));
                var firestoreEntity = tarea.Adapt<FirestoreTarea>();
                var subcollectionPath = GetSubcollectionPath(tarea.PlantillaId);
                await _firebase.AddAsync(subcollectionPath, tarea.Id, firestoreEntity, ct);
                ids.Add(tarea.Id);
            }

            return ids;
        }

        public async Task<Tarea?> GetAsync(string plantillaId, string tareaId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(plantillaId)) throw new ArgumentException("plantillaId requerido", nameof(plantillaId));
            if (string.IsNullOrWhiteSpace(tareaId)) throw new ArgumentException("tareaId requerido", nameof(tareaId));

            var subcollectionPath = GetSubcollectionPath(plantillaId);
            var firestoreEntity = await _firebase.GetAsync<FirestoreTarea>(subcollectionPath, tareaId, ct);
            var entity = firestoreEntity?.Adapt<Tarea>();

            return entity;
        }

        public async Task<IEnumerable<Tarea>> GetAllAsync(CancellationToken ct = default)
        {

            throw new NotImplementedException("Usar QueryAsync con condición PlantillaId.");

        }

        public async Task UpdateAsync(string id, Tarea tareaActualizada, CancellationToken ct = default)
        {
            if (tareaActualizada == null)
                throw new ArgumentNullException(nameof(tareaActualizada));

            if (string.IsNullOrWhiteSpace(tareaActualizada.PlantillaId))
                throw new ArgumentException("PlantillaId requerido en Tarea", nameof(tareaActualizada));

            if (string.IsNullOrWhiteSpace(tareaActualizada.Id))
                throw new ArgumentException("Id de la tarea requerido", nameof(tareaActualizada));

            var firestoreEntity = tareaActualizada.Adapt<FirestoreTarea>();

            var subcollectionPath = GetSubcollectionPath(tareaActualizada.PlantillaId);

            await _firebase.UpdateAsync(subcollectionPath, id, firestoreEntity,
                                        cancellationToken: ct);
        }

        // New overload: Update with merge option
        public async Task UpdateAsync(string id, Tarea tareaActualizada, bool merge, CancellationToken ct = default)
        {
            if (tareaActualizada == null)
                throw new ArgumentNullException(nameof(tareaActualizada));

            if (string.IsNullOrWhiteSpace(tareaActualizada.PlantillaId))
                throw new ArgumentException("PlantillaId requerido en Tarea", nameof(tareaActualizada));

            if (string.IsNullOrWhiteSpace(tareaActualizada.Id))
                throw new ArgumentException("Id de la tarea requerido", nameof(tareaActualizada));

            var firestoreEntity = tareaActualizada.Adapt<FirestoreTarea>();
            var subcollectionPath = GetSubcollectionPath(tareaActualizada.PlantillaId);
            await _firebase.UpdateAsync(subcollectionPath, id, firestoreEntity, merge, ct);
        }

        // New overload: partial update using dictionary (PATCH)
        public async Task UpdateAsync(string id, IDictionary<string, object> updates, bool useSetMerge = true, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("id requerido", nameof(id));
            if (updates == null) throw new ArgumentNullException(nameof(updates));
            if (updates.Count == 0) return;

            // Avoid collection-group query that requires an index by searching subcollections sequentially.
            var plantillaId = await FindPlantillaIdForTareaAsync(id, ct);
            if (string.IsNullOrWhiteSpace(plantillaId)) throw new KeyNotFoundException($"Tarea {id} no encontrada");

            var subcollectionPath = GetSubcollectionPath(plantillaId);
            await _firebase.UpdateAsync(subcollectionPath, id, updates, useSetMerge, ct);
        }

        public async Task DeleteAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("id requerido", nameof(id));

            var plantillaId = await FindPlantillaIdForTareaAsync(id, ct);
            if (string.IsNullOrWhiteSpace(plantillaId)) throw new KeyNotFoundException($"Tarea {id} no encontrada");

            var subcollectionPath = GetSubcollectionPath(plantillaId);
            await _firebase.DeleteAsync(subcollectionPath, id, ct);
        }

        // Helper: look through plantillatareas subcollections to find which plantilla contains the tarea.
        // This avoids performing a collection-group query that requires a composite index.
        private async Task<string?> FindPlantillaIdForTareaAsync(string tareaId, CancellationToken ct = default)
        {
            // Get all plantillas and probe their tareas subcollection for the document id.
            var plantillas = await _firebase.GetAllAsync<FirestorePlantillaTarea>("plantillatareas", ct);
            if (plantillas == null) return null;

            foreach (var p in plantillas)
            {
                if (string.IsNullOrWhiteSpace(p.PlantillaId)) continue;
                var subPath = GetSubcollectionPath(p.PlantillaId);
                var tarea = await _firebase.GetAsync<FirestoreTarea>(subPath, tareaId, ct);
                if (tarea != null)
                    return p.PlantillaId;
            }

            return null;
        }
    }
}
