using Convivia.Domain.Entities;
using Convivia.Application.Repositories;
using Convivia.Shared.DTOs;
using Mapster;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Convivia.Application.Services
{
    public class KarmaEstadisticasService
    {
        private readonly IKarmaEstadisticasRepository _karmaRepository;
        private readonly IUsuarioEspacioRepository _usuarioEspacioRepository;
        private readonly ILogger<KarmaEstadisticasService> _logger;

        public KarmaEstadisticasService(
            IKarmaEstadisticasRepository karmaRepository,
            IUsuarioEspacioRepository usuarioEspacioRepository,
            ILogger<KarmaEstadisticasService> logger)
        {
            _karmaRepository = karmaRepository ?? throw new ArgumentNullException(nameof(karmaRepository));
            _usuarioEspacioRepository = usuarioEspacioRepository ?? throw new ArgumentNullException(nameof(usuarioEspacioRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Obtiene las estadísticas de karma con reset automático si es necesario
        /// </summary>
        public async Task<KarmaEstadisticasDto?> GetKarmaEstadisticasAsync(
            string espacioId,
            string usuarioEspacioId, 
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(espacioId)) 
                throw new ArgumentNullException(nameof(espacioId));
            if (string.IsNullOrWhiteSpace(usuarioEspacioId)) 
                throw new ArgumentNullException(nameof(usuarioEspacioId));

            var estadisticas = await _karmaRepository.GetByUsuarioEspacioIdAsync(espacioId, usuarioEspacioId, ct);

            // Si no existen, crearlas
            if (estadisticas == null)
            {
                estadisticas = await CreateEstadisticasAsync(espacioId, usuarioEspacioId, ct);
                if (estadisticas == null)
                {
                    _logger.LogWarning("No se pudo crear estadísticas para {UsuarioEspacioId} en {EspacioId}", 
                        usuarioEspacioId, espacioId);
                    return null;
                }
            }

            return estadisticas.Adapt<KarmaEstadisticasDto>();
        }

        /// <summary>
        /// Suma karma a las estadísticas de un usuario
        /// </summary>
        public async Task<KarmaEstadisticasDto> AddKarmaAsync(
            string espacioId,
            string usuarioEspacioId, 
            int karmaAmount, 
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(espacioId))
                throw new ArgumentNullException(nameof(espacioId));
            
            if (string.IsNullOrWhiteSpace(usuarioEspacioId))
                throw new ArgumentNullException(nameof(usuarioEspacioId));

            if (karmaAmount <= 0)
                throw new ArgumentException("karmaAmount debe ser mayor a 0", nameof(karmaAmount));

            // Verificar si existen estadísticas, si no, crearlas
            var estadisticas = await _karmaRepository.GetByUsuarioEspacioIdAsync(espacioId, usuarioEspacioId, ct);
            
            if (estadisticas == null)
            {
                estadisticas = await CreateEstadisticasAsync(espacioId, usuarioEspacioId, ct);
                if (estadisticas == null)
                    throw new InvalidOperationException($"No se pudo crear estadísticas para {usuarioEspacioId}");
            }

            // Agregar karma
            var updated = await _karmaRepository.AddKarmaAsync(espacioId, usuarioEspacioId, karmaAmount, ct);
            return updated.Adapt<KarmaEstadisticasDto>();
        }

        /// <summary>
        /// Resta karma a las estadísticas de un usuario (sin permitir valores negativos)
        /// </summary>
        public async Task<KarmaEstadisticasDto?> SubtractKarmaAsync(
            string espacioId,
            string usuarioEspacioId, 
            int karmaAmount, 
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(espacioId))
                throw new ArgumentNullException(nameof(espacioId));
            
            if (string.IsNullOrWhiteSpace(usuarioEspacioId))
                throw new ArgumentNullException(nameof(usuarioEspacioId));

            if (karmaAmount <= 0)
                throw new ArgumentException("karmaAmount debe ser mayor a 0", nameof(karmaAmount));

            // Verificar si existen estadísticas
            var estadisticas = await _karmaRepository.GetByUsuarioEspacioIdAsync(espacioId, usuarioEspacioId, ct);
            
            if (estadisticas == null)
            {
                _logger.LogWarning("No existen estadísticas para restar karma: {UsuarioEspacioId} en {EspacioId}", 
                    usuarioEspacioId, espacioId);
                return null;
            }

            // Si el karma total es 0, no restar
            if (estadisticas.KarmaTotal <= 0)
            {
                _logger.LogDebug("El karma del usuario {UsuarioEspacioId} ya es 0, no se resta", usuarioEspacioId);
                return estadisticas.Adapt<KarmaEstadisticasDto>();
            }

            // Restar karma
            var updated = await _karmaRepository.SubtractKarmaAsync(espacioId, usuarioEspacioId, karmaAmount, ct);
            return updated.Adapt<KarmaEstadisticasDto>();
        }

        private async Task<KarmaEstadisticas?> CreateEstadisticasAsync(string espacioId, string usuarioEspacioId, CancellationToken ct)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(usuarioEspacioId))
                {
                    _logger.LogWarning("El usuarioEspacioId es nulo o vacío");
                    return null;
                }

                var estadisticas = new KarmaEstadisticas(usuarioEspacioId);
                var id = await _karmaRepository.AddAsync(espacioId, estadisticas, ct);

                _logger.LogInformation("Estadísticas de karma creadas para {UsuarioEspacioId} con ID {Id} en espacio {EspacioId}", 
                    usuarioEspacioId, id, espacioId);

                // Verificar que se crearon correctamente obteniendo el registro
                var created = await _karmaRepository.GetByUsuarioEspacioIdAsync(espacioId, usuarioEspacioId, ct);
                if (created == null)
                {
                    _logger.LogWarning("Las estadísticas se guardaron pero no se pudieron recuperar para {UsuarioEspacioId} en {EspacioId}", 
                        usuarioEspacioId, espacioId);
                }

                return created ?? estadisticas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando estadísticas para {UsuarioEspacioId} en {EspacioId}", 
                    usuarioEspacioId, espacioId);
                return null;
            }
        }
    }
}
