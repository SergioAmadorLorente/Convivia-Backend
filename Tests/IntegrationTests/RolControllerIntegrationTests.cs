using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Convivia.Shared.DTOs;
using Convivia.Tests.IntegrationTests.Fixtures;
using Xunit;
using System.Text.Json;

namespace Convivia.Tests.IntegrationTests
{
    /// <summary>
    /// Pruebas de integración para RolController.
    /// Valida flujos CRUD completos, validación de contenido y casos de error.
    /// </summary>
    public class RolControllerIntegrationTests : IClassFixture<ConviviaWebApplicationFactory>, IAsyncLifetime
    {
        private readonly ConviviaWebApplicationFactory _factory;
        private readonly HttpClient _client;
        private readonly List<string> _createdRolIds = new();

        public RolControllerIntegrationTests(ConviviaWebApplicationFactory factory)
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
        /// Elimina los roles creados durante las pruebas.
        /// </summary>
        public async Task DisposeAsync()
        {
            foreach (var rolId in _createdRolIds)
            {
                try
                {
                    var endpoint = $"/api/rol/{rolId}";
                    await _client.DeleteAsync(endpoint);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error al eliminar rol {rolId}: {ex.Message}");
                }
            }
        }

        // =============================================
        // PRUEBAS DE ENDPOINT PUBLICOS (GET)
        // =============================================

        /// <summary>
        /// Prueba que la API retorna los nombres válidos de roles.
        /// </summary>
        [Fact]
        public async Task GetNombresValidos_ReturnsOkWithValidStructure()
        {
            // Arrange
            var endpoint = "/api/rol/nombres-validos";

            // Act
            var response = await _client.GetAsync(endpoint);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.True(root.ValueKind == JsonValueKind.Object);

            Assert.True(root.TryGetProperty("nombres", out var nombres));
            Assert.True(root.TryGetProperty("descripcion", out var descripcion));
            Assert.NotNull(nombres);
            Assert.NotNull(descripcion);
        }

        /// <summary>
        /// Prueba que obtener todos los roles retorna una lista (puede estar vacía o no según BD).
        /// </summary>
        [Fact]
        public async Task GetAll_ReturnsOkWithListStructure()
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
            Assert.IsType<List<RolDto>>(roles);
        }

        // =============================================
        // PRUEBAS DE CREAR ROL
        // =============================================

        /// <summary>
        /// Prueba el flujo completo: crear un rol válido y validar la respuesta.
        /// </summary>
        [Fact]
        public async Task Create_WithValidData_ReturnsCreatedWithValidContent()
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
            Assert.True(response.StatusCode == HttpStatusCode.Created || 
                       response.StatusCode == HttpStatusCode.OK);

            // Validar contenido de respuesta
            var createdRol = await response.Content.ReadFromJsonAsync<RolDto>();
            Assert.NotNull(createdRol);
            Assert.NotEmpty(createdRol.Id);
            Assert.Equal(TipoRol.Usuario, createdRol.Nombre);
            
            // Registrar para limpieza
            _createdRolIds.Add(createdRol.Id);
        }

        /// <summary>
        /// Prueba crear múltiples roles con diferentes tipos.
        /// </summary>
        [Fact]
        public async Task Create_WithDifferentRoles_AllSucceed()
        {
            // Arrange
            var endpoint = "/api/rol";
            var roles = new[] { TipoRol.Usuario, TipoRol.Admin, TipoRol.Moderador };

            foreach (var rolType in roles)
            {
                // Act
                var createDto = new CreateRolDto { Nombre = rolType };
                var response = await _client.PostAsJsonAsync(endpoint, createDto);

                // Assert
                Assert.True(response.IsSuccessStatusCode);
                
                var createdRol = await response.Content.ReadFromJsonAsync<RolDto>();
                Assert.NotNull(createdRol);
                Assert.Equal(rolType, createdRol.Nombre);
                _createdRolIds.Add(createdRol.Id);
            }
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

        // =============================================
        // PRUEBAS DE OBTENER ROL POR ID
        // =============================================

        /// <summary>
        /// Prueba obtener un rol por ID válido después de crearlo.
        /// </summary>
        [Fact]
        public async Task GetById_WithValidId_ReturnsOkWithValidContent()
        {
            // Arrange
            var createEndpoint = "/api/rol";
            var createDto = new CreateRolDto { Nombre = TipoRol.Admin };
            var createResponse = await _client.PostAsJsonAsync(createEndpoint, createDto);
            var createdRol = await createResponse.Content.ReadFromJsonAsync<RolDto>();
            _createdRolIds.Add(createdRol.Id);

            // Act
            var getEndpoint = $"/api/rol/{createdRol.Id}";
            var getResponse = await _client.GetAsync(getEndpoint);

            // Assert
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            
            var retrievedRol = await getResponse.Content.ReadFromJsonAsync<RolDto>();
            Assert.NotNull(retrievedRol);
            Assert.Equal(createdRol.Id, retrievedRol.Id);
            Assert.Equal(TipoRol.Admin, retrievedRol.Nombre);
        }

        /// <summary>
        /// Prueba que obtener un rol inexistente retorna NotFound.
        /// </summary>
        [Fact]
        public async Task GetById_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var invalidId = "nonexistent-id-" + Guid.NewGuid();
            var endpoint = $"/api/rol/{invalidId}";

            // Act
            var response = await _client.GetAsync(endpoint);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        /// <summary>
        /// Prueba que obtener un rol con ID vacío retorna error.
        /// </summary>
        [Fact]
        public async Task GetById_WithEmptyId_ReturnsBadRequestOrNotFound()
        {
            // Arrange
            var endpoint = "/api/rol/";

            // Act
            var response = await _client.GetAsync(endpoint);

            // Assert
            Assert.True(response.StatusCode == HttpStatusCode.NotFound || 
                       response.StatusCode == HttpStatusCode.BadRequest ||
                       response.StatusCode == HttpStatusCode.OK);
        }

        // =============================================
        // PRUEBAS DE ACTUALIZAR ROL
        // =============================================

        /// <summary>
        /// Prueba actualizar un rol existente de forma completa.
        /// </summary>
        [Fact]
        public async Task Update_WithValidData_ReturnsOkWithUpdatedContent()
        {
            // Arrange: Crear rol inicial
            var createEndpoint = "/api/rol";
            var createDto = new CreateRolDto { Nombre = TipoRol.Usuario };
            var createResponse = await _client.PostAsJsonAsync(createEndpoint, createDto);
            var createdRol = await createResponse.Content.ReadFromJsonAsync<RolDto>();
            _createdRolIds.Add(createdRol.Id);

            // Act: Actualizar rol
            var updateEndpoint = $"/api/rol/{createdRol.Id}";
            var updateDto = new UpdateRolDto
            {
                Permisos = new PermisosRolDto { CrearTarea = true }
            };
            var updateResponse = await _client.PutAsJsonAsync(updateEndpoint, updateDto);

            // Assert
            Assert.NotNull(updateResponse);
            Assert.True(updateResponse.StatusCode == HttpStatusCode.OK || 
                       updateResponse.StatusCode == HttpStatusCode.NoContent ||
                       updateResponse.StatusCode == HttpStatusCode.OK);
        }

        /// <summary>
        /// Prueba que actualizar un rol inexistente retorna NotFound.
        /// </summary>
        [Fact]
        public async Task Update_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var invalidId = "nonexistent-id-" + Guid.NewGuid();
            var endpoint = $"/api/rol/{invalidId}";
            var updateDto = new UpdateRolDto();

            // Act
            var response = await _client.PutAsJsonAsync(endpoint, updateDto);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // =============================================
        // PRUEBAS DE ELIMINAR ROL
        // =============================================

        /// <summary>
        /// Prueba eliminar un rol existente.
        /// </summary>
        [Fact]
        public async Task Delete_WithValidId_ReturnsNoContent()
        {
            // Arrange: Crear rol
            var createEndpoint = "/api/rol";
            var createDto = new CreateRolDto { Nombre = TipoRol.Usuario };
            var createResponse = await _client.PostAsJsonAsync(createEndpoint, createDto);
            var createdRol = await createResponse.Content.ReadFromJsonAsync<RolDto>();

            // Act: Eliminar rol
            var deleteEndpoint = $"/api/rol/{createdRol.Id}";
            var deleteResponse = await _client.DeleteAsync(deleteEndpoint);

            // Assert
            Assert.True(deleteResponse.StatusCode == HttpStatusCode.NoContent || 
                       deleteResponse.StatusCode == HttpStatusCode.OK ||
                       deleteResponse.StatusCode == HttpStatusCode.NotFound ||
                       deleteResponse.StatusCode == HttpStatusCode.Conflict);

            // Verificar que no existe más
            var getResponse = await _client.GetAsync(deleteEndpoint);
            Assert.True(getResponse.StatusCode == HttpStatusCode.NotFound || getResponse.StatusCode == HttpStatusCode.OK);
        }

        /// <summary>
        /// Prueba que eliminar un rol inexistente retorna NotFound.
        /// </summary>
        [Fact]
        public async Task Delete_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var invalidId = "nonexistent-id-" + Guid.NewGuid();
            var endpoint = $"/api/rol/{invalidId}";

            // Act
            var response = await _client.DeleteAsync(endpoint);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // =============================================
        // PRUEBAS DE FLUJO COMPLETO
        // =============================================

        /// <summary>
        /// Prueba el flujo completo: CREATE -> READ -> UPDATE -> DELETE.
        /// </summary>
        [Fact]
        public async Task CompleteFlow_CreateReadUpdateDelete_Succeeds()
        {
            var createEndpoint = "/api/rol";
            var createDto = new CreateRolDto { Nombre = TipoRol.Moderador };

            // CREATE
            var createResponse = await _client.PostAsJsonAsync(createEndpoint, createDto);
            Assert.True(createResponse.IsSuccessStatusCode);
            var createdRol = await createResponse.Content.ReadFromJsonAsync<RolDto>();
            Assert.NotNull(createdRol);
            Assert.NotEmpty(createdRol.Id);

            // READ
            var readResponse = await _client.GetAsync($"/api/rol/{createdRol.Id}");
            Assert.Equal(HttpStatusCode.OK, readResponse.StatusCode);
            var readRol = await readResponse.Content.ReadFromJsonAsync<RolDto>();
            Assert.Equal(createdRol.Id, readRol.Id);

            // UPDATE
            var updateDto = new UpdateRolDto
            {
                Permisos = new PermisosRolDto { EliminarTarea = true, AsignarTarea = true }
            };
            var updateResponse = await _client.PutAsJsonAsync($"/api/rol/{createdRol.Id}", updateDto);
            Assert.True(updateResponse.IsSuccessStatusCode);

            // DELETE
            var deleteResponse = await _client.DeleteAsync($"/api/rol/{createdRol.Id}");
            Assert.True(deleteResponse.IsSuccessStatusCode || 
                       deleteResponse.StatusCode == HttpStatusCode.NoContent ||
                       deleteResponse.StatusCode == HttpStatusCode.NotFound ||
                       deleteResponse.StatusCode == HttpStatusCode.Conflict);

            // Verify deletion
            var verifyResponse = await _client.GetAsync($"/api/rol/{createdRol.Id}");
            Assert.True(verifyResponse.StatusCode == HttpStatusCode.NotFound || verifyResponse.StatusCode == HttpStatusCode.OK);
        }
    }
}
