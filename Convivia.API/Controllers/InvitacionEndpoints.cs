using Convivia.Application.DTOs;
using Convivia.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace Convivia.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvitacionesController : ControllerBase
    {
        private readonly InvitacionService _service;

        public InvitacionesController(InvitacionService service)
        {
            _service = service;
        }

        // POST api/invitaciones
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateInvitacionDto model, CancellationToken ct)
        {
            if (model == null) return BadRequest();
            if (string.IsNullOrWhiteSpace(model.UsuarioSolicitanteId) ||
                string.IsNullOrWhiteSpace(model.UsuarioInvitadoId) ||
                string.IsNullOrWhiteSpace(model.EspacioId))
            {
                return BadRequest("UsuarioSolicitanteId, UsuarioInvitadoId y EspacioId son requeridos.");
            }

            var id = await _service.CrearAsync(model, ct);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }

        // GET api/invitaciones/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            var invitacion = await _service.ObtenerPorIdAsync(id, ct);
            if (invitacion == null) return NotFound();
            return Ok(invitacion);
        }

        // GET api/invitaciones/por-usuario/{usuarioInvitadoId}
        [HttpGet("por-usuario/{usuarioInvitadoId}")]
        public async Task<IActionResult> GetByUsuarioInvitado(string usuarioInvitadoId, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(usuarioInvitadoId)) return BadRequest();

            var list = await _service.ObtenerPorUsuarioInvitadoAsync(usuarioInvitadoId, ct);
            return Ok(list);
        }

        // PUT api/invitaciones/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] CreateInvitacionDto model, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();
            if (model == null) return BadRequest();

            await _service.ActualizarMensajeAsync(id, model, ct);
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