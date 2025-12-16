
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
        public string Id { get; set; } = Guid.NewGuid().ToString("N");


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
        
    }
}