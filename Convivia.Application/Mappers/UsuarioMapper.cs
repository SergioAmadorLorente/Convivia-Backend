/*
using Convivia.Application.DTOs;
using Convivia.Domain.Models;

namespace Convivia.Application.Mappers
{
    public static class UsuarioMapper
    {
        public static UsuarioDto ToDto(UsuarioPersist usuario)
        {
            return new UsuarioDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Email = usuario.Email,
                Telefono = usuario.Telefono,
                Premium = usuario.Premium,
                FechaRegistro = usuario.FechaRegistro,
                UsuarioEspacioIds = usuario.UsuarioEspacioIds,
                InvitacionIds = usuario.InvitacionIds
            };
        }

        public static UsuarioPersist FromCrearDto(CreateUsuarioDto dto)
        {
            return new UsuarioPersist
            {
                Id = Guid.NewGuid().ToString(),
                Nombre = dto.Nombre,
                Email = dto.Email,
                Password = dto.Password,
                Telefono = dto.Telefono,
                Premium = dto.Premium,
                FechaRegistro = DateTime.UtcNow
            };
        }

        public static void ApplyActualizarDto(UsuarioPersist usuario, UpdateUsuarioDto dto)
        {
            if (dto.Nombre != null) usuario.Nombre = dto.Nombre;
            if (dto.Email != null) usuario.Email = dto.Email;
            if (dto.Password != null) usuario.Password = dto.Password;
            if (dto.Telefono != null) usuario.Telefono = dto.Telefono;
        }
    }
}
*/