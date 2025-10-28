using AuthApiDemo.Controllers;
using AuthApiDemo.Endpoints;
using AuthApiDemo.Infraestructure;
using AuthApiDemo.Services;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;


var builder = WebApplication.CreateBuilder(args);

builder.Logging
       .ClearProviders()
       .AddConsole()
       .AddDebug();

// 2) FirestoreDb como singleton
builder.Services.AddSingleton(provider =>
    FirestoreDb.Create("convivia-862f2") // tu ID de proyecto
);

// Add services to the container.

FirebaseConfig.InitializeFirebase();
builder.Services.AddScoped<IFirebaseService, FirebaseService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<TareaService>();
builder.Services.AddScoped<EspacioService>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<InvitacionService>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Comentado porque no vamos a usar https de momento, necesitamos certifiación
//app.UseHttpsRedirection();

app.UseAuthorization();

// Endpoints
app.MapControllers();
app.MapEspacioEndpoints();
app.MapPeticionEndpoints();
app.MapInvitacionEndpoints();
// app.MapUsuarioEndpoints();
// Endpoint para introducir datos de prueba
app.MapPost("/api/usuarios/importar-datos", async (UserService userService) =>
{
    var ok = await userService.ProbarConexionAsync();
    return ok ? Results.Ok("Datos importados correctamente") : Results.Problem("Error al importar datos");
});

app.Run();
