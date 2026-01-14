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
    /// Pruebas de integración para PermisosController.
    /// Utiliza ConviviaWebApplicationFactory para mockear Firebase.
    /// </summary>
    public class PermisosControllerIntegrationTests : IClassFixture<ConviviaWebApplicationFactory>
    {
        private readonly ConviviaWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public PermisosControllerIntegrationTests(ConviviaWebApplicationFactory factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task Create_WithValidData_ReturnsCreated()
        {
            // Arrange
            var endpoint = "/api/permisos";
            var createDto = new CreatePermisoDto
            {
                Rol = TipoRol.Usuario
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
            var endpoint = "/api/permisos";

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
            var endpoint = $"/api/permisos/{invalidId}";

            // Act
            var response = await _client.GetAsync(endpoint);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetByRol_WithValidRol_ReturnsOk()
        {
            // Arrange
            var rol = TipoRol.Usuario;
            var endpoint = $"/api/permisos/rol/{rol}";

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
            var endpoint = $"/api/permisos/{invalidId}";
            var updateDto = new UpdatePermisoDto();

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
            var endpoint = $"/api/permisos/{invalidId}/merge";
            var updateDto = new UpdatePermisoDto();

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
            var endpoint = $"/api/permisos/{invalidId}";
            var updateDto = new UpdatePermisoDto();

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
            var endpoint = $"/api/permisos/{invalidId}";

            // Act
            var response = await _client.DeleteAsync(endpoint);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
