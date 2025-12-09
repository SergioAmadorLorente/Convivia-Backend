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

            var reserva = await _service.AddAsync(model, ct);
            if (reserva == null) return BadRequest("No se pudo crear la reserva.");

            return CreatedAtAction(nameof(GetById), new { id = reserva.idReserva }, reserva);
        }

        // GET api/reservas/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            var reserva = await _service.ObtenerPorIdAsync(id, ct);
            if (reserva == null) return NotFound();

            return Ok(reserva);
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

        // PUT api/reservas/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateReservaDto model, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();
            if (model == null) return BadRequest();

            try
            {
                var updated = await _service.UpdateAsync(id, model, ct);
                return Ok(updated);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PATCH api/reservas/{id}
        [HttpPatch("{id}")]
        public async Task<IActionResult> PartialUpdate(string id, [FromBody] UpdateReservaDto model, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();
            if (model == null) return BadRequest();

            var result = await _service.ParcialActualizarAsync(id, model, ct);
            if (!result) return NotFound();

            return NoContent();
        }

        // DELETE api/reservas/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            try
            {
                await _service.EliminarAsync(id, ct);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}