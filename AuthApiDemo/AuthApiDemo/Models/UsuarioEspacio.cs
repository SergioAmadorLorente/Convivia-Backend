namespace AuthApiDemo.Models
{

    public class UsuarioEspacio
    {

        public string Id_UsuarioEspacio { get; set; } = Guid.NewGuid().ToString();

        public bool ausente { get; set; }

        public int karma { get; set; }
        public string rol { get; set; }

        public Espacio espacio { get; set; }

        public Usuario usuario { get; set; }

        public List<Tarea> TareasAsignadas { get; set; } = new List<Tarea>();
        public Permiso permiso { get; set; }
        public List<Factura> Facturas { get; set; } = new List<Factura>(); // ?

        // Constructor por defecto vacio para pruebas
        public UsuarioEspacio()
        {
        }

        // Constructor que asigna el permiso según el rol proporcionado
        public UsuarioEspacio(Usuario usuario, Espacio espacio, Permiso permiso, bool ausente = false, int karma = 0, string rol)
        {
            Id_UsuarioEspacio = Guid.NewGuid().ToString();
            this.usuario = usuario;
            this.espacio = espacio;
            this.ausente = ausente;
            this.karma = karma;

            if (rol == "admin")
            {
                this.permiso = Permiso.Admin;
            }
            else if (rol == "usuario")
            {
                this.permiso = Permiso.Usuario;
            }
            else
            {
                this.permiso = Permiso.Huesped;
            }
        }

        // Constructor que recibe un objeto Permiso directamente
        public UsuarioEspacio(Usuario usuario, Espacio espacio, Permiso permiso, bool ausente = false, int karma = 0)
        {
            Id_UsuarioEspacio = Guid.NewGuid().ToString();
            this.usuario = usuario;
            this.espacio = espacio;
            this.ausente = ausente;
            this.karma = karma;
            this.permiso = permiso;
        }

        // Métodos

        // Cambia el permiso del usuario en el espacio
        public void cambiarPermiso(Permiso permiso)
        {
            this.permiso = permiso;
        }

        // Marca al usuario como ausente
        public void salirEspacio()
        {
            espacio.Usuarios.Remove(this.usuario);
            this.usuario.UsuariosEspacios.Remove(this);
        }

        // Marca al usuario como presente
        public void unirseEspacio(Espacio espacio)
        {
            if (!espacio.Usuarios.Contains(this.usuario))
            {
                espacio.Usuarios.Add(this.usuario);
            }
            if (!this.usuario.UsuariosEspacios.Contains(this))
            {
                this.usuario.UsuariosEspacios.Add(this);
            }
        }

        // Suma puntos de karma al usuario
        public void sumKarma(int puntos)
        {
            this.karma += puntos;
        }

        // Suma puntos de karma y marca la tarea como completada
        public void sumKarmaAndPassTask(Tarea tarea)
        {
            tarea.cambiarEstado(true);
            karma += tarea.karma;
        }

        // Resta puntos de karma al usuario
        public void creearTarea(Tarea tarea)
        {
            this.TareasAsignadas.Add(tarea);
            tarea.Usuarios.Add(this);
        }

        public void crearTareaDePlantilla()
        {
            // TODO
        }

        public void crearPlantilla()
        {
            // TODO
        
        }

        // Asigna una tarea al usuario
        public void guardarTarea(Tarea tarea)
        {
            tarea.agregarUsuarios(new List<UsuarioEspacio> { this });
            TareasAsignadas.Add(tarea);
        }

        // Elimina una tarea asignada al usuarios
        public void reservarSala(Sala sala, DateTime fechaInicio, DateTime fechaFin)
        {
            if (sala.crearReserva(fechaInicio, fechaFin, this.usuario))
            {
                Console.WriteLine("Reserva creada con éxito.");
            }
            else
            {
                Console.WriteLine("No se pudo crear la reserva.");
            }
        }

        // Elimina una reserva realizada por el usuario
        public void cancelarReserva(Sala sala, Reserva r)
        {
            sala.eliminarReserva(r.Id_Reserva);
        }

    }
}

