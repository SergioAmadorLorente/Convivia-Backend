using Convivia.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convivia.Application.Mappers
{
    internal class InvitacionMapper
    {
        // Metodo que recibe invitacion y devuelve InvitacionDto
        public static InvitacionDto ToDto(Convivia.Domain.Models.Invitacion domain)
        {
            if (domain == null) return null;
            return new InvitacionDto
            {
                Id = domain.Id,
                UsuarioSolicitanteId = domain.UsuarioSolicitante?.Id_UsuarioEspacio,
                UsuarioInvitadoId = domain.UsuarioInvitado?.Id, // Es comproba "?" si es null primer per evitar que llancin exepcions
                EspacioId = domain.Espacio?.Id_Espacio,
                Mensaje = domain.Mensaje,
                Fecha = domain.Fecha,
                Estado = domain.Estado
            };
        }

        // Metodo que recibe CreateInvitacionDto y devuelve invitacion
        public static Convivia.Domain.Models.Invitacion FromCreateDto(CreateInvitacionDto dto)
        {
            if (dto == null) return null;

            var domain = new Convivia.Domain.Models.Invitacion
            {
                Mensaje = dto.Mensaje,
                Fecha = DateTime.UtcNow,
                Estado = "pendiente"
            };

            return domain; 
        }

        // Metodo para actualizar el mensaje de invitacion, no permito que se cambie ni la fecha ni el id creo que es mejor  por ahora
        public static void UpdateDomainFromDto(Convivia.Domain.Models.Invitacion domain, CreateInvitacionDto dto)
        {
            if (domain == null || dto == null) return;
            domain.Mensaje = dto.Mensaje ?? domain.Mensaje;
        }
    }
}
