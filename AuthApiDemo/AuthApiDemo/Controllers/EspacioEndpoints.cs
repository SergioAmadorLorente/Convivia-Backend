using AuthApiDemo.Models;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AuthApiDemo.Endpoints;

public static class EspacioEndpoints
{
    private readonly FirestoreDb _db;
    private readonly ILogger<EspacioEndpoints> _logger;

    public EspacioEndpoints(ILogger<EspacioEndpoints> logger)
    {
        _logger = logger;

        Environment.SetEnvironmentVariable(
            "GOOGLE_APPLICATION_CREDENTIALS",
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Firebase/serviceAccount.json")
        );

        _db = FirestoreDb.Create("convivia-862f2");
    }


    public static void MapEspacioEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/espacio/usuarios", async (HttpRequest httpRequest, UserService userService) =>
        {
            using var reader = new StreamReader(httpRequest.Body);
            var espacioId = await reader.ReadToEndAsync();

            var usuarios = await userService.ObtenerUsuariosPorEspacioId(espacioId);

            if (usuarios == null)
                return Results.NotFound(new { message = "Espacio no encontrado o sin usuarios" });

            return Results.Ok(usuarios);
        });
    }

}
