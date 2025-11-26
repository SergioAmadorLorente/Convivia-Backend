using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Convivia.Domain.Models
{
    /// <summary>
    /// Representa un espacio que puede contener salas, UsuariosEspacios, peticiones         e invitaciones.
    /// </summary>
    public class Espacio
    {
        public string Id { get; set; } = Guid.NewGuid().ToString(); // <- ahora con setter

        public string Nombre { get; set; }

        public string? Direccion { get; set; }

        /* Fins seguent sprint a fix
       public List<Sala> Salas { get; set; } = new List<Sala>();

       public List<UsuarioEspacio> UsuarioEspacios { get; set; } = new List<UsuarioEspacio>();

       public List<Peticion> Peticiones { get; set; } = new List<Peticion>();

       public List<Invitacion> InvitacionesEnviadas { get; set; } = new List<Invitacion>();
       */
        public Espacio() { }

       
    }
}