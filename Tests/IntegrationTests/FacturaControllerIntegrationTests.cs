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
    /// Pruebas de integración para FacturaController.
    /// Ahora se conecta a Firestore real (BD de prueba: convivia-testing).
    /// </summary>
    public class FacturaControllerIntegrationTests : IClassFixture<ConviviaWebApplicationFactory>, IAsyncLifetime
    {
        private readonly ConviviaWebApplicationFactory _factory;
        private readonly HttpClient _client;
        private readonly List<string> _createdFacturaIds = new();

        public FacturaControllerIntegrationTests(ConviviaWebApplicationFactory factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _client = _factory.CreateClient();
        }

        /// <summary>
        /// Inicialización antes de cada clase de tests.
        /// </summary>
        public Task InitializeAsync() => Task.CompletedTask;

        /// <summary>
        /// Limpieza después de todos los tests de esta clase.
        /// Elimina las facturas creadas durante las pruebas.
        /// </summary>
        public async Task DisposeAsync()
        {
            foreach (var facturaId in _createdFacturaIds)
            {
                try
                {
                    var endpoint = $"/api/factura/{facturaId}";
                    await _client.DeleteAsync(endpoint);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error al eliminar factura {facturaId}: {ex.Message}");
                }
            }
        }

        [Fact]
        public async Task Create_WithValidData_ReturnsCreated()
        {
            // Arrange
            var endpoint = "/api/factura";
            var createDto = new CreateFacturaDto
            {
                Nombre = "Test Factura",
                Precio = 100.50m,
                Pagado = false,
                Reparto = new Dictionary<string, float> { { "user1", 100.50f } }
            };

            // Act
            var response = await _client.PostAsJsonAsync(endpoint, createDto);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK);

            // Registrar ID para limpieza si fue creado exitosamente
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var result = await response.Content.ReadFromJsonAsync<FacturaDto>();
                    if (!string.IsNullOrEmpty(result?.Id))
                        _createdFacturaIds.Add(result.Id);
                }
                catch { /* Ignorar errores */ }
            }
        }

        [Fact]
        public async Task Create_WithMissingNombre_ReturnsBadRequest()
        {
            // Arrange
            var endpoint = "/api/factura";
            var createDto = new CreateFacturaDto
            {
                Nombre = "",
                Precio = 100.50m
            };

            // Act
            var response = await _client.PostAsJsonAsync(endpoint, createDto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Create_WithNegativePrice_ReturnsBadRequest()
        {
            // Arrange
            var endpoint = "/api/factura";
            var createDto = new CreateFacturaDto
            {
                Nombre = "Test Factura",
                Precio = -50m
            };

            // Act
            var response = await _client.PostAsJsonAsync(endpoint, createDto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetAll_ReturnsOk()
        {
            // Arrange
            var endpoint = "/api/factura";

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
            var endpoint = $"/api/factura/{invalidId}";

            // Act
            var response = await _client.GetAsync(endpoint);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task PutOverwrite_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var invalidId = "nonexistent-id";
            var endpoint = $"/api/factura/{invalidId}";
            var updateDto = new UpdateFacturaDto
            {
                Nombre = "Updated Factura",
                Precio = 150.00m
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
            var endpoint = $"/api/factura/{invalidId}/merge";
            var updateDto = new UpdateFacturaDto
            {
                Nombre = "Updated Factura"
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
            var endpoint = $"/api/factura/{invalidId}";
            var updateDto = new UpdateFacturaDto
            {
                Nombre = "Updated Factura"
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
            var endpoint = $"/api/factura/{invalidId}";

            // Act
            var response = await _client.DeleteAsync(endpoint);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
