using Convivia.Application.DTOs;
using Convivia.Application.Services;
using Convivia.Infrastructure.Services;
using Convivia.Shared.DTOs;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Convivia.API.Controllers
{
    [ApiController]
    [Route("api/tareas/{espacioid}")]
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
            var TareaDto = await _service.AddAsync(espacioid, dto);
            return TareaDto;
        }

        [HttpGet]
        public async Task<ActionResult<List<TareaDto>>> GetAllTareas(string espacioid)
        {
            var list = await _service.GetAsync(espacioid);
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TareaDto>> GetTareaById(string espacioid, string id)
        {
            var TareaDto = await _service.GetByIdAsync(espacioid, id);
            return TareaDto != null ? Ok(TareaDto) : NotFound();
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
            var TareaDto = await _service.UpdateAsync(espacioid, id, dto);
            return TareaDto != null ? Ok(TareaDto) : NotFound();
        }
    }
}