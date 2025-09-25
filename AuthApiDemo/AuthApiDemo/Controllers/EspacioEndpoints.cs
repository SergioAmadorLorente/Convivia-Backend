using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using AuthApiDemo.Models;

namespace AuthApiDemo.Endpoints;

public static class EspacioEndpoints
{
    public static void MapEspacioEndpoints(this IEndpointRouteBuilder app)
    {

        // per quan volguem provar amb dades reals

        /*app.MapPost("/espacio/usuarios", (string request) =>
        {
            var espacio = espacios.FirstOrDefault(e => e.Id_Espacio == request.EspacioId);
            if (espacio == null)
                return Results.NotFound(new { message = "Espacio no encontrado" });

            Console.WriteLine("EspacioId recibido: " + request.EspacioId);

            var usuario = espacio.Usuarios.FirstOrDefault();

            if (usuario == null)
                return Results.NotFound(new { message = "El espacio no contiene usuarios" });

            return Results.Ok(espacio.Usuarios);
        });*/
    }
}
