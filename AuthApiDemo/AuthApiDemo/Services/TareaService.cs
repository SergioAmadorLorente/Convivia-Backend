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
        private readonly IFirebaseService _firebase;

        public TareaService(IFirebaseService firebase)
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

            var existing = await _firebase.QueryMultipleConditionsAsync<TareaPersist>(
                COLLECTION,
                new (string field, object val)[] { ("Nombre", dto.Nombre), ("FechaLimite", dto.FechaLimite) }
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

        /*public async Task<List<TareaDto>> GetByUsuarioAsync(string idusuario)
        {

            var tareaPersist = await _firebase.QueryAsync<TareaPersist>(COLLECTION, "UsuarioEspacioIds", idusuario);
            var tareaDto = new List<TareaDto>();

            foreach (var tarea in tareaPersist)
            {

                var tareafinal = TareaMapper.ToDto(tarea);
                if(tareafinal != null) { 
                    tareaDto.Add(tareafinal);
                }

            }

            return tareaDto;

        }*/

        // Listar todas las tareas
        public async Task<List<TareaDto>> GetAllAsync()
        {
            var list = await _firebase.GetAllAsync<TareaPersist>(COLLECTION);
            var result = new List<TareaDto>();
            foreach (var item in list)
                result.Add(TareaMapper.ToDto(item));
            return result;
        }

        // Obtener tareas por estado
        public async Task<List<TareaDto>> GetByEstadoAsync(bool estado)
        {
            var list = await _firebase.QueryAsync<TareaPersist>(COLLECTION, "Estado", estado);
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

        // PATCH: Actualización parcial de tarea
        public async Task<TareaDto?> PatchAsync(string id, UpdateTareaDto dto)
        {
            var persist = await _firebase.GetAsync<TareaPersist>(COLLECTION, id);
            if (persist == null)
                return null;

            if (dto.Nombre != null) persist.Nombre = dto.Nombre;
            if (dto.FechaLimite.HasValue) persist.FechaLimite = dto.FechaLimite.Value;
            if (dto.Descripcion != null) persist.Descripcion = dto.Descripcion;
            if (dto.UsuarioEspacioIds != null) persist.UsuarioEspacioIds = dto.UsuarioEspacioIds;
            if (dto.Karma.HasValue) persist.Karma = dto.Karma.Value;
            if (dto.Foto != null) persist.Foto = dto.Foto;
            if (dto.Prorroga.HasValue) persist.Prorroga = dto.Prorroga;
            if (dto.Estado.HasValue) persist.Estado = dto.Estado.Value;
            if (dto.FechaRealizacion.HasValue) persist.FechaRealizacion = dto.FechaRealizacion;

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
