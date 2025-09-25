using Google.Cloud.Firestore;
using System.Threading.Tasks;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using System;
using AuthApiDemo.Models;
using System.IO;
using Microsoft.Extensions.Logging;
using System.Text.Json;


using AuthApiDemo.Models;
using Google.Cloud.Firestore;

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

    public async Task<Espacio?> GetEspacioByIdAsync(string espacioId)
    {
        var docRef = _db.Collection("espacios").Document(espacioId);
        var snapshot = await docRef.GetSnapshotAsync();

        return snapshot.Exists ? snapshot.ConvertTo<Espacio>() : null;
    }

    public async Task<List<UsuarioEspacio>> GetUsuariosDelEspacioAsync(string espacioId)
    {
        var espacio = await GetEspacioByIdAsync(espacioId);
        return espacio?.UsuariosEspacios ?? new List<UsuarioEspacio>();
    }

    public async Task<List<Sala>> GetSalasDelEspacioAsync(string espacioId)
    {
        var espacio = await GetEspacioByIdAsync(espacioId);
        return espacio?.Salas ?? new List<Sala>();
    }

    //provar connexió

public async Task<bool> ProbarConexionAsync()
    {
        try
        {
            string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Firebase/base1.json");
            string jsonContent = await File.ReadAllTextAsync(jsonPath);

            using var document = JsonDocument.Parse(jsonContent);
            var root = document.RootElement;

            if (root.ValueKind != JsonValueKind.Object)
                throw new Exception("El JSON debe ser un objeto con colecciones como propiedades.");

            foreach (var coleccion in root.EnumerateObject())
            {
                string nombreColeccion = coleccion.Name;
                var documentos = coleccion.Value;

                if (documentos.ValueKind != JsonValueKind.Object)
                {
                    _logger.LogWarning($"La colección '{nombreColeccion}' no tiene formato válido.");
                    continue;
                }

                CollectionReference coleccionRef = _db.Collection(nombreColeccion);

                foreach (var documento in documentos.EnumerateObject())
                {
                    string idDocumento = documento.Name;
                    var contenido = ConvertJsonElement(documento.Value);

                    // Convertir referencias tipo "Coleccion/id" en DocumentReference
                    var contenidoFinal = ConvertReferences(contenido);

                    DocumentReference docRef = coleccionRef.Document(idDocumento);
                    await docRef.SetAsync(contenidoFinal);
                    Console.WriteLine($"Insertado documento '{idDocumento}' en colección '{nombreColeccion}'.");
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al insertar datos en Firestore: {ex.Message}");
            return false;
        }
    }

    private object ConvertReferences(object data)
    {
        if (data is Dictionary<string, object> dict)
        {
            var result = new Dictionary<string, object>();
            foreach (var kvp in dict)
            {
                result[kvp.Key] = ConvertReferences(kvp.Value);
            }
            return result;
        }
        else if (data is List<object> list)
        {
            return list.Select(ConvertReferences).ToList();
        }
        else if (data is string str && str.Contains("/") && str.Split('/').Length == 2)
        {
            var parts = str.Split('/');
            return _db.Collection(parts[0]).Document(parts[1]);
        }
        else
        {
            return data;
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