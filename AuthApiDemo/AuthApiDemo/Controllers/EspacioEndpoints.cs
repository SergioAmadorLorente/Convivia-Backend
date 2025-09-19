using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using AuthApiDemo.Models;

namespace AuthApiDemo.Endpoints;

public static class EspacioEndpoints
{
    public static void MapEspacioEndpoints(this IEndpointRouteBuilder app)
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
        
        app.MapPost("/espacio/salas", (EspacioRequest request) =>
        {
            var espacio = espacios.FirstOrDefault(e => e.Id_Espacio == request.EspacioId);
            if (espacio == null)
                return Results.NotFound(new { message = "Espacio no encontrado" });

            Console.WriteLine("EspacioId recibido: " + request.EspacioId);

            var sala = espacio.Salas.FirstOrDefault();

            if (sala == null)
                return Results.NotFound(new { message = "El espacio no contiene salas" });

            return Results.Ok(espacio.Salas);
        });

        app.MapPost("/espacio/usuarios", (EspacioRequest request) =>
        {
            var espacio = espacios.FirstOrDefault(e => e.Id_Espacio == request.EspacioId);
            if (espacio == null)
                return Results.NotFound(new { message = "Espacio no encontrado" });

            Console.WriteLine("EspacioId recibido: " + request.EspacioId);

            var usuario = espacio.Usuarios.FirstOrDefault();

            if (usuario == null)
                return Results.NotFound(new { message = "El espacio no contiene usuarios" });

            return Results.Ok(espacio.Usuarios);
        });
    }
}
