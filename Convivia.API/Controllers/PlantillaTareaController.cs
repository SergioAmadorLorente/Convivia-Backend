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
    [Route("api/plantillas")]
    [Produces("application/json")]
    public class PlantillaTareaController : ControllerBase
    {
        private readonly PlantillaTareaService _service;

        public PlantillaTareaController(PlantillaTareaService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<ActionResult<string>> CreatePlantilla(CreatePlantillaTareaDto dto)
        {
            var plantillaDtoId = await _service.AddAsync(dto);
            return plantillaDtoId;
        }

        [HttpGet]
        public async Task<ActionResult<List<PlantillaTareaDto>>> GetAllPlantillas()
        {
            var list = await _service.GetAsync();
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PlantillaTareaDto>> GetPlantillaById(string id)
        {
            var plantillaDto = await _service.GetByIdAsync(id);
            return plantillaDto != null ? Ok(plantillaDto) : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePlantilla(string id)
        {
            var deleted = await _service.DeleteAsync(id);
            return deleted ? NoContent() : NotFound();
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<PlantillaTareaDto>> UpdatePlantilla(string id, UpdatePlantillaTareaDto dto)
        {
            var plantillaDto = await _service.UpdateAsync(id, dto);
            return plantillaDto != null ? Ok(plantillaDto) : NotFound();
        }
    }
}