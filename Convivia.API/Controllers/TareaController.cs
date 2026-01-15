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
using System.Threading;

namespace Convivia.API.Controllers
{
    [ApiController]
    [Route("api/espacios/{espacioid}/tareas")]
    [Produces("application/json")]
    public class TareaController : ControllerBase
    {
        private readonly TareaService _service;
        private readonly PlantillaTareaService _ptservice;

        public TareaController(TareaService service, PlantillaTareaService ptservice)
        {
            _service = service;
            _ptservice = ptservice;
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
            var list = await _ptservice.GetAllByEspacioAsync(espacioid);
            return !list.Any() ? NotFound() : Ok(list);
        }

        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<TareaDto>>> Filter(
            string espacioid, 
            [FromQuery] int? diaSemana, 
            [FromQuery] string? estado,
            [FromQuery] string? usuarioId)
        {
            var res = await _service.FilterAsync(espacioid, diaSemana, estado, usuarioId);
            return (res == null || !res.Any()) ? NotFound() : Ok(res);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTarea(string espacioid, string id)
        {
            var deleted = await _service.DeleteAsync(espacioid, id);
            return deleted ? NoContent() : NotFound();
        }

        [HttpPut("{plantillaId}/{tareaId}")]
        public async Task<ActionResult<TareaDto>> PutOverwrite(string espacioid, string plantillaId, string tareaId, [FromBody] UpdateTareaDto dto, CancellationToken ct)
        {
            var updated = await _service.UpdateCompleteAsync(espacioid, plantillaId, tareaId, dto, ct);
            return updated != null ? Ok(updated) : NotFound();
        }

        [HttpPut("{plantillaId}/{tareaId}/merge")]
        public async Task<ActionResult<TareaDto>> PutMerge(string espacioid, string plantillaId, string tareaId, [FromBody] UpdateTareaDto dto, CancellationToken ct)
        {
            var updated = await _service.UpdateMergeAsync(espacioid, plantillaId, tareaId, dto, ct);
            return updated != null ? Ok(updated) : NotFound();
        }

        [HttpPatch("{plantillaId}/{tareaId}")]
        public async Task<ActionResult<TareaDto>> Patch(string espacioid, string plantillaId, string tareaId, [FromBody] UpdateTareaDto dto, CancellationToken ct)
        {
            var updated = await _service.UpdatePartialAsync(espacioid, plantillaId, tareaId, dto, ct);
            return updated != null ? Ok(updated) : NotFound();
        }
    }
}