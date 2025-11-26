using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Convivia.Shared.Repositories;
using Convivia.Shared.Services;
using Convivia.Infrastructure.Repositories;
using Convivia.Infrastructure.Services;

namespace Convivia.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Repositorios concretos
            services.AddScoped<IInvitacionRepository, InvitacionRepository>();
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            services.AddScoped<IEspacioRepository, EspacioRepository>();
            services.AddScoped<ISalaRepository, SalaRepository>();

            // Servicios de infraestructura
            services.AddScoped<IFirebaseService, FirebaseService>();


            return services;
        }
    }
}