using Convivia.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convivia.Application.Mappers
{
    public class EspacioMapper
    {
        // Metodo que recibe Espacio y devuelve EspacioDto
        public static EspacioDto ToDto(Convivia.Domain.Models.Espacio domain)
        {
            if (domain == null) return null;
            return new EspacioDto
            {
                Id = domain.Id,
                Nombre = domain.Nombre,
                Direccion = domain?.Direccion
            };
        }

        // Metodo que recibe CreateInvitacionDto y devuelve invitacion
        public static Convivia.Domain.Models.Espacio FromCreateDto(CreateEspacioDto dto)
        {
            if (dto == null) return null;

            var domain = new Convivia.Domain.Models.Espacio
            {
                Nombre = dto.Nombre,
                Direccion = dto?.Direccion
            };

            return domain;
        }

        // Metodo para actualizar el mensaje de invitacion, no permito que se cambie ni la fecha ni el id creo que es mejor  por ahora
        public static void UpdateDomainFromDto(Convivia.Domain.Models.Espacio domain, CreateEspacioDto dto)
        {
            if (domain == null || dto == null) return;
            domain.Nombre = dto.Nombre ?? domain.Nombre;
            domain.Direccion = dto.Direccion ?? domain.Direccion;
        }
    }
}
