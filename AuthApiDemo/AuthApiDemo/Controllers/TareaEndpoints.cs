using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using AuthApiDemo.Models;

namespace AuthApiDemo.Endpoints;

public static class TareaEndpoints
{/*
    public static void MapTareaEndpoints(this IEndpointRouteBuilder app)
    {

        var espacios = new List<Espacio>
        {
            new Espacio
            {
                Id_Espacio = "abc123",
                Nombre = "Casa Madrid",
                Salas = new List<Sala>
                {
                    new Sala { Nombre = "Cocina", Id_Espacio = "abc123", Descripcion = "sala para cocinar", },
                    new Sala { Nombre = "Salón", Descripcion = "sala para estar", Id_Espacio = "abc123" }
                    // new Sala {"Salón", "sala para estar", "abc123"}
                }
            },
            new Espacio
            {
                Id_Espacio = "xyz789",
                Nombre = "Casa Barcelona",
                Salas = new List<Sala>
                {
                    //new Sala { Nombre = "Terraza", Id_Espacio = "xyz789", Descripcion = "sala para estar fuera"}
                }
            }
        };

        var Tareas = new List<Tarea>
        {
            new Tarea
            {
                Id_Tarea = "abc123",
                Usuarios = new List<UsuarioEspacio>(),
                FechaRealizacion = DateTime.Now,
                FechaLimite = DateTime.Now.AddDays(7),
                Estado = false,
                karma = 10
            },

            new Tarea
            {

                Id_Tarea = "def456",
                Usuarios = new List<UsuarioEspacio>(),
                FechaRealizacion = DateTime.Now,
                FechaLimite = DateTime.Now.AddDays(3),
                Estado = true,
                karma = 15

            },

            new Tarea
            {
                Id_Tarea = "ghi789",
                Usuarios = new List<UsuarioEspacio>(),
                FechaRealizacion = DateTime.Now,
                FechaLimite = DateTime.Now.AddDays(5),
                Estado = false,
                karma = 20
            }

        };  

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
