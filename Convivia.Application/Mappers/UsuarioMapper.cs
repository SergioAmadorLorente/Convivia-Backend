using System;
using Mapster;
using Convivia.Shared.DTOs;
using Convivia.Domain.Entities;
using Convivia.Application.Mappers;

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
                .Map(dest => dest.FechaRegistro, src => DateTime.UtcNow)
                .Ignore(dest => dest.Id) // El Id se genera automáticamente en el constructor
                .Ignore(dest => dest.UsuarioEspacios) // No se mapean colecciones en la creación
                .Ignore(dest => dest.Invitaciones);
            
            TypeAdapterConfig<UpdateUsuarioDto, Usuario>.NewConfig()
                .IgnoreNullValues(true)
                .Ignore(dest => dest.Id)
                .Ignore(dest => dest.FechaRegistro) // No permitir cambiar la fecha de registro
                .Ignore(dest => dest.UsuarioEspacios) // No se mapean colecciones en la actualización
                .Ignore(dest => dest.Invitaciones);
            
            // Configuración para mapear de Usuario a UsuarioDto (ignora colecciones)
            TypeAdapterConfig<Usuario, UsuarioDto>.NewConfig()
                .Ignore(dest => dest.Id); // El Id ya se mapea automáticamente
        }
        
        // Métodos de conveniencia usando Mapster
        public static UsuarioDto ToDto(Usuario domain) 
            => domain?.Adapt<UsuarioDto>();
        
        public static Usuario FromCreateDto(CreateUsuarioDto dto) 
            => dto?.Adapt<Usuario>();
        
        public static void UpdateDomainFromDto(Usuario domain, UpdateUsuarioDto dto)
        {
            if (domain == null || dto == null) return;
            dto.Adapt(domain);
        }
    }
}