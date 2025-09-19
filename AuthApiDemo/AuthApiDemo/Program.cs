using AuthApiDemo.Endpoints;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<UserService>();


var app = builder.Build();

FirebaseApp.Create(new AppOptions()
{
    Credential = GoogleCredential.FromFile("Firebase/serviceAccount.json")
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//FirebaseApp.Create(new AppOptions()
//{
    //Credential = GoogleCredential.FromFile("Firebase/serviceAccount.json")
//});

// Comentado porque no vamos a usar https de momento, necesitamos certifiación
//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapEspacioEndpoints();

app.Run();
