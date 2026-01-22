using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Convivia.Shared.Services;
using Convivia.Infrastructure.Infraestructure;
using Convivia.Infrastructure.Services;
using Google.Cloud.Firestore;
using System.Threading;

namespace Convivia.Tests.IntegrationTests.Fixtures
{
    /// <summary>
    /// WebApplicationFactory personalizado que se conecta a Firestore real (BD de prueba).
    /// Levanta la app en entorno "Test" con appsettings.Test.json.
    /// Las credenciales se leen desde serviceAccount.json (convivia-testing).
    /// </summary>
    public class ConviviaWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Test");

            builder.ConfigureServices(services =>
            {

                var firestoreDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(FirestoreDb));
                if (firestoreDescriptor != null)
                {
                    services.Remove(firestoreDescriptor);
                }

                var firebaseServiceDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IFirebaseService));
                if (firebaseServiceDescriptor != null)
                {
                    services.Remove(firebaseServiceDescriptor);
                }

                FirebaseConfig.InitializeFirebase();

                services.AddSingleton(provider =>
                {
                    var config = provider.GetRequiredService<IConfiguration>();
                    var projectId = config["Firebase:ProjectId"] ?? "convivia-862f2";
                    return FirestoreDb.Create(projectId);
                });

                services.AddScoped<IFirebaseService, FirebaseService>();
            });

            builder.ConfigureAppConfiguration((ctx, config) =>
            {
                config.AddJsonFile("appsettings.Test.json", optional: true);
            });
        }
    }
}
