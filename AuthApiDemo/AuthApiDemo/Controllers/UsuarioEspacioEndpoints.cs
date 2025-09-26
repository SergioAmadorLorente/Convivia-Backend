using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using AuthApiDemo.Models;

namespace AuthApiDemo.Endpoints;

public static class UsuarioEspacioEndpoints
{

    // Datos de ejemplo

    private static List<UsuarioEspacio> usuariosespacios = new List<UsuarioEspacio>();
    private static Espacio espacio = new Espacio
    {

        Nombre = "Espacio de Ejemplo",
        UsuariosEspacios = usuariosespacios,
        
    };

    private static UsuarioEspacio usuarioespacio = new UsuarioEspacio
    {

        Usuario = new Usuario { Nombre = "Usuario 1", Email = "jgibert06@gmail.com", Password = "password1" },
        Espacio = espacio,
        Permiso = Permiso.Admin,
        Rol = "admin",
        Ausente = false,
        Karma = 100,
        TareasAsignadas = new List<Tarea>
        {
            new Tarea
            {
                Id_Tarea = "tarea1",
                FechaRealizacion = DateTime.Now,
                FechaLimite = DateTime.Now.AddDays(7),
                Estado = false,
                karma = 10,
                Usuarios = usuariosespacios
            },
            new Tarea
            {
                Id_Tarea = "tarea2",
                FechaRealizacion = DateTime.Now,
                FechaLimite = DateTime.Now.AddDays(3),
                Estado = true,
                karma = 15,
                Usuarios = usuariosespacios
            }
        }

    };

    public static void MapTareaEndpoints(this IEndpointRouteBuilder app)
    {

        app.MapPost("/usuario/tareas", (UsuarioEspacio requestu) =>
        {

            var usuarioespacio = usuariosespacios.FirstOrDefault(u => u.Id_UsuarioEspacio == requestu.Id_UsuarioEspacio);

            if (usuarioespacio == null)
                return Results.NotFound(new { message = "Usuario no encontrado" });

            var tarea = usuarioespacio.Tareas.FirstOrDefault(t => t.Usuarios.Contains(usuarioespacio));

            if (tarea == null)
                return Results.NotFound(new { message = "El usuario no tiene tareas asignadas" });

            List<Tarea> tareasUsuario = usuarioespacio.TareasAsignadas;

            return Results.Ok(tareasUsuario);
        });

    }



}
