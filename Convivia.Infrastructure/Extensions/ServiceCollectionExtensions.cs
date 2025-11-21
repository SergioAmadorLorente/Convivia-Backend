using Microsoft.Extensions.DependencyInjection;
using Convivia.Shared.Repositories;
using Convivia.Shared.Services;
using Convivia.Infrastructure.Services;
using Convivia.Infrastructure.Repositories;
using Convivia.Domain.Models;
using Microsoft.Extensions.Configuration;

namespace Convivia.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Repositorios concretos
            services.AddScoped<IInvitacionRepository, InvitacionRepository>();
            services.AddScoped<IPlantillaTareaRepository<PlantillaTarea>, PlantillaTareaRepository>();

            // Servicios de infraestructura
            services.AddScoped<IFirebaseService, FirebaseService>();


            return services;
        }
    }
}