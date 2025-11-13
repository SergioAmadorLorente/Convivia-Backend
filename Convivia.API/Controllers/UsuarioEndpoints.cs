/*
 * 
 * 
 * 
 * 
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
//using AuthApiDemo.Models;
using Convivia.Application.DTOs;
using Convivia.Interface.Services;

namespace AuthApiDemo.Controllers
{
    public static class UsuarioEndpoints
    {
        public static void MapUsuarioEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/usuarios").WithTags("Usuarios");
            // Obtener usuario por id
            group.MapGet("/{id}", async (string id, UsuarioService service) =>
            {
                var usuario = await service.GetAsync(id);
                return usuario != null ? Results.Ok(usuario) : Results.NotFound();
            })
            .Produces<Usuario>(200)
            .Produces(404)
            .Produces(500);
            // Obtener usuario por email
            group.MapGet("/byemail/{email}", async (string email, UsuarioService service) =>
            {
                var usuario = await service.GetByEmailAsync(email);
                return usuario != null ? Results.Ok(usuario) : Results.NotFound();
            })
            .Produces<Usuario>(200)
            .Produces(404)
            .Produces(500);
            // Crear usuario
            group.MapPost("/", async (Usuario usuario, UsuarioService service) =>
            {
                var createdUsuario = await service.AddAsync(usuario);
                return Results.Created($"/api/usuarios/{createdUsuario.Id}", createdUsuario);
            })
            .Produces<Usuario>(201)
            .Produces(400)
            .Produces(409)
            .Produces(500);
            // Actualizar usuario
            group.MapPut("/{id}", async (string id, Usuario updatedUsuario, UsuarioService service) =>
            {
                var usuario = await service.UpdateAsync(id, updatedUsuario);
                return usuario != null ? Results.Ok(usuario) : Results.NotFound();
            })
            .Produces<Usuario>(200)
            .Produces(404)
            .Produces(500);

            // Contar usuarios 
            group.MapGet("/count", async (UsuarioService service) =>
            {
                var count = await service.CountUsuariosAsync();
                return Results.Ok(new { Count = count });
            })
            .Produces(200)
            .Produces(500);

            // Eliminar usuario
            group.MapDelete("/{id}", async (string id, UsuarioService service) =>
            {
                var deleted = await service.DeleteAsync(id);
                return deleted ? Results.NoContent() : Results.NotFound();

            })
            .Produces(204)
            .Produces(404)
            .Produces(500);


            // TODO acabar map patch 
            // PATCH: Actualización parcial de usuario
            group.MapPatch("/{id}", async (string id, UpdateUsuarioDto dto, UsuarioService service) =>
            {
                var usuario = await service.PatchAsync(id, dto);
                return usuario != null ? Results.Ok(usuario) : Results.NotFound();
            })
            .Produces<UsuarioDto>(200)
            .Produces(400)
            .Produces(404)
            .Produces(500);
        }

    }
}
*/