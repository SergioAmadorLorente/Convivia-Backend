using Convivia.API.Middleware;
using Convivia.Application.Extensions;
using Convivia.Infrastructure.Extensions;
using Convivia.Infrastructure.Infraestructure;
using Convivia.Infrastructure.Correlation;
using Google.Cloud.Firestore;
using Mapster;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// servicios, logging, Firebase, Firestore singleton...
builder.Logging.ClearProviders().AddConsole().AddDebug();
FirebaseConfig.InitializeFirebase();
builder.Services.AddSingleton(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var projectId = config["Firebase:ProjectId"] ?? Environment.GetEnvironmentVariable("FIREBASE_PROJECT_ID");
    if (string.IsNullOrWhiteSpace(projectId)) throw new InvalidOperationException("Falta Firebase ProjectId.");
    return FirestoreDb.Create(projectId);
});

// otros servicios...
builder.Services.AddMemoryCache();
builder.Services.AddMapster();
builder.Services.AddSingleton(TypeAdapterConfig.GlobalSettings);
builder.Services.AddSingleton<MapsterMapper.IMapper, MapsterMapper.ServiceMapper>();
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Necesario para acceder a HttpContext desde servicios (CorrelationProvider, etc.)
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICorrelationProvider, CorrelationProvider>();

var app = builder.Build();

// Middlewares: orden crítico
// 1) CorrelationId debe ejecutarse lo más temprano posible para que el resto del pipeline lo vea.
// 2) ExceptionHandling debe venir inmediatamente después para capturar y reutilizar el correlation id.
app.UseCorrelationId(); // 1. CorrelationId
app.UseMiddleware<ExceptionHandlingMiddleware>(); // 2. Exception handling middleware

app.UseRouting();       // 3. Routing
app.UseAuthentication();// 4. Auth
app.UseAuthorization(); // 5. Authorization

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();   // 6. Endpoints

app.Run();
