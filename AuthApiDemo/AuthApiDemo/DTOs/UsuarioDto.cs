namespace AuthApiDemo.DTOs
{
    public class UsuarioDto
    {
        public string Id { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
        public string? Telefono { get; set; }
        public bool Premium { get; set; }
        public DateTime FechaRegistro { get; set; }

        public List<string> UsuarioEspacioIds { get; set; } = new();
        public List<string> InvitacionIds { get; set; } = new();

        // Contrsenya no s'inclou per seguretat
    }
}
