using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;

namespace Convivia.Domain.Entities
{
    public class Usuario
    {
        public string Id { get; set; } // No GUID!!!!!!!!

        
        public string Nombre { get; set; }

        
        public string Email { get; set; }

        
        public string Password { get; set; }

        
        public string? Telefono { get; set; }

        
        public bool Premium { get; set; }

        
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

      
        public List<UsuarioEspacio> UsuarioEspacios { get; set; } = new List<UsuarioEspacio>();

        public List<Invitacion> Invitaciones { get; set; } = new List<Invitacion>();

        public Usuario()
        {
        }

        public Usuario(string nombre, string email, string password, string? telefono = null, bool premium = false)
        {
            Nombre = nombre;
            Email = email;
            Password = password;
            Telefono = telefono;
            Premium = premium;
        }
    }
}