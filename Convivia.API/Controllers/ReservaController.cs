using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Convivia.Shared.DTOs;
using Convivia.Application.Services;

namespace Convivia.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservasController : ControllerBase
    {
        private readonly ReservaService _service;

        public ReservasController(ReservaService service)
        {
            _service = service;
        }

        // POST api/reservas
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateReservaDto model, CancellationToken ct)
        {
            if (model == null) return BadRequest();
            if (string.IsNullOrWhiteSpace(model.description)) return BadRequest("La descripción es necesaria");
            if (string.IsNullOrWhiteSpace(model.idSala)) return BadRequest("IdSala es requqrido");
            if (string.IsNullOrWhiteSpace(model.idUser)) return BadRequest("IdUser es requerido");

            var created = await _service.CrearReservaAsync(model, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // GET api/reservas/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            var reserva = await _service.ObtenerReservaAsync(id, ct);
            if (reserva == null) return NotFound();
            return Ok(reserva);
        }

        // GET api/reserva
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var list = await _service.ListarTodasAsync(ct);
            return Ok(list);
        }

        // GET api/reservas/por-usuario/{usuarioId}
        [HttpGet("por-usuario/{usuarioId}")]
        public async Task<IActionResult> GetByUsuario(string usuarioId, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(usuarioId)) return BadRequest();

            var list = await _service.ObtenerPorUsuarioAsync(usuarioId, ct);
            return Ok(list);
        }

        // GET api/reservas/por-sala/{salaId}
        [HttpGet("por-sala/{salaId}")]
        public async Task<IActionResult> GetBySala(string salaId, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(salaId)) return BadRequest();

            var list = await _service.ObtenerPorSalaAsync(salaId, ct);
            return Ok(list);
        }

        // PUT api/reserva/{id}
        // Overwrite completo: reemplaza todo el documento en Firestore.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOverwrite(string id, [FromBody] UpdateReservaDto model, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id) || model == null) return BadRequest();

            var updated = await _service.ActualizarReservaCompletaAsync(id, model, ct);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        // PUT api/reserva/{id}/merge
        // Merge explícito: fusiona los campos del DTO con el documento existente.
        [HttpPut("{id}/merge")]
        public async Task<IActionResult> PutMerge(string id, [FromBody] UpdateReservaDto model, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id) || model == null) return BadRequest();

            var updated = await _service.ActualizarReservaMergeAsync(id, model, ct);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        // PATCH api/reserva/{id}
        // Parcial: actualiza solo los campos enviados (IDictionary -> Update parcial en Firestore).
        [HttpPatch("{id}")]
        public async Task<IActionResult> Patch(string id, [FromBody] UpdateReservaDto model, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id) || model == null) return BadRequest();

            var updated = await _service.ActualizarReservaParcialAsync(id, model, ct);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        // DELETE api/reserva/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            var resultat = await _service.EliminarReservaAsync(id, ct);
            return resultat ? NoContent() : NotFound();
        }
    }
}