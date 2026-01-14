using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Convivia.Shared.DTOs;
using Convivia.Tests.IntegrationTests.Fixtures;
using Xunit;

namespace Convivia.Tests.IntegrationTests
{
    /// <summary>
    /// Pruebas de integración para PlantillaTareaController.
    /// Utiliza ConviviaWebApplicationFactory para mockear Firebase.
    /// </summary>
    public class PlantillaTareaControllerIntegrationTests : IClassFixture<ConviviaWebApplicationFactory>
    {
        private readonly ConviviaWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public PlantillaTareaControllerIntegrationTests(ConviviaWebApplicationFactory factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task Update_WithInvalidEspacioId_ReturnsNotFound()
        {
            // Arrange
            var espacioid = "nonexistent-espacio-id";
            var plantillaId = "nonexistent-plantilla-id";
            var endpoint = $"/api/espacio/{espacioid}/{plantillaId}";
            var updateDto = new UpdatePlantillaTareaDto
            {
                Nombre = "Updated Plantilla"
            };

            // Act
            var response = await _client.PatchAsJsonAsync(endpoint, updateDto);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

    }
}
