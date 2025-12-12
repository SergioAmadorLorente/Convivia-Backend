using Microsoft.AspNetCore.Mvc;
using Convivia.Application.Services;
using Convivia.Shared.DTOs;
using System.Threading;
using System.Threading.Tasks;

namespace Convivia.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EspacioController : ControllerBase
    {
        private readonly EspacioService _service;

        public EspacioController(EspacioService service)
        {
            _service = service;
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            try
            {
                await _service.EliminarAsync(id, ct);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                // RESTRICT violation -> 409 Conflict
                return Conflict(new { message = ex.Message });
            }
        }

        // POST api/espacio
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEspacioDto model, CancellationToken ct)
        {
            if (model == null) return BadRequest();
            if (string.IsNullOrWhiteSpace(model.Nombre))
            {
                return BadRequest("Nombre es requerido.");
            }

            var id = await _service.CrearAsync(model, ct);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }

        // GET api/espacio/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            var espacio = await _service.ObtenerPorIdAsync(id, ct);
            if (espacio == null) return NotFound();
            return Ok(espacio);
        }

        // GET api/espacio/por-direccion/{direccion}
        [HttpGet("por-direccion/{direccion}")]
        public async Task<IActionResult> GetByDireccion(string direccion, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(direccion)) return BadRequest();

            var list = await _service.ObtenerPorDireccionAsync(direccion, ct);
            return Ok(list);
        }

        // PUT api/espacio/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] CreateEspacioDto model, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();
            if (model == null) return BadRequest();

            await _service.ActualizarAsync(id, model, ct);
            return NoContent();
        }

        // PATCH api/espacio/{id}
        [HttpPatch("{id}")]
        public async Task<IActionResult> PartialUpdate(string id, [FromBody] UpdateEspacioDto model, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();
            if (model == null) return BadRequest();

            var success = await _service.ParcialActualizarAsync(id, model, ct);
            if (!success) return NotFound();
            return NoContent();
        }

        // DELETE api/espacio/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            await _service.EliminarAsync(id, ct);
            return NoContent();
        }
    }
}