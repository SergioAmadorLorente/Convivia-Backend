using System;
using Mapster;
using Convivia.Shared.DTOs;
using Convivia.Domain.Entities;
using Convivia.Application.Mappers.Config;

namespace Convivia.Application.Mappers
{
    public class UsuarioMapper
    {
        // Registrar las configuraciones de Mapster una sola vez al cargar la clase.
        static UsuarioMapper()
        {
            // Asegura que las reglas genéricas (incluyendo IgnoreNullValues para Update DTO) estén registradas.
            MapsterConfig.RegisterPair<Usuario, UsuarioDto, CreateUsuarioDto, UpdateUsuarioDto>(TypeAdapterConfig.GlobalSettings);
            
            // Configuraciones personalizadas específicas para Usuario
            TypeAdapterConfig<CreateUsuarioDto, Usuario>.NewConfig()
                .Ignore(dest => dest.FechaRegistro)
                .Ignore(dest => dest.UsuarioEspacios) // No se mapean colecciones en la creación
                .Ignore(dest => dest.Invitaciones);
            
            TypeAdapterConfig<UpdateUsuarioDto, Usuario>.NewConfig()
                .IgnoreNullValues(true)
                .Ignore(dest => dest.FechaRegistro) // No permitir cambiar la fecha de registro
                .Ignore(dest => dest.UsuarioEspacios) // No se mapean colecciones en la actualización
                .Ignore(dest => dest.Invitaciones);
            
            // Configuración para mapear de Usuario a UsuarioDto (ignora colecciones)
            TypeAdapterConfig<Usuario, UsuarioDto>.NewConfig()
                .Ignore(dest => dest.Id); // El Id ya se mapea automáticamente
        }
        

    }
}