using Convivia.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Convivia.Domain.Entities
{
    /// <summary>
    /// Representa un espacio que puede contener salas, UsuariosEspacios, peticiones e invitaciones.
    /// </summary>
    public class Espacio
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N"); // <- ahora con setter
        public string Nombre { get; set; }

        public string? Direccion { get; set; }

        public List<Sala> Salas { get; set; } = new List<Sala>();

        public List<UsuarioEspacio> UsuarioEspacios { get; set; } = new List<UsuarioEspacio>();

        public List<Peticion> Peticiones { get; set; } = new List<Peticion>();

        public List<Invitacion> InvitacionesEnviadas { get; set; } = new List<Invitacion>();

        public Espacio() { }

        public Espacio(string name, string? direccion = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("El nombre del espacio no puede estar vacío.");

            Nombre = name;
            Direccion = direccion;
        }

        public bool AdmitirUsuario(Usuario usuario)
        {
            if (usuario == null)
                throw new ArgumentNullException(nameof(usuario));

            if (UsuarioEspacios.Any(u => u.Usuario.Id == usuario.Id))
                throw new InvalidOperationException("El usuario ya está admitido en el espacio.");

            if (!Peticiones.Any(p => p.IdSolicitante == usuario.Id))
                throw new InvalidOperationException("No existe una petición para este usuario.");

            var rolUsuario = new Rol();
            rolUsuario.SetConfiguracionUsuario();
            var permisoUsuario = new Permiso(rolUsuario);

            UsuarioEspacio usuarioEspacio = new UsuarioEspacio
            {
                Usuario = usuario,
                Espacio = this,
                Permiso = permisoUsuario,
                Ausente = false,
                karma = 0
            };

            UsuarioEspacios.Add(usuarioEspacio);
            Peticiones.RemoveAll(p => p.IdSolicitante == usuario.Id);
            return true;
        }
    }
}