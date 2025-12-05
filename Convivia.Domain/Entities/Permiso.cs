namespace Convivia.Domain.Entities
{
    public class Permiso
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public Rol Rol { get; set; }
        public bool CrearTarea { get; set; }
        public bool EliminarTarea { get; set; }
        public bool EditarTarea { get; set; }
        public bool AñadirUsuario { get; set; }
        public bool EliminarUsuario { get; set; }
        public bool AsignarTarea { get; set; }
        public bool AsignarseTarea { get; set; }

        public Permiso()
        {
            Rol = new Rol();
        }
        
        public Permiso(
                Rol rol,
                bool crearTarea = false,
                bool eliminarTarea = false,
                bool editarTarea = false,
                bool añadirUsuario = false,
                bool eliminarUsuario = false,
                bool asignarTarea = false,
                bool asignarseTarea = true)
        {
            this.Rol = rol ?? new Rol();
            this.CrearTarea = crearTarea;
            this.EliminarTarea = eliminarTarea;
            this.EditarTarea = editarTarea;
            this.AñadirUsuario = añadirUsuario;
            this.EliminarUsuario = eliminarUsuario;
            this.AsignarTarea = asignarTarea;
            this.AsignarseTarea = asignarseTarea;
        }
    }
}
