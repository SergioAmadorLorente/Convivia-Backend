namespace AuthApiDemo.Models
{
    public class InvitacionPersist
    {
        public string Id { get; set; } = default!;
        public string UsuarioSolicitanteId { get; set; } = default!;
        public string UsuarioInvitadoId { get; set; } = default!;
        public string EspacioId { get; set; } = default!;
        public string Mensaje { get; set; } = default!;
        public DateTime Fecha { get; set; }
        public string Estado { get; set; } = default!;
    }
}
