using Convivia.Application.Services;
using Convivia.Infrastructure.Services;
using Convivia.Shared.DTOs;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Convivia.API.Controllers
{
    [ApiController]
    [Route("api/espacios/{espacioid}/tareas")]
    [Produces("application/json")]
    public class TareaController : ControllerBase
    {
        private readonly TareaService _service;

        public TareaController(TareaService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<ActionResult<List<string>>> CreateTarea(string espacioid, [FromBody] CreateTareaDto dto)
        {
            var tareaIds = await _service.AddAsync(espacioid, dto);
            return CreatedAtAction(nameof(GetTareasByEspacioId), new { espacioid = espacioid }, tareaIds);
        }

        [HttpGet("{plantillaId}/{tareaId}")]
        public async Task<ActionResult<TareaDto>> GetTareaById(string espacioid, string plantillaId, string tareaId)
        {
            var tareaDto = await _service.GetByEspacioAndPlantillaAndTareaAsync(espacioid, plantillaId, tareaId);
            return tareaDto != null ? Ok(tareaDto) : NotFound();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PlantillaTareaDto>>> GetTareasByEspacioId(string espacioid)
        {
            var list = await _service.GetAllByEspacioAsync(espacioid);
            return !list.Any() ? NotFound() : Ok(list);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTarea(string espacioid, string id)
        {
            var deleted = await _service.DeleteAsync(espacioid, id);
            return deleted ? NoContent() : NotFound();
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<TareaDto>> UpdateTarea(string espacioid,string id, [FromBody] UpdateTareaDto dto)
        {
            var tareaDto = await _service.UpdateAsync(espacioid, id, dto);
            return tareaDto != null ? Ok(tareaDto) : NotFound();
        }
    }
}