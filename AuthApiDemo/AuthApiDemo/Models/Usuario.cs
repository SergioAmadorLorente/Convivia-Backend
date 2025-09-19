using Microsoft.AspNetCore.Identity;
namespace AuthApiDemo.Models
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

        public List<UsuarioEspacio> UsuariosEspacios { get; set; } = new List<UsuarioEspacio>();

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
                .FirstOrDefault(i => i.Espacio.Id_Espacio == espacio.Id_Espacio);
            
            var peticion = espacio.Peticiones
                .FirstOrDefault(p => p.Solicitante == this);

            
            if (invitacion != null && peticion != null)
            {
                if (!espacio.Usuarios.Contains(this))
                {
                    espacio.Usuarios.Add(this);
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

            nuevoEspacio.Usuarios.Add(this);
            
        }

    }

}
