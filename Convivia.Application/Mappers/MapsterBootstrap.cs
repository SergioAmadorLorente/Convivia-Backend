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

            // Permiso mappings (DTO <-> Domain)
            Config.MapsterConfig.RegisterPair<Permiso, PermisoDto, CreatePermisoDto, UpdatePermisoDto>(config);

            // Invitacion mappings (DTO <-> Domain)
            Config.MapsterConfig.RegisterPair<Invitacion, InvitacionDto, CreateInvitacionDto, UpdateInvitacionDto>(config);

            // Peticion mappings (DTO <-> Domain)
            Config.MapsterConfig.RegisterPair<Peticion, PeticionDto, CreatePeticionDto, UpdatePeticionDto>(config);
            // Tarea mappings (DTO <-> Domain)

            Config.MapsterConfig.RegisterPair<Tarea, TareaDto, CreateTareaDto, UpdateTareaDto>(config);

            // PlantillaTarea mappings (DTO <-> Domain)
            Config.MapsterConfig.RegisterPair<PlantillaTarea, PlantillaTareaDto, CreatePlantillaTareaDto, UpdatePlantillaTareaDto>(config);

            Config.MapsterConfig.RegisterPair<Reserva, ReservaDto, CreateReservaDto, UpdateReservaDto>(config);

            // Scan infrastructure assembly for IRegister implementations (Domain <-> Persistence)
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
