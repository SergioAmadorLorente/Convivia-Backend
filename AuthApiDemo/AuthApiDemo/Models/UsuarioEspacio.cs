namespace AuthApiDemo.Models
{

    public class UsuarioEspacio
    {

        public string Id_UsuarioEspacio { get; set; } = Guid.NewGuid().ToString();

        public bool ausente { get; set; }

        public int karma { get; set; }

        public Espacio espacio { get; set; }

        public Usuario usuario { get; set; }

        public List<Tarea> TareasAsignadas { get; set; } = new List<Tarea>();
        public Permiso permiso { get; set; }

        public UsuarioEspacio()
        {
        }

        public UsuarioEspacio(Usuario usuario, Espacio espacio, bool ausente = false, int karma = 0, Permiso permiso)
        {
            Id_UsuarioEspacio = Guid.NewGuid().ToString();
            this.usuario = usuario;
            this.espacio = espacio;
            this.ausente = ausente;
            this.karma = karma;
            this.permiso = permiso;
        }

        public void cambiarPermiso(Permiso permiso)
        {
            this.permiso = permiso;
        }

        public void salirEspacio()
        {
            espacio.Usuarios.Remove(this.usuario);
            this.usuario.UsuariosEspacios.Remove(this);
        }
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
        public void sumKarma(int puntos)
        {
            this.karma += puntos;
        }

        public void sumKarmaAndPassTask(Tarea tarea)
        {
            tarea.cambiarEstado(true);
            karma += tarea.karma;
        }

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
        public void guardarTarea(Tarea tarea)
        {
            tarea.agregarUsuarios(new List<UsuarioEspacio> { this });
            TareasAsignadas.Add(tarea);
        }
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

        public void cancelarReserva(Sala sala, Reserva r)
        {
            sala.eliminarReserva(r.Id_Reserva);
        }

        public void creaerFacutra(Factura factura)
        {
            //
        }

    }
}

