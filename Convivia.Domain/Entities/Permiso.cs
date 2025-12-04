namespace Convivia.Domain.Entities
{
    public class Permiso
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Rol { get; set; }
        public bool CrearTarea { get; set; }
        public bool EliminarTarea { get; set; }
        public bool EditarTarea { get; set; }
        public bool AñadirUsuario { get; set; }
        public bool EliminarUsuario { get; set; }
        public bool AsignarTarea { get; set; }
        public bool AsignarseTarea { get; set; }

        // Roles válidos permitidos
        public static readonly string[] RolesValidos = { "Usuario", "Admin" };

        public static Permiso Usuario = new Permiso("Usuario", true, false, true, false, false, false, true);
        public static Permiso Admin = new Permiso("Admin", true, true, true, true, true, true, true);

        public Permiso()
        {
            SetConfigurarcionUsuario();
        }
        
        public Permiso(
                string rol,
                bool crearTarea = false,
                bool eliminarTarea = false,
                bool editarTarea = false,
                bool añadirUsuario = false,
                bool eliminarUsuario = false,
                bool asignarTarea = false,
                bool asignarseTarea = true)
        {
            if (!EsRolValido(rol))
            {
                throw new ArgumentException($"Rol '{rol}' no válido. Los roles permitidos son: {string.Join(", ", RolesValidos)}");
            }

            this.Rol = rol;
            this.CrearTarea = crearTarea;
            this.EliminarTarea = eliminarTarea;
            this.EditarTarea = editarTarea;
            this.AñadirUsuario = añadirUsuario;
            this.EliminarUsuario = eliminarUsuario;
            this.AsignarTarea = asignarTarea;
            this.AsignarseTarea = asignarseTarea;
        }

        /// <summary>
        /// Valida si un rol es válido
        /// </summary>
        public static bool EsRolValido(string rol)
        {
            return !string.IsNullOrWhiteSpace(rol) && 
                   RolesValidos.Contains(rol, StringComparer.OrdinalIgnoreCase);
        }

        public void SetConfigurarcionUsuario()
        {
            this.Rol = "Usuario";
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
            this.Rol = "Admin";
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
