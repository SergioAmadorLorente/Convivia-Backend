using AuthApiDemo.Models;
using AuthApiDemo.DTOs;
using AuthApiDemo.Mappers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AuthApiDemo.Services
{
    public class EspacioService
    {
        public const string COLLECTION = "espacios";
        private readonly IFirebaseService _firebase;

        public EspacioService(IFirebaseService firebase)
        {
            _firebase = firebase;
        }

        // Crear espacio
        public async Task<EspacioDto> AddAsync(CreateEspacioDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre))
                throw new ArgumentException("El nombre no puede estar vacío.");

            var idEspacio = Guid.NewGuid().ToString();
            var persist = EspacioMapper.ToPersist(dto, idEspacio);

            var existing = await _firebase.QueryMultipleConditionsAsync<EspacioPersist>(
                COLLECTION,
                new (string field, object val)[] { ("Nombre", dto.Nombre) }
            );
            if (existing.Count > 0)
                throw new InvalidOperationException("Ya existe un espacio con ese nombre.");

            await _firebase.AddAsync(COLLECTION, idEspacio, persist);
            return EspacioMapper.ToDto(persist);
        }

        // Obtener espacio por id
        public async Task<EspacioDto?> GetAsync(string id)
        {
            var persist = await _firebase.GetAsync<EspacioPersist>(COLLECTION, id);
            return persist != null ? EspacioMapper.ToDto(persist) : null;
        }

        // Listar todos los espacios
        public async Task<List<EspacioDto>> GetAllAsync()
        {
            var list = await _firebase.QueryAsync<EspacioPersist>(COLLECTION, "Id_Espacio", null);
            var result = new List<EspacioDto>();
            foreach (var item in list)
                result.Add(EspacioMapper.ToDto(item));
            return result;
        }

        // Actualizar espacio
        public async Task<EspacioDto?> UpdateAsync(string id, CreateEspacioDto dto)
        {
            var persist = await _firebase.GetAsync<EspacioPersist>(COLLECTION, id);
            if (persist == null)
                return null;

            persist.Nombre = dto.Nombre;
            persist.Direccion = dto.Direccion;
            persist.SalaIds = dto.SalaIds ?? new List<string>();
            persist.UsuarioEspacioIds = dto.UsuarioEspacioIds ?? new List<string>();
            persist.PeticionIds = dto.PeticionIds ?? new List<string>();
            persist.InvitacionIds = dto.InvitacionIds ?? new List<string>();

            await _firebase.UpdateAsync(COLLECTION, id, persist);
            return EspacioMapper.ToDto(persist);
        }

        // PATCH: Actualización parcial de espacio
        public async Task<EspacioDto?> PatchAsync(string id, UpdateEspacioDto dto)
        {
            var persist = await _firebase.GetAsync<EspacioPersist>(COLLECTION, id);
            if (persist == null)
                return null;

            if (dto.Nombre != null) persist.Nombre = dto.Nombre;
            if (dto.Direccion != null) persist.Direccion = dto.Direccion;
            if (dto.SalaIds != null) persist.SalaIds = dto.SalaIds;
            if (dto.UsuarioEspacioIds != null) persist.UsuarioEspacioIds = dto.UsuarioEspacioIds;
            if (dto.PeticionIds != null) persist.PeticionIds = dto.PeticionIds;
            if (dto.InvitacionIds != null) persist.InvitacionIds = dto.InvitacionIds;

            await _firebase.UpdateAsync(COLLECTION, id, persist);
            return EspacioMapper.ToDto(persist);
        }

        // Eliminar espacio
        public async Task<bool> DeleteAsync(string id)
        {
            var persist = await _firebase.GetAsync<EspacioPersist>(COLLECTION, id);
            if (persist == null)
                return false;
            await _firebase.DeleteAsync(COLLECTION, id);
            return true;
        }
    }
}
