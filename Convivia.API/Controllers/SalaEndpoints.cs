using Convivia.Application.DTOs;
using Convivia.Interface.Services;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

using System;
using System.Collections.Generic;

namespace AuthApiDemo.Endpoints
{
    public static class SalaEndpoints
    {
        public static void MapSalaEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/salas")
                           .WithTags("Salas");

            // POST /api/salas -> crear sala
            group.MapPost("/", async (CreateSalaDto dto, SalaService service) =>
            {
                try
                {
                    var created = await service.AddAsync(dto);
                    return Results.Created($"/api/salas/{created.IdSala}", created);
                }
                catch (ArgumentException ex) { return Results.BadRequest(new { message = ex.Message }); }
                catch (InvalidOperationException ex) { return Results.Conflict(new { message = ex.Message }); }
                catch (Exception ex) { return Results.Problem(title: "Error interno", detail: ex.Message); }
            })
            .WithName("CrearSala")
            .Produces<SalaDto>(201)
            .Produces(400)
            .Produces(409)
            .WithOpenApi();

            // GET /api/salas/{id} -> obtener por id
            group.MapGet("/{id}", async (string id, SalaService service) =>
            {
                try
                {
                    var sala = await service.GetAsync(id);
                    return sala is not null ? Results.Ok(sala) : Results.NotFound();
                }
                catch (Exception ex) { return Results.Problem(title: "Error interno", detail: ex.Message); }
            })
            .WithName("GetSala")
            .Produces<SalaDto>(200)
            .Produces(404)
            .WithOpenApi();

            // GET /api/salas?espacioId=xxx -> listar (opcional filtro por espacio)
            group.MapGet("/", async (string? espacioId, SalaService service) =>
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(espacioId))
                    {
                        var filtered = await service.GetByEspacioAsync(espacioId);
                        // Devolvemos los DTOs tal cual (IdEspacio es string en SalaDto)
                        return Results.Ok(filtered);
                    }

                    var list = await service.GetAllAsync();
                    return Results.Ok(list);
                }
                catch (Exception ex) { return Results.Problem(title: "Error interno", detail: ex.Message); }
            })
            .WithName("ListSalas")
            .Produces<List<SalaDto>>(200)
            .WithOpenApi();

            // PATCH /api/salas/{id} -> actualización parcial
            group.MapPatch("/{id}", async (string id, UpdateSalaDto dto, SalaService service) =>
            {
                try
                {
                    var updated = await service.PatchAsync(id, dto);
                    return updated is not null ? Results.Ok(updated) : Results.NotFound();
                }
                catch (ArgumentException ex) { return Results.BadRequest(new { message = ex.Message }); }
                catch (Exception ex) { return Results.Problem(title: "Error interno", detail: ex.Message); }
            })
            .WithName("PatchSala")
            .Produces<SalaDto>(200)
            .Produces(400)
            .Produces(404)
            .WithOpenApi();

            // DELETE /api/salas/{id}
            group.MapDelete("/{id}", async (string id, SalaService service) =>
            {
                try
                {
                    var ok = await service.DeleteAsync(id);
                    return ok ? Results.NoContent() : Results.NotFound();
                }
                catch (Exception ex) { return Results.Problem(title: "Error interno", detail: ex.Message); }
            })
            .WithName("EliminarSala")
            .Produces(204)
            .Produces(404)
            .WithOpenApi();
        }
    }
}