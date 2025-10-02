using AuthApiDemo.Models;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AuthApiDemo.Endpoints;

public static class EspacioEndpoints
{
    public static void MapEspacioEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/espacio/usuarios", async (string request, UserService userService) =>
        {

            if (string.IsNullOrWhiteSpace(request))
                return Results.BadRequest(new { message = "EspacioId no proporcionado" });

            var usuarios = await userService.ObtenerUsuariosPorEspacioId(request);

            if (usuarios == null || usuarios.Count == 0)
                return Results.NotFound(new { message = "Espacio no encontrado o sin usuarios" });

            return Results.Ok(usuarios);
        });

        app.MapGet("/espacio/usuarios/{id}", async (string id, UserService userService) =>
        {
            if (string.IsNullOrWhiteSpace(id))
                return Results.BadRequest(new { message = "EspacioId no proporcionado" });

            var usuarios = await userService.ObtenerUsuariosPorEspacioId(id);

            if (usuarios == null || usuarios.Count == 0)
                return Results.NotFound(new { message = "Espacio no encontrado o sin usuarios" });

            return Results.Ok(usuarios);
        });

        app.MapGet("/espacios/{id}", async (string id, UserService userService) =>
        {
            var espacio = await userService.GetEspacioByIdAsync(id);
            return espacio is null ? Results.NotFound() : Results.Ok(espacio);
        });

        app.MapPost("/espacios", async (Espacio espacio, UserService userService) =>
        {
            await userService.CrearEspacioAsync(espacio);
            return Results.Created($"/espacios/{espacio.Id_Espacio}", espacio);
        });

        app.MapPut("/espacios/{id}", async (string id, Espacio update, UserService userService) =>
        {
            var espacio = await userService.GetEspacioByIdAsync(id);
            if (espacio is null) return Results.NotFound();

            espacio.Nombre = update.Nombre ?? espacio.Nombre;
            espacio.Direccion = update.Direccion ?? espacio.Direccion;
            await userService.ActualizarEspacioAsync(espacio);
            return Results.Ok(espacio);
        });

        app.MapDelete("/espacios/{id}", async (string id, UserService userService) =>
        {
            var espacio = await userService.GetEspacioByIdAsync(id);
            if (espacio is null) return Results.NotFound();
            await userService.EliminarEspacioAsync(id);
            return Results.NoContent();
        });

        app.MapGet("/espacios/{id}/usuarios", async (string id, UserService userService) =>
        {
            var usuarios = await userService.GetUsuariosDelEspacioAsync(id);
            return Results.Ok(usuarios);
        });

        app.MapPost("/espacios/{id}/usuarios", async (string id, Usuario usuario, UserService userService) =>
        {
            var espacio = await userService.GetEspacioByIdAsync(id);
            if (espacio is null) return Results.NotFound();
            try
            {
                var admitido = espacio.AdmitirUsuario(usuario);
                if (!admitido) return Results.BadRequest(new { message = "No se pudo admitir el usuario." });
                await userService.ActualizarEspacioAsync(espacio);
                return Results.Ok(espacio.UsuarioEspacios);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        });

        app.MapDelete("/espacios/{id}/usuarios/{usuarioId}", async (string id, string usuarioId, UserService userService) =>
        {
            var espacio = await userService.GetEspacioByIdAsync(id);
            if (espacio is null) return Results.NotFound();
            var usuarioEspacio = espacio.UsuarioEspacios.FirstOrDefault(u => u.Usuario.Id == usuarioId);
            if (usuarioEspacio is null) return Results.NotFound();
            var eliminado = espacio.EliminarUsuario(usuarioEspacio);
            if (!eliminado) return Results.BadRequest();
            await userService.ActualizarEspacioAsync(espacio);
            return Results.NoContent();
        });

        app.MapGet("/espacios/{id}/peticiones", async (string id, UserService userService) =>
        {
            var espacio = await userService.GetEspacioByIdAsync(id);
            return espacio is null ? Results.NotFound() : Results.Ok(espacio.Peticiones);
        });

        app.MapPost("/espacios/{id}/peticiones", async (string id, Peticion peticion, UserService userService) =>
        {
            var espacio = await userService.GetEspacioByIdAsync(id);
            if (espacio is null) return Results.NotFound();
            espacio.Peticiones.Add(peticion);
            await userService.ActualizarEspacioAsync(espacio);
            return Results.Ok(espacio.Peticiones);
        });

        app.MapDelete("/espacios/{id}/peticiones/{peticionId}", async (string id, string peticionId, UserService userService) =>
        {
            var espacio = await userService.GetEspacioByIdAsync(id);
            if (espacio is null) return Results.NotFound();
            var peticion = espacio.Peticiones.FirstOrDefault(p => p.Id == peticionId);
            if (peticion is null) return Results.NotFound();
            espacio.Peticiones.Remove(peticion);
            await userService.ActualizarEspacioAsync(espacio);
            return Results.NoContent();
        });

        app.MapGet("/espacios/{id}/invitaciones", async (string id, UserService userService) =>
        {
            var espacio = await userService.GetEspacioByIdAsync(id);
            return espacio is null ? Results.NotFound() : Results.Ok(espacio.InvitacionesEnviadas);
        });

        app.MapPost("/espacios/{id}/invitaciones", async (string id, Invitacion invitacion, UserService userService) =>
        {
            var espacio = await userService.GetEspacioByIdAsync(id);
            if (espacio is null) return Results.NotFound();
            espacio.InvitacionesEnviadas.Add(invitacion);
            await userService.ActualizarEspacioAsync(espacio);
            return Results.Ok(espacio.InvitacionesEnviadas);
        });

        app.MapGet("/espacios/{id}/salas", async (string id, UserService userService) =>
        {
            var salas = await userService.GetSalasDelEspacioAsync(id);
            return Results.Ok(salas);
        });

        app.MapPost("/espacios/{id}/salas", async (string id, Sala sala, UserService userService) =>
        {
            var espacio = await userService.GetEspacioByIdAsync(id);
            if (espacio is null) return Results.NotFound();
            try
            {
                var nuevaSala = espacio.CrearSala(sala.Nombre, sala.Descripcion);
                await userService.ActualizarEspacioAsync(espacio);
                return Results.Created($"/espacios/{id}/salas/{nuevaSala.Nombre}", nuevaSala);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        });

        app.MapGet("/espacios/{id}/salas/{nombre}", async (string id, string nombre, UserService userService) =>
        {
            var espacio = await userService.GetEspacioByIdAsync(id);
            if (espacio is null) return Results.NotFound();
            var sala = espacio.BuscarSalaNombre(nombre);
            return sala is null ? Results.NotFound() : Results.Ok(sala);
        });

        app.MapDelete("/espacios/{id}/salas/{nombre}", async (string id, string nombre, UserService userService) =>
        {
            var espacio = await userService.GetEspacioByIdAsync(id);
            if (espacio is null) return Results.NotFound();
            var eliminado = espacio.EliminarSala(nombre);
            if (!eliminado) return Results.BadRequest();
            await userService.ActualizarEspacioAsync(espacio);
            return Results.NoContent();
        });

    }

}
