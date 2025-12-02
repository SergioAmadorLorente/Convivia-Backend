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
            // Sala mappings
            Config.MapsterConfig.RegisterPair<Sala, SalaDto, CreateSalaDto, UpdateSalaDto>(config);

            // Usuario mappings (DTO <-> Domain)
            Config.MapsterConfig.RegisterPair<Usuario, UsuarioDto, CreateUsuarioDto, UpdateUsuarioDto>(config);

            // Invitacion mappings (DTO <-> Domain)
            Config.MapsterConfig.RegisterPair<Invitacion, InvitacionDto, CreateInvitacionDto, UpdateInvitacionDto>(config);

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
