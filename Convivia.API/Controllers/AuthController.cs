
/*
 * 
 * Auth controller no deveria tener una referencia al modelo
 * 
 * using Microsoft.AspNetCore.Mvc;
using AuthApiDemo.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthApiDemo.Controllers
{
    // Indica que esta clase es un controlador de API
    [ApiController]

    // Define la ruta base del controlador: api/auth
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        // Lista estática simulada de usuarios registrados (solo para pruebas)
        private static List<User> users = new List<User>
        {
            new User { Email = "test@domain.com", Password = "password1" },
            new User { Email = "user@example.com", Password = "12345678" }
        };

        // Endpoint HTTP POST que recibe datos de login
        [HttpPost("login")]
        public IActionResult Login([FromBody] User loginData)
        {
            // Busca un usuario en la lista que coincida con el email y la contraseña recibidos
            var user = users.FirstOrDefault(u => u.Email == loginData.Email && u.Password == loginData.Password);

            // Si no se encuentra, devuelve un error 401 (no autorizado)
            if (user == null)
            {
                return Unauthorized(new { message = "Email o contraseña incorrecta" });
            }

            // Si se encuentra, devuelve un mensaje de éxito (200 OK)
            return Ok(new { message = "Login exitoso" });
        }

        // firestore

        [HttpGet("probar-firestore")]
        public async Task<IActionResult> ProbarFirestore([FromServices] UserService userService)
        {
            bool ok = await userService.ProbarConexionAsync();
            if (ok) return Ok("Conexión exitosa");
            else return StatusCode(500, "Error de conexión");
        }
    }
}*/