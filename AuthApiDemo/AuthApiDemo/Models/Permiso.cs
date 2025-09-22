namespace AuthApiDemo.Models
{
    public class Permiso
    {
        public string Id_Reserva { get; set; } = Guid.NewGuid().ToString();
        public string Rol { get; set; }
        public bool CrearTarea { get; set; }
        public bool EliminarTarea { get; set; }
        public bool EditarTarea { get; set; }
        public bool AñadirUsuario { get; set; }
        public bool EliminarUsuario { get; set; }
        public bool AsignarTarea { get; set; }
        public bool AsignarseTarea { get; set; }
    }
}
