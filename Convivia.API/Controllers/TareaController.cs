using Convivia.Application.DTOs;
using Convivia.Application.Services;
using Convivia.Infrastructure.Services;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Convivia.API.Controllers
{
    [ApiController]
    [Route("api/tareas")]
    [Produces("application/json")]
    public class TareaController : ControllerBase
    {
        private readonly TareaService _service;

        public TareaController(TareaService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<ActionResult<TareaDto>> CreateTarea(CreateTareaDto dto)
        {
            var TareaDto = await _service.AddAsync(dto);
            return CreatedAtAction(nameof(GetTareaById), new { id = TareaDto.Id }, TareaDto);
        }

        [HttpGet]
        public async Task<ActionResult<List<TareaDto>>> GetAllTareas()
        {
            var list = await _service.GetAsync();
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TareaDto>> GetTareaById(string id)
        {
            var TareaDto = await _service.GetByIdAsync(id);
            return TareaDto != null ? Ok(TareaDto) : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTarea(string id)
        {
            var deleted = await _service.DeleteAsync(id);
            return deleted ? NoContent() : NotFound();
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<TareaDto>> UpdateTarea(string id, UpdateTareaDto dto)
        {
            var TareaDto = await _service.UpdateAsync(id, dto);
            return TareaDto != null ? Ok(TareaDto) : NotFound();
        }
    }
}