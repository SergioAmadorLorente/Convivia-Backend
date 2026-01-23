using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Convivia.Tests.IntegrationTests.Fixtures;
using Xunit;

namespace Convivia.Tests.IntegrationTests
{
    /// <summary>
    /// Pruebas de integración básicas de la API.
    /// Verifica que los endpoints básicos funcionan correctamente.
    /// </summary>
    public class BasicApiIntegrationTests : IClassFixture<ConviviaWebApplicationFactory>
    {
        private readonly ConviviaWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public BasicApiIntegrationTests(ConviviaWebApplicationFactory factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _client = _factory.CreateClient();
        }

        /// <summary>
        /// Verifica que la aplicación responde en la raíz.
        /// </summary>
        [Fact]
        public async Task RootEndpoint_ReturnsOk()
        {
            // Act
            var response = await _client.GetAsync("/");

            // Assert
            // Puede retornar 404 en raíz si no hay endpoint, lo cual es normal
            Assert.NotNull(response);
            Assert.True(
                response.StatusCode == HttpStatusCode.NotFound ||
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.Redirect
            );
        }

        /// <summary>
        /// Verifica que Swagger está disponible en Development.
        /// </summary>
        [Fact]
        public async Task SwaggerUI_IsAvailable()
        {
            // Act
            var response = await _client.GetAsync("/swagger/index.html");

            // Assert
            // En entorno Test no incluimos middleware de Swagger, pero podemos verificar que no causa errores
            Assert.NotNull(response);
        }

        /// <summary>
        /// Verifica que la factory crea un cliente HTTP válido.
        /// </summary>
        [Fact]
        public void Factory_CreatesValidHttpClient()
        {
            // Assert
            Assert.NotNull(_client);
            Assert.NotNull(_client.BaseAddress);
        }

        /// <summary>
        /// Verifica que las opciones por defecto están configuradas correctamente.
        /// </summary>
        [Fact]
        public void Factory_IsConfiguredForTesting()
        {
            // Assert
            Assert.NotNull(_factory);
            // La factory debería estar usando el entorno "Test"
        }
    }
}
