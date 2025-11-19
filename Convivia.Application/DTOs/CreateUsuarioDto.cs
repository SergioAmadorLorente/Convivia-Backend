namespace Convivia.Application.DTOs
{
    public class CreateUsuarioDto
    {
        public string Nombre { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string? Telefono { get; set; }
        public bool Premium { get; set; } = false;
    }
}
