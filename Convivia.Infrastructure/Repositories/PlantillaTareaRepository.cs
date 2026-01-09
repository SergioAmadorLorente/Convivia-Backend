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
        private const string COLLECTION = "plantillatareas";
        private readonly ILogger<PlantillaTareaRepository> _logger;

        public PlantillaTareaRepository(IFirebaseService firebase, ILogger<PlantillaTareaRepository> logger)
            : base(firebase, logger: logger as ILogger<Repository<FirestorePlantillaTarea>> ?? throw new ArgumentNullException(nameof(logger)), collection: "plantillatareas")
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> AddAsync(PlantillaTarea plantilla, CancellationToken ct = default)
        {
            if (plantilla == null) throw new ArgumentNullException(nameof(plantilla));
            var firestoreEntity = plantilla.Adapt<FirestorePlantillaTarea>();
            // Convert FechaLimite to string format for persistence
            if (plantilla.FechaLimite.HasValue)
                firestoreEntity.FechaLimite = plantilla.FechaLimite.Value.ToString("yyyy-MM-dd");
            else
                firestoreEntity.FechaLimite = null;
            return await base.AddAsync(firestoreEntity, ct);
        }

        public async Task<PlantillaTarea?> GetByIdAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));

            var firestoreEntity = await _firebase.GetAsync<FirestorePlantillaTarea>(COLLECTION, id);
            if (firestoreEntity == null)
            {
                return null;
            }
            var entity = firestoreEntity.Adapt<PlantillaTarea>();
            // Convert FechaLimite string back to DateOnly
            if (!string.IsNullOrWhiteSpace(firestoreEntity.FechaLimite))
            {
                if (DateOnly.TryParse(firestoreEntity.FechaLimite, out var fechaOnly))
                    entity.FechaLimite = fechaOnly;
            }
            return entity;
        }

        public async Task<IEnumerable<PlantillaTarea>> GetAllAsync(CancellationToken ct = default)
        {
            var firestoreEntities = await base.GetAllAsync(ct);
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

            var firestoreEntity = plantilla.Adapt<FirestorePlantillaTarea>();
            // Convert FechaLimite to string format for persistence
            if (plantilla.FechaLimite.HasValue)
                firestoreEntity.FechaLimite = plantilla.FechaLimite.Value.ToString("yyyy-MM-dd");
            else
                firestoreEntity.FechaLimite = null;
            await _firebase.UpdateAsync(COLLECTION, firestoreEntity.Id, firestoreEntity);
        }

        public async Task UpdateAsync(string id, PlantillaTarea plantilla, bool merge, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (plantilla == null) throw new ArgumentNullException(nameof(plantilla));

            var firestoreEntity = plantilla.Adapt<FirestorePlantillaTarea>();
            // Convert FechaLimite to string format for persistence
            if (plantilla.FechaLimite.HasValue)
                firestoreEntity.FechaLimite = plantilla.FechaLimite.Value.ToString("yyyy-MM-dd");
            else
                firestoreEntity.FechaLimite = null;
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

        public async Task<PlantillaTarea?> GetByEspacioAndIdAsync(string espacioid, string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(espacioid)) throw new ArgumentException("espacioid requerido", nameof(espacioid));
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("id requerido", nameof(id));

            // Try direct get by document id first (faster and less error prone)
            var direct = await _firebase.GetAsync<FirestorePlantillaTarea>(COLLECTION, id, ct);
            if (direct != null)
            {
                if (string.Equals(direct.EspacioId, espacioid, StringComparison.OrdinalIgnoreCase))
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
                else
                {
                    _logger.LogInformation("GetByEspacioAndIdAsync: direct document EspacioId does not match requested EspacioId. direct={DirectEspacio}, requested={Requested}", direct.EspacioId, espacioid);
                    Console.WriteLine($"[DEBUG] GetByEspacioAndIdAsync: direct document EspacioId={direct.EspacioId} does not match requested EspacioId={espacioid}");
                }
            }

            var conditions = new (string field, object val)[]
            {
                (nameof(FirestorePlantillaTarea.EspacioId), espacioid),
                (nameof(FirestorePlantillaTarea.Id), id)
            };

            var firestoreEntities = await _firebase.QueryMultipleConditionsAsync<FirestorePlantillaTarea>(COLLECTION, conditions, ct);
            if (firestoreEntities == null)
            {
                return null;
            }

            var first = firestoreEntities.FirstOrDefault();

            if (first == null) return null;

            var resultEntity = first.Adapt<PlantillaTarea>();
            // Convert FechaLimite string back to DateOnly
            if (!string.IsNullOrWhiteSpace(first.FechaLimite))
            {
                if (DateOnly.TryParse(first.FechaLimite, out var fechaOnly))
                    resultEntity.FechaLimite = fechaOnly;
            }

            return resultEntity;
        }
    }
}