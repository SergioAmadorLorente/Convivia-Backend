using Microsoft.AspNetCore.Mvc;

namespace Convivia.API.Controllers
{
    [ApiController]
    [Route("health")]
    public class HealthController : ControllerBase
    {
        // Metodo sencillio para verificar que la API está funcionando
        [HttpGet]
        public IActionResult Get() => Ok(new { status = "Healthy" });
    }
}
