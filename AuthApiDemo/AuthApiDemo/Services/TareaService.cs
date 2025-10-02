using AuthApiDemo.Models;
using AuthApiDemo.DTOs;
using AuthApiDemo.Mappers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AuthApiDemo.Services
{
    public class TareaService
    {
        public const string COLLECTION = "tareas";
        private readonly FirebaseService _firebase;

        public TareaService(FirebaseService firebase)
        {
            _firebase = firebase;
        }

        // Crear tarea
        public async Task<TareaDto> AddAsync(CreateTareaDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre))
                throw new ArgumentException("El nombre no puede estar vacío.");

            var idTarea = Guid.NewGuid().ToString();
            var persist = TareaMapper.ToPersist(dto, idTarea);

            // Regla: evitar duplicados por nombre y fecha límite
            var existing = await _firebase.QueryMultipleConditionsAsync<TareaPersist>(
                COLLECTION,
                new Dictionary<string, object> { { "Nombre", dto.Nombre }, { "FechaLimite", dto.FechaLimite } }
            );
            if (existing.Count > 0)
                throw new InvalidOperationException("Ya existe una tarea con ese nombre y fecha límite.");

            await _firebase.AddAsync(COLLECTION, idTarea, persist);
            return TareaMapper.ToDto(persist);
        }

        // Obtener tarea por id
        public async Task<TareaDto?> GetAsync(string id)
        {
            var persist = await _firebase.GetAsync<TareaPersist>(COLLECTION, id);
            return persist != null ? TareaMapper.ToDto(persist) : null;
        }

        // Listar todas las tareas
        public async Task<List<TareaDto>> GetAllAsync()
        {
            var list = await _firebase.QueryAsync<TareaPersist>(COLLECTION);
            var result = new List<TareaDto>();
            foreach (var item in list)
                result.Add(TareaMapper.ToDto(item));
            return result;
        }

        // Actualizar tarea
        public async Task<TareaDto?> UpdateAsync(string id, CreateTareaDto dto)
        {
            var persist = await _firebase.GetAsync<TareaPersist>(COLLECTION, id);
            if (persist == null)
                return null;

            persist.Nombre = dto.Nombre;
            persist.FechaLimite = dto.FechaLimite;
            persist.Descripcion = dto.Descripcion;
            persist.UsuarioEspacioIds = dto.UsuarioEspacioIds ?? new List<string>();
            persist.Karma = dto.Karma;
            persist.Foto = dto.Foto;
            persist.Prorroga = dto.Prorroga;

            await _firebase.UpdateAsync(COLLECTION, id, persist);
            return TareaMapper.ToDto(persist);
        }

        // Eliminar tarea
        public async Task<bool> DeleteAsync(string id)
        {
            var persist = await _firebase.GetAsync<TareaPersist>(COLLECTION, id);
            if (persist == null)
                return false;
            await _firebase.DeleteAsync(COLLECTION, id);
            return true;
        }
    }
}
