// Mappers/InvitacionMapper.cs

using System;
using Convivia.Application.DTOs;
using Convivia.Domain.Models;

namespace Convivia.Application.Mappers
{
    public static class InvitacionMapper
    {
        // Modelo -> Persistencia
        public static InvitacionPersist ToPersist(Invitacion invitacion)
        {
            if (invitacion == null) throw new ArgumentNullException(nameof(invitacion));

            return new InvitacionPersist
            {
                Id = string.IsNullOrWhiteSpace(invitacion.Id) ? Guid.NewGuid().ToString() : invitacion.Id,
                UsuarioSolicitante = invitacion.UsuarioSolicitante?.Id_UsuarioEspacio ?? string.Empty,
                UsuarioInvitado = invitacion.UsuarioInvitado?.Id ?? string.Empty,
                Espacio = invitacion.Espacio?.Id_Espacio ?? string.Empty,
                Mensaje = invitacion.Mensaje ?? string.Empty,
                Fecha = invitacion.Fecha == default ? DateTime.UtcNow : invitacion.Fecha,
                Estado = invitacion.Estado ?? "pendiente"
            };
        }

        // Persistencia + objetos resueltos -> Modelo
        public static Invitacion ToModel(InvitacionPersist persist, UsuarioEspacio solicitante = null, Usuario invitado = null, Espacio espacio = null)
        {
            if (persist == null) throw new ArgumentNullException(nameof(persist));

            return new Invitacion
            {
                UsuarioSolicitante = solicitante!,
                UsuarioInvitado = invitado!,
                Espacio = espacio!,
                Mensaje = persist.Mensaje,
                Fecha = persist.Fecha,
                Estado = persist.Estado
            };
        }

        // Modelo -> DTO (respuesta API)
        public static InvitacionDto ToDto(Invitacion invitacion)
        {
            if (invitacion == null) throw new ArgumentNullException(nameof(invitacion));

            return new InvitacionDto
            {
                Id = invitacion.Id,
                UsuarioSolicitanteId = invitacion.UsuarioSolicitante?.Id_UsuarioEspacio ?? string.Empty,
                UsuarioInvitadoId = invitacion.UsuarioInvitado?.Id ?? string.Empty,
                EspacioId = invitacion.Espacio?.Id_Espacio ?? string.Empty,
                Mensaje = invitacion.Mensaje ?? string.Empty,
                Fecha = invitacion.Fecha,
                Estado = invitacion.Estado ?? "pendiente"
            };
        }

        // Persist -> DTO directo (sin resolver objetos)
        public static InvitacionDto ToDto(this InvitacionPersist persist)
        {
            if (persist == null) throw new ArgumentNullException(nameof(persist));

            return new InvitacionDto
            {
                Id = persist.Id,
                UsuarioSolicitanteId = persist.UsuarioSolicitante,
                UsuarioInvitadoId = string.IsNullOrEmpty(persist.UsuarioInvitado) ? null : persist.UsuarioInvitado,
                EspacioId = persist.Espacio,
                Mensaje = persist.Mensaje ?? string.Empty,
                Fecha = persist.Fecha,
                Estado = persist.Estado ?? "pendiente"
            };
        }
    }
}



