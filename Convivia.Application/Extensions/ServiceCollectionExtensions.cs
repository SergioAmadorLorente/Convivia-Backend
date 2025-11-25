using Convivia.Application.Services;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using Convivia.Application.Mappers;

namespace Convivia.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Registrar servicios de aplicación
            // Si tienes interfaz IInvitacionService: services.AddScoped<IInvitacionService, InvitacionService>();
            services.AddScoped<InvitacionService>();
            services.AddScoped<SalaService>();

            // Registrar mappers, validators, MediatR, etc. si procede
            MapsterConfig.RegisterPair<Convivia.Domain.Models.Sala, Convivia.Shared.DTOs.SalaDto, Convivia.Shared.DTOs.CreateSalaDto, Convivia.Shared.DTOs.UpdateSalaDto>(TypeAdapterConfig.GlobalSettings);

            // services.AddAutoMapper(typeof(YourProfile).Assembly);

            return services;
        }
    }
}