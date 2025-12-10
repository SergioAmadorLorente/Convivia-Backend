using Convivia.Domain.Entities;
using Convivia.Domain.Repositories;
using Convivia.Infrastructure.Models;
using Convivia.Shared.Services;
using Mapster;
using Microsoft.Extensions.Logging;

namespace Convivia.Infrastructure.Repositories
{
    public class TareaRepository : ITareaRepository
    {
        private readonly IFirebaseService _firebase;
        private readonly ILogger<TareaRepository> _logger;

        public TareaRepository(IFirebaseService firebase, ILogger<TareaRepository> logger)
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
            // Use tarea.Id as document id so stored document name matches the Id field inside the document
            await _firebase.AddAsync(subcollectionPath, tarea.Id, firestoreEntity, ct);
            return tarea.Id;
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
                // Ensure document id equals tarea.Id
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

        public async Task<List<Tarea>> GetAllAsync(string plantillaId, CancellationToken ct = default)
        {
            
            if (string.IsNullOrWhiteSpace(plantillaId)) throw new ArgumentException("plantillaId requerido", nameof(plantillaId));
            var subcollectionPath = GetSubcollectionPath(plantillaId);
            var firestoreEntities = await _firebase.GetAllAsync<FirestoreTarea>(subcollectionPath, ct);
            List<Tarea> lista = new List<Tarea>();
            if (firestoreEntities == null || !firestoreEntities.Any())
                return lista;
            foreach (var entity in firestoreEntities)
            {
                var entitytarea = entity.Adapt<Tarea>();
                lista.Add(entitytarea);
            }
            return lista;

        }

        public async Task UpdateAsync(string id, Tarea tarea, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("id requerido", nameof(id));
            if (tarea == null) throw new ArgumentNullException(nameof(tarea));
            if (string.IsNullOrWhiteSpace(tarea.PlantillaId)) throw new ArgumentException("PlantillaId requerido en Tarea", nameof(tarea));

            var firestoreEntity = tarea.Adapt<FirestoreTarea>();
            var subcollectionPath = GetSubcollectionPath(tarea.PlantillaId);
            await _firebase.UpdateAsync(subcollectionPath, id, firestoreEntity, cancellationToken: ct);
        }

        public async Task UpdateAsyncList(List<Tarea> tareas, Tarea tareaactualizada, CancellationToken ct = default)
        {
            if (tareas == null) throw new ArgumentNullException(nameof(tareas));
            if (tareaactualizada == null) throw new ArgumentNullException(nameof(tareaactualizada));
            foreach (var tarea in tareas)
            {
                if (string.IsNullOrWhiteSpace(tarea.PlantillaId)) throw new ArgumentException("PlantillaId requerido en Tarea", nameof(tarea));
                var firestoreEntity = tareaactualizada.Adapt<FirestoreTarea>();
                var subcollectionPath = GetSubcollectionPath(tarea.PlantillaId);
                await _firebase.UpdateAsync(subcollectionPath, tarea.Id, firestoreEntity, cancellationToken: ct);
            }
        }

        public async Task DeleteAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("id requerido", nameof(id));

            // Find the tarea in collection group to obtain plantillaId
            var matches = await _firebase.QueryCollectionGroupAsync<FirestoreTarea>("tareas", nameof(FirestoreTarea.Id), id, ct);
            var found = matches?.FirstOrDefault();
            if (found == null) throw new KeyNotFoundException($"Tarea {id} no encontrada");

            var plantillaId = found.PlantillaId;
            if (string.IsNullOrWhiteSpace(plantillaId)) throw new InvalidOperationException("PlantillaId no presente en el documento de tarea.");

            var subcollectionPath = GetSubcollectionPath(plantillaId);
            await _firebase.DeleteAsync(subcollectionPath, id, ct);
        }
    }
}