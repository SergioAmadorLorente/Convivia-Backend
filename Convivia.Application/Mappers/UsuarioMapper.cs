/*
using Convivia.Shared.DTOs;
using Convivia.Domain.Models;

namespace Convivia.Application.Mappers
{
    public class UsuarioMapper
    {
        // Metodo que recibe Usuario y devuelve UsuarioDto
        public static UsuarioDto ToDto(Convivia.Domain.Models.Usuario domain)
        {
            if (domain == null) return null;
            return new UsuarioDto
            {
                Id = domain.Id,
                Nombre = domain.Nombre,
                Email = domain.Email,
                Telefono = domain?.Telefono,
                Premium = domain.Premium,
                FechaRegistro = domain.FechaRegistro.Date
            };
        }

        // Metodo que recibe CreateUsuarioDto y devuelve Usuario
        public static Convivia.Domain.Models.Usuario FromCreateDto(CreateUsuarioDto dto)
        {
            if (dto == null) return null;

            var domain = new Convivia.Domain.Models.Usuario
            {
                Nombre = dto.Nombre,
                Email = dto.Email,
                FechaRegistro = DateTime.UtcNow,
                Password = dto.Password,
                Telefono = dto?.Telefono
            };

            return domain;
        }

        // Metodo para actualizar el mensaje de Usuario, no permito que se cambie ni la fecha ni el id creo que es mejor  por ahora
        public static void UpdateDomainFromDto(Convivia.Domain.Models.Usuario domain, CreateUsuarioDto dto)
        {
            if (domain == null || dto == null) return;

            domain.Nombre = dto.Nombre ?? domain.Nombre;
            domain.Email = dto.Email ?? domain.Email;
            domain.Password = dto.Password ?? domain.Password;
            domain.Telefono = dto.Telefono ?? domain.Telefono;


        }
    }
}
*/