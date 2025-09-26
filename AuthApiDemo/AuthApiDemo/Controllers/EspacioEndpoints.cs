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

    }

}
