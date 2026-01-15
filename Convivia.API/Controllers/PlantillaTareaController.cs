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
    [Route("api/espacio/{espacioid}")]
    [Produces("application/json")]
    public class PlantillaTareaController : ControllerBase
    {
        private readonly PlantillaTareaService _service;

        public PlantillaTareaController(PlantillaTareaService service)
        {
            _service = service;
        }

        [HttpPatch("{plantillaId}")]
        public async Task<ActionResult<PlantillaTareaDto>> UpdatePlantilla(string espacioid, string plantillaId, UpdatePlantillaTareaDto dto)
        {
            var plantillaDto = await _service.UpdateAsync(espacioid, plantillaId, dto);
            return plantillaDto != null ? Ok(plantillaDto) : NotFound();
        }
    }
}