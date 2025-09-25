using Google.Cloud.Firestore;
namespace AuthApiDemo.Models
{

    [FirestoreData]
    public class UsuarioEspacio
    {
        [FirestoreProperty]
        public string Id_UsuarioEspacio { get; set; } = Guid.NewGuid().ToString();

        [FirestoreProperty]
        public bool Ausente { get; set; }

        [FirestoreProperty]
        public int Karma { get; set; }

        [FirestoreProperty]
        public string Rol { get; set; }

        [FirestoreProperty]
        public Espacio Espacio { get; set; }

        [FirestoreProperty]
        public Usuario Usuario { get; set; }

        [FirestoreProperty]
        public List<Tarea> TareasAsignadas { get; set; } = new();

        [FirestoreProperty]
        public Permiso Permiso { get; set; }

        [FirestoreProperty]
        public List<Factura> Facturas { get; set; } = new();

 // ?

        // Constructor por defecto vacio para pruebas
        public UsuarioEspacio()
        {
        }

        // Constructor que asigna el permiso según el rol proporcionado
        public UsuarioEspacio(Usuario usuario, Espacio espacio, Permiso permiso, string rol, bool ausente = false, int karma = 0)
        {
            Id_UsuarioEspacio = Guid.NewGuid().ToString();
            this.Usuario = usuario;
            this.Espacio = espacio;
            this.Ausente = ausente;
            this.Karma = karma;

            if (rol == "admin")
            {
                this.Permiso = Permiso.Admin;
            }
            else if (rol == "usuario")
            {
                this.Permiso = Permiso.Usuario;
            }
            else
            {
                this.Permiso = Permiso.Huesped;
            }
        }

        // Constructor que recibe un objeto Permiso directamente
        public UsuarioEspacio(Usuario usuario, Espacio espacio, Permiso permiso, bool ausente = false, int karma = 0)
        {
            Id_UsuarioEspacio = Guid.NewGuid().ToString();
            this.Usuario = usuario;
            this.Espacio = espacio;
            this.Ausente = ausente;
            this.Karma = karma;
            this.Permiso = permiso;
        }

        // Métodos

        // Cambia el permiso del usuario en el espacio
        public void cambiarPermiso(Permiso permiso)
        {
            this.Permiso = permiso;
        }

        // Marca al usuario como ausente
        public void salirEspacio()
        {
            Espacio.UsuariosEspacios.Remove(this);
            this.Usuario.UsuariosEspacios.Remove(this);
        }

        // Marca al usuario como presente
        public void unirseEspacio(Espacio espacio)
        {
            if (!espacio.UsuariosEspacios.Contains(this))
            {
                espacio.UsuariosEspacios.Add(this);
            }
            if (!this.Usuario.UsuariosEspacios.Contains(this))
            {
                this.Usuario.UsuariosEspacios.Add(this);
            }
        }

        // Suma puntos de karma al usuario
        public void sumKarma(int puntos)
        {
            this.Karma += puntos;
        }

        // Suma puntos de karma y marca la tarea como completada
        public void sumKarmaAndPassTask(Tarea tarea)
        {
            tarea.cambiarEstado(true);
            Karma += tarea.karma;
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
            if (sala.crearReserva(fechaInicio, fechaFin, this.Usuario))
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

