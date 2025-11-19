using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;

namespace Convivia.Domain.Models
{
    [FirestoreData]
    public class Usuario
    {
        [FirestoreProperty]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [FirestoreProperty]
        public string Nombre { get; set; }

        [FirestoreProperty]
        public string Email { get; set; }

        [FirestoreProperty]
        public string Password { get; set; }

        [FirestoreProperty]
        public string? Telefono { get; set; }

        [FirestoreProperty]
        public bool Premium { get; set; }

        [FirestoreProperty]
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        public List<UsuarioEspacio> UsuarioEspacios { get; set; } = new List<UsuarioEspacio>();

        [JsonIgnore]
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

        // Métodos (lógica de negocio)
        public void unirseEspacio(Espacio espacio)
        {
            var invitacion = Invitaciones
                .FirstOrDefault(i => i.Espacio.Id_Espacio == espacio.Id_Espacio);

            var peticion = espacio.Peticiones
                .FirstOrDefault(p => p.Solicitante == this);

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

            var nuevaPeticion = new Peticion
            {
                Id = Guid.NewGuid().ToString(),
                Solicitante = this,
                Mensaje = $"Solicitud de acceso al espacio {espacio.Nombre}",
                Fecha = DateTime.UtcNow,
                Estado = "pendiente"
            };

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