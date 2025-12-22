using Mapster;
using Convivia.Shared.DTOs;
using Convivia.Domain.Entities;
using Convivia.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Convivia.Application.Services
{
    public class UsuarioEspacioService
    {
        private readonly IUsuarioEspacioRepository _repo;
        private readonly ILogger<UsuarioEspacioService> _logger;
        private readonly IFacturaRepository _facturaRepo;

        public UsuarioEspacioService(IUsuarioEspacioRepository repo, ILogger<UsuarioEspacioService> logger, IFacturaRepository facturaRepo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _facturaRepo = facturaRepo ?? throw new ArgumentNullException(nameof(facturaRepo));  
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

        // Obtener UsuariosEspacios por EspacioId
        public async Task<IEnumerable<UsuarioEspacioDto>> ObtenerPorEspacioAsync(string espacioId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(espacioId)) return Enumerable.Empty<UsuarioEspacioDto>();
            var usuariosEspacios = await _repo.GetByEspacioIdAsync(espacioId, ct);
            return usuariosEspacios.Select(ue => ue.Adapt<UsuarioEspacioDto>());
        }

        // Obtener UsuariosEspacios por UsuarioId
        public async Task<IEnumerable<UsuarioEspacioDto>> ObtenerPorUsuarioAsync(string usuarioId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(usuarioId)) return Enumerable.Empty<UsuarioEspacioDto>();
            var usuariosEspacios = await _repo.GetByUsuarioIdAsync(usuarioId, ct);
            return usuariosEspacios.Select(ue => ue.Adapt<UsuarioEspacioDto>());
        }

        // Obtener todos
        public async Task<IEnumerable<UsuarioEspacioDto>> ObtenerTodosAsync(CancellationToken ct = default)
        {
            _logger.LogInformation("Llamando a GetAllAsync desde UsuarioEspacioService");
            var usuariosEspacios = await _repo.GetAllAsync(ct);
            _logger.LogInformation("GetAllAsync devolvió {Count} elementos", usuariosEspacios?.Count() ?? 0);
            return usuariosEspacios.Select(ue => ue.Adapt<UsuarioEspacioDto>());
        }

        // Actualizar UsuarioEspacio
        public async Task<UsuarioEspacioDto> UpdateAsync(string id, UpdateUsuarioEspacioDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Id requerido");
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var usuarioEspacioExistente = await _repo.GetByIdAsync(id, ct);
            if (usuarioEspacioExistente == null) throw new KeyNotFoundException($"UsuarioEspacio con Id {id} no encontrado");

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
        // on delete restriction: no debe tener facturas asociadas (debe gestionarse en el back i no en el front para mejorar la experiencia de usuario)
        public async Task EliminarAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Id requerido", nameof(id));
            if (await _facturaRepo.ExistsByUsuarioEspacioIdAsync(id, ct).ConfigureAwait(false))
                throw new InvalidOperationException($"No se puede eliminar el UsuarioEspacio {id}: existen facturas asociadas.");

            await _repo.DeleteAsync(id, ct);
        }
    }
}
