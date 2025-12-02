using Mapster;
using System.Reflection;

namespace Convivia.Application.Mappers
{
    public static class MapsterBootstrap
    {
        public static void Configure(TypeAdapterConfig config)
        {
            // Register application-level mappings (DTO <-> Domain)
            // Sala mappings
            Config.MapsterConfig.RegisterPair<Convivia.Domain.Entities.Sala, Convivia.Shared.DTOs.SalaDto, Convivia.Shared.DTOs.CreateSalaDto, Convivia.Shared.DTOs.UpdateSalaDto>(config);

            // Usuario mappings (DTO <-> Domain)
            Config.MapsterConfig.RegisterPair<Convivia.Domain.Entities.Usuario, Convivia.Shared.DTOs.UsuarioDto, Convivia.Shared.DTOs.CreateUsuarioDto, Convivia.Shared.DTOs.UpdateUsuarioDto>(config);

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
