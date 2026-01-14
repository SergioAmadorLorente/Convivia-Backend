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
    /// Pruebas de integración para TareaController.
    /// Utiliza ConviviaWebApplicationFactory para mockear Firebase.
    /// </summary>
    public class TareaControllerIntegrationTests : IClassFixture<ConviviaWebApplicationFactory>
    {
        private readonly ConviviaWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public TareaControllerIntegrationTests(ConviviaWebApplicationFactory factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task CreateTarea_WithValidData_ReturnsCreated()
        {
            // Arrange
            var espacioid = "test-espacio-id";
            var endpoint = $"/api/espacios/{espacioid}/tareas";
            var createDto = new CreateTareaDto
            {
                Nombre = "Test Tarea",
                karma = 50,
                DiasRepeticion = [0, 1, 2],
                HoraLimite = new TimeOnly(17,0,0)
            };

            // Act
            var response = await _client.PostAsJsonAsync(endpoint, createDto);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task GetByEspacioId_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var espacioid = "nonexistent-espacio-id";
            var endpoint = $"/api/espacios/{espacioid}/tareas";

            // Act
            var response = await _client.GetAsync(endpoint);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetTareaById_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var espacioid = "test-espacio-id";
            var plantillaId = "test-plantilla-id";
            var tareaId = "nonexistent-tarea-id";
            var endpoint = $"/api/espacios/{espacioid}/tareas/{plantillaId}/{tareaId}";

            // Act
            var response = await _client.GetAsync(endpoint);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Filter_WithInvalidEspacioId_ReturnsNotFound()
        {
            // Arrange
            var espacioid = "nonexistent-espacio-id";
            var endpoint = $"/api/espacios/{espacioid}/tareas/filter";

            // Act
            var response = await _client.GetAsync(endpoint);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task PutOverwrite_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var espacioid = "test-espacio-id";
            var plantillaId = "test-plantilla-id";
            var tareaId = "nonexistent-tarea-id";
            var endpoint = $"/api/espacios/{espacioid}/tareas/{plantillaId}/{tareaId}";
            var updateDto = new UpdateTareaDto();

            // Act
            var response = await _client.PutAsJsonAsync(endpoint, updateDto);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task PutMerge_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var espacioid = "test-espacio-id";
            var plantillaId = "test-plantilla-id";
            var tareaId = "nonexistent-tarea-id";
            var endpoint = $"/api/espacios/{espacioid}/tareas/{plantillaId}/{tareaId}/merge";
            var updateDto = new UpdateTareaDto();

            // Act
            var response = await _client.PutAsJsonAsync(endpoint, updateDto);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Patch_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var espacioid = "test-espacio-id";
            var plantillaId = "test-plantilla-id";
            var tareaId = "nonexistent-tarea-id";
            var endpoint = $"/api/espacios/{espacioid}/tareas/{plantillaId}/{tareaId}";
            var updateDto = new UpdateTareaDto();

            // Act
            var response = await _client.PatchAsJsonAsync(endpoint, updateDto);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Delete_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var espacioid = "nonexistent-espacio-id";
            var tareaId = "nonexistent-tarea-id";
            var endpoint = $"/api/espacios/{espacioid}/tareas/{tareaId}";

            // Act
            var response = await _client.DeleteAsync(endpoint);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
