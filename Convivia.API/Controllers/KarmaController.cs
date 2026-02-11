using Convivia.Application.Services;
using Convivia.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
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
    }
}
