using Convivia.API.Endpoints;
using Convivia.Application.Extensions;
using Convivia.Infrastructure.Infraestructure;
using Convivia.Infrastructure.Extensions;
using Google.Cloud.Firestore;
 
var builder = WebApplication.CreateBuilder(args);

// logging, Firebase init y FirestoreDb singleton (tu código actual)
builder.Logging.ClearProviders().AddConsole().AddDebug();
FirebaseConfig.InitializeFirebase();

builder.Services.AddSingleton(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var projectId = config["Firebase:ProjectId"] ?? Environment.GetEnvironmentVariable("FIREBASE_PROJECT_ID");
    if (string.IsNullOrWhiteSpace(projectId)) throw new InvalidOperationException("Falta Firebase ProjectId.");
    return FirestoreDb.Create(projectId);
});

// Registrar capas (antes de Build)
builder.Services.AddApplicationServices();               // registra InvitacionService, mappers, etc.
builder.Services.AddInfrastructure(builder.Configuration); // registra IInvitacionRepository, IFirebaseService, FirebaseService, etc.

// Controllers y Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Middleware y mapeo de endpoints
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
// app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

//app.MapEspacioEndpoints();
app.MapPeticionEndpoints();

//app.MapSalaEndpoints();
// app.MapInvitacionEndpoints(); // descomenta cuando esté listo

app.Run();