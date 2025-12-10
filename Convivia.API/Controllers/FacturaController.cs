using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Convivia.Shared.DTOs;
using Convivia.Application.Services;

namespace Convivia.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FacturaController : ControllerBase
    {
        private readonly FacturaService _service;

        public FacturaController(FacturaService service)
        {
            _service = service;
        }

        // POST api/factura
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateFacturaDto model, CancellationToken ct)
        {
            if (model == null) return BadRequest();
            if (string.IsNullOrWhiteSpace(model.Nombre)) return BadRequest("Nombre es requerido.");
            if (model.Precio < 0) return BadRequest("Precio no puede ser negativo.");

            var created = await _service.CrearFacturaAsync(model);
            return CreatedAtAction(nameof(GetById), new { id = created.IdFactura }, created);
        }

        // GET api/factura/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            var factura = await _service.ObtenerFacturaAsync(id);
            if (factura == null) return NotFound();
            return Ok(factura);
        }

        // GET api/factura
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var list = await _service.ListarTodasAsync();
            return Ok(list);
        }

        // PUT api/factura/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateFacturaDto model, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id) || model == null) return BadRequest();

            await _service.ActualizarFacturaAsync(id, model);
            return NoContent();
        }

        // DELETE api/factura/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            await _service.EliminarFacturaAsync(id);
            return NoContent();
        }
    }
}
