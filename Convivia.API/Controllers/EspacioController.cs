using Microsoft.AspNetCore.Mvc;
using Convivia.Application.DTOs;
using Convivia.Infrastructure.Services;

namespace Convivia.API.Controllers
{
    [ApiController]
    [Route("api/espacios")]
    public class EspacioController : ControllerBase
    {
        private readonly EspacioService _service;
        public EspacioController(EspacioService service) => _service = service;

        [HttpPost]
        public async Task<ActionResult<EspacioDto>> Create([FromBody] CreateEspacioDto dto)
        {
            var espacioDto = await _service.AddAsync(dto);
            return Created($"/api/espacios/{espacioDto.Id_Espacio}", espacioDto);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EspacioDto>> Get(string id)
        {
            var espacioDto = await _service.GetAsync(id);
            return espacioDto != null ? Ok(espacioDto) : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var deleted = await _service.DeleteAsync(id);
            return deleted ? NoContent() : NotFound();
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<EspacioDto>> Patch(string id, [FromBody] UpdateEspacioDto dto)
        {
            var espacioDto = await _service.PatchAsync(id, dto);
            return espacioDto != null ? Ok(espacioDto) : NotFound();
        }
    }
}
