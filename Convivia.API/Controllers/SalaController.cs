using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Convivia.Shared.DTOs;
using Convivia.Application.Services;

namespace Convivia.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalaController : ControllerBase
    {
        private readonly SalaService _service;

        public SalaController(SalaService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSalaDto model, CancellationToken ct)
        {
            if (model == null) return BadRequest();

            if (string.IsNullOrWhiteSpace(model.Nombre) ||
                string.IsNullOrWhiteSpace(model.Descripcion) ||
                string.IsNullOrWhiteSpace(model.IdEspacio))
            {
                return BadRequest("Nombre, Descripcion y IdEspacio son requeridos.");
            }
            var id = await _service.CrearAsync(model, ct);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();
            var sala = await _service.ObtenerPorIdAsync(id, ct);
            if (sala == null) return NotFound();
            return Ok(sala);
        }

        [HttpGet("por-espacio/{idEspacio}")]
        public async Task<IActionResult> GetByEspacioId(string idEspacio, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(idEspacio)) return BadRequest();
            var list = await _service.ObtenerPorEspacioAsync(idEspacio, ct);
            return Ok(list);
        }

        // PUT = reemplazo completo: usar CreateSalaDto (campos obligatorios) y Servicio.ActualizarAsync espera CreateSalaDto
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] CreateSalaDto model, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id) || model == null) return BadRequest();
            if (string.IsNullOrWhiteSpace(model.Nombre) ||
                string.IsNullOrWhiteSpace(model.Descripcion) ||
                string.IsNullOrWhiteSpace(model.IdEspacio))
            {
                return BadRequest("Nombre, Descripcion y IdEspacio son requeridos.");
            }

            await _service.ActualizarAsync(id, model, ct);
            return NoContent();
        }

        // PATCH = actualización parcial: usar UpdateSalaDto y Servicio.ParcialActualizarAsync
        [HttpPatch("{id}")]
        public async Task<IActionResult> Patch(string id, [FromBody] UpdateSalaDto model, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id) || model == null) return BadRequest();
            var updated = await _service.ParcialActualizarAsync(id, model, ct);
            if (!updated) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();
            await _service.EliminarAsync(id, ct);
            return NoContent();
        }
    }
}