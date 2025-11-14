using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using Convivia.Application.DTOs;
using Convivia.Infrastructure.Services;

namespace Convivia.API.Endpoints;

public static class EspacioEndpoints
{
    public static void MapEspacioEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/espacios").WithTags("Espacios");

        // Crear espacio
        group.MapPost("/", async (CreateEspacioDto dto, EspacioService service) =>
        {
            var espacioDto = await service.AddAsync(dto);
            return Results.Created($"/api/espacios/{espacioDto.Id_Espacio}", espacioDto);
        })
        .Produces<EspacioDto>(201)
        .Produces(400)
        .Produces(409)
        .Produces(500);

        // Obtener espacio por id
        group.MapGet("/{id}", async (string id, EspacioService service) =>
        {
            var espacioDto = await service.GetAsync(id);
            return espacioDto != null ? Results.Ok(espacioDto) : Results.NotFound();
        })
        .Produces<EspacioDto>(200)
        .Produces(404)
        .Produces(500);

        // Eliminar espacio
        group.MapDelete("/{id}", async (string id, EspacioService service) =>
        {
            var deleted = await service.DeleteAsync(id);
            return deleted ? Results.NoContent() : Results.NotFound();
        })
        .Produces(204)
        .Produces(404)
        .Produces(500);

        // PATCH: Actualización parcial de espacio
        group.MapPatch("/{id}", async (string id, UpdateEspacioDto dto, EspacioService service) =>
        {
            var espacioDto = await service.PatchAsync(id, dto);
            return espacioDto != null ? Results.Ok(espacioDto) : Results.NotFound();
        })
        .Produces<EspacioDto>(200)
        .Produces(400)
        .Produces(404)
        .Produces(500);
    }
}
