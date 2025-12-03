using Convivia.Shared.DTOs;
using Convivia.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Convivia.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PeticionController : ControllerBase
    {
        private readonly PeticionService _service;

        public PeticionController(PeticionService service)
        {
            _service = service;
        }

        [HttpPost]
        [ProducesResponseType(typeof(PeticionDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CrearPeticion([FromBody] CreatePeticionDto dto)
        {
            try
            {
                var created = await _service.CrearPeticionAsync(dto);
                return CreatedAtAction(nameof(GetPeticion), new { id = created.Id }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return Problem(title: "Error interno", detail: ex.Message);
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PeticionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPeticion(string id)
        {
            try
            {
                var peticion = await _service.ObtenerPeticionAsync(id);
                return peticion is not null ? Ok(peticion) : NotFound();
            }
            catch (Exception ex)
            {
                return Problem(title: "Error interno", detail: ex.Message);
            }
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<PeticionDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListPeticiones([FromQuery] string? estado, [FromQuery] string? espacioId, [FromQuery] string? solicitanteId)
        {
            try
            {
                var list = await _service.ListarTodasAsync();

                // Aplicar filtros si se proporcionan
                if (!string.IsNullOrWhiteSpace(estado))
                {
                    list = list.Where(p => p.Estado?.Equals(estado, StringComparison.OrdinalIgnoreCase) == true).ToList();
                }

                if (!string.IsNullOrWhiteSpace(espacioId))
                {
                    list = list.Where(p => p.IdEspacio?.Equals(espacioId, StringComparison.OrdinalIgnoreCase) == true).ToList();
                }

                if (!string.IsNullOrWhiteSpace(solicitanteId))
                {
                    list = list.Where(p => p.IdSolicitante?.Equals(solicitanteId, StringComparison.OrdinalIgnoreCase) == true).ToList();
                }

                return Ok(list);
            }
            catch (Exception ex)
            {
                return Problem(title: "Error interno", detail: ex.Message);
            }
        }

        [HttpPost("{id}/estado")]
        [ProducesResponseType(typeof(PeticionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CambiarEstadoPeticion(string id, [FromBody] CambiarEstadoPeticionDto dto)
        {
            try
            {
                var updated = await _service.CambiarEstadoAsync(id, dto.Accion);
                return updated is not null ? Ok(updated) : NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return Problem(title: "Error interno", detail: ex.Message);
            }
        }

        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(PeticionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PatchPeticion(string id, [FromBody] UpdatePeticionDto dto)
        {
            try
            {
                var updated = await _service.ActualizarParcialAsync(id, dto);
                return updated is not null ? Ok(updated) : NotFound();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return Problem(title: "Error interno", detail: ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> EliminarPeticion(string id)
        {
            try
            {
                var ok = await _service.EliminarPeticionAsync(id);
                return ok ? NoContent() : NotFound();
            }
            catch (Exception ex)
            {
                return Problem(title: "Error interno", detail: ex.Message);
            }
        }
    }

    // DTO auxiliar para cambiar estado
    public class CambiarEstadoPeticionDto
    {
        public string Accion { get; set; } = string.Empty;
    }
}
