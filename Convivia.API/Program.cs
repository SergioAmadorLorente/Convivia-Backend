using Convivia.API.Controllers;
using Convivia.API.Middleware;
using Convivia.Application.Extensions;
using Convivia.Application.Mappers;
using Convivia.Application.Services;
using Convivia.Infrastructure.Extensions;
using Convivia.Infrastructure.Infraestructure;
using Convivia.Infrastructure.Repositories;
using Convivia.Shared.DTOs;
using Google.Cloud.Firestore;
using Mapster;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using System.Text.Json.Serialization;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.AspNetCore.Hosting", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message:lj} CorrelationId={CorrelationId} RequestId={RequestId}{NewLine}{Exception}")
    .CreateLogger();


var builder = WebApplication.CreateBuilder(args);

// logging, Firebase init y FirestoreDb singleton (tu cÃ³digo actual)
// Registrar Serilog en el host
builder.Host.UseSerilog();

// Servicios y configuración
builder.Logging.ClearProviders().AddConsole().AddDebug();
FirebaseConfig.InitializeFirebase();

builder.Services.AddSingleton(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var projectId = config["Firebase:ProjectId"] ?? Environment.GetEnvironmentVariable("FIREBASE_PROJECT_ID");
    if (string.IsNullOrWhiteSpace(projectId)) throw new InvalidOperationException("Falta Firebase ProjectId.");
    return FirestoreDb.Create(projectId);
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

// Registrar servicios que faltaban explÃ­citamente (asegura que la implementaciÃ³n exista)
builder.Services.AddScoped<IUsuarioEspacioService, UsuarioEspacioService>();

// Controllers y Swagger - DO NOT add JsonStringEnumConverter here so enums are serialized as numbers (defaults)
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

var app = builder.Build();

// Middleware y mapeo de endpoints
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
// app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

//app.MapEspacioEndpoints();
// Middlewares: orden crÃ­tico
// 1) CorrelationId debe ejecutarse lo mÃ¡s temprano posible para que el resto del pipeline lo vea.
// 2) ExceptionHandling debe venir inmediatamente despuÃ©s para capturar y reutilizar el correlation id.
app.UseCorrelationId(); // 1. CorrelationId
app.UseMiddleware<ExceptionHandlingMiddleware>(); // 2. Exception handling middleware

//app.MapSalaEndpoints();
// app.MapInvitacionEndpoints(); // descomenta cuando estÃ© listo

app.Run();

public partial class Program { }
// Pipeline de middlewares (orden crítico)

// 1) Middleware inline que garantiza CorrelationId desde el inicio y lo empuja al LogContext.
app.Use(async (context, next) =>
{
    const string headerName = "X-Correlation-ID";

    var incoming = context.Request.Headers[headerName].ToString();
    string correlationId = !string.IsNullOrWhiteSpace(incoming) && Guid.TryParse(incoming, out _)
        ? incoming
        : Guid.NewGuid().ToString();

    // Exponer inmediatamente para que otros middlewares lo lean
    context.Items["CorrelationId"] = correlationId;

    // Establecer TraceIdentifier para que SerilogRequestLogging (y otros) puedan usarlo como RequestId
    context.TraceIdentifier = correlationId;

    // Poner cabecera de respuesta de forma inmediata
    context.Response.Headers[headerName] = correlationId;

    // Empujar al LogContext para que Serilog lo vea desde el inicio del request
    using (LogContext.PushProperty("CorrelationId", correlationId))
    {
        await next();
    }
});


app.Use(async (context, next) =>
{
    var sw = System.Diagnostics.Stopwatch.StartNew();

    // Intentar obtener CorrelationId (ya lo puso el middleware anterior)
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

        // Log de excepción (ya lo captura tu ExceptionHandlingMiddleware, pero lo dejamos por seguridad)
        Log.Error(ex, "Request error {Method} {Path}{Query} after {Elapsed:0.0000} ms CorrelationId={CorrelationId} RequestId={RequestId}",
            method, path, query, sw.Elapsed.TotalMilliseconds, correlationId, context.TraceIdentifier);

        throw;
    }
});



// 3) Middleware de manejo de excepciones (debe venir después para poder leer el CorrelationId)
app.UseMiddleware<ExceptionHandlingMiddleware>();

// 4) Routing y auth
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

try
{
    app.Run();
}
finally
{
    Log.CloseAndFlush();
}
