using Convivia.Application.Services;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using Convivia.Application.Mappers.Config;
using Convivia.Application.Mappers;

namespace Convivia.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Registrar servicios de aplicación
            services.AddScoped<InvitacionService>();
            services.AddScoped<UsuarioService>();
            services.AddScoped<EspacioService>();
            services.AddScoped<PlantillaTareaService>();
            services.AddScoped<PeticionService>();
            services.AddScoped<TareaService>();
            services.AddScoped<PermisoService>();
            services.AddScoped<ReservaService>();
            services.AddScoped<UsuarioEspacioService>();
            services.AddScoped<FacturaService>();

            // Centralizar Mapster configuration
            MapsterBootstrap.Configure(TypeAdapterConfig.GlobalSettings);

            return services;
        }
    }
}