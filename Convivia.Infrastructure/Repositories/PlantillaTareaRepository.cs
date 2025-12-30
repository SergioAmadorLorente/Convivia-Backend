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

        public PlantillaTareaRepository(
            IFirebaseService firebase,
            ILogger<PlantillaTareaRepository> logger,
            ILoggerFactory loggerFactory)
            : base(firebase, loggerFactory.CreateLogger<Repository<FirestorePlantillaTarea>>(), COLLECTION)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> AddAsync(PlantillaTarea plantilla, CancellationToken ct = default)
        {
            if (plantilla == null) throw new ArgumentNullException(nameof(plantilla));
            var firestoreEntity = plantilla.Adapt<FirestorePlantillaTarea>();
            return await base.AddAsync(firestoreEntity, ct);
        }

        public async Task<PlantillaTarea?> GetByIdAsync(string id, CancellationToken ct = default)
        {
            var firestoreEntity = await base.GetByIdAsync(id, ct);
            return firestoreEntity?.Adapt<PlantillaTarea>();
        }

        public async Task<IEnumerable<PlantillaTarea>> GetAllAsync(CancellationToken ct = default)
        {
            var firestoreEntities = await base.GetAllAsync(ct);
            return firestoreEntities == null ? new List<PlantillaTarea>() : firestoreEntities.Select(e => e.Adapt<PlantillaTarea>()).ToList();
        }

        public async Task UpdateAsync(string id, PlantillaTarea plantilla, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (plantilla == null) throw new ArgumentNullException(nameof(plantilla));

            var firestoreEntity = plantilla.Adapt<FirestorePlantillaTarea>();
            await base.UpdateAsync(id, firestoreEntity, ct);
        }

        public async Task UpdateAsync(string id, PlantillaTarea plantilla, bool merge, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (plantilla == null) throw new ArgumentNullException(nameof(plantilla));

            var firestoreEntity = plantilla.Adapt<FirestorePlantillaTarea>();
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