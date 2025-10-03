using AuthApiDemo.DTOs;
using AuthApiDemo.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;

namespace AuthApiDemo.Endpoints
{
    public static class InvitacionEndpoints
    {
        public static void MapInvitacionEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/invitaciones")
                           .WithTags("Invitaciones");

            // POST /api/invitaciones  -> crear
            group.MapPost("/", async (CreateInvitacionDto dto, InvitacionService service) =>
            {
                try
                {
                    var created = await service.CrearInvitacionAsync(dto);
                    return Results.Created($"/api/invitaciones/{created.Id}", created);
                }
                catch (ArgumentException ex) { return Results.BadRequest(new { message = ex.Message }); }
                catch (InvalidOperationException ex) { return Results.Conflict(new { message = ex.Message }); }
                catch (Exception ex)
                {
                    // logging si queréis: (ILogger) se puede inyectar en la lambda si hace falta
                    return Results.Problem(title: "Error interno", detail: ex.Message);
                }
            })
            .WithName("CrearInvitacion")
            .Produces<InvitacionDto>(201)
            .Produces(400)
            .Produces(409)
            .WithOpenApi();

            // GET /api/invitaciones/{id} -> obtener por id
            group.MapGet("/{id}", async (string id, InvitacionService service) =>
            {
                try
                {
                    var inv = await service.GetInvitacionAsync(id);
                    return inv is not null ? Results.Ok(inv) : Results.NotFound();
                }
                catch (Exception ex) { return Results.Problem(title: "Error interno", detail: ex.Message); }
            })
            .WithName("GetInvitacion")
            .Produces<InvitacionDto>(200)
            .Produces(404)
            .WithOpenApi();

            // GET /api/invitaciones?espacioId=xxx  -> lista por espacio (opcional)
            group.MapGet("/", async (string? espacioId, InvitacionService service) =>
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(espacioId))
                    {
                        // si quieres, añade un método GetPorEspacio en el service; por ahora usamos GetAll y filtramos
                        var all = await service.GetAllInvitacionesAsync();
                        var filtered = all.FindAll(i => string.Equals(i.EspacioId, espacioId, StringComparison.OrdinalIgnoreCase));
                        return Results.Ok(filtered);
                    }

                    var list = await service.GetAllInvitacionesAsync();
                    return Results.Ok(list);
                }
                catch (Exception ex) { return Results.Problem(title: "Error interno", detail: ex.Message); }
            })
            .WithName("ListInvitaciones")
            .Produces<List<InvitacionDto>>(200)
            .WithOpenApi();

            // PATCH /api/invitaciones/{id}/estado -> cambiar estado
            group.MapPatch("/{id}/estado", async (string id, StateChangeDto payload, InvitacionService service) =>
            {
                try
                {
                    var ok = await service.CambiarEstadoAsync(id, payload);
                    return ok ? Results.NoContent() : Results.NotFound();
                }
                catch (ArgumentException ex) { return Results.BadRequest(new { message = ex.Message }); }
                catch (InvalidOperationException ex) { return Results.BadRequest(new { message = ex.Message }); }
                catch (KeyNotFoundException) { return Results.NotFound(); }
                catch (Exception ex) { return Results.Problem(title: "Error interno", detail: ex.Message); }
            })
            .WithName("CambiarEstadoInvitacion")
            .Produces(204)
            .Produces(400)
            .Produces(404)
            .WithOpenApi();

            // DELETE /api/invitaciones/{id}
            group.MapDelete("/{id}", async (string id, InvitacionService service) =>
            {
                try
                {
                    var ok = await service.DeleteInvitacionAsync(id);
                    return ok ? Results.NoContent() : Results.NotFound();
                }
                catch (Exception ex) { return Results.Problem(title: "Error interno", detail: ex.Message); }
            })
            .WithName("EliminarInvitacion")
            .Produces(204)
            .Produces(404)
            .WithOpenApi();
        }
    }
}