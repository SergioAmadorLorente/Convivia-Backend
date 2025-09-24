using Google.Cloud.Firestore;
using System.Threading.Tasks;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using System;
using AuthApiDemo.Models;
using System.IO;
using Microsoft.Extensions.Logging;
using System.Text.Json;


public class UserService
{
    private readonly FirestoreDb _db;
    private readonly ILogger<UserService> _logger;

    public UserService(ILogger<UserService> logger)
    {
        _logger = logger;

        Environment.SetEnvironmentVariable(
            "GOOGLE_APPLICATION_CREDENTIALS",
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Firebase/serviceAccount.json")
        );

        _db = FirestoreDb.Create("convivia-862f2");
    }


public async Task<bool> ProbarConexionAsync()
{
    try
    {
        string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Firebase/usuarios1.json");
        string jsonContent = await File.ReadAllTextAsync(jsonPath);

        // Deserializar como JsonDocument
        using var document = JsonDocument.Parse(jsonContent);
        var root = document.RootElement;

        if (root.ValueKind != JsonValueKind.Array)
            throw new Exception("El JSON debe ser un array de objetos.");

        CollectionReference usuariosRef = _db.Collection("Usuarios");

        foreach (var element in root.EnumerateArray())
        {
            var usuario = new Dictionary<string, object>();

            foreach (var property in element.EnumerateObject())
            {
                usuario[property.Name] = ConvertJsonElement(property.Value);
            }

            DocumentReference nuevoDoc = await usuariosRef.AddAsync(usuario);
            Console.WriteLine("Documento creado con ID: " + nuevoDoc.Id);
        }

        return true;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, $"Error al insertar usuarios: {ex.Message}");
        return false;
    }
}

    // Convierte JsonElement a tipos nativos
    private object ConvertJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt32(out var i) ? i : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            JsonValueKind.Array => element.EnumerateArray().Select(ConvertJsonElement).ToList(),
            JsonValueKind.Object => element.EnumerateObject().ToDictionary(
                prop => prop.Name,
                prop => ConvertJsonElement(prop.Value)
            ),
            _ => element.ToString()
        };
    }



}