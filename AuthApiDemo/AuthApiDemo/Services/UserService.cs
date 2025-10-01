using AuthApiDemo.Models;
using FirebaseAdmin;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

/// <summary>
/// Servicio para gestionar operaciones relacionadas con usuarios y espacios en Firestore.
/// </summary>
public class UserService
{
    private readonly ILogger<UserService> _logger;
    private readonly FirestoreDb _db;

    // Único constructor: recibe Logger y FirestoreDb del contenedor DI
    public UserService(ILogger<UserService> logger, FirestoreDb db)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

/// <summary>
/// Obtiene un espacio por su identificador.
/// </summary>
/// <param name="espacioId">Identificador del espacio.</param>
/// <returns>Instancia de <see cref="Espacio"/> si existe, o null si no se encuentra.</returns>
public async Task<Espacio?> GetEspacioByIdAsync(string espacioId)
    {
        var docRef = _db.Collection("espacios").Document(espacioId);
        var snapshot = await docRef.GetSnapshotAsync();
        return snapshot.Exists ? snapshot.ConvertTo<Espacio>() : null;
    }

    /// <summary>
    /// Obtiene la lista de usuarios asociados a un espacio.
    /// </summary>
    /// <param name="espacioId">Identificador del espacio.</param>
    /// <returns>Lista de <see cref="UsuarioEspacio"/> asociados al espacio.</returns>
    public async Task<List<UsuarioEspacio>> GetUsuariosDelEspacioAsync(string espacioId)
    {
        var espacio = await GetEspacioByIdAsync(espacioId);
        return espacio?.UsuarioEspacios ?? new List<UsuarioEspacio>();
    }

    /// <summary>
    /// Obtiene la lista de salas asociadas a un espacio.
    /// </summary>
    /// <param name="espacioId">Identificador del espacio.</param>
    /// <returns>Lista de <see cref="Sala"/> asociadas al espacio.</returns>
    public async Task<List<Sala>> GetSalasDelEspacioAsync(string espacioId)
    {
        var espacio = await GetEspacioByIdAsync(espacioId);
        return espacio?.Salas ?? new List<Sala>();
    }

    /// <summary>
    /// Inserta datos de prueba en Firestore a partir de un archivo JSON.
    /// El JSON debe tener colecciones como propiedades y documentos como objetos.
    /// </summary>
    /// <returns>True si la inserción fue exitosa, false si hubo errores.</returns>
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

                    // Convierte referencias tipo "Coleccion/id" en DocumentReference
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

    /// <summary>
    /// Convierte referencias en los datos a instancias de DocumentReference si corresponde.
    /// </summary>
    /// <param name="data">Datos a convertir.</param>
    /// <returns>Datos convertidos con referencias de Firestore.</returns>
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

    /// <summary>
    /// Convierte un elemento JSON a tipos nativos de C#.
    /// </summary>
    /// <param name="element">Elemento JSON a convertir.</param>
    /// <returns>Objeto nativo equivalente.</returns>
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

    // conseguir usuarios por espacio id
    public async Task<List<UsuarioEspacioResponse>?> ObtenerUsuariosPorEspacioId(string espacioId)
    {
        if (string.IsNullOrWhiteSpace(espacioId))
            return null;

        espacioId = espacioId.Trim().Trim('\"');

        var espacioSnap = await _db
            .Collection("espacios")
            .Document(espacioId)
            .GetSnapshotAsync();

        if (!espacioSnap.Exists)
            return null;

        var referencias = espacioSnap.GetValue<List<DocumentReference>?>("usuarioEspacios");
        if (referencias == null || referencias.Count == 0)
            return null;

        var fetchTasks = referencias
            .Select(r => r.GetSnapshotAsync())
            .ToList();

        var userSnaps = await Task.WhenAll(fetchTasks);

        var usuarios = userSnaps
            .Where(s => s.Exists)
            .Select(s => s.ConvertTo<UsuarioEspacio>())
            .Select(u => u.ToResponse())
            .ToList();

        return usuarios.Count > 0 ? usuarios : null;
    }

}