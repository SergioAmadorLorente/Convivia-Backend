using Convivia.Aplicacion.DTOs;
using Convivia.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Convivia.Application.Mappers
{
    public class UsuarioMapper
    {
        // Método que recibe Usuario (dominio) y devuelve UsuarioDto
        public static UsuarioDto ToDto(Convivia.Domain.Models.Usuario domain)
        {
            if (domain == null) return null;
            return new UsuarioDto
            {
                Id = domain.Id,
                Nombre = domain.Nombre,
                Email = domain.Email,
                Telefono = domain.Telefono,
                Premium = domain.Premium,
                FechaRegistro = domain.FechaRegistro,
                // Para relaciones, usa IDs para evitar cargar listas completas (DDD: referencias ligeras)
                UsuarioEspacioIds = domain.UsuarioEspacios?.Select(ue => ue.Id_UsuarioEspacio).ToList() ?? new List<string>(),
                InvitacionIds = domain.Invitaciones?.Select(i => i.Id).ToList() ?? new List<string>()
            };
        }

        // Método que recibe CreateUsuarioDto y devuelve Usuario (dominio)
        public static Convivia.Domain.Models.Usuario FromCreateDto(CreateUsuarioDto dto)
        {
            if (dto == null) return null;

            var domain = new Convivia.Domain.Models.Usuario
            {
                Nombre = dto.Nombre,
                Email = dto.Email,
                Password = dto.Password, // Recuerda hashear en el servicio (ej. BCrypt)
                Telefono = dto.Telefono,
                Premium = dto.Premium
            };

            return domain;
        }

        // Método para actualizar Usuario desde UpdateUsuarioDto (solo campos no-null)
        public static void UpdateDomainFromDto(Convivia.Domain.Models.Usuario domain, UpdateUsuarioDto dto)
        {
            if (domain == null || dto == null) return;
            domain.Nombre = dto.Nombre ?? domain.Nombre;
            domain.Email = dto.Email ?? domain.Email;
            domain.Password = dto.Password ?? domain.Password; // Hashea si se actualiza
            domain.Telefono = dto.Telefono ?? domain.Telefono;
            if (dto.Premium.HasValue) domain.Premium = dto.Premium.Value; // Solo actualiza si no es null
        }
    }
}