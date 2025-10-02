using AuthApiDemo.DTOs;
using AuthApiDemo.Models;

namespace AuthApiDemo.Mappers
{
    public static class InvitacionMapper
    {
        // De Create DTO -> objeto para persistir (genera Id y Fecha)
        public static InvitacionPersist ToPersist(this CreateInvitacionDto dto)
        {
            return new InvitacionPersist
            {
                Id = Guid.NewGuid().ToString(),
                UsuarioSolicitanteId = dto.UsuarioSolicitanteId,
                UsuarioInvitadoId = dto.UsuarioInvitadoId,
                EspacioId = dto.EspacioId,
                Mensaje = dto.Mensaje ?? string.Empty,
                Fecha = DateTime.UtcNow,
                Estado = "pendiente"
            };
        }

        // De persistencia -> DTO de respuesta
        public static InvitacionDto ToDto(this InvitacionPersist p)
        {
            return new InvitacionDto
            {
                Id = p.Id,
                UsuarioSolicitanteId = p.UsuarioSolicitanteId,
                UsuarioInvitadoId = p.UsuarioInvitadoId,
                EspacioId = p.EspacioId,
                Mensaje = p.Mensaje,
                Fecha = p.Fecha,
                Estado = p.Estado
            };
        }
    }
}