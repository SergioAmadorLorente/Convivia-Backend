using Convivia.Application.Services;
using Convivia.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Convivia.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Registrar servicios de aplicación
            // Si tienes interfaz IInvitacionService: services.AddScoped<IInvitacionService, InvitacionService>();
            services.AddScoped<InvitacionService>();
            services.AddScoped<UsuarioService>();


            services.AddScoped<PeticionService>();


            // Registrar mappers, validators, MediatR, etc. si procede
            // services.AddAutoMapper(typeof(YourProfile).Assembly);

            return services;
        }
    }
}