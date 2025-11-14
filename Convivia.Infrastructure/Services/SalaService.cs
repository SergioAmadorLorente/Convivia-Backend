using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Convivia.Domain.Models;
using Convivia.Application.DTOs;
using Convivia.Application.Mappers;

namespace Convivia.Infrastructure.Services
{
    public class SalaService
    {
        public const string COLLECTION = "salas";
        private readonly IFirebaseService _firebase;

        public SalaService(IFirebaseService firebase)
        {
            _firebase = firebase ?? throw new ArgumentNullException(nameof(firebase));
        }

        // Crear sala -> devuelve SalaDto
        public async Task<SalaDto> AddAsync(CreateSalaDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Nombre)) throw new ArgumentException("El nombre no puede estar vacío.");

            var idSala = Guid.NewGuid().ToString();
            var persist = SalaMapper.ToPersist(dto, idSala);

            var existing = await _firebase.QueryMultipleConditionsAsync<SalaPersist>(
                COLLECTION,
                new (string field, object val)[] { ("Nombre", dto.Nombre), ("IdEspacio", dto.IdEspacio ?? string.Empty) }
            );

            if (existing.Count > 0)
                throw new InvalidOperationException("Ya existe una sala con ese nombre en el espacio.");

            await _firebase.AddAsync(COLLECTION, idSala, persist);
            return SalaMapper.ToDto(persist);
        }

        // Obtener sala por id
        public async Task<SalaDto?> GetAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            var persist = await _firebase.GetAsync<SalaPersist>(COLLECTION, id);
            return persist != null ? SalaMapper.ToDto(persist) : null;
        }

        // Listar todas las salas
        public async Task<List<SalaDto>> GetAllAsync()
        {
            var list = await _firebase.QueryAsync<SalaPersist>(COLLECTION, "IdSala", null);
            var result = new List<SalaDto>(list.Count);
            foreach (var item in list)
                result.Add(SalaMapper.ToDto(item));
            return result;
        }

        // Listar salas por espacio
        public async Task<List<SalaDto>> GetByEspacioAsync(string espacioId)
        {
            if (string.IsNullOrWhiteSpace(espacioId)) throw new ArgumentNullException(nameof(espacioId));

            var list = await _firebase.QueryAsync<SalaPersist>(COLLECTION, "IdEspacio", espacioId);
            var result = new List<SalaDto>(list.Count);
            foreach (var item in list)
                result.Add(SalaMapper.ToDto(item));
            return result;
        }

        // Actualización completa (replace) -> devuelve SalaDto
        public async Task<SalaDto?> UpdateAsync(string id, CreateSalaDto dto)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var persist = await _firebase.GetAsync<SalaPersist>(COLLECTION, id);
            if (persist == null) return null;

            persist.Nombre = dto.Nombre;
            persist.Descripcion = dto.Descripcion;
            persist.IdEspacio = dto.IdEspacio;

            try
            {
                var reservasPropDto = dto.GetType().GetProperty("Reservas");
                var reservasValue = reservasPropDto?.GetValue(dto) as List<Reserva>;
                if (reservasValue != null)
                    persist.GetType().GetProperty("Reservas")?.SetValue(persist, reservasValue);
            }
            catch
            {
                // compatibilidad: ignorar si alguna propiedad no existe
            }

            await _firebase.UpdateAsync(COLLECTION, id, persist);
            return SalaMapper.ToDto(persist);
        }

        // Patch (actualización parcial) -> devuelve SalaDto
        public async Task<SalaDto?> PatchAsync(string id, UpdateSalaDto dto)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var persist = await _firebase.GetAsync<SalaPersist>(COLLECTION, id);
            if (persist == null) return null;

            persist.Nombre = string.IsNullOrWhiteSpace(dto.Nombre) ? persist.Nombre : dto.Nombre;
            persist.Descripcion = dto.Descripcion ?? persist.Descripcion;
            persist.IdEspacio = dto.IdEspacio ?? persist.IdEspacio;

            try
            {
                var reservasProp = dto.GetType().GetProperty("Reservas");
                var reservasVal = reservasProp?.GetValue(dto) as List<Reserva>;
                if (reservasVal != null)
                    persist.GetType().GetProperty("Reservas")?.SetValue(persist, reservasVal);
            }
            catch { }

            await _firebase.UpdateAsync(COLLECTION, id, persist);
            return SalaMapper.ToDto(persist);
        }

        // Eliminar sala
        public async Task<bool> DeleteAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));

            var exist = await _firebase.GetAsync<SalaPersist>(COLLECTION, id);
            if (exist == null) return false;

            await _firebase.DeleteAsync(COLLECTION, id);
            return true;
        }
    }
}