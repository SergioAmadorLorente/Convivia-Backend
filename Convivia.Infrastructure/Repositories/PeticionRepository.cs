using Convivia.Infrastructure.Models;
using Convivia.Domain.Models;
using Convivia.Application.Interfaces;
using Convivia.Shared.Services;

namespace Convivia.Infrastructure.Repositories
{
    /// <summary>
    /// Implementación del repositorio de peticiones usando Firebase
    /// Toda la lógica de Firestore está encapsulada aquí
    /// </summary>
    public class PeticionRepository : IPeticionRepository
    {
        private readonly IFirebaseService _firebaseService;
        private const string COLLECTION = "peticiones";

        public PeticionRepository(IFirebaseService firebaseService)
        {
            _firebaseService = firebaseService ?? throw new ArgumentNullException(nameof(firebaseService));
        }

        public async Task<Peticion> AddAsync(Peticion peticion)
        {
            if (peticion == null) throw new ArgumentNullException(nameof(peticion));

            // Convertir entidad de dominio a modelo de persistencia
            var persist = new PeticionPersist
            {
                Id = peticion.Id,
                Mensaje = peticion.Mensaje,
                Fecha = peticion.Fecha,
                Estado = peticion.Estado,
                IdSolicitante = peticion.IdSolicitante,
                IdEspacio = peticion.IdEspacio
            };

            // Guardar en Firebase
            await _firebaseService.AddAsync(COLLECTION, peticion.Id, persist);

            return peticion;
        }

        public async Task<Peticion?> GetByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));

            var persist = await _firebaseService.GetAsync<PeticionPersist>(COLLECTION, id);
            if (persist == null) return null;

            // Reconstruir entidad de dominio desde modelo de persistencia
            return Peticion.Reconstruir(persist.Id, persist.Mensaje, persist.Fecha, persist.Estado, persist.IdSolicitante, persist.IdEspacio);
        }

        public async Task<List<Peticion>> GetAllAsync()
        {
            var persistList = await _firebaseService.GetAllAsync<PeticionPersist>(COLLECTION);
            
            var entities = new List<Peticion>();
            foreach (var persist in persistList)
            {
                entities.Add(Peticion.Reconstruir(persist.Id, persist.Mensaje, persist.Fecha, persist.Estado, persist.IdSolicitante, persist.IdEspacio));
            }

            return entities;
        }

        public async Task<List<Peticion>> GetByEstadoAsync(string estado)
        {
            if (string.IsNullOrWhiteSpace(estado)) throw new ArgumentNullException(nameof(estado));

            var persistList = await _firebaseService.QueryAsync<PeticionPersist>(COLLECTION, "Estado", estado);
            
            var entities = new List<Peticion>();
            foreach (var persist in persistList)
            {
                entities.Add(Peticion.Reconstruir(persist.Id, persist.Mensaje, persist.Fecha, persist.Estado, persist.IdSolicitante, persist.IdEspacio));
            }

            return entities;
        }

        public async Task<Peticion> UpdateAsync(Peticion peticion)
        {
            if (peticion == null) throw new ArgumentNullException(nameof(peticion));

            // Convertir entidad de dominio a modelo de persistencia
            var persist = new PeticionPersist
            {
                Id = peticion.Id,
                Mensaje = peticion.Mensaje,
                Fecha = peticion.Fecha,
                Estado = peticion.Estado,
                IdSolicitante = peticion.IdSolicitante,
                IdEspacio = peticion.IdEspacio
            };

            // Actualizar en Firebase
            await _firebaseService.UpdateAsync(COLLECTION, peticion.Id, persist);

            return peticion;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));

            var persist = await _firebaseService.GetAsync<PeticionPersist>(COLLECTION, id);
            if (persist == null) return false;

            await _firebaseService.DeleteAsync(COLLECTION, id);
            return true;
        }
    }
}
