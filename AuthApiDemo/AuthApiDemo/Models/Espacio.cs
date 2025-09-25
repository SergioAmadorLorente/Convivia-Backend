namespace AuthApiDemo.Models
{
    /// <summary>
    /// Representa un espacio que puede contener salas, usuarios, peticiones e invitaciones.
    /// </summary>
    public class Espacio
    {
        /// <summary>
        /// Identificador único del espacio.
        /// </summary>
        public string Id_Espacio { get; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Nombre del espacio.
        /// </summary>
        public string Nombre { get; set; }

        /// <summary>
        /// Dirección del espacio (opcional).
        /// </summary>
        public string? Direccion { get; set; }

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
        /// Colección de salas asociadas al espacio.
        /// </summary>
        public List<Sala> Salas { get; set; } = new List<Sala>();

        /// <summary>
        /// Colección de usuarios que pertenecen al espacio.
        /// </summary>
        public List<Usuario> Usuarios { get; set; } = new List<Usuario>();

        /// <summary>
        /// Colección de peticiones de acceso al espacio.
        /// </summary>
        public List<Peticion> Peticiones { get; set; } = new List<Peticion>();

        /// <summary>
        /// Colección de invitaciones enviadas desde el espacio.
        /// </summary>
        public List<Invitacion> InvitacionesEnviadas { get; set; } = new List<Invitacion>();

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

            if (Usuarios.Any(u => u.Id == usuario.Id))
                throw new InvalidOperationException("El usuario ya está admitido en el espacio.");

            if (!Peticiones.Any(p => p.Solicitante == usuario))
                throw new InvalidOperationException("No existe una petición para este usuario.");

            Usuarios.Add(usuario);
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
                Id = Guid.NewGuid().ToString(),
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
        public bool EliminarUsuario(string nomusuario)
        {
            if (string.IsNullOrWhiteSpace(nomusuario))
                throw new ArgumentException("El nombre del usuario no puede estar vacío.");

            Usuario usuario = Usuarios.Find(u => u.Nombre == nomusuario);
            if (usuario != null)
            {
                Usuarios.Remove(usuario);
                return true;
            }
            return false;
        }
    }
}