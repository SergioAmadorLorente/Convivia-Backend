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
        public async Task<ActionResult<string>> CreateTarea(string espacioid, CreateTareaDto dto)
        {
            var tareaId = await _service.AddAsync(espacioid, dto);
            return CreatedAtAction(nameof(GetTareaById), new { espacioid = espacioid, id = tareaId }, tareaId);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TareaDto>> GetTareaById(string espacioid, string id)
        {
            var tareaDto = await _service.GetByIdAsync(espacioid, id);
            return tareaDto != null ? Ok(tareaDto) : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTarea(string espacioid, string id)
        {
            var deleted = await _service.DeleteAsync(espacioid, id);
            return deleted ? NoContent() : NotFound();
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<TareaDto>> UpdateTarea(string espacioid, string id, UpdateTareaDto dto)
        {
            var tareaDto = await _service.UpdateAsync(espacioid, id, dto);
            return tareaDto != null ? Ok(tareaDto) : NotFound();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TareaDto>>> GetTareasByEspacioId(string espacioid)
        {
            var list = await _service.GetAllByEspacioAsync(espacioid);
            return !list.Any() ? NotFound() : Ok(list);
        }
    }
}