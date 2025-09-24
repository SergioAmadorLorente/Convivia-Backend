namespace AuthApiDemo.Models
{
    public class Permiso
    {
        public string id_Reserva { get; set; } = Guid.NewGuid().ToString();
        public string rol { get; set; }
        public bool crearTarea { get; set; }
        public bool eliminarTarea { get; set; }
        public bool editarTarea { get; set; }
        public bool añadirUsuario { get; set; }
        public bool eliminarUsuario { get; set; }
        public bool asignarTarea { get; set; }
        public bool asignarseTarea { get; set; }

        public Permiso()
        {
            setConfigurarcionHuesped();
        }
        public Permiso(
                string rol,
                bool crearTarea,
                bool eliminarTarea,
                bool editarTarea,
                bool añadirUsuario,
                bool eliminarUsuario,
                bool asignarTarea,
                bool asignarseTarea)
        {
            this.rol = rol;
            this.crearTarea = crearTarea;
            this.eliminarTarea = eliminarTarea;
            this.editarTarea = editarTarea;
            this.añadirUsuario = añadirUsuario;
            this.eliminarUsuario = eliminarUsuario;
            this.asignarTarea = asignarTarea;
            this.asignarseTarea = asignarseTarea;
        }
        public Permiso(String rol)
        {
            this.rol = rol;
            if(rol == "Admin")
            {
                setConfigurarcionAdmin();
            } else if(rol == "Usuario")
            {
                setConfigurarcionUsuario();
            } else
            {
                setConfigurarcionHuesped();
            }
        }


        public void setConfigurarcionHuesped()
        {
            this.rol = "Huesped";
            this.crearTarea = false;
            this.eliminarTarea = false;
            this.editarTarea = false;
            this.añadirUsuario = false;
            this.eliminarUsuario = false;
            this.asignarTarea = false;
            this.asignarseTarea = true;
        }
        public void setConfigurarcionUsuario()
        {
            this.rol = "Huesped";
            this.crearTarea = true;
            this.eliminarTarea = false;
            this.editarTarea = true;
            this.añadirUsuario = false;
            this.eliminarUsuario = false;
            this.asignarTarea = false;
            this.asignarseTarea = true;
        }
        public void setConfigurarcionAdmin()
        {
            this.rol = "Huesped";
            this.crearTarea = true;
            this.eliminarTarea = true;
            this.editarTarea = true;
            this.añadirUsuario = true;
            this.eliminarUsuario = true;
            this.asignarTarea = true;
            this.asignarseTarea = true;
        }
    }

}
