using Convivia.Domain.Entities;
using Convivia.Domain.Models;
using Convivia.Domain.Repositories;
using Convivia.Infrastructure.Models;
using Convivia.Shared.DTOs;
using Convivia.Shared.Repositories;
using Convivia.Shared.Services;
using Google.Cloud.Firestore;
using Mapster;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            await _firebase.AddAsync(subcollectionPath, firestoreEntity, ct);
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
                await _firebase.AddAsync(subcollectionPath, firestoreEntity, ct);
                ids.Add(tarea.Id);
            }
            
            return ids;
        }

        public async Task<Tarea?> GetAsync(string espacioid, string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("id requerido", nameof(id));
            if (string.IsNullOrWhiteSpace(espacioid)) throw new ArgumentException("espacioid requerido", nameof(espacioid));

            // Query collection group 'tareas' across all plantillas where EspacioId == espacioid and Id == id
            var conditions = new (string field, object val)[]
            {
                (nameof(FirestoreTarea.EspacioId), espacioid),
                (nameof(FirestoreTarea.Id), id)
            };

            var firestoreEntities = await _firebase.QueryMultipleConditionsAsync<FirestoreTarea>("plantillatareas/*/tareas", conditions, ct);
            var firestoreEntity = firestoreEntities?.FirstOrDefault();
            var entity = firestoreEntity?.Adapt<Tarea>();
            return entity;
        }

        public async Task<List<Tarea?>> GetAllByEspacioIdAsync(string espacioid, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(espacioid)) throw new ArgumentException("espacioid requerido", nameof(espacioid));

            // Use collection group to query all tareas across all plantillas by EspacioId
            var firestoreEntities = await _firebase.QueryCollectionGroupAsync<FirestoreTarea>("tareas", nameof(FirestoreTarea.EspacioId), espacioid, ct);
            var lista = new List<Tarea?>();

            if (firestoreEntities == null || !firestoreEntities.Any())
                return lista;

            foreach (var entity in firestoreEntities)
            {
                var mappedEntity = entity.Adapt<Tarea>();
                lista.Add(mappedEntity);
            }

            return lista;
        }

        public async Task<List<Tarea>> GetAllAsync(CancellationToken ct = default)
        {
            // Return all tareas across all plantillas using collection group
            var firestoreEntities = await _firebase.QueryCollectionGroupAllAsync<FirestoreTarea>("tareas", ct);
            var lista = new List<Tarea>();

            if (firestoreEntities == null || !firestoreEntities.Any())
                return lista;

            foreach (var entity in firestoreEntities)
            {
                var mappedEntity = entity.Adapt<Tarea>();
                lista.Add(mappedEntity);
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