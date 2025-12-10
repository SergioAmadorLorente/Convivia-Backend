using Mapster;
using System.Reflection;
using Convivia.Domain.Entities;
using Convivia.Shared.DTOs;
namespace Convivia.Application.Mappers
{
    public static class MapsterBootstrap
    {
        public static void Configure(TypeAdapterConfig config)
        {
            // Register application-level mappings (DTO <-> Domain)
            
            // Espacio mappings (DTO <-> Domain)
            Config.MapsterConfig.RegisterPair<Espacio, EspacioDto, CreateEspacioDto, UpdateEspacioDto>(config);

            // Sala mappings
            Config.MapsterConfig.RegisterPair<Sala, SalaDto, CreateSalaDto, UpdateSalaDto>(config);

            // Usuario mappings (DTO <-> Domain)
            Config.MapsterConfig.RegisterPair<Usuario, UsuarioDto, CreateUsuarioDto, UpdateUsuarioDto>(config);

            // Rol mappings (DTO <-> Domain)
            Config.MapsterConfig.RegisterPair<Rol, RolDto, CreateRolDto, UpdateRolDto>(config);

            // Configuración personalizada para Permiso: CreatePermisoDto -> Permiso
            // Mapster convierte string -> Rol automáticamente usando RolTypeConverter
            config.NewConfig<CreatePermisoDto, Permiso>()
                .Map(dest => dest.Rol, src => src.Rol); // RolTypeConverter maneja string -> Rol automáticamente

            // Configuración personalizada: Permiso -> PermisoDto
            // Expandir las propiedades del Rol en el DTO para facilitar consumo en API
            config.NewConfig<Permiso, PermisoDto>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.Rol, src => src.Rol.Nombre)
                // Mapear automáticamente todas las propiedades de Rol a PermisoDto
                .Map(dest => dest, src => src.Rol);

            // Invitacion mappings (DTO <-> Domain)
            Config.MapsterConfig.RegisterPair<Invitacion, InvitacionDto, CreateInvitacionDto, UpdateInvitacionDto>(config);

            // Peticion mappings (DTO <-> Domain)
            Config.MapsterConfig.RegisterPair<Peticion, PeticionDto, CreatePeticionDto, UpdatePeticionDto>(config);
            // Tarea mappings (DTO <-> Domain)

            Config.MapsterConfig.RegisterPair<Tarea, TareaDto, CreateTareaDto, UpdateTareaDto>(config);

            // PlantillaTarea mappings (DTO <-> Domain)
            Config.MapsterConfig.RegisterPair<PlantillaTarea, PlantillaTareaDto, CreatePlantillaTareaDto, UpdatePlantillaTareaDto>(config);

            // Scan infrastructure assembly for IRegister implementations (Domain <-> Persistence)
            // Auto-registers: RolTypeConverter (string<->Rol), PermisoMappingConfig, and other mapping configs
            try
            {
                var infraAssembly = Assembly.Load("Convivia.Infrastructure");
                config.Scan(infraAssembly);
            }
            catch
            {
                // ignore if assembly not available during design-time operations
            }
        }
    }
}
