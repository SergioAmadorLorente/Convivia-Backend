using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Convivia.Shared.DTOs;
using Convivia.Application.Services;

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

        // POST api/invitacion
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

            var created = await _service.CrearInvitacionAsync(model, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        [HttpGet("throw")]
        public IActionResult Throw() => throw new Exception("test-exception");


        // GET api/invitaciones/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            var invitacion = await _service.ObtenerInvitacionAsync(id, ct);
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

        // PUT api/invitaciones/{id}  -> actualizar mensaje (flujo existente)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMensaje(string id, [FromBody] CreateInvitacionDto model, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id) || model == null) return BadRequest();

            try
            {
                await _service.ActualizarMensajeAsync(id, model, ct);
                var updated = await _service.ObtenerInvitacionAsync(id, ct);
                return updated == null ? NotFound() : Ok(updated);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // DELETE api/invitaciones/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            var resultat = await _service.EliminarInvitacionAsync(id, ct);
            return resultat ? NoContent() : NotFound();
        }
    }
}