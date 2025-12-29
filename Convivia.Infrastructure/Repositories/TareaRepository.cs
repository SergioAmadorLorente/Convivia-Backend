using Convivia.Domain.Entities;
using Convivia.Application.Repositories;
using Convivia.Infrastructure.Models;
using Convivia.Shared.Services;
using Mapster;
using Microsoft.Extensions.Logging;

namespace Convivia.Infrastructure.Repositories
{
    public class TareaRepository : Repository<FirestoreTarea>, ITareaRepository
    {
        private readonly ILogger<TareaRepository> _logger;

        public TareaRepository(IFirebaseService firebase, ILogger<TareaRepository> logger)
             : base(firebase, logger: logger as ILogger<Repository<FirestoreTarea>> ?? throw new ArgumentNullException(nameof(logger)), collection: "tareas")
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private string GetSubcollectionPath(string plantillaId)
        {
            if (string.IsNullOrWhiteSpace(plantillaId)) throw new ArgumentException("plantillaId requerido", nameof(plantillaId));
            return $"plantillatareas/{plantillaId}/tareas";
        }

        public async Task<List<string>> AddAsyncList(List<Tarea> tareas, CancellationToken ct = default)
        {
            if (tareas == null) throw new ArgumentNullException(nameof(tareas));
            var ids = new List<string>();
            foreach (var tarea in tareas)
            {
                if (string.IsNullOrWhiteSpace(tarea.PlantillaId)) throw new ArgumentException("PlantillaId requerido en Tarea", nameof(tarea));
                var firestoreEntity = tarea.Adapt<FirestoreTarea>();
                if (tarea.Prorroga.HasValue)
                    firestoreEntity.ProrrogaSegundos = tarea.Prorroga.Value.TotalSeconds;
                else
                    firestoreEntity.ProrrogaSegundos = null;

                if (tarea.HoraLimite.HasValue)
                    firestoreEntity.HoraLimite = tarea.HoraLimite.Value.ToString("HH:mm");
                else
                    firestoreEntity.HoraLimite = null;

                var subcollectionPath = GetSubcollectionPath(tarea.PlantillaId);
                await _firebase.AddAsync(subcollectionPath, tarea.Id, firestoreEntity, ct);
                ids.Add(tarea.Id);
            }

            return ids;
        }

        public async Task<string> AddAsync(Tarea tarea, CancellationToken ct = default)
        {
            if (tarea == null) throw new ArgumentNullException(nameof(tarea));
            if (string.IsNullOrWhiteSpace(tarea.PlantillaId)) throw new ArgumentException("PlantillaId requerido en Tarea", nameof(tarea));

            var firestoreEntity = tarea.Adapt<FirestoreTarea>();
            if (tarea.Prorroga.HasValue)
                firestoreEntity.ProrrogaSegundos = tarea.Prorroga.Value.TotalSeconds;
            else
                firestoreEntity.ProrrogaSegundos = null;

            if (tarea.HoraLimite.HasValue)
                firestoreEntity.HoraLimite = tarea.HoraLimite.Value.ToString("HH:mm");
            else
                firestoreEntity.HoraLimite = null;

            var subcollectionPath = GetSubcollectionPath(tarea.PlantillaId);
            await _firebase.AddAsync(subcollectionPath, tarea.Id, firestoreEntity, ct);
            return tarea.Id;
        }

        public async Task<Tarea> GetByIdAsync(string id, CancellationToken ct = default)
        {
            throw new NotImplementedException("Usar GetAsync.");
        }

        public async Task<Tarea?> GetInstanciaAsync(string plantillaId, string tareaId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(plantillaId)) throw new ArgumentException("plantillaId requerido", nameof(plantillaId));
            if (string.IsNullOrWhiteSpace(tareaId)) throw new ArgumentException("tareaId requerido", nameof(tareaId));

            var subcollectionPath = GetSubcollectionPath(plantillaId);
            var firestoreEntity = await _firebase.GetAsync<FirestoreTarea>(subcollectionPath, tareaId, ct);
            if (firestoreEntity == null) return null;
            var entity = firestoreEntity.Adapt<Tarea>();
            var ft = firestoreEntity;
            if (ft.ProrrogaSegundos.HasValue)
                entity.Prorroga = TimeSpan.FromSeconds(ft.ProrrogaSegundos.Value);
            else
                entity.Prorroga = null;

            if (!string.IsNullOrWhiteSpace(ft.HoraLimite))
            {
                if (TimeOnly.TryParse(ft.HoraLimite, out var to))
                    entity.HoraLimite = to;
                else
                    entity.HoraLimite = null;
            }
            else
            {
                entity.HoraLimite = null;
            }

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
            if (tareaActualizada.Prorroga.HasValue)
                firestoreEntity.ProrrogaSegundos = tareaActualizada.Prorroga.Value.TotalSeconds;
            else
                firestoreEntity.ProrrogaSegundos = null;

            if (tareaActualizada.HoraLimite.HasValue)
                firestoreEntity.HoraLimite = tareaActualizada.HoraLimite.Value.ToString("HH:mm");
            else
                firestoreEntity.HoraLimite = null;

            var subcollectionPath = GetSubcollectionPath(tareaActualizada.PlantillaId);

            await _firebase.UpdateAsync(subcollectionPath, id, firestoreEntity,
                                        cancellationToken: ct);
        }

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
            if (tareaActualizada.Prorroga.HasValue)
                firestoreEntity.ProrrogaSegundos = tareaActualizada.Prorroga.Value.TotalSeconds;
            else
                firestoreEntity.ProrrogaSegundos = null;

            if (tareaActualizada.HoraLimite.HasValue)
                firestoreEntity.HoraLimite = tareaActualizada.HoraLimite.Value.ToString("HH:mm");
            else
                firestoreEntity.HoraLimite = null;
            await _firebase.UpdateAsync(subcollectionPath, id, firestoreEntity, merge, ct);
        }

        public async Task UpdateAsync(string id, IDictionary<string, object> updates, bool useSetMerge = true, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("id requerido", nameof(id));
            if (updates == null) throw new ArgumentNullException(nameof(updates));
            if (updates.Count == 0) return;

            var plantillaId = await FindPlantillaIdForTareaAsync(id, ct);
            if (string.IsNullOrWhiteSpace(plantillaId)) throw new KeyNotFoundException($"Tarea {id} no encontrada");

            var subcollectionPath = GetSubcollectionPath(plantillaId);
            await _firebase.UpdateAsync(subcollectionPath, id, updates, useSetMerge, ct);
        }

        public override async Task DeleteAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("id requerido", nameof(id));

            var plantillaId = await FindPlantillaIdForTareaAsync(id, ct);
            if (string.IsNullOrWhiteSpace(plantillaId)) throw new KeyNotFoundException($"Tarea {id} no encontrada");

            var subcollectionPath = GetSubcollectionPath(plantillaId);
            await _firebase.DeleteAsync(subcollectionPath, id, ct);
        }

        private async Task<string?> FindPlantillaIdForTareaAsync(string tareaId, CancellationToken ct = default)
        {
            var plantillas = await _firebase.GetAllAsync<FirestorePlantillaTarea>("plantillatareas", ct);
            if (plantillas == null) return null;

            foreach (var p in plantillas)
            {
                if (string.IsNullOrWhiteSpace(p.Id)) continue;
                var subPath = GetSubcollectionPath(p.Id);
                var tarea = await _firebase.GetAsync<FirestoreTarea>(subPath, tareaId, ct);
                if (tarea != null)
                    return p.Id;
            }

            return null;
        }
    }
}