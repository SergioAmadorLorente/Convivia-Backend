using Convivia.Domain.Entities;
using Convivia.Domain.Models;
using Convivia.Domain.Repositories;
using Convivia.Infrastructure.Models;
using Convivia.Shared.DTOs;
using Convivia.Shared.Repositories;
using Convivia.Shared.Services;
using Mapster;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Convivia.Infrastructure.Repositories
{
    public class TareaRepository : ITareaRepository
    {
        private const string COLLECTION = "tareas";
        private readonly IFirebaseService _firebase;
        private readonly ILogger<TareaRepository> _logger;

        public TareaRepository(IFirebaseService firebase, ILogger<TareaRepository> logger)
        {
            _firebase = firebase ?? throw new ArgumentNullException(nameof(firebase));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> AddAsync(Tarea tarea, CancellationToken ct = default)
        {
            if (tarea == null) throw new ArgumentNullException(nameof(tarea));
            var firestoreEntity = tarea.Adapt<FirestoreTarea>();
            await _firebase.AddAsync(COLLECTION, firestoreEntity.Id, firestoreEntity);
            return firestoreEntity.Id;
        }

        public async Task<Tarea?> GetAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));

            var firestoreEntity = await _firebase.GetAsync<FirestoreTarea>(COLLECTION, id);
            var entity = firestoreEntity?.Adapt<Tarea>();
            return entity;
        }

        public async Task<List<Tarea>> GetAllByEspacioIdAsync(string espacioid, CancellationToken ct = default)
        {

            if(string.IsNullOrWhiteSpace(espacioid)) throw new ArgumentNullException(nameof(espacioid));
            var entidadesEspacio = await _firebase.QueryAsync<FirestoreTarea>(COLLECTION, nameof(FirestoreTarea.EspacioId), espacioid);
            List<Tarea> lista = new List<Tarea>();
            if (!entidadesEspacio.Any())
                return lista;
            foreach(var pte in entidadesEspacio)
            {

                var entidadmapeada = pte.Adapt<Tarea>();
                lista.Add(entidadmapeada);

            }

            return lista;
        }

        public async Task<List<Tarea>> GetAllAsync(CancellationToken ct = default)
        {
            var firestoreEntities = await _firebase.GetAllAsync<FirestoreTarea>(COLLECTION);

            List<Tarea> lista = new List<Tarea>();

            if (firestoreEntities == null || !firestoreEntities.Any())
                return lista;

            foreach (var entity in firestoreEntities)
            {
                var entitytarea = entity.Adapt<Tarea>();
                lista.Add(entitytarea);
            }

            return lista;
        }

        public async Task UpdateAsync(string id, Tarea tarea, CancellationToken ct = default)
        {
            if (tarea == null) throw new ArgumentNullException(nameof(tarea));

            var firestoreEntity = tarea.Adapt<FirestoreTarea>();
            await _firebase.UpdateAsync(COLLECTION, firestoreEntity.Id, firestoreEntity);

        }

        public async Task DeleteAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            await _firebase.DeleteAsync(COLLECTION, id);
        }

        public async Task<List<Tarea>> GetByUsuarioEspacioIdAsync(string usuarioEspacioid, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(usuarioEspacioid)) throw new ArgumentNullException(nameof(usuarioEspacioid));
            var tareasUsuarioEspacio = await _firebase.QueryAsync<FirestoreTarea>(COLLECTION, nameof(FirestoreTarea.UsuarioEspacioId), usuarioEspacioid);
            List<Tarea> lista = new List<Tarea>();
            if (!tareasUsuarioEspacio.Any())
                return lista;
            foreach (var pte in tareasUsuarioEspacio)
            {

                var tareamapped = pte.Adapt<Tarea>();
                lista.Add(tareamapped);

            }

            return lista;
        }
    }
}