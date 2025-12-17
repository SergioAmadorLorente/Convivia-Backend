using Convivia.Shared.DTOs;
using Convivia.Domain.Entities;
using Convivia.Infrastructure.Models;
using Convivia.Shared.Repositories;
using Convivia.Shared.Services;
using Mapster;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Convivia.Domain.Repositories;

namespace Convivia.Infrastructure.Repositories
{
    public class PlantillaTareaRepository : IPlantillaTareaRepository
    {
        private const string COLLECTION = "plantillatareas";
        private readonly IFirebaseService _firebase;
        private readonly ILogger<PlantillaTareaRepository> _logger;

        public PlantillaTareaRepository(IFirebaseService firebase, ILogger<PlantillaTareaRepository> logger)
        {
            _firebase = firebase ?? throw new ArgumentNullException(nameof(firebase));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> AddAsync(PlantillaTarea plantilla, CancellationToken ct = default)
        {
            if (plantilla == null) throw new ArgumentNullException(nameof(plantilla));

            var firestoreEntity = plantilla.Adapt<FirestorePlantillaTarea>();
            await _firebase.AddAsync(COLLECTION, firestoreEntity.PlantillaId, firestoreEntity);
            return firestoreEntity.PlantillaId;
        }

        public async Task<PlantillaTarea?> GetAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));

            var firestoreEntity = await _firebase.GetAsync<FirestorePlantillaTarea>(COLLECTION, id);
            if (firestoreEntity == null)
            {
                _logger.LogInformation("GetAsync: no document found in collection {Collection} with id {Id}", COLLECTION, id);
                Console.WriteLine($"[DEBUG] GetAsync: no document found in collection {COLLECTION} with id {id}");
                return null;
            }
            var entity = firestoreEntity.Adapt<PlantillaTarea>();
            _logger.LogInformation("GetAsync: document found id={Id}", id);
            Console.WriteLine($"[DEBUG] GetAsync: document found id={id}");
            return entity;
        }

        public async Task<List<PlantillaTarea>> GetAllAsync(CancellationToken ct = default)
        {
            var firestoreEntities = await _firebase.GetAllAsync<FirestorePlantillaTarea>(COLLECTION);

            List<PlantillaTarea> lista = new List<PlantillaTarea>();

            if (firestoreEntities == null || !firestoreEntities.Any())
                return lista;

            foreach (var entity in firestoreEntities)
            {
                var entityplantilla = entity.Adapt<PlantillaTarea>();
                lista.Add(entityplantilla);
            }

            return lista;
        }

        public async Task UpdateAsync(string id, PlantillaTarea plantilla, CancellationToken ct = default)
        {
            if (plantilla == null) throw new ArgumentNullException(nameof(plantilla));

            var firestoreEntity = plantilla.Adapt<FirestorePlantillaTarea>();
            firestoreEntity.HoraLimite = plantilla.HoraLimite.ToString();
            firestoreEntity.StartDate = plantilla.StartDate?.ToString();
            firestoreEntity.EndDate = plantilla.EndDate?.ToString();
            await _firebase.UpdateAsync(COLLECTION, firestoreEntity.PlantillaId, firestoreEntity);

        }

        public async Task DeleteAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            await _firebase.DeleteAsync(COLLECTION, id);
        }

        public async Task<PlantillaTarea?> GetByEspacioAndIdAsync(string espacioid, string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(espacioid)) throw new ArgumentException("espacioid requerido", nameof(espacioid));
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("id requerido", nameof(id));

            _logger.LogInformation("GetByEspacioAndIdAsync: attempting direct Get by document id {Id} in {Collection}", id, COLLECTION);
            Console.WriteLine($"[DEBUG] GetByEspacioAndIdAsync: attempting direct Get by document id {id} in {COLLECTION}");

            // Try direct get by document id first (faster and less error prone)
            var direct = await _firebase.GetAsync<FirestorePlantillaTarea>(COLLECTION, id, ct);
            if (direct != null)
            {
                _logger.LogInformation("GetByEspacioAndIdAsync: direct get found document with id={Id}, EspacioId={EspacioId}", id, direct.EspacioId);
                Console.WriteLine($"[DEBUG] GetByEspacioAndIdAsync: direct get found document id={id}, EspacioId={direct.EspacioId}");
                if (string.Equals(direct.EspacioId, espacioid, StringComparison.OrdinalIgnoreCase))
                {
                    return direct.Adapt<PlantillaTarea>();
                }
                else
                {
                    _logger.LogInformation("GetByEspacioAndIdAsync: direct document EspacioId does not match requested EspacioId. direct={DirectEspacio}, requested={Requested}", direct.EspacioId, espacioid);
                    Console.WriteLine($"[DEBUG] GetByEspacioAndIdAsync: direct document EspacioId={direct.EspacioId} does not match requested EspacioId={espacioid}");
                    // continue to query by fields in case the plantillaid passed is not document id but a field
                }
            }

            _logger.LogInformation("GetByEspacioAndIdAsync: querying collection {Collection} for EspacioId={EspacioId} PlantillaId={PlantillaId}", COLLECTION, espacioid, id);
            Console.WriteLine($"[DEBUG] GetByEspacioAndIdAsync: querying {COLLECTION} EspacioId={espacioid} PlantillaId={id}");

            var conditions = new (string field, object val)[]
            {
                (nameof(FirestorePlantillaTarea.EspacioId), espacioid),
                (nameof(FirestorePlantillaTarea.PlantillaId), id)
            };

            var firestoreEntities = await _firebase.QueryMultipleConditionsAsync<FirestorePlantillaTarea>(COLLECTION, conditions, ct);
            if (firestoreEntities == null)
            {
                _logger.LogInformation("GetByEspacioAndIdAsync: Query returned null for EspacioId={EspacioId} PlantillaId={PlantillaId}", espacioid, id);
                Console.WriteLine($"[DEBUG] GetByEspacioAndIdAsync: Query returned null for EspacioId={espacioid} PlantillaId={id}");
                return null;
            }

            var first = firestoreEntities.FirstOrDefault();
            _logger.LogInformation("GetByEspacioAndIdAsync: Query returned {Count} results, firstId={FirstId}", firestoreEntities.Count, first?.PlantillaId);
            Console.WriteLine($"[DEBUG] GetByEspacioAndIdAsync: Query returned {firestoreEntities.Count} results, firstId={first?.PlantillaId}");

            if (first == null) return null;

            return first.Adapt<PlantillaTarea>();
        }
    }
}