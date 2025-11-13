
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using Convivia.Domain.Models;
namespace AuthApiDemo.Endpoints;

public static class PeticionEndpoints
{
    private static List<Peticion> peticiones = new List<Peticion>();

    public static void MapPeticionEndpoints(this IEndpointRouteBuilder app)
    {
        // Obtener todas las peticiones
        app.MapGet("/peticiones", () =>
        {
            return Results.Ok(peticiones);
        });

        // Obtener una petición por ID
        app.MapGet("/peticiones/{id}", (string id) =>
        {
            var peticion = peticiones.FirstOrDefault(p => p.Id == id);
            if (peticion == null)
            {
                return Results.NotFound(new { message = "Petición no encontrada" });
            }
            return Results.Ok(peticion);
        });

        // Crear una nueva petición
        app.MapPost("/peticiones", (Peticion nuevaPeticion) =>
        {
            if (nuevaPeticion == null || string.IsNullOrWhiteSpace(nuevaPeticion.Mensaje))
            {
                return Results.BadRequest(new { message = "Datos de la petición inválidos" });
            }

            peticiones.Add(nuevaPeticion);
            return Results.Created($"/peticiones/{nuevaPeticion.Id}", nuevaPeticion);
        });

        // Actualizar una petición existente
        app.MapPut("/peticiones/{id}", (string id, Peticion peticionActualizada) =>
        {
            var peticion = peticiones.FirstOrDefault(p => p.Id == id);
            if (peticion == null)
            {
                return Results.NotFound(new { message = "Petición no encontrada" });
            }

            peticion.Mensaje = peticionActualizada.Mensaje;
            peticion.Estado = peticionActualizada.Estado;
            peticion.Fecha = peticionActualizada.Fecha;
            return Results.Ok(peticion);
        });

        // Eliminar una petición
        app.MapDelete("/peticiones/{id}", (string id) =>
        {
            var peticion = peticiones.FirstOrDefault(p => p.Id == id);
            if (peticion == null)
            {
                return Results.NotFound(new { message = "Petición no encontrada" });
            }

            peticiones.Remove(peticion);
            return Results.NoContent();
        });
    }
}
