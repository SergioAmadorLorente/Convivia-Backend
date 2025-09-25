using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using AuthApiDemo.Models;

namespace AuthApiDemo.Endpoints;

public static class UsuarioEspacioEndpoints
{
    /*public static void MapTareaEndpoints(this IEndpointRouteBuilder app)
    {

        

        app.MapPost("/usuario/tareas", (UsuarioEspacio requestu) =>
        {

            var usuarioespacio = usuariosespacios.FirstOrDefault(u => u.Id_UsuarioEspacio == requestu.Id_UsuarioEspacio);

            if (usuarioespacio == null)
                return Results.NotFound(new { message = "Usuario no encontrado" });

            var tarea = usuarioespacio.Tareas.FirstOrDefault(t => t.Usuarios.Contains(usuarioespacio));

            if (tarea == null)
                return Results.NotFound(new { message = "El usuario no tiene tareas asignadas" });

            List<Tarea> tareasUsuario = espacio.Tareas.Where(t => t.Usuarios.Contains(usuarioespacio)).ToList();

            return Results.Ok(tareasUsuario);
        });

    }*/
}
