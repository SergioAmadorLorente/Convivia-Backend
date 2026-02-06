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

        private string GetSubcollectionPath(string espacioId, string plantillaId)
        {
            if (string.IsNullOrWhiteSpace(espacioId)) throw new ArgumentException("espacioId requerido", nameof(espacioId));
            if (string.IsNullOrWhiteSpace(plantillaId)) throw new ArgumentException("plantillaId requerido", nameof(plantillaId));
            return $"espacios/{espacioId}/plantillatareas/{plantillaId}/tareas";
        }

        public async Task<List<string>> AddAsyncList(string espacioId, List<Tarea> tareas, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(espacioId)) throw new ArgumentException("espacioId requerido", nameof(espacioId));
            if (tareas == null) throw new ArgumentNullException(nameof(tareas));
            var ids = new List<string>();
            foreach (var tarea in tareas)
            {
                if (string.IsNullOrWhiteSpace(tarea.PlantillaId)) throw new ArgumentException("PlantillaId requerido en Tarea", nameof(tarea));
                
                var firestoreEntity = tarea.Adapt<FirestoreTarea>();

                if (tarea.HoraLimite.HasValue)
                    firestoreEntity.HoraLimite = tarea.HoraLimite.Value.ToString("HH:mm");
                else
                    firestoreEntity.HoraLimite = null;

                var subcollectionPath = GetSubcollectionPath(espacioId, tarea.PlantillaId);
                await _firebase.AddAsync(subcollectionPath, tarea.Id, firestoreEntity, ct);
                ids.Add(tarea.Id);
            }

            return ids;
        }

        public async Task<string> AddAsync(string espacioId, Tarea tarea, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(espacioId)) throw new ArgumentException("espacioId requerido", nameof(espacioId));
            if (tarea == null) throw new ArgumentNullException(nameof(tarea));
            if (string.IsNullOrWhiteSpace(tarea.PlantillaId)) throw new ArgumentException("PlantillaId requerido en Tarea", nameof(tarea));

            var firestoreEntity = tarea.Adapt<FirestoreTarea>();

            if (tarea.HoraLimite.HasValue)
                firestoreEntity.HoraLimite = tarea.HoraLimite.Value.ToString("HH:mm");
            else
                firestoreEntity.HoraLimite = null;

            var subcollectionPath = GetSubcollectionPath(espacioId, tarea.PlantillaId);
            await _firebase.AddAsync(subcollectionPath, tarea.Id, firestoreEntity, ct);
            return tarea.Id;
        }

        public async Task<string> AddAsync(Tarea tarea, CancellationToken ct = default)
        {
            throw new NotImplementedException("Use AddAsync with espacioId. Tarea requires espacioId context.");
        }

        public async Task<Tarea> GetByIdAsync(string id, CancellationToken ct = default)
        {
            throw new NotImplementedException("Usar GetAsync.");
        }

        public async Task<Tarea?> GetInstanciaAsync(string plantillaId, string tareaId, CancellationToken ct = default)
        {
            throw new NotImplementedException("Use GetInstanciaAsync with espacioId. Tarea requires espacioId.");
        }

        public async Task<Tarea?> GetInstanciaAsync(string espacioId, string plantillaId, string tareaId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(espacioId)) throw new ArgumentException("espacioId requerido", nameof(espacioId));
            if (string.IsNullOrWhiteSpace(plantillaId)) throw new ArgumentException("plantillaId requerido", nameof(plantillaId));
            if (string.IsNullOrWhiteSpace(tareaId)) throw new ArgumentException("tareaId requerido", nameof(tareaId));

            var subcollectionPath = GetSubcollectionPath(espacioId, plantillaId);
            var firestoreEntity = await _firebase.GetAsync<FirestoreTarea>(subcollectionPath, tareaId, ct);
            if (firestoreEntity == null) return null;
            var entity = firestoreEntity.Adapt<Tarea>();
            var ft = firestoreEntity;

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

        public async Task UpdateAsync(string espacioId, string id, Tarea tareaActualizada, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(espacioId))
                throw new ArgumentException("espacioId requerido", nameof(espacioId));

            if (tareaActualizada == null)
                throw new ArgumentNullException(nameof(tareaActualizada));

            if (string.IsNullOrWhiteSpace(tareaActualizada.PlantillaId))
                throw new ArgumentException("PlantillaId requerido en Tarea", nameof(tareaActualizada));

            if (string.IsNullOrWhiteSpace(tareaActualizada.Id))
                throw new ArgumentException("Id de la tarea requerido", nameof(tareaActualizada));

            var firestoreEntity = tareaActualizada.Adapt<FirestoreTarea>();

            if (tareaActualizada.HoraLimite.HasValue)
                firestoreEntity.HoraLimite = tareaActualizada.HoraLimite.Value.ToString("HH:mm");
            else
                firestoreEntity.HoraLimite = null;

            var subcollectionPath = GetSubcollectionPath(espacioId, tareaActualizada.PlantillaId);

            await _firebase.UpdateAsync(subcollectionPath, id, firestoreEntity,
                                        cancellationToken: ct);
        }

        public async Task UpdateAsync(string espacioId, string id, Tarea tareaActualizada, bool merge, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(espacioId))
                throw new ArgumentException("espacioId requerido", nameof(espacioId));

            if (tareaActualizada == null)
                throw new ArgumentNullException(nameof(tareaActualizada));

            if (string.IsNullOrWhiteSpace(tareaActualizada.PlantillaId))
                throw new ArgumentException("PlantillaId requerido en Tarea", nameof(tareaActualizada));

            if (string.IsNullOrWhiteSpace(tareaActualizada.Id))
                throw new ArgumentException("Id de la tarea requerido", nameof(tareaActualizada));

            var firestoreEntity = tareaActualizada.Adapt<FirestoreTarea>();
            var subcollectionPath = GetSubcollectionPath(espacioId, tareaActualizada.PlantillaId);

            if (tareaActualizada.HoraLimite.HasValue)
                firestoreEntity.HoraLimite = tareaActualizada.HoraLimite.Value.ToString("HH:mm");
            else
                firestoreEntity.HoraLimite = null;
            await _firebase.UpdateAsync(subcollectionPath, id, firestoreEntity, merge, ct);
        }

        public async Task UpdateAsync(string id, Tarea tareaActualizada, CancellationToken ct = default)
        {
            throw new NotImplementedException("Use UpdateAsync with espacioId. Tarea requires espacioId context.");
        }

        public async Task UpdateAsync(string id, Tarea tareaActualizada, bool merge, CancellationToken ct = default)
        {
            throw new NotImplementedException("Use UpdateAsync with espacioId. Tarea requires espacioId context.");
        }

        public async Task UpdateAsync(string id, IDictionary<string, object> updates, bool useSetMerge = true, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("id requerido", nameof(id));
            if (updates == null) throw new ArgumentNullException(nameof(updates));
            if (updates.Count == 0) return;

            var (espacioId, plantillaId) = await FindEspacioAndPlantillaIdForTareaAsync(id, ct);
            if (string.IsNullOrWhiteSpace(espacioId) || string.IsNullOrWhiteSpace(plantillaId)) 
                throw new KeyNotFoundException($"Tarea {id} no encontrada");

            var subcollectionPath = GetSubcollectionPath(espacioId, plantillaId);
            await _firebase.UpdateAsync(subcollectionPath, id, updates, useSetMerge, ct);
        }

        public override async Task DeleteAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("id requerido", nameof(id));

            var (espacioId, plantillaId) = await FindEspacioAndPlantillaIdForTareaAsync(id, ct);
            if (string.IsNullOrWhiteSpace(espacioId) || string.IsNullOrWhiteSpace(plantillaId)) 
                throw new KeyNotFoundException($"Tarea {id} no encontrada");

            var subcollectionPath = GetSubcollectionPath(espacioId, plantillaId);
            await _firebase.DeleteAsync(subcollectionPath, id, ct);
        }

        private async Task<(string? espacioId, string? plantillaId)> FindEspacioAndPlantillaIdForTareaAsync(string tareaId, CancellationToken ct = default)
        {
            var espacios = await _firebase.GetAllAsync<FireStoreEspacio>("espacios", ct);
            if (espacios == null) return (null, null);

            foreach (var espacio in espacios)
            {
                if (string.IsNullOrWhiteSpace(espacio.Id)) continue;
                
                var plantillasPath = $"espacios/{espacio.Id}/plantillatareas";
                var plantillas = await _firebase.GetAllAsync<FirestorePlantillaTarea>(plantillasPath, ct);
                if (plantillas == null) continue;

                foreach (var p in plantillas)
                {
                    if (string.IsNullOrWhiteSpace(p.Id)) continue;
                    var subPath = GetSubcollectionPath(espacio.Id, p.Id);
                    var tarea = await _firebase.GetAsync<FirestoreTarea>(subPath, tareaId, ct);
                    if (tarea != null)
                        return (espacio.Id, p.Id);
                }
            }

            return (null, null);
        }

        public async Task<List<Tarea>> GetByUsuarioEspacioIdAsync(string usuarioEspacioId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(usuarioEspacioId))
                throw new ArgumentNullException(nameof(usuarioEspacioId));

            // Consulta Firestore por UsuarioEspacioId
            var tareasUsuarioEspacio = await _firebase
                .QueryAsync<FirestoreTarea>(_collection, nameof(FirestoreTarea.UsuarioEspacioId), usuarioEspacioId)
                .ConfigureAwait(false);

            // Si no hay tareas, devolver lista vacía
            if (!tareasUsuarioEspacio.Any())
                return new List<Tarea>();

            // Mapear FirestoreTarea → Tarea
            var lista = new List<Tarea>();
            foreach (var pte in tareasUsuarioEspacio)
            {
                var tareaMapped = pte.Adapt<Tarea>()!;
                lista.Add(tareaMapped);
            }

            return lista;
        }

    }
}