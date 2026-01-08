using Mapster;
using Convivia.Shared.DTOs;
using Convivia.Domain.Entities;
using Convivia.Application.Repositories;
using Microsoft.Extensions.Logging;

namespace Convivia.Application.Services
{
    public class SalaService
    {
        private readonly ISalaRepository _repo;
        private readonly ILogger<SalaService> _logger;

        public SalaService(ISalaRepository repo, ILogger<SalaService> logger)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Crear sala
        public async Task<SalaDto?> AddAsync(CreateSalaDto dto, CancellationToken ct = default)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Nombre)) throw new ArgumentException("Nombre requerido");

            var salaDomain = dto.Adapt<Sala>(); // Aquí la entidad ya genera su Id
            var createdSala = await _repo.AddAsync(salaDomain, ct);

            return createdSala?.Adapt<SalaDto>();
        }

        // Obtener sala por Id
        public async Task<SalaDto?> ObtenerPorIdAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            var sala = await _repo.GetByIdAsync(id, ct);
            return sala?.Adapt<SalaDto>();
        }

        // Obtener salas por espacio
        public async Task<IEnumerable<SalaDto>> ObtenerPorEspacioAsync(string idEspacio, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(idEspacio)) return Enumerable.Empty<SalaDto>();
            var salas = await _repo.GetByEspacioIdAsync(idEspacio, ct);
            return salas.Select(s => s.Adapt<SalaDto>());
        }

        // Actualizar sala
        public async Task<SalaDto> UpdateAsync(string id, UpdateSalaDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Id requerido");
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var salaExistente = await _repo.GetByIdAsync(id, ct);
            if (salaExistente == null) throw new KeyNotFoundException($"Sala con Id {id} no encontrada");

            salaExistente.Nombre = dto.Nombre ?? salaExistente.Nombre;
            salaExistente.Descripcion = dto.Descripcion ?? salaExistente.Descripcion;
            salaExistente.IdEspacio = dto.IdEspacio ?? salaExistente.IdEspacio;

            await _repo.UpdateAsync(id, salaExistente, ct);
            return salaExistente.Adapt<SalaDto>();
        }

        // Eliminar sala
        public async Task EliminarAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Id requerido");
            await _repo.DeleteAsync(id, ct);
        }

        // Actualización parcial
        public async Task<bool> ParcialActualizarAsync(string id, UpdateSalaDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Id requerido");
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var sala = await _repo.GetByIdAsync(id, ct);
            if (sala == null) return false;

            var changed = false;

            if (!string.IsNullOrWhiteSpace(dto.Nombre) && dto.Nombre != sala.Nombre)
            {
                sala.Nombre = dto.Nombre;
                changed = true;
            }

            if (!string.IsNullOrWhiteSpace(dto.Descripcion) && dto.Descripcion != sala.Descripcion)
            {
                sala.Descripcion = dto.Descripcion;
                changed = true;
            }

            if (!string.IsNullOrWhiteSpace(dto.IdEspacio) && dto.IdEspacio != sala.IdEspacio)
            {
                sala.IdEspacio = dto.IdEspacio;
                changed = true;
            }

            if (!changed) return true; // nada que actualizar

            await _repo.UpdateAsync(id, sala, ct);
            return true;
        }
    }
}