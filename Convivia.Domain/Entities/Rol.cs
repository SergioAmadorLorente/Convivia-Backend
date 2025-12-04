namespace Convivia.Domain.Entities
{
    public class Rol
    {
        public string Nombre { get; set; }
        public bool CrearTarea { get; set; }
        public bool EliminarTarea { get; set; }
        public bool EditarTarea { get; set; }
        public bool AñadirUsuario { get; set; }
        public bool EliminarUsuario { get; set; }
        public bool AsignarTarea { get; set; }
        public bool AsignarseTarea { get; set; }

        public Rol()
        {
        }

        public Rol(string nombre)
        {
            Nombre = nombre;
        }

        public static Rol Usuario => new Rol("Usuario");
        public static Rol Admin => new Rol("Admin");

        public void SetConfigurarcionUsuario()
        {
            this.Nombre = "Usuario";
            this.CrearTarea = true;
            this.EliminarTarea = false;
            this.EditarTarea = true;
            this.AñadirUsuario = false;
            this.EliminarUsuario = false;
            this.AsignarTarea = false;
            this.AsignarseTarea = true;
        }
        
        public void SetConfigurarcionAdmin()
        {
            this.Nombre = "Admin";
            this.CrearTarea = true;
            this.EliminarTarea = true;
            this.EditarTarea = true;
            this.AñadirUsuario = true;
            this.EliminarUsuario = true;
            this.AsignarTarea = true;
            this.AsignarseTarea = true;
        }
    }
}
