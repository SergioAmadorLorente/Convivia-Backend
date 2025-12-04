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
            var entity = firestoreEntity?.Adapt<PlantillaTarea>();
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
            await _firebase.UpdateAsync(COLLECTION, firestoreEntity.PlantillaId, firestoreEntity);

        }

        public async Task DeleteAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            await _firebase.DeleteAsync(COLLECTION, id);
        }
    }
}