using System;
using System.Threading.Tasks;
using Convivia.Domain.Models;
using Convivia.Application.DTOs;
using Convivia.Application.Mappers;
using System.Collections.Generic;

namespace Convivia.Infrastructure.Services
{
    public class PlantillaTareaService
    {
        public const string COLLECTION = "plantillatareas";
        private readonly IFirebaseService _firebase;

        public PlantillaTareaService(IFirebaseService firebase)
        {
            _firebase = firebase ?? throw new ArgumentNullException(nameof(firebase));
        }

        public async Task<PlantillaTareaDto> AddAsync(CreatePlantillaTareaDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Nombre)) throw new ArgumentException("El nombre no puede estar vacío.");

            var id = Guid.NewGuid().ToString();
            var persist = PlantillaTareaMapper.ToPersist(dto, id);

            var existing = await _firebase.QueryAsync<PlantillaTareaPersist>(COLLECTION, "Nombre", dto.Nombre);
            if (existing.Count > 0)
                throw new InvalidOperationException("Ya existe una plantilla de tarea con ese nombre.");

            await _firebase.AddAsync(COLLECTION, id, persist);
            return PlantillaTareaMapper.ToDto(persist);
        }

        public async Task<PlantillaTareaDto?> GetAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            var persist = await _firebase.GetAsync<PlantillaTareaPersist>(COLLECTION, id);
            return persist != null ? PlantillaTareaMapper.ToDto(persist) : null;
        }

        public async Task<List<PlantillaTareaDto>> GetAllAsync()
        {
            var list = await _firebase.GetAllAsync<PlantillaTareaPersist>(COLLECTION);
            var result = new List<PlantillaTareaDto>(list.Count);
            foreach (var item in list)
                result.Add(PlantillaTareaMapper.ToDto(item));
            return result;
        }

        public async Task<PlantillaTareaDto?> UpdateAsync(string id, CreatePlantillaTareaDto dto)
        {
            var persist = await _firebase.GetAsync<PlantillaTareaPersist>(COLLECTION, id);
            if (persist == null) return null;

            persist.Nombre = dto.Nombre;
            persist.FechaCreacion = dto.FechaCreacion;
            persist.PuntosKarma = dto.PuntosKarma;
            persist.Disponible = dto.Disponible;
            persist.DiasRepeticion = dto.DiasRepeticion ?? new List<int>();

            await _firebase.UpdateAsync(COLLECTION, id, persist);
            return PlantillaTareaMapper.ToDto(persist);
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var persist = await _firebase.GetAsync<PlantillaTareaPersist>(COLLECTION, id);
            if (persist == null) return false;
            await _firebase.DeleteAsync(COLLECTION, id);
            return true;
        }
    }
}
