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
    /// Pruebas de integración para EspacioController.
    /// Utiliza ConviviaWebApplicationFactory para mockear Firebase.
    /// </summary>
    public class EspacioControllerIntegrationTests : IClassFixture<ConviviaWebApplicationFactory>
    {
        private readonly ConviviaWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public EspacioControllerIntegrationTests(ConviviaWebApplicationFactory factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task Create_WithValidData_ReturnsCreated()
        {
            // Arrange
            var endpoint = "/api/espacio";
            var createDto = new CreateEspacioDto
            {
                Nombre = "Test Espacio"
            };

            // Act
            var response = await _client.PostAsJsonAsync(endpoint, createDto);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetAll_ReturnsOk()
        {
            // Arrange
            var endpoint = "/api/espacio";

            // Act
            var response = await _client.GetAsync(endpoint);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetById_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var invalidId = "nonexistent-id";
            var endpoint = $"/api/espacio/{invalidId}";

            // Act
            var response = await _client.GetAsync(endpoint);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetByDireccion_WithValidDireccion_ReturnsOk()
        {
            // Arrange
            var direccion = "Calle Principal 123";
            var endpoint = $"/api/espacio/por-direccion/{direccion}";

            // Act
            var response = await _client.GetAsync(endpoint);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task PutOverwrite_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var invalidId = "nonexistent-id";
            var endpoint = $"/api/espacio/{invalidId}";
            var updateDto = new UpdateEspacioDto
            {
                Nombre = "Updated Espacio"
            };

            // Act
            var response = await _client.PutAsJsonAsync(endpoint, updateDto);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task PutMerge_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var invalidId = "nonexistent-id";
            var endpoint = $"/api/espacio/{invalidId}/merge";
            var updateDto = new UpdateEspacioDto
            {
                Nombre = "Updated Espacio"
            };

            // Act
            var response = await _client.PutAsJsonAsync(endpoint, updateDto);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Patch_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var invalidId = "nonexistent-id";
            var endpoint = $"/api/espacio/{invalidId}";
            var updateDto = new UpdateEspacioDto
            {
                Nombre = "Updated Espacio"
            };

            // Act
            var response = await _client.PatchAsJsonAsync(endpoint, updateDto);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Delete_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var invalidId = "nonexistent-id";
            var endpoint = $"/api/espacio/{invalidId}";

            // Act
            var response = await _client.DeleteAsync(endpoint);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetInvitationCode_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var invalidId = "nonexistent-id";
            var endpoint = $"/api/espacio/{invalidId}/getCode";

            // Act
            var response = await _client.GetAsync(endpoint);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task JoinSpaceByCode_WithInvalidCode_ReturnsNotFound()
        {
            // Arrange
            var invalidCode = "invalid-code";
            var endpoint = $"/api/espacio/{invalidCode}/usuario";
            var joinDto = new JoinByCodeDto
            {
                UsuarioId = "test-user-id"
            };

            // Act
            var response = await _client.PostAsJsonAsync(endpoint, joinDto);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task JoinSpaceByCode_WithMissingUsuarioId_ReturnsBadRequest()
        {
            // Arrange
            var code = "test-code";
            var endpoint = $"/api/espacio/{code}/usuario";
            var joinDto = new JoinByCodeDto
            {
                UsuarioId = ""
            };

            // Act
            var response = await _client.PostAsJsonAsync(endpoint, joinDto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
