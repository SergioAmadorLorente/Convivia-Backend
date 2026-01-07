using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Convivia.Application.Repositories;
using Convivia.Shared.Services;
using Convivia.Infrastructure.Repositories;
using Convivia.Infrastructure.Services;
using Convivia.Domain.Entities;

namespace Convivia.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Servicios de infraestructura
            services.AddScoped<IFirebaseService, FirebaseService>();

            // Repositorios concretos
            services.AddScoped<IInvitacionRepository, InvitacionRepository>();
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            services.AddScoped<IEspacioRepository, EspacioRepository>();
            services.AddScoped<IPeticionRepository, PeticionRepository>();
            services.AddScoped<IPlantillaTareaRepository, PlantillaTareaRepository>();
            services.AddScoped<ITareaRepository, TareaRepository>();
            services.AddScoped<IPermisoRepository, PermisoRepository>();
            services.AddScoped<IRolRepository, RolRepository>();
            services.AddScoped<IReservaRepository, ReservaRepository>();
            services.AddScoped<IUsuarioEspacioRepository, UsuarioEspacioRepository>();
            services.AddScoped<IFacturaRepository, FacturaRepository>();

            return services;
        }
    }
}