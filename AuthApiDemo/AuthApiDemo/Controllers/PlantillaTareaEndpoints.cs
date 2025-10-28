/*
using Microsoft.AspNetCore.Routing;


using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using AuthApiDemo.Models;
using AuthApiDemo.DTOs;
using AuthApiDemo.Services;

namespace AuthApiDemo.Endpoints;

public static class PlantillaTareaEndpoints
{
    public static void MapPlantillaTareaEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tareas").WithTags("Tareas");

        // Crear tarea
        group.MapPost("/", async (CreateTareaDto dto, TareaService service, PlantillaTareaService plantillaService) =>
        {
            if (dto.Fechas == null || dto.Fechas.Count == 0)
                return Results.BadRequest("Debe especificar al menos una fecha.");

            // Si hay más de una fecha, crear plantilla y tareas
            if (dto.Fechas.Count > 1)
            {
                var plantillaDto = await plantillaService.CreateFromTareaDtoAsync(dto);
                var tareas = await service.CreateFromPlantillaAsync(plantillaDto, dto.Fechas);
                return Results.Created("/api/tareas", tareas);
            }

            // Si solo hay una fecha, crear tarea directamente
            var tareaDto = await service.AddAsync(dto);
            return Results.Created($"/api/tareas/{tareaDto.IdTarea}", tareaDto);
        })
        .Produces<List<TareaDto>>(201)
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

        // Obtener tarea por usuario
        /*app.MapGet("/api/tareasperusuari/{idusuari}", async (string idusuari, TareaService service) =>
        {
            var tareaDto = await service.GetAsync(idusuari);
            return tareaDto != null ? Results.Ok(tareaDto) : Results.NotFound();
        })
        .Produces<TareaDto>(200)
        .Produces(404)
        .Produces(500);

        / Eliminar tarea
        group.MapDelete("/{id}", async (string id, TareaService service) =>
        {
            var deleted = await service.DeleteAsync(id);
            return deleted ? Results.NoContent() : Results.NotFound();
        })
        .Produces(204)
        .Produces(404)
        .Produces(500);

        // PATCH: Actualización parcial de tarea
        group.MapPatch("/{id}", async (string id, UpdateTareaDto dto, TareaService service) =>
        {
            var tareaDto = await service.PatchAsync(id, dto);
            return tareaDto != null ? Results.Ok(tareaDto) : Results.NotFound();
        })
        .Produces<TareaDto>(200)
        .Produces(400)
        .Produces(404)
        .Produces(500);

        group.MapGet("/", async (TareaService service) =>
        {
            var tareas = await service.GetAllAsync();
            return Results.Ok(tareas);
        })
        .Produces<List<TareaDto>>(200)
        .Produces(500);

        group.MapGet("/filtrarestado", async (bool estado, TareaService service) =>
        {
            var tareas = await service.GetByEstadoAsync(estado);
            return Results.Ok(tareas);
        })
        .Produces<List<TareaDto>>(200)
        .Produces(500);

        group.MapGet("/filtrarfecha", async (string fechainicio, string fechafinal, TareaService service) =>
        {
            var inicio = DateTime.Parse(fechainicio, null, System.Globalization.DateTimeStyles.RoundtripKind);
            var final = DateTime.Parse(fechafinal, null, System.Globalization.DateTimeStyles.RoundtripKind);

            var todas = await service.GetAllAsync();

            var tareasFiltradas = todas.Where(t => t.FechaLimite >= inicio && t.FechaLimite <= final).ToList();

            if (tareasFiltradas.Count() != 0)
                return Results.Ok(tareasFiltradas);
            else
                return Results.NotFound();
        })
        .Produces<List<TareaDto>>(200)
        .Produces(500);

        group.MapPatch("/actualizarvariastareas", async (PatchVariasTareasDto data, TareaService service) =>
        {
            var tareaDto = await service.PatchVariasAsync(data.ListaIds, data.Dto);
            return tareaDto != null ? Results.Ok(tareaDto) : Results.NotFound();
        })
        .Produces<TareaDto>(200)
        .Produces(400)
        .Produces(404)
        .Produces(500);

    }
}
*/