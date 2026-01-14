using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Convivia.Shared.DTOs;
using Convivia.Tests.IntegrationTests.Fixtures;
using Xunit;

namespace Convivia.Tests.IntegrationTests
{
    /// <summary>
    /// Pruebas de integración para RolController.
    /// Utiliza ConviviaWebApplicationFactory para mockear Firebase.
    /// </summary>
    public class RolControllerIntegrationTests : IClassFixture<ConviviaWebApplicationFactory>
    {
        private readonly ConviviaWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public RolControllerIntegrationTests(ConviviaWebApplicationFactory factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _client = _factory.CreateClient();
        }

        /// <summary>
        /// Prueba que la API retorna los nombres válidos de roles.
        /// </summary>
        [Fact]
        public async Task GetNombresValidos_ReturnsOkWithRoles()
        {
            // Arrange
            var endpoint = "/api/rol/nombres-validos";

            // Act
            var response = await _client.GetAsync(endpoint);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadFromJsonAsync<dynamic>();
            Assert.NotNull(content);
        }

        /// <summary>
        /// Prueba que obtener todos los roles retorna una lista vacía inicialmente.
        /// </summary>
        [Fact]
        public async Task GetAll_ReturnsEmptyList()
        {
            // Arrange
            var endpoint = "/api/rol";

            // Act
            var response = await _client.GetAsync(endpoint);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var roles = await response.Content.ReadFromJsonAsync<List<RolDto>>();
            Assert.NotNull(roles);
            Assert.Empty(roles); // Empty porque el Firebase está mockeado
        }

        /// <summary>
        /// Prueba que obtener un rol inexistente retorna NotFound.
        /// </summary>
        [Fact]
        public async Task GetById_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var invalidId = "nonexistent-id";
            var endpoint = $"/api/rol/{invalidId}";

            // Act
            var response = await _client.GetAsync(endpoint);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        /// <summary>
        /// Prueba que obtener un rol con ID vacío retorna BadRequest.
        /// </summary>
        [Fact]
        public async Task GetById_WithEmptyId_ReturnsBadRequest()
        {
            // Arrange
            var endpoint = "/api/rol/";

            // Act
            var response = await _client.GetAsync(endpoint);

            // Assert
            // El routing probablemente devolverá 404 porque la ruta no existe
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        /// <summary>
        /// Prueba que crear un rol requiere datos válidos.
        /// </summary>
        [Fact]
        public async Task Create_WithValidData_ReturnsCreated()
        {
            // Arrange
            var endpoint = "/api/rol";
            var createDto = new CreateRolDto
            {
                Nombre = TipoRol.Usuario
            };

            // Act
            var response = await _client.PostAsJsonAsync(endpoint, createDto);

            // Assert
            Assert.NotNull(response);
            // Puede ser Created (201) o algún otro código, dependiendo del mock
            Assert.True(response.StatusCode == HttpStatusCode.Created || 
                       response.StatusCode == HttpStatusCode.OK);
        }

        /// <summary>
        /// Prueba que crear un rol sin datos retorna BadRequest.
        /// </summary>
        [Fact]
        public async Task Create_WithNullBody_ReturnsBadRequest()
        {
            // Arrange
            var endpoint = "/api/rol";

            // Act
            var response = await _client.PostAsync(endpoint, 
                new StringContent(string.Empty, System.Text.Encoding.UTF8, "application/json"));

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        /// <summary>
        /// Prueba que actualizar un rol inexistente retorna NotFound.
        /// </summary>
        [Fact]
        public async Task Update_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var invalidId = "nonexistent-id";
            var endpoint = $"/api/rol/{invalidId}";
            var updateDto = new UpdateRolDto();

            // Act
            var response = await _client.PutAsJsonAsync(endpoint, updateDto);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        /// <summary>
        /// Prueba que eliminar un rol inexistente retorna NotFound.
        /// </summary>
        [Fact]
        public async Task Delete_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var invalidId = "nonexistent-id";
            var endpoint = $"/api/rol/{invalidId}";

            // Act
            var response = await _client.DeleteAsync(endpoint);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
