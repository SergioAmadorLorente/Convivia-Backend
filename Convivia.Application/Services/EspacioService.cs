using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Convivia.Shared.DTOs;
using Microsoft.Extensions.Logging;
using Convivia.Shared.Repositories;
using Convivia.Shared.Services;
using Mapster;
using Microsoft.Extensions.Caching.Memory;

namespace Convivia.Application.Services
{
    public class EspacioService
    {
        private readonly IEspacioRepository _repo;
        private readonly IUsuarioRepository _usuarioRepo;
        private readonly ILogger<EspacioService> _logger;
        private readonly IMemoryCache _cache;
        private readonly UsuarioEspacioService _usuarioEspacioService;

        public EspacioService(
            IEspacioRepository repo,
            IUsuarioRepository usuarioRepo,
            ILogger<EspacioService> logger, 
            IMemoryCache cache,
            UsuarioEspacioService usuarioEspacioService)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _usuarioRepo = usuarioRepo ?? throw new ArgumentNullException(nameof(usuarioRepo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _usuarioEspacioService = usuarioEspacioService ?? throw new ArgumentNullException(nameof(usuarioEspacioService));
        }

        public async Task<string> CrearAsync(CreateEspacioDto dto, CancellationToken ct = default)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Nombre)) throw new ArgumentException("Nombre requerido");

            // Usar Mapster para mapear CreateEspacioDto -> EspacioDto
            var espacio = dto.Adapt<EspacioDto>();
            espacio.Id = Guid.NewGuid().ToString("N");

            try
            {
                return await _repo.AddAsync(espacio, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando espacio");
                throw;
            }
        }

        public async Task<EspacioDto?> ObtenerPorIdAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            try
            {
                return await _repo.GetByIdAsync(id, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ObtenerPorId {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<EspacioDto>> ObtenerPorDireccionAsync(string direccion, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(direccion)) return Enumerable.Empty<EspacioDto>();
            try
            {
                return await _repo.GetByDireccionAsync(direccion, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ObtenerPorDireccion {Direccion}", direccion);
                throw;
            }
        }

        public async Task ActualizarAsync(string id, CreateEspacioDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Id requerido");
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            try
            {
                var espacioExistente = await _repo.GetByIdAsync(id, ct);
                if (espacioExistente == null) throw new KeyNotFoundException($"Espacio con Id {id} no encontrado");

                espacioExistente.Nombre = dto.Nombre ?? espacioExistente.Nombre;
                espacioExistente.Direccion = dto.Direccion ?? espacioExistente.Direccion;

                await _repo.UpdateAsync(id, espacioExistente, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando espacio {Id}", id);
                throw;
            }
        }

        public async Task EliminarAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Id requerido");
            try
            {
                await _repo.DeleteAsync(id, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando espacio {Id}", id);
                throw;
            }
        }

        public async Task<bool> ParcialActualizarAsync(string id, UpdateEspacioDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Id requerido");
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            try
            {
                var espacio = await _repo.GetByIdAsync(id, ct);
                if (espacio == null) return false;

                var changed = false;

                if (!string.IsNullOrWhiteSpace(dto.Nombre) && dto.Nombre != espacio.Nombre)
                {
                    espacio.Nombre = dto.Nombre;
                    changed = true;
                }

                if (dto.Direccion != null && dto.Direccion != espacio.Direccion)
                {
                    espacio.Direccion = dto.Direccion;
                    changed = true;
                }

                if (!changed) return true;

                await _repo.UpdateAsync(id, espacio, ct);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parcial actualizando espacio {Id}", id);
                throw;
            }
        }

        public async Task<string> GenerarCodigoInvitacionAsync(string espacioId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(espacioId)) throw new ArgumentException("Id requerido");

            var espacio = await _repo.GetByIdAsync(espacioId, ct);
            if (espacio == null) throw new KeyNotFoundException($"Espacio con Id {espacioId} no encontrado");

            // Verificar si ya existe un código válido para este espacio
            var espacioKey = $"EspacioId_{espacioId}";
            if (_cache.TryGetValue(espacioKey, out string codigoExistente))
            {
                _logger.LogInformation("Código de invitación existente {Codigo} devuelto para espacio {EspacioId}", codigoExistente, espacioId);
                return codigoExistente;
            }

            // Generar un nuevo código único
            string codigo;
            do
            {
                codigo = GenerarCodigoAleatorio();
            } while (_cache.TryGetValue($"InvitacionCode_{codigo}", out _));

            var cacheKey = $"InvitacionCode_{codigo}";
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(1));

            // Guardar el código con el espacioId y viceversa
            _cache.Set(cacheKey, espacioId, cacheOptions);
            _cache.Set(espacioKey, codigo, cacheOptions);

            _logger.LogInformation("Código de invitación {Codigo} generado para espacio {EspacioId}", codigo, espacioId);
            return codigo;
        }

        public async Task<UsuarioEspacioDto> UnirUsuarioPorCodigoAsync(string codigo, string usuarioId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(codigo)) throw new ArgumentException("Código requerido");
            if (string.IsNullOrWhiteSpace(usuarioId)) throw new ArgumentException("UsuarioId requerido");

            var cacheKey = $"InvitacionCode_{codigo}";
            if (!_cache.TryGetValue(cacheKey, out string espacioId))
            {
                _logger.LogWarning("Código de invitación {Codigo} no válido o expirado", codigo);
                return null;
            }

            var espacio = await _repo.GetByIdAsync(espacioId, ct);
            if (espacio == null)
            {
                _logger.LogWarning("Espacio {EspacioId} no encontrado para código {Codigo}", espacioId, codigo);
                return null;
            }

            var usuariosEspaciosExistentes = await _usuarioEspacioService.ObtenerPorEspacioAsync(espacioId, ct);
            if (usuariosEspaciosExistentes.Any(ue => ue.UsuarioId == usuarioId))
            {
                _logger.LogWarning("Usuario {UsuarioId} ya está en el espacio {EspacioId}", usuarioId, espacioId);
                return null;
            }

            var createDto = new CreateUsuarioEspacioDto
            {
                UsuarioId = usuarioId,
                EspacioId = espacioId,
                Ausente = false,
                Karma = 0,
                Rol = "Usuario"
            };

            var usuarioEspacio = await _usuarioEspacioService.AddAsync(createDto, ct);

            if (usuarioEspacio != null)
            {
                // Actualizar las listas de referencias en Usuario y Espacio
                await _repo.AddUsuarioEspacioIdAsync(espacioId, usuarioEspacio.Id_UsuarioEspacio, ct);
                await _usuarioRepo.AddUsuarioEspacioIdAsync(usuarioId, usuarioEspacio.Id_UsuarioEspacio, ct);
                _logger.LogInformation("Usuario {UsuarioId} se unió al espacio {EspacioId} usando código {Codigo}. UsuarioEspacioId: {UsuarioEspacioId}", usuarioId, espacioId, codigo, usuarioEspacio.Id_UsuarioEspacio);
            }

            return usuarioEspacio;
        }

        private string GenerarCodigoAleatorio()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }
    }
}