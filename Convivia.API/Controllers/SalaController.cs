using Convivia.Application.Services;
using Convivia.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Convivia.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalaController : ControllerBase
    {
        private readonly SalaService _service;
        private readonly ILogger<SalaController> _logger;

        public SalaController(SalaService service, ILogger<SalaController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // POST: api/sala
        [HttpPost]
        public async Task<ActionResult<SalaDto>> Crear([FromBody] CreateSalaDto dto, CancellationToken ct)
        {
            var result = await _service.AddAsync(dto, ct);
            if (result == null) return BadRequest("No se pudo crear la sala");
            return CreatedAtAction(nameof(ObtenerPorId), new { id = result.Id }, result);
        }

        // GET: api/sala/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<SalaDto>> ObtenerPorId(string id, CancellationToken ct)
        {
            var result = await _service.ObtenerPorIdAsync(id, ct);
            if (result == null) return NotFound();
            return Ok(result);
        }

        // GET: api/sala/espacio/{idEspacio}
        [HttpGet("espacio/{idEspacio}")]
        public async Task<ActionResult<IEnumerable<SalaDto>>> ObtenerPorEspacio(string idEspacio, CancellationToken ct)
        {
            var result = await _service.ObtenerPorEspacioAsync(idEspacio, ct);
            return Ok(result);
        }

        // PUT: api/sala/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<SalaDto>> Actualizar(string id, [FromBody] UpdateSalaDto dto, CancellationToken ct)
        {
            var result = await _service.UpdateAsync(id, dto, ct);
            return Ok(result);
        }

        // PATCH: api/sala/{id}
        [HttpPatch("{id}")]
        public async Task<ActionResult> ActualizacionParcial(string id, [FromBody] UpdateSalaDto dto, CancellationToken ct)
        {
            var result = await _service.ParcialActualizarAsync(id, dto, ct);
            if (!result) return NotFound();
            return NoContent();
        }

        // DELETE: api/sala/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> Eliminar(string id, CancellationToken ct)
        {
            await _service.EliminarAsync(id, ct);
            return NoContent();
        }
    }
}