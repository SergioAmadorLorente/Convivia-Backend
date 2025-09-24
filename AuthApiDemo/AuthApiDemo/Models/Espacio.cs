namespace AuthApiDemo.Models
{
    public class Espacio
    {

        public string Id_Espacio { get; set; } = Guid.NewGuid().ToString();

        public string Nombre { get; set; }

        public string? Direccion { get; set; }

        public Espacio() { }

        public Espacio(string name, string? direccion = null)
        {
            Nombre = name;
            Direccion = direccion;
        }

        public List<Sala> Salas { get; set; } = new List<Sala>();
        public List<Usuario> Usuarios { get; set; } = new List<Usuario>();
        public List<Peticion> Peticiones { get; set; } = new List<Peticion>();

        public List<Invitacion> InvitacionesEnviadas { get; set; } = new List<Invitacion>();

        public Sala CrearSala(string nombre, string? descripcion = null)
        {
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


        public bool EliminarSala(string nomsala)
        {
            Sala sala = Salas.Find(s => s.Nombre == nomsala);
            if (sala != null)
            {
                Salas.Remove(sala);
                return true;
            }

            else
            {

                Console.WriteLine("Sala no encontrada");
                return false;

            }

        }

        public Sala BuscarSalaNombre(string nomsala)
        {
            Sala sala = Salas.Find(s => s.Nombre == nomsala);
            if (sala != null)
            {
                return sala;
            }
            else
            {
                Console.WriteLine("Sala no encontrada");
                return null;
            }
        }

        public bool AdmitirUsuario(Usuario usuario)
        {

            Peticiones.Find(p => p.Solicitante == usuario);
            if (usuario != null)
            {
                Usuarios.Add(usuario);
                Peticiones.RemoveAll(p => p.Solicitante == usuario);
                return true;
            }
            else
            {
                Console.WriteLine("Usuario no encontrado");
                return false;
            }

        }

        public void invitarUsuario(Usuario usuario)
        {
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

        public bool EliminarUsuario(string nomusuario)
        {
            Usuario usuario = Usuarios.Find(u => u.Nombre == nomusuario);
            if (usuario != null)
            {
                Usuarios.Remove(usuario);
                return true;
            }
            else
            {
                Console.WriteLine("Usuario no encontrado");
                return false;
            }
        }

    }

}