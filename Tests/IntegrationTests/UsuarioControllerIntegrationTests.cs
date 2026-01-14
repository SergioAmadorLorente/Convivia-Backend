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
    /// Pruebas de integración para UsuarioController.
    /// Ahora se conecta a Firestore real (BD de prueba: convivia-testing).
    /// </summary>
    public class UsuarioControllerIntegrationTests : IClassFixture<ConviviaWebApplicationFactory>, IAsyncLifetime
    {
        private readonly ConviviaWebApplicationFactory _factory;
        private readonly HttpClient _client;
        private readonly List<string> _createdUserIds = new();

        public UsuarioControllerIntegrationTests(ConviviaWebApplicationFactory factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _client = _factory.CreateClient();
        }

        /// <summary>
        /// Inicialización antes de cada clase de tests (no se usa en este caso).
        /// </summary>
        public Task InitializeAsync() => Task.CompletedTask;

        /// <summary>
        /// Limpieza después de todos los tests de esta clase.
        /// Elimina los usuarios creados durante las pruebas.
        /// </summary>
        public async Task DisposeAsync()
        {
            foreach (var userId in _createdUserIds)
            {
                try
                {
                    var endpoint = $"/api/usuario/{userId}";
                    await _client.DeleteAsync(endpoint);
                }
                catch (Exception ex)
                {
                    // Log silencioso: no queremos fallar la limpieza si una eliminación falla
                    System.Diagnostics.Debug.WriteLine($"Error al eliminar usuario {userId}: {ex.Message}");
                }
            }
        }

        [Fact]
        public async Task Create_WithValidData_ReturnsOk()
        {
            // Arrange
            var endpoint = "/api/usuario";
            var createDto = new CreateUsuarioDto
            {
                Nombre = "Test Usuario",
                Email = "test@example.com",
                Password = "password123"
            };

            // Act
            var response = await _client.PostAsJsonAsync(endpoint, createDto);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created);

            // Si la creación fue exitosa, registrar el ID para limpieza
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var result = await response.Content.ReadFromJsonAsync<CreateUsuarioDto>();
                    if (result?.Email != null)
                    {
                        // Obtener el usuario para conseguir su ID
                        var getResponse = await _client.GetAsync($"/api/usuario/correo/{result.Email}");
                        if (getResponse.IsSuccessStatusCode)
                        {
                            var userDto = await getResponse.Content.ReadFromJsonAsync<dynamic>();
                            if (userDto?["Id"] != null)
                                _createdUserIds.Add(userDto["Id"].ToString());
                        }
                    }
                }
                catch { /* Ignorar errores al obtener el ID */ }
            }
        }

        [Fact]
        public async Task Create_WithMissingNombre_ReturnsBadRequest()
        {
            // Arrange
            var endpoint = "/api/usuario";
            var createDto = new CreateUsuarioDto
            {
                Nombre = "",
                Email = "test@example.com",
                Password = "password123"
            };

            // Act
            var response = await _client.PostAsJsonAsync(endpoint, createDto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Create_WithMissingEmail_ReturnsBadRequest()
        {
            // Arrange
            var endpoint = "/api/usuario";
            var createDto = new CreateUsuarioDto
            {
                Nombre = "Test Usuario",
                Email = "",
                Password = "password123"
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
            var endpoint = "/api/usuario";

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
            var endpoint = $"/api/usuario/{invalidId}";

            // Act
            var response = await _client.GetAsync(endpoint);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetByEmail_WithInvalidEmail_ReturnsNotFound()
        {
            // Arrange
            var invalidEmail = "nonexistent@example.com";
            var endpoint = $"/api/usuario/correo/{invalidEmail}";

            // Act
            var response = await _client.GetAsync(endpoint);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetByEmail_WithEmptyEmail_ReturnsBadRequest()
        {
            // Arrange
            var endpoint = $"/api/usuario/correo/";

            // Act
            var response = await _client.GetAsync(endpoint);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PutOverwrite_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var invalidId = "nonexistent-id";
            var endpoint = $"/api/usuario/{invalidId}";
            var updateDto = new UpdateUsuarioDto
            {
                Nombre = "Updated Usuario"
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
            var endpoint = $"/api/usuario/{invalidId}/merge";
            var updateDto = new UpdateUsuarioDto
            {
                Nombre = "Updated Usuario"
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
            var endpoint = $"/api/usuario/{invalidId}";
            var updateDto = new UpdateUsuarioDto
            {
                Nombre = "Updated Usuario"
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
            var endpoint = $"/api/usuario/{invalidId}";

            // Act
            var response = await _client.DeleteAsync(endpoint);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
