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
    [FirestoreData]
    public class Espacio
    {
        [FirestoreProperty]
        public string Id_Espacio { get; set; } = Guid.NewGuid().ToString(); // <- ahora con setter

        [FirestoreProperty]
        public string Nombre { get; set; }

        [FirestoreProperty]
        public string? Direccion { get; set; }

        [JsonIgnore]
        public List<Sala> Salas { get; set; } = new List<Sala>();

        [JsonIgnore]
        public List<UsuarioEspacio> UsuarioEspacios { get; set; } = new List<UsuarioEspacio>();

        [JsonIgnore]
        public List<Peticion> Peticiones { get; set; } = new List<Peticion>();

        [JsonIgnore]
        public List<Invitacion> InvitacionesEnviadas { get; set; } = new List<Invitacion>();

        public Espacio() { }

        public Espacio(string name, string? direccion = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("El nombre del espacio no puede estar vacío.");

            Nombre = name;
            Direccion = direccion;
        }

        public Sala CrearSala(string nombre, string? descripcion = null)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre de la sala no puede estar vacío.");

            if (Salas.Any(s => s.Nombre == nombre))
                throw new InvalidOperationException("Ya existe una sala con ese nombre.");

            // Crear la sala usando inicializador de objeto para evitar dependencias de constructor
            var nuevaSala = new Sala
            {
                Nombre = nombre,
                Descripcion = descripcion,
                Id_Sala = Guid.NewGuid().ToString(),
                Id_Espacio = this.Id_Espacio,
                Espacio = this
            };

            Salas.Add(nuevaSala);
            return nuevaSala;
        }

        public bool EliminarSala(string nomsala)
        {
            if (string.IsNullOrWhiteSpace(nomsala))
                throw new ArgumentException("El nombre de la sala no puede estar vacío.");

            Sala sala = Salas.Find(s => s.Nombre == nomsala);
            if (sala != null)
            {
                Salas.Remove(sala);
                return true;
            }
            return false;
        }

        public Sala BuscarSalaNombre(string nomsala)
        {
            if (string.IsNullOrWhiteSpace(nomsala))
                throw new ArgumentException("El nombre de la sala no puede estar vacío.");

            return Salas.Find(s => s.Nombre == nomsala);
        }

        public bool AdmitirUsuario(Usuario usuario)
        {
            if (usuario == null)
                throw new ArgumentNullException(nameof(usuario));

            if (UsuarioEspacios.Any(u => u.Usuario.Id == usuario.Id))
                throw new InvalidOperationException("El usuario ya está admitido en el espacio.");

            if (!Peticiones.Any(p => p.IdSolicitante == usuario.Id))
                throw new InvalidOperationException("No existe una petición para este usuario.");

            UsuarioEspacio usuarioEspacio = new UsuarioEspacio
            {
                Usuario = usuario,
                Espacio = this,
                Permiso = Permiso.Usuario,
                Ausente = false,
                Karma = 0
            };

            UsuarioEspacios.Add(usuarioEspacio);
            Peticiones.RemoveAll(p => p.IdSolicitante == usuario.Id);
            return true;
        }

        public void InvitarUsuario(Usuario usuario)
        {
            if (usuario == null)
                throw new ArgumentNullException(nameof(usuario));

            Invitacion nuevaInvitacion = new Invitacion
            {
                Espacio = this,
                UsuarioInvitado = usuario,
                Fecha = DateTime.UtcNow
            };
            InvitacionesEnviadas.Add(nuevaInvitacion);
            usuario.Invitaciones.Add(nuevaInvitacion);
        }

        public bool EliminarUsuario(UsuarioEspacio usuariodelespacio)
        {
            if (usuariodelespacio != null)
            {
                UsuarioEspacios.Remove(usuariodelespacio);
                return true;
            }
            return false;
        }
    }
}