using Convivia.Domain.Models;
using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Convivia.Shared.Services;
using Convivia.Infrastructure.Models;
using Convivia.Shared.Repositories;

namespace Convivia.Infrastructure.Repositories
{
    public class PlantillaTareaRepository : IPlantillaTareaRepository<PlantillaTarea>
    {
        private const string COLLECTION = "plantillatareas";
        private readonly IFirebaseService _firebase;

        public PlantillaTareaRepository(IFirebaseService firebase)
        {
            _firebase = firebase ?? throw new ArgumentNullException(nameof(firebase));
        }

        public async Task AddAsync(PlantillaTarea plantilla)
        {
            if (plantilla == null) throw new ArgumentNullException(nameof(plantilla));

            var firestoreEntity = plantilla.Adapt<FirestorePlantillaTarea>();
            await _firebase.AddAsync(COLLECTION, firestoreEntity.PlantillaId, firestoreEntity);
        }

        public async Task<PlantillaTarea?> GetAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));

            var firestoreEntity = await _firebase.GetAsync<FirestorePlantillaTarea>(COLLECTION, id);
            return firestoreEntity?.Adapt<PlantillaTarea>();
        }

        public async Task<List<PlantillaTarea>> GetAllAsync()
        {
            var firestoreEntities = await _firebase.GetAllAsync<FirestorePlantillaTarea>(COLLECTION);

            if (firestoreEntities == null || !firestoreEntities.Any())
                return new List<PlantillaTarea>();

            return firestoreEntities
                .Select(e => e.Adapt<PlantillaTarea>())
                .ToList();
        }

        public async Task UpdateAsync(PlantillaTarea plantilla)
        {
            if (plantilla == null) throw new ArgumentNullException(nameof(plantilla));

            var firestoreEntity = plantilla.Adapt<FirestorePlantillaTarea>();
            await _firebase.UpdateAsync(COLLECTION, firestoreEntity.PlantillaId, firestoreEntity);
        }

        public async Task DeleteAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            await _firebase.DeleteAsync(COLLECTION, id);
        }

        public async Task<List<PlantillaTarea>> QueryByFieldAsync(string field, object value)
        {
            throw new NotImplementedException();
        }
    }
}