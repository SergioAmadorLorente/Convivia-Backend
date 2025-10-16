using Google.Cloud.Firestore;
using System.Text.Json.Serialization;

namespace AuthApiDemo.Models
{
    /// <summary>
    /// Representa un espacio que puede contener salas, UsuariosEspacios, peticiones e invitaciones.
    /// </summary>
    [FirestoreData]
    public class Espacio
    {
        /// <summary>
        /// Identificador único del espacio.
        /// </summary>
        [FirestoreProperty]
        public string Id_Espacio { get; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Nombre del espacio.
        /// </summary>
        [FirestoreProperty]
        public string Nombre { get; set; }

        /// <summary>
        /// Dirección del espacio (opcional).
        /// </summary>
        [FirestoreProperty]
        public string? Direccion { get; set; }

        /// <summary>
        /// Colección de salas asociadas al espacio.
        /// </summary>
        [JsonIgnore]
        public List<Sala> Salas { get; set; } = new List<Sala>();

        /// <summary>
        /// Colección de UsuariosEspacios que pertenecen al espacio.
        /// </summary>
        [JsonIgnore]
        public List<UsuarioEspacio> UsuarioEspacios { get; set; } = new List<UsuarioEspacio>();

        /// <summary>
        /// Colección de peticiones de acceso al espacio.
        /// </summary>
        [JsonIgnore]
        public List<Peticion> Peticiones { get; set; } = new List<Peticion>();

        /// <summary>
        /// Colección de invitaciones enviadas desde el espacio.
        /// </summary>
        [JsonIgnore]
        public List<Invitacion> InvitacionesEnviadas { get; set; } = new List<Invitacion>();

        /// <summary>
        /// Constructor por defecto, útil para pruebas o deserialización.
        /// </summary>
        public Espacio() { }

        /// <summary>
        /// Constructor principal que inicializa el espacio con nombre y dirección.
        /// Valida que el nombre no esté vacío.
        /// </summary>
        public Espacio(string name, string? direccion = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("El nombre del espacio no puede estar vacío.");

            Nombre = name;
            Direccion = direccion;
        }

        /// <summary>
        /// Crea una nueva sala en el espacio, validando el nombre y evitando duplicados.
        /// </summary>
        public Sala CrearSala(string nombre, string? descripcion = null)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre de la sala no puede estar vacío.");

            if (Salas.Any(s => s.Nombre == nombre))
                throw new InvalidOperationException("Ya existe una sala con ese nombre.");

            Sala nuevaSala = new Sala
            {
                Id_Sala = Guid.NewGuid().ToString(),
                Nombre = nombre,
                Descripcion = descripcion,
                Id_Espacio = this.Id_Espacio,
                Espacio = this
            };

            Salas.Add(nuevaSala);
            return nuevaSala;
        }

        /// <summary>
        /// Elimina una sala del espacio por su nombre.
        /// </summary>  
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

        /// <summary>
        /// Busca y devuelve una sala por su nombre.
        /// </summary>
        public Sala BuscarSalaNombre(string nomsala)
        {
            if (string.IsNullOrWhiteSpace(nomsala))
                throw new ArgumentException("El nombre de la sala no puede estar vacío.");

            return Salas.Find(s => s.Nombre == nomsala);
        }

        /// <summary>
        /// Admite un usuario en el espacio si existe una petición previa y no está ya admitido.
        /// </summary>
        public bool AdmitirUsuario(Usuario usuario)
        {
            if (usuario == null)
                throw new ArgumentNullException(nameof(usuario));

            if (UsuarioEspacios.Any(u => u.Usuario.Id == usuario.Id))
                throw new InvalidOperationException("El usuario ya está admitido en el espacio.");

            if (!Peticiones.Any(p => p.Solicitante == usuario))
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
            Peticiones.RemoveAll(p => p.Solicitante == usuario);
            return true;
        }

        /// <summary>
        /// Envía una invitación a un usuario para unirse al espacio.
        /// </summary>
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

        /// <summary>
        /// Elimina un usuario del espacio por su nombre.
        /// </summary>
        public bool EliminarUsuario(UsuarioEspacio usuariodelespacio)
        {

            var usuariocomprobado = this.UsuarioEspacios.Find(u => u.Usuario.Nombre == usuariodelespacio.Usuario.Nombre);

            if (usuariodelespacio != null)
            {
                UsuarioEspacios.Remove(usuariodelespacio);
                return true;
            }

            return false;
        }
    }
}