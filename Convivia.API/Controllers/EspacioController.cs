using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Convivia.Shared.DTOs;
using Convivia.Application.Services;

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

        // POST api/invitaciones
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEspacioDto model, CancellationToken ct)
        {
            if (model == null) return BadRequest();
            if (string.IsNullOrWhiteSpace(model.Nombre) ||
                string.IsNullOrWhiteSpace(model.Direccion))
            {
                return BadRequest("Nombre y Direccion son requeridos.");
            }

            var id = await _service.CrearAsync(model, ct);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }

        // GET api/invitaciones/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            var espacio = await _service.ObtenerPorIdAsync(id, ct);
            if (espacio == null) return NotFound();
            return Ok(espacio);
        }

        // GET api/invitaciones/por-usuario/{usuarioInvitadoId}
        [HttpGet("por-usuario/{direccion}")]
        public async Task<IActionResult> GetByDireccion(string direccion, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(direccion)) return BadRequest();

            var list = await _service.ObtenerPorDireccionAsync(direccion, ct);
            return Ok(list);
        }

        // PUT api/invitaciones/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] CreateEspacioDto model, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();
            if (model == null) return BadRequest();

            await _service.ActualizarDireccionAsync(id, model, ct);
            return NoContent();
        }

        // DELETE api/invitaciones/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            var removed = await _service.EliminarAsync(id, ct);
            if (!removed) return NotFound();
            return NoContent();
        }
    }
}