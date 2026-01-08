namespace Convivia.Domain.Entities
{
    public class Rol
    {
        public string Id { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public bool CrearTarea { get; set; }
        public bool EliminarTarea { get; set; }
        public bool EditarTarea { get; set; }
        public bool AñadirUsuario { get; set; }
        public bool EliminarUsuario { get; set; }
        public bool AsignarTarea { get; set; }
        public bool AsignarseTarea { get; set; }
        public bool EliminarResidencia { get; set; }

        public Rol()
        {
        }

        public Rol(string nombre)
        {
            Nombre = nombre;
        }

        public static Rol Usuario
        {
            get
            {
                var rol = new Rol("Usuario");
                rol.SetConfiguracionUsuario();
                return rol;
            }
        }
        public static Rol Admin
        {
            get
            {
                var rol = new Rol("Admin");
                rol.SetConfiguracionAdmin();
                return rol;
            }
        }

        public static Rol Moderador
        {
            get
            {
                var rol = new Rol("Moderador");
                rol.SetConfiguracionModerador();
                return rol;
            }
        }

        public void SetConfiguracionUsuario()
        {
            this.Nombre = "Usuario";
            this.CrearTarea = true;
            this.EliminarTarea = false;
            this.EditarTarea = true;
            this.AñadirUsuario = false;
            this.EliminarUsuario = false;
            this.AsignarTarea = false;
            this.AsignarseTarea = true;
            this.EliminarResidencia = false;
        }
        
        public void SetConfiguracionAdmin()
        {
            this.Nombre = "Admin";
            this.CrearTarea = true;
            this.EliminarTarea = true;
            this.EditarTarea = true;
            this.AñadirUsuario = true;
            this.EliminarUsuario = true;
            this.AsignarTarea = true;
            this.AsignarseTarea = true;
            this.EliminarResidencia = true;
        }

        public void SetConfiguracionModerador()
        {
            this.Nombre = "Moderador";
            this.CrearTarea = true;
            this.EliminarTarea = true;
            this.EditarTarea = true;
            this.AñadirUsuario = true;
            this.EliminarUsuario = true;
            this.AsignarTarea = true;
            this.AsignarseTarea = true;
            this.EliminarResidencia = false;
        }
    }
}
