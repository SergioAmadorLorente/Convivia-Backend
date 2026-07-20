using Convivia.API.Controllers;
using Convivia.API.Middleware;
using Convivia.Application.Extensions;
using Convivia.Application.Mappers;
using Convivia.Application.Services;
using Convivia.Infrastructure.Extensions;
using Convivia.Infrastructure.Infraestructure;
using Convivia.Infrastructure.Correlation;
using Convivia.Shared.Correlation;
using Convivia.Infrastructure.Repositories;
using Convivia.Shared.DTOs;
using Google.Cloud.Firestore;
using Mapster;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using System.Text.Json.Serialization;
using Google.Api.Gax.Grpc;
using Google.Cloud.Firestore.V1;
using Google.Apis.Auth.OAuth2;


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.AspNetCore.Hosting", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    // esta la comento o me peta INVESTIGAR .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message:lj} CorrelationId={CorrelationId} RequestId={RequestId}{NewLine}{Exception}")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Registrar Serilog en el host
//builder.Host.UseSerilog();   esta la comento o me peta INVESTIGAR

// Servicios y configuración
builder.Logging.ClearProviders().AddConsole().AddDebug();
FirebaseConfig.InitializeFirebase();

builder.Services.AddSingleton(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();

    var projectId = config["Firebase:ProjectId"]
        ?? Environment.GetEnvironmentVariable("FIREBASE_PROJECT_ID");

    if (string.IsNullOrWhiteSpace(projectId))
        throw new InvalidOperationException("Falta Firebase ProjectId.");

    GoogleCredential credential;

    var json = Environment.GetEnvironmentVariable("FIREBASE_CREDENTIALS_JSON");

    if (!string.IsNullOrWhiteSpace(json))
    {
        credential = GoogleCredential.FromJson(json);
    }
    else
    {
        credential = GoogleCredential.GetApplicationDefault();
    }

    var client = new FirestoreClientBuilder
    {
        Credential = credential
    }.Build();

    return FirestoreDb.Create(projectId, client);
});

// Registrar MemoryCache
builder.Services.AddMemoryCache();

// Registrar Mapster antes de los servicios que lo usan
builder.Services.AddMapster();
builder.Services.AddSingleton(TypeAdapterConfig.GlobalSettings);
builder.Services.AddSingleton<MapsterMapper.IMapper, MapsterMapper.ServiceMapper>();

// Registrar capas (una sola vez)
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructure(builder.Configuration);

// Registrar servicios que faltaban explícitamente (asegura que la implementación exista)
builder.Services.AddScoped<IUsuarioEspacioService, UsuarioEspacioService>();

// Controllers y Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Habilitar comentarios XML del proyecto API
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
    // Habilitar comentarios XML del proyecto DTOs
    var sharedXmlPath = Path.Combine(AppContext.BaseDirectory, "Convivia.Shared.xml");
    if (File.Exists(sharedXmlPath))
        c.IncludeXmlComments(sharedXmlPath);
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICorrelationProvider, CorrelationProvider>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors("AllowAll");

// Middlewares y mapeo de endpoints
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection(); // descomentar si usas HTTPS en dev

// Middlewares: orden crítico
// 1) CorrelationId debe ejecutarse lo más temprano posible para que el resto del pipeline lo vea.
// 2) ExceptionHandling debe venir inmediatamente después para capturar y reutilizar el correlation id.
app.UseCorrelationId(); // 1. CorrelationId (tu middleware de clase)
app.UseMiddleware<ExceptionHandlingMiddleware>(); // 2. Exception handling middleware

// Middleware de logging de petición (opcional, mantiene trazas de inicio/fin)
app.Use(async (context, next) =>
{
    var sw = System.Diagnostics.Stopwatch.StartNew();

    // Intentar obtener CorrelationId (ya lo puso el middleware UseCorrelationId)
    var correlationId = context.Items["CorrelationId"]?.ToString() ?? context.TraceIdentifier ?? string.Empty;

    // Datos básicos de la petición
    var method = context.Request?.Method;
    var path = context.Request?.Path.Value;
    var query = context.Request?.QueryString.Value;

    // Log de inicio de petición
    Log.Information("Request starting {Method} {Path}{Query} CorrelationId={CorrelationId} RequestId={RequestId}",
        method, path, query, correlationId, context.TraceIdentifier);

    try
    {
        await next();
        sw.Stop();

        // Log de respuesta (éxito)
        Log.Information("Request finished {Method} {Path}{Query} responded {StatusCode} in {Elapsed:0.0000} ms CorrelationId={CorrelationId} RequestId={RequestId}",
            method, path, query, context.Response?.StatusCode, sw.Elapsed.TotalMilliseconds, correlationId, context.TraceIdentifier);
    }
    catch (Exception ex)
    {
        sw.Stop();

        // Log de excepción (ExceptionHandlingMiddleware también lo captura)
        Log.Error(ex, "Request error {Method} {Path}{Query} after {Elapsed:0.0000} ms CorrelationId={CorrelationId} RequestId={RequestId}",
            method, path, query, sw.Elapsed.TotalMilliseconds, correlationId, context.TraceIdentifier);

        throw;
    }
});

// 4) Routing y auth
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Ejecutar la app y asegurar cierre de Serilog
try
{
    app.Run();
}
finally
{
    Log.CloseAndFlush();
}

// Declaración para permitir WebApplicationFactory en tests
public partial class Program { }
