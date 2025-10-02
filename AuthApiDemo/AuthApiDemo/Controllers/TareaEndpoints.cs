using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using AuthApiDemo.Models;
using AuthApiDemo.DTOs;
using AuthApiDemo.Services;

namespace AuthApiDemo.Endpoints;

public static class TareaEndpoints
{
    public static void MapTareaEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tareas").WithTags("Tareas");

        // Crear tarea
        group.MapPost("/", async (CreateTareaDto dto, TareaService service) =>
        {
            var tareaDto = await service.AddAsync(dto);
            return Results.Created($"/api/tareas/{tareaDto.IdTarea}", tareaDto);
        })
        .Produces<TareaDto>(201)
        .Produces(400)
        .Produces(409)
        .Produces(500);

        // Obtener tarea por id
        group.MapGet("/{id}", async (string id, TareaService service) =>
        {
            var tareaDto = await service.GetAsync(id);
            return tareaDto != null ? Results.Ok(tareaDto) : Results.NotFound();
        })
        .Produces<TareaDto>(200)
        .Produces(404)
        .Produces(500);

        // Listar todas las tareas
        group.MapGet("/", async (TareaService service) =>
        {
            var tareas = await service.GetAllAsync();
            return Results.Ok(tareas);
        })
        .Produces<List<TareaDto>>(200)
        .Produces(500);

        // Actualizar tarea
        group.MapPut("/{id}", async (string id, CreateTareaDto dto, TareaService service) =>
        {
            var tareaDto = await service.UpdateAsync(id, dto);
            return tareaDto != null ? Results.Ok(tareaDto) : Results.NotFound();
        })
        .Produces<TareaDto>(200)
        .Produces(400)
        .Produces(404)
        .Produces(500);

        // Eliminar tarea
        group.MapDelete("/{id}", async (string id, TareaService service) =>
        {
            var deleted = await service.DeleteAsync(id);
            return deleted ? Results.NoContent() : Results.NotFound();
        })
        .Produces(204)
        .Produces(404)
        .Produces(500);
    }
}
