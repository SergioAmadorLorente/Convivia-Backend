using Convivia.Application.Services;
using Convivia.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Convivia.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioEspacioController : ControllerBase
    {
        private readonly UsuarioEspacioService _service;
        private readonly ILogger<UsuarioEspacioController> _logger;

        public UsuarioEspacioController(UsuarioEspacioService service, ILogger<UsuarioEspacioController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // POST: api/usuarioespacio
        [HttpPost]
        public async Task<ActionResult<UsuarioEspacioDto>> Crear([FromBody] CreateUsuarioEspacioDto dto, CancellationToken ct)
        {
            var result = await _service.AddAsync(dto, ct);
            if (result == null) return BadRequest("No se pudo crear el UsuarioEspacio");
            return CreatedAtAction(nameof(ObtenerPorId), new { id = result.Id_UsuarioEspacio }, result);
        }

        // GET: api/usuarioespacio/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<UsuarioEspacioDto>> ObtenerPorId(string id, CancellationToken ct)
        {
            var result = await _service.ObtenerPorIdAsync(id, ct);
            if (result == null) return NotFound();
            return Ok(result);
        }

        // GET: api/usuarioespacio/espacio/{espacioId}
        [HttpGet("espacio/{espacioId}")]
        public async Task<ActionResult<IEnumerable<UsuarioEspacioDto>>> ObtenerPorEspacio(string espacioId, CancellationToken ct)
        {
            var result = await _service.ObtenerPorEspacioAsync(espacioId, ct);
            return Ok(result);
        }

        // GET: api/usuarioespacio/usuario/{usuarioId}
        [HttpGet("usuario/{usuarioId}")]
        public async Task<ActionResult<IEnumerable<UsuarioEspacioDto>>> ObtenerPorUsuario(string usuarioId, CancellationToken ct)
        {
            var result = await _service.ObtenerPorUsuarioAsync(usuarioId, ct);
            return Ok(result);
        }

        // GET: api/usuarioespacio
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UsuarioEspacioDto>>> ObtenerTodos(CancellationToken ct)
        {
            _logger.LogInformation("Endpoint ObtenerTodos llamado");
            var result = await _service.ObtenerTodosAsync(ct);
            _logger.LogInformation("ObtenerTodos devuelve {Count} elementos", result?.Count() ?? 0);
            return Ok(result);
        }

        // PUT: api/usuarioespacio/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<UsuarioEspacioDto>> Actualizar(string id, [FromBody] UpdateUsuarioEspacioDto dto, CancellationToken ct)
        {
            var result = await _service.UpdateAsync(id, dto, ct);
            return Ok(result);
        }

        // PATCH: api/usuarioespacio/{id}
        [HttpPatch("{id}")]
        public async Task<ActionResult> ActualizacionParcial(string id, [FromBody] UpdateUsuarioEspacioDto dto, CancellationToken ct)
        {
            var result = await _service.ParcialActualizarAsync(id, dto, ct);
            if (!result) return NotFound();
            return NoContent();
        }

        // DELETE: api/usuarioespacio/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> Eliminar(string id, CancellationToken ct)
        {
            await _service.EliminarAsync(id, ct);
            return NoContent();
        }
    }
}
