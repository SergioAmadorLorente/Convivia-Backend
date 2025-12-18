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
    public class PlantillaTareaRepository : Repository<FirestorePlantillaTarea>, IPlantillaTareaRepository
    {
        private const string COLLECTION = "plantillatareas";
        private readonly IFirebaseService _firebase;
        private readonly ILogger<PlantillaTareaRepository> _logger;

        public PlantillaTareaRepository(IFirebaseService firebase, ILogger<PlantillaTareaRepository> logger)
            : base(firebase, logger: logger as ILogger<Repository<FirestorePlantillaTarea>> ?? throw new ArgumentNullException(nameof(logger)), collection: "plantillatareas")
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

        public async Task<PlantillaTarea?> GetByIdAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));

            var firestoreEntity = await _firebase.GetAsync<FirestorePlantillaTarea>(COLLECTION, id);
            if (firestoreEntity == null)
            {
                return null;
            }
            var entity = firestoreEntity.Adapt<PlantillaTarea>();
            return entity;
        }

        public async Task<IEnumerable<PlantillaTarea>> GetAllAsync(CancellationToken ct = default)
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
            await _firebase.UpdateAsync(COLLECTION, firestoreEntity.PlantillaId, firestoreEntity);

        }

        public async Task UpdateAsync(string id, PlantillaTarea entitie, bool merge, CancellationToken ct = default)
        {

            throw new NotImplementedException();

        }

        public async Task UpdateAsync(string id, IDictionary<string, object> updates, bool useSetMerge = true, CancellationToken ct = default)
        {

            throw new NotImplementedException();

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

            // Try direct get by document id first (faster and less error prone)
            var direct = await _firebase.GetAsync<FirestorePlantillaTarea>(COLLECTION, id, ct);
            if (direct != null)
            {
                
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

            var conditions = new (string field, object val)[]
            {
                (nameof(FirestorePlantillaTarea.EspacioId), espacioid),
                (nameof(FirestorePlantillaTarea.PlantillaId), id)
            };

            var firestoreEntities = await _firebase.QueryMultipleConditionsAsync<FirestorePlantillaTarea>(COLLECTION, conditions, ct);
            if (firestoreEntities == null)
            {
                return null;
            }

            var first = firestoreEntities.FirstOrDefault();

            if (first == null) return null;

            return first.Adapt<PlantillaTarea>();
        }
    }
}