namespace Convivia.Application.DTOs
{
    public class StateChangeDto
    {
        public string NuevoEstado { get; set; } = default!;
        public string? AccionanteUsuarioId { get; set; }
    }
}
