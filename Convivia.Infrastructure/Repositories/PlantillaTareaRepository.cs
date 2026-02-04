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
    public class PlantillaTareaRepository : Repository<FirestorePlantillaTarea>, IPlantillaTareaRepository
    {
        private const string ESPACIOS_COLLECTION = "espacios";
        private const string PLANTILLATAREAS_SUBCOLLECTION = "plantillatareas";
        private readonly ILogger<PlantillaTareaRepository> _logger;

        public PlantillaTareaRepository(IFirebaseService firebase, ILogger<PlantillaTareaRepository> logger)
            : base(firebase, logger: logger as ILogger<Repository<FirestorePlantillaTarea>> ?? throw new ArgumentNullException(nameof(logger)), collection: "espacios/temp/plantillatareas")
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private string GetCollectionPath(string espacioId)
        {
            return $"{ESPACIOS_COLLECTION}/{espacioId}/{PLANTILLATAREAS_SUBCOLLECTION}";
        }

        public async Task<string> AddAsync(PlantillaTarea plantilla, CancellationToken ct = default)
        {
            if (plantilla == null) throw new ArgumentNullException(nameof(plantilla));
            if (string.IsNullOrWhiteSpace(plantilla.EspacioId)) throw new ArgumentException("EspacioId es requerido", nameof(plantilla.EspacioId));
            
            var firestoreEntity = plantilla.Adapt<FirestorePlantillaTarea>();
            // Convert FechaLimite to string format for persistence
            if (plantilla.FechaLimite.HasValue)
                firestoreEntity.FechaLimite = plantilla.FechaLimite.Value.ToString("yyyy-MM-dd");
            else
                firestoreEntity.FechaLimite = null;
            
            var collectionPath = GetCollectionPath(plantilla.EspacioId);
            await _firebase.AddAsync(collectionPath, firestoreEntity.Id, firestoreEntity, ct);
            return firestoreEntity.Id;
        }

        public async Task<PlantillaTarea?> GetByIdAsync(string id, CancellationToken ct = default)
        {
            throw new NotImplementedException("Use GetByEspacioAndIdAsync instead. PlantillaTarea requires espacioId.");
        }

        public async Task<IEnumerable<PlantillaTarea>> GetAllAsync(CancellationToken ct = default)
        {
            throw new NotImplementedException("Use GetAllByEspacioAsync instead. PlantillaTarea requires espacioId.");
        }

        public async Task<IEnumerable<PlantillaTarea>> GetAllByEspacioAsync(string espacioId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(espacioId)) throw new ArgumentException("espacioId requerido", nameof(espacioId));

            var collectionPath = GetCollectionPath(espacioId);
            var firestoreEntities = await _firebase.GetAllAsync<FirestorePlantillaTarea>(collectionPath, ct);
            if (firestoreEntities == null) return new List<PlantillaTarea>();
            
            var result = new List<PlantillaTarea>();
            foreach (var e in firestoreEntities)
            {
                var entity = e.Adapt<PlantillaTarea>();
                // Convert FechaLimite string back to DateOnly
                if (!string.IsNullOrWhiteSpace(e.FechaLimite))
                {
                    if (DateOnly.TryParse(e.FechaLimite, out var fechaOnly))
                        entity.FechaLimite = fechaOnly;
                }
                result.Add(entity);
            }
            return result;
        }

        public async Task UpdateAsync(string id, PlantillaTarea plantilla, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (plantilla == null) throw new ArgumentNullException(nameof(plantilla));
            if (string.IsNullOrWhiteSpace(plantilla.EspacioId)) throw new ArgumentException("EspacioId es requerido", nameof(plantilla.EspacioId));

            var firestoreEntity = plantilla.Adapt<FirestorePlantillaTarea>();
            // Convert FechaLimite to string format for persistence
            if (plantilla.FechaLimite.HasValue)
                firestoreEntity.FechaLimite = plantilla.FechaLimite.Value.ToString("yyyy-MM-dd");
            else
                firestoreEntity.FechaLimite = null;
            
            var collectionPath = GetCollectionPath(plantilla.EspacioId);
            await _firebase.UpdateAsync(collectionPath, firestoreEntity.Id, firestoreEntity, ct);
        }

        public async Task UpdateAsync(string id, PlantillaTarea plantilla, bool merge, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (plantilla == null) throw new ArgumentNullException(nameof(plantilla));
            if (string.IsNullOrWhiteSpace(plantilla.EspacioId)) throw new ArgumentException("EspacioId es requerido", nameof(plantilla.EspacioId));

            var firestoreEntity = plantilla.Adapt<FirestorePlantillaTarea>();
            // Convert FechaLimite to string format for persistence
            if (plantilla.FechaLimite.HasValue)
                firestoreEntity.FechaLimite = plantilla.FechaLimite.Value.ToString("yyyy-MM-dd");
            else
                firestoreEntity.FechaLimite = null;
            
            var collectionPath = GetCollectionPath(plantilla.EspacioId);
            await _firebase.UpdateAsync(collectionPath, id, firestoreEntity, merge, ct);
        }

        public async Task UpdateAsync(string id, IDictionary<string, object> updates, bool useSetMerge = true, CancellationToken ct = default)
        {
            throw new NotImplementedException("Use UpdateAsync with espacioId. PlantillaTarea requires espacioId.");
        }

        public async Task UpdateAsync(string espacioId, string id, IDictionary<string, object> updates, bool useSetMerge = true, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(espacioId)) throw new ArgumentException("espacioId requerido", nameof(espacioId));
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("id requerido", nameof(id));

            var collectionPath = GetCollectionPath(espacioId);
            await _firebase.UpdateAsync(collectionPath, id, updates, useSetMerge, ct);
        }

        public async Task DeleteAsync(string id, CancellationToken ct = default)
        {
            throw new NotImplementedException("Use DeleteAsync with espacioId. PlantillaTarea requires espacioId.");
        }

        public async Task DeleteAsync(string espacioId, string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(espacioId)) throw new ArgumentException("espacioId requerido", nameof(espacioId));
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            
            var collectionPath = GetCollectionPath(espacioId);
            await _firebase.DeleteAsync(collectionPath, id, ct);
        }

        public async Task<PlantillaTarea?> GetByEspacioAndIdAsync(string espacioid, string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(espacioid)) throw new ArgumentException("espacioid requerido", nameof(espacioid));
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("id requerido", nameof(id));

            var collectionPath = GetCollectionPath(espacioid);
            var direct = await _firebase.GetAsync<FirestorePlantillaTarea>(collectionPath, id, ct);
            
            
            if (direct != null)
            {
                var entity = direct.Adapt<PlantillaTarea>();
                // Convert FechaLimite string back to DateOnly
                if (!string.IsNullOrWhiteSpace(direct.FechaLimite))
                {
                    if (DateOnly.TryParse(direct.FechaLimite, out var fechaOnly))
                        entity.FechaLimite = fechaOnly;
                }
                return entity;
            }

            return null;
        }
    }
}