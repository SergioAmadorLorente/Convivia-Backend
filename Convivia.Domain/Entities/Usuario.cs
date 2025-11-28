using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;

namespace Convivia.Domain.Entities
{
    public class Usuario
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        
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

        // Métodos

        public void unirseEspacio(Espacio espacio)
        {
            var invitacion = Invitaciones
                .FirstOrDefault(i => i.Espacio.Id == espacio.Id);

            // Buscar petición por ID (ahora solo tiene Id, Mensaje, Fecha, Estado)
            var peticion = espacio.Peticiones
                .FirstOrDefault(p => p.Id == this.Id); // Asumiendo que el ID de petición coincide con el usuario

            if (invitacion != null && peticion != null)
            {
                if (!espacio.UsuarioEspacios.Contains(this.UsuarioEspacios.FirstOrDefault(u => u.Usuario.Id == this.Id)))
                {
                    UsuarioEspacio usuarioEspacio = new UsuarioEspacio
                    {
                        Usuario = this,
                        Espacio = espacio,
                        Permiso = Permiso.Usuario,
                        Ausente = false,
                        Karma = 0
                    };

                    espacio.UsuarioEspacios.Add(usuarioEspacio);
                }

                espacio.Peticiones.Remove(peticion);
                this.Invitaciones.Remove(invitacion);
                return;
            }

            if (peticion != null)
            {
                return;
            }

            // Crear nueva petición usando el constructor público con ID autogenerado
            var nuevaPeticion = new Peticion(
                id: Guid.NewGuid().ToString(),
                mensaje: $"Solicitud de acceso al espacio {espacio.Nombre}",
                idSolicitante: this.Id,
                idEspacio: espacio.Id
            );

            espacio.Peticiones.Add(nuevaPeticion);
        }

        public void actualizarPerfil(string? Nombre = null, string? Email = null, string? Telefono = null, string? Password = null)
        {
            if (Nombre != null) this.Nombre = Nombre;
            if (Email != null) this.Email = Email;
            if (Password != null) this.Password = Password;
            if (Telefono != null) this.Telefono = Telefono;
        }

        public void crearEspacio(string nombre, string direccion)
        {
            Espacio nuevoEspacio = new Espacio
            {
                Nombre = nombre,
                Direccion = direccion,
            };

            nuevoEspacio.UsuarioEspacios.Add(
                new UsuarioEspacio(this, nuevoEspacio, Permiso.Admin, "admin", false, 0)
            );
        }
    }
}