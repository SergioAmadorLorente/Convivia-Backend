namespace Convivia.Domain.Entities
{
    public class Permiso
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public Rol Rol { get; set; }

        public Permiso()
        {
            Rol = new Rol();
        }
        
        public Permiso(Rol rol)
        {
            this.Rol = rol ?? new Rol();
        }
    }
}
