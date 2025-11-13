using AuthApiDemo.Controllers;
using AuthApiDemo.Endpoints;
using AuthApiDemo.Infraestructure;
using AuthApiDemo.Services;

using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;

var builder = WebApplication.CreateBuilder(args);

// Configuración de logging
builder.Logging
       .ClearProviders()
       .AddConsole()
       .AddDebug();

// FirestoreDb como singleton
builder.Services.AddSingleton(provider =>
    FirestoreDb.Create("convivia-862f2") // tu ID de proyecto
);

// Inicializar Firebase
FirebaseConfig.InitializeFirebase();

// Registro de servicios en el contenedor de dependencias
builder.Services.AddScoped<IFirebaseService, FirebaseService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<TareaService>();
builder.Services.AddScoped<EspacioService>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<InvitacionService>();
builder.Services.AddScoped<SalaService>(); // 👈 Faltaba este

// Controllers y Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Middleware de desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Si quieres HTTPS, descomenta esta línea
// app.UseHttpsRedirection();

app.UseAuthorization();

// Mapear controladores y endpoints minimal API
app.MapControllers();
app.MapEspacioEndpoints();
app.MapPeticionEndpoints();
app.MapInvitacionEndpoints();
app.MapSalaEndpoints();

// Endpoint de prueba para importar datos
app.MapPost("/api/usuarios/importar-datos", async (UserService userService) =>
{
    var ok = await userService.ProbarConexionAsync();
    return ok ? Results.Ok("Datos importados correctamente")
              : Results.Problem("Error al importar datos");
});

app.Run();