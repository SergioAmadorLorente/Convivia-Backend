using Convivia.API.Middleware;
using Convivia.Application.Extensions;
using Convivia.Infrastructure.Extensions;
using Convivia.Infrastructure.Infraestructure;
using Convivia.Infrastructure.Queues;
using Convivia.Infrastructure.HostedServices;
using Convivia.Shared.Contracts;
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
builder.Services.AddScoped<MapsterMapper.IMapper, MapsterMapper.ServiceMapper>();
builder.Services.AddApplicationServices();
builder.Services.AddSingleton<IErrorQueue>(sp => new InMemoryErrorQueue(capacity: 1000));
builder.Services.AddHostedService<TestErrorConsumer>();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Middlewares: orden recomendado
app.UseCorrelationId(); // 1. CorrelationId
app.UseMiddleware<ExceptionHandlingMiddleware>();

// 2. Exception handling middleware (implementar y registrar)
// app.UseMiddleware<ExceptionHandlingMiddleware>();

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
