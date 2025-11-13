using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AuthApiDemo.DTOs;
using AuthApiDemo.Services;

namespace AuthApiDemo.Controllers
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
        [ProducesResponseType(typeof(TareaDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TareaDto>> CreateTarea(CreateTareaDto dto)
        {
            var tareaDto = await _service.AddAsync(dto);
            return CreatedAtAction(nameof(GetTareaById), new { id = tareaDto.IdTarea }, tareaDto);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(TareaDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TareaDto>> GetTareaById(string id)
        {
            var tareaDto = await _service.GetAsync(id);
            return tareaDto != null ? Ok(tareaDto) : NotFound();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteTarea(string id)
        {
            var deleted = await _service.DeleteAsync(id);
            return deleted ? NoContent() : NotFound();
        }

        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(TareaDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TareaDto>> PatchTarea(string id, UpdateTareaDto dto)
        {
            var tareaDto = await _service.PatchAsync(id, dto);
            return tareaDto != null ? Ok(tareaDto) : NotFound();
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<TareaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<TareaDto>>> GetAllTareas()
        {
            var tareas = await _service.GetAllAsync();
            return Ok(tareas);
        }

        [HttpGet("filtrarestado")]
        [ProducesResponseType(typeof(List<TareaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<TareaDto>>> GetByEstado(bool estado)
        {
            var tareas = await _service.GetByEstadoAsync(estado);
            return Ok(tareas);
        }

        [HttpGet("filtrarfecha")]
        [ProducesResponseType(typeof(List<TareaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<TareaDto>>> GetByFecha(string fechainicio, string fechafinal)
        {
            var inicio = DateTime.Parse(fechainicio, null, System.Globalization.DateTimeStyles.RoundtripKind);
            var final = DateTime.Parse(fechafinal, null, System.Globalization.DateTimeStyles.RoundtripKind);

            var todas = await _service.GetAllAsync();
            var tareasFiltradas = todas.Where(t => t.FechaLimite >= inicio && t.FechaLimite <= final).ToList();

            return tareasFiltradas.Count != 0 ? Ok(tareasFiltradas) : NotFound();
        }

        [HttpPatch("actualizarvariastareas")]
        [ProducesResponseType(typeof(TareaDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TareaDto>> PatchVariasTareas(PatchVariasTareasDto data)
        {
            var tareaDto = await _service.PatchVariasAsync(data.ListaIds, data.Dto);
            return tareaDto != null ? Ok(tareaDto) : NotFound();
        }
    }
}
