using Convivia.Domain.Entities;
using Convivia.Domain.Repositories;
using Convivia.Shared.DTOs;
using Convivia.Shared.Helpers;
using Mapster;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Convivia.Application.Services
{
    public class UsuarioEspacioService
    {
        private readonly IUsuarioEspacioRepository _repo;
        private readonly ILogger<UsuarioEspacioService> _logger;
        private readonly IFacturaRepository _facturaRepo;
        private readonly ITareaRepository _tareaRepo;

        public UsuarioEspacioService(IUsuarioEspacioRepository repo, ILogger<UsuarioEspacioService> logger, IFacturaRepository facturaRepo, ITareaRepository tareaRepo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _facturaRepo = facturaRepo ?? throw new ArgumentNullException(nameof(facturaRepo)); 
            _tareaRepo = tareaRepo ?? throw new ArgumentNullException(nameof(tareaRepo));
        }

        // Crear UsuarioEspacio
        public async Task<UsuarioEspacioDto?> AddAsync(CreateUsuarioEspacioDto dto, CancellationToken ct = default)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.UsuarioId)) throw new ArgumentException("UsuarioId requerido");
            if (string.IsNullOrWhiteSpace(dto.EspacioId)) throw new ArgumentException("EspacioId requerido");

            var usuarioEspacioDomain = dto.Adapt<UsuarioEspacio>();
            var createdUsuarioEspacio = await _repo.AddAsync(usuarioEspacioDomain, ct);

            return createdUsuarioEspacio?.Adapt<UsuarioEspacioDto>();
        }

        // Obtener UsuarioEspacio por Id
        public async Task<UsuarioEspacioDto?> ObtenerPorIdAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;

            var usuarioEspacio = await _repo.GetByIdAsync(id, ct);
            return usuarioEspacio?.Adapt<UsuarioEspacioDto>();
        }

        // Obtener UsuariosEspacios por EspacioId (lista)
        public async Task<IEnumerable<UsuarioEspacioDto>> ObtenerPorEspacioAsync(string espacioId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(espacioId))
                return Enumerable.Empty<UsuarioEspacioDto>();

            var lista = await _repo.GetByEspacioIdAsync(espacioId, ct);
            return lista.Adapt<IEnumerable<UsuarioEspacioDto>>();
        }

        // Obtener UsuarioEspacio por UsuarioId (único)
        public async Task<UsuarioEspacioDto?> ObtenerPorUsuarioAsync(string usuarioId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(usuarioId))
                return null;

            var usuarioEspacio = await _repo.GetByUsuarioIdAsync(usuarioId, ct);
            return usuarioEspacio?.Adapt<UsuarioEspacioDto>();
        }

        // Obtener todos
        public async Task<IEnumerable<UsuarioEspacioDto>> ObtenerTodosAsync(CancellationToken ct = default)
        {
            _logger.LogInformation("Llamando a GetAllAsync desde UsuarioEspacioService");

            var usuariosEspacios = await _repo.GetAllAsync(ct);

            _logger.LogInformation("GetAllAsync devolvió {Count} elementos", usuariosEspacios?.Count() ?? 0);

            return usuariosEspacios.Adapt<IEnumerable<UsuarioEspacioDto>>();
        }

        // Actualizar UsuarioEspacio
        public async Task<UsuarioEspacioDto> UpdateAsync(string id, UpdateUsuarioEspacioDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Id requerido");
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var usuarioEspacioExistente = await _repo.GetByIdAsync(id, ct);
            if (usuarioEspacioExistente == null)
                throw new KeyNotFoundException($"UsuarioEspacio con Id {id} no encontrado");

            if (dto.Ausente.HasValue) usuarioEspacioExistente.Ausente = dto.Ausente.Value;
            if (dto.Karma.HasValue) usuarioEspacioExistente.Karma = dto.Karma.Value;
            if (!string.IsNullOrWhiteSpace(dto.Rol)) usuarioEspacioExistente.Rol = dto.Rol;

            var updated = await _repo.UpdateAsync(id, usuarioEspacioExistente, ct);
            return updated!.Adapt<UsuarioEspacioDto>();
        }

        // Actualización parcial
        public async Task<bool> ParcialActualizarAsync(string id, UpdateUsuarioEspacioDto dto, CancellationToken ct = default)
        {
            try
            {
                await UpdateAsync(id, dto, ct);
                return true;
            }
            catch (KeyNotFoundException)
            {
                return false;
            }
        }

        // Eliminar UsuarioEspacio
        public async Task EliminarAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Id requerido", nameof(id));

            if (await _facturaRepo.ExistsByUsuarioEspacioIdAsync(id, ct))
                throw new InvalidOperationException($"No se puede eliminar el UsuarioEspacio {id}: existen facturas asociadas.");

            var tareas = await _tareaRepo.GetByUsuarioEspacioIdAsync(id, ct);

            foreach (var tarea in tareas)
            {
                tarea!.UsuarioEspacioId = null;
                await _tareaRepo.UpdateAsync(tarea.Id, tarea, ct);
            }

            await _repo.DeleteAsync(id, ct);
        }
    }
}
