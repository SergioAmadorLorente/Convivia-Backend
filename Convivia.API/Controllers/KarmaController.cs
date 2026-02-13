using Convivia.Application.Services;
using Convivia.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Convivia.API.Controllers
{
    /// <summary>
    /// Controlador para gestionar las estadísticas de karma de usuarios en espacios
    /// </summary>
    [ApiController]
    [Route("api/espacio")]
    [Produces("application/json")]
    public class KarmaController : ControllerBase
    {
        private readonly KarmaEstadisticasService _karmaService;

        public KarmaController(KarmaEstadisticasService karmaService)
        {
            _karmaService = karmaService ?? throw new System.ArgumentNullException(nameof(karmaService));
        }

        /// <summary>
        /// Obtiene las estadísticas de karma de un UsuarioEspacio en un espacio específico.
        /// Incluye karma total, semanal y mensual. Los valores se resetean automáticamente cada semana y mes.
        /// </summary>
        /// <param name="espacioId">ID del Espacio</param>
        /// <param name="usuarioEspacioId">ID del UsuarioEspacio</param>
        /// <param name="ct">Token de cancelación</param>
        /// <returns>Estadísticas de karma del UsuarioEspacio</returns>
        /// <response code="200">Retorna las estadísticas de karma</response>
        /// <response code="404">No se encontraron estadísticas para el UsuarioEspacio en el espacio</response>
        [HttpGet("{espacioId}/karma/{usuarioEspacioId}")]
        public async Task<ActionResult<KarmaEstadisticasDto>> GetKarmaEstadisticas(
            string espacioId,
            string usuarioEspacioId,
            CancellationToken ct = default)
        {
            try
            {
                var estadisticas = await _karmaService.GetKarmaEstadisticasAsync(espacioId, usuarioEspacioId, ct);

                if (estadisticas == null)
                {
                    return NotFound(new 
                    { 
                        message = $"No se encontraron estadísticas de karma para el UsuarioEspacio {usuarioEspacioId} en el espacio {espacioId}",
                        espacioId,
                        usuarioEspacioId
                    });
                }

                return Ok(estadisticas);
            }
            catch (System.ArgumentNullException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new 
                { 
                    message = "Error interno al obtener estadísticas de karma", 
                    error = ex.Message 
                });
            }
        }

        /// <summary>
        /// Obtiene el ranking de karma de usuarios en un espacio.
        /// Los usuarios se ordenan de mayor a menor karma según el tipo especificado.
        /// </summary>
        /// <param name="espacioId">ID del Espacio</param>
        /// <param name="tipoKarma">Tipo de karma para ordenar: "total", "semanal" o "mensual" (por defecto "total")</param>
        /// <param name="top">Número máximo de usuarios a retornar (por defecto 10, máximo 100)</param>
        /// <param name="ct">Token de cancelación</param>
        /// <returns>Lista de usuarios ordenados por karma descendente con su posición</returns>
        /// <response code="200">Retorna el ranking de usuarios por karma</response>
        /// <response code="400">Parámetros inválidos</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("{espacioId}/karma/ranking")]
        public async Task<ActionResult<List<KarmaRankingDto>>> GetKarmaRanking(
            string espacioId,
            [FromQuery] string tipoKarma = "total",
            [FromQuery] int top = 10,
            CancellationToken ct = default)
        {
            try
            {
                // Validar límite máximo
                if (top > 100)
                {
                    return BadRequest(new 
                    { 
                        message = "El parámetro 'top' no puede ser mayor a 100",
                        top
                    });
                }

                var ranking = await _karmaService.GetKarmaRankingAsync(espacioId, tipoKarma, top, ct);

                return Ok(new
                {
                    espacioId,
                    tipoKarma,
                    totalUsuarios = ranking.Count,
                    ranking
                });
            }
            catch (System.ArgumentNullException ex)
            {
                return BadRequest(new { message = ex.Message, parameter = ex.ParamName });
            }
            catch (System.ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new 
                { 
                    message = "Error interno al obtener el ranking de karma", 
                    error = ex.Message 
                });
            }
        }
    }
}
