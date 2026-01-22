using Convivia.API.Controllers;
using Convivia.API.Middleware;
using Convivia.Application.Extensions;
using Convivia.Application.Mappers;
using Convivia.Infrastructure.Extensions;
using Convivia.Infrastructure.Infraestructure;
using Convivia.Infrastructure.Repositories;
using Convivia.Shared.DTOs;
using Google.Cloud.Firestore;
using Mapster;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// logging, Firebase init y FirestoreDb singleton (tu cÃ³digo actual)
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
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructure(builder.Configuration);

// Registrar capas (antes de Build)
builder.Services.AddApplicationServices();               // registra InvitacionService, mappers, etc.
builder.Services.AddInfrastructure(builder.Configuration); // registra IInvitacionRepository, IFirebaseService, FirebaseService, etc.

// Controllers y Swagger - DO NOT add JsonStringEnumConverter here so enums are serialized as numbers (defaults)
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Necesario para acceder a HttpContext desde servicios (CorrelationProvider, etc.)
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Middleware y mapeo de endpoints
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
// app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

//app.MapEspacioEndpoints();
// Middlewares: orden crítico
// 1) CorrelationId debe ejecutarse lo más temprano posible para que el resto del pipeline lo vea.
// 2) ExceptionHandling debe venir inmediatamente después para capturar y reutilizar el correlation id.
app.UseCorrelationId(); // 1. CorrelationId
app.UseMiddleware<ExceptionHandlingMiddleware>(); // 2. Exception handling middleware

//app.MapSalaEndpoints();
// app.MapInvitacionEndpoints(); // descomenta cuando estÃ© listo

app.Run();

public partial class Program { }