using Convivia.Aplicacion.DTOs;
using Convivia.Application.DTOs;
using Convivia.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Convivia.API.Controllers
{
    public static class UsuarioEndpoints
    {
        public static void MapUsuarioEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/usuarios").WithTags("Usuarios");

            // Obtener usuario por id (devuelve UsuarioDto)
            group.MapGet("/{id}", async (string id, UsuarioService service) =>
            {
                try
                {
                    var usuario = await service.GetAsync(id);
                    return usuario != null ? Results.Ok(usuario) : Results.NotFound();
                }
                catch (Exception ex)
                {
                    return Results.Problem("Error interno del servidor.", statusCode: 500);
                }
            })
            .Produces<UsuarioDto>(200)
            .Produces(404)
            .Produces(500);

            // Obtener usuario por email (devuelve UsuarioDto)
            group.MapGet("/byemail/{email}", async (string email, UsuarioService service) =>
            {
                try
                {
                    var usuario = await service.GetByEmailAsync(email);
                    return usuario != null ? Results.Ok(usuario) : Results.NotFound();
                }
                catch (Exception ex)
                {
                    return Results.Problem("Error interno del servidor.", statusCode: 500);
                }
            })
            .Produces<UsuarioDto>(200)
            .Produces(404)
            .Produces(500);

            // Crear usuario (recibe CreateUsuarioDto, devuelve UsuarioDto)
            group.MapPost("/", async (CreateUsuarioDto dto, UsuarioService service) =>
            {
                try
                {
                    var createdUsuario = await service.AddAsync(dto);
                    return Results.Created($"/api/usuarios/{createdUsuario.Id}", createdUsuario);
                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(ex.Message); // Validaciones (ej. nombre vacío)
                }
                catch (InvalidOperationException ex)
                {
                    return Results.Conflict(ex.Message); // Email duplicado
                }
                catch (Exception ex)
                {
                    return Results.Problem("Error interno del servidor.", statusCode: 500);
                }
            })
            .Produces<UsuarioDto>(201)
            .Produces(400)
            .Produces(409)
            .Produces(500);

            // Actualizar usuario completo (recibe UpdateUsuarioDto, devuelve UsuarioDto)
            group.MapPut("/{id}", async (string id, UpdateUsuarioDto dto, UsuarioService service) =>
            {
                try
                {
                    var usuario = await service.UpdateAsync(id, dto);
                    return usuario != null ? Results.Ok(usuario) : Results.NotFound();
                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(ex.Message);
                }
                catch (Exception ex)
                {
                    return Results.Problem("Error interno del servidor.", statusCode: 500);
                }
            })
            .Produces<UsuarioDto>(200)
            .Produces(400)
            .Produces(404)
            .Produces(500);

            // PATCH: Actualización parcial de usuario (recibe UpdateUsuarioDto, devuelve UsuarioDto)
            group.MapPatch("/{id}", async (string id, UpdateUsuarioDto dto, UsuarioService service) =>
            {
                try
                {
                    var usuario = await service.PatchAsync(id, dto);
                    return usuario != null ? Results.Ok(usuario) : Results.NotFound();
                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(ex.Message);
                }
                catch (Exception ex)
                {
                    return Results.Problem("Error interno del servidor.", statusCode: 500);
                }
            })
            .Produces<UsuarioDto>(200)
            .Produces(400)
            .Produces(404)
            .Produces(500);

            // Contar usuarios
            group.MapGet("/count", async (UsuarioService service) =>
            {
                try
                {
                    var count = await service.CountUsuariosAsync();
                    return Results.Ok(new { Count = count });
                }
                catch (Exception ex)
                {
                    return Results.Problem("Error interno del servidor.", statusCode: 500);
                }
            })
            .Produces(200)
            .Produces(500);

            // Eliminar usuario
            group.MapDelete("/{id}", async (string id, UsuarioService service) =>
            {
                try
                {
                    var deleted = await service.DeleteAsync(id);
                    return deleted ? Results.NoContent() : Results.NotFound();
                }
                catch (Exception ex)
                {
                    return Results.Problem("Error interno del servidor.", statusCode: 500);
                }
            })
            .Produces(204)
            .Produces(404)
            .Produces(500);
        }
    }
}