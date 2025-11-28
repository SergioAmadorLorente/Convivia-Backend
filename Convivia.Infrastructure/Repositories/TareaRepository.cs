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
    public class TareaRepository : ITareaRepository<Tarea>
    {
        private const string COLLECTION = "tareas";
        private readonly IFirebaseService _firebase;

        public TareaRepository(IFirebaseService firebase)
        {
            _firebase = firebase ?? throw new ArgumentNullException(nameof(firebase));
        }

        public async Task AddAsync(Tarea Tarea)
        {
            if (Tarea == null) throw new ArgumentNullException(nameof(Tarea));
            Console.WriteLine($"Firestore.FechaLimite Kind: {Tarea.FechaLimite.Kind}");
            Console.WriteLine($"Firestore.Prorroga Kind: {Tarea.Prorroga?.Kind}");
            var firestoreEntity = Tarea.Adapt<FirestoreTarea>();
            
            await _firebase.AddAsync(COLLECTION, firestoreEntity.Id, firestoreEntity);
        }

        public async Task<Tarea?> GetAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));

            var firestoreEntity = await _firebase.GetAsync<FirestoreTarea>(COLLECTION, id);
            return firestoreEntity?.Adapt<Tarea>();
        }

        public async Task<List<Tarea>> GetAllAsync()
        {
            var firestoreEntities = await _firebase.GetAllAsync<FirestoreTarea>(COLLECTION);

            if (firestoreEntities == null || !firestoreEntities.Any())
                return new List<Tarea>();

            return firestoreEntities
                .Select(e => e.Adapt<Tarea>())
                .ToList();
        }

        public async Task UpdateAsync(Tarea Tarea)
        {
            if (Tarea == null) throw new ArgumentNullException(nameof(Tarea));

            var firestoreEntity = Tarea.Adapt<FirestoreTarea>();
            await _firebase.UpdateAsync(COLLECTION, firestoreEntity.Id, firestoreEntity);
        }

        public async Task DeleteAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            await _firebase.DeleteAsync(COLLECTION, id);
        }

        public async Task<List<Tarea>> QueryByFieldAsync(string field, object value)
        {
            throw new NotImplementedException();
        }
    }
}