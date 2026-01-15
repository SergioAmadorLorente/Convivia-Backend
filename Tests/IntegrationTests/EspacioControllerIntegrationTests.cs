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
    /// Valida flujos CRUD completos, validación de contenido real y casos de error.
    /// </summary>
    public class EspacioControllerIntegrationTests : IClassFixture<ConviviaWebApplicationFactory>, IAsyncLifetime
    {
        private readonly ConviviaWebApplicationFactory _factory;
        private readonly HttpClient _client;
        private readonly List<string> _createdEspacioIds = new();

        public EspacioControllerIntegrationTests(ConviviaWebApplicationFactory factory)
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
        /// Elimina los espacios creados durante las pruebas.
        /// </summary>
        public async Task DisposeAsync()
        {
            foreach (var espacioId in _createdEspacioIds)
            {
                try
                {
                    var endpoint = $"/api/espacio/{espacioId}";
                    await _client.DeleteAsync(endpoint);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error al eliminar espacio {espacioId}: {ex.Message}");
                }
            }
        }

        // =============================================
        // PRUEBAS DE CREAR ESPACIO
        // =============================================

        /// <summary>
        /// Prueba crear un espacio con datos válidos y validar la respuesta.
        /// </summary>
        [Fact]
        public async Task Create_WithValidData_ReturnsCreatedWithValidContent()
        {
            // Arrange
            var endpoint = "/api/espacio";
            var createDto = new CreateEspacioDto
            {
                Nombre = $"Test Espacio {Guid.NewGuid()}"
            };

            // Act
            var response = await _client.PostAsJsonAsync(endpoint, createDto);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK);

            var createdEspacio = await response.Content.ReadFromJsonAsync<EspacioDto>();
            Assert.NotNull(createdEspacio);
            Assert.NotEmpty(createdEspacio.Id);
            Assert.Equal(createDto.Nombre, createdEspacio.Nombre);

            _createdEspacioIds.Add(createdEspacio.Id);
        }

        /// <summary>
        /// Prueba crear múltiples espacios con diferentes nombres.
        /// </summary>
        [Fact]
        public async Task Create_WithMultipleEspacios_AllSucceed()
        {
            // Arrange
            var endpoint = "/api/espacio";

            for (int i = 0; i < 3; i++)
            {
                // Act
                var createDto = new CreateEspacioDto
                {
                    Nombre = $"Espacio {i}-{Guid.NewGuid()}"
                };
                var response = await _client.PostAsJsonAsync(endpoint, createDto);

                // Assert
                Assert.True(response.IsSuccessStatusCode);
                
                var createdEspacio = await response.Content.ReadFromJsonAsync<EspacioDto>();
                Assert.NotNull(createdEspacio);
                Assert.NotEmpty(createdEspacio.Id);
                _createdEspacioIds.Add(createdEspacio.Id);
            }
        }

        // =============================================
        // PRUEBAS DE OBTENER ESPACIOS
        // =============================================

        /// <summary>
        /// Prueba obtener todos los espacios retorna OK.
        /// </summary>
        [Fact]
        public async Task GetAll_ReturnsOkWithList()
        {
            // Arrange
            var endpoint = "/api/espacio";

            // Act
            var response = await _client.GetAsync(endpoint);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var espacios = await response.Content.ReadFromJsonAsync<List<EspacioDto>>();
            Assert.NotNull(espacios);
            Assert.IsType<List<EspacioDto>>(espacios);
        }

        /// <summary>
        /// Prueba obtener espacio por ID válido después de crearlo.
        /// </summary>
        [Fact]
        public async Task GetById_WithValidId_ReturnsOkWithValidContent()
        {
            // Arrange: Crear espacio
            var createEndpoint = "/api/espacio";
            var createDto = new CreateEspacioDto
            {
                Nombre = $"GetByID Test {Guid.NewGuid()}"
            };
            var createResponse = await _client.PostAsJsonAsync(createEndpoint, createDto);
            var createdEspacio = await createResponse.Content.ReadFromJsonAsync<EspacioDto>();
            _createdEspacioIds.Add(createdEspacio.Id);

            // Act
            var getEndpoint = $"/api/espacio/{createdEspacio.Id}";
            var getResponse = await _client.GetAsync(getEndpoint);

            // Assert
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            
            var retrievedEspacio = await getResponse.Content.ReadFromJsonAsync<EspacioDto>();
            Assert.NotNull(retrievedEspacio);
            Assert.Equal(createdEspacio.Id, retrievedEspacio.Id);
            Assert.Equal(createdEspacio.Nombre, retrievedEspacio.Nombre);
        }

        /// <summary>
        /// Prueba que obtener espacio con ID inexistente retorna NotFound.
        /// </summary>
        [Fact]
        public async Task GetById_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var invalidId = "nonexistent-id-" + Guid.NewGuid();
            var endpoint = $"/api/espacio/{invalidId}";

            // Act
            var response = await _client.GetAsync(endpoint);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        /// <summary>
        /// Prueba obtener espacios por dirección (puede retornar lista vacía).
        /// </summary>
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

            var espacios = await response.Content.ReadFromJsonAsync<List<EspacioDto>>();
            Assert.NotNull(espacios);
        }

        // =============================================
        // PRUEBAS DE ACTUALIZAR ESPACIO
        // =============================================

        /// <summary>
        /// Prueba actualizar un espacio de forma completa (PUT).
        /// </summary>
        [Fact]
        public async Task PutOverwrite_WithValidData_ReturnsOkWithUpdatedContent()
        {
            // Arrange: Crear espacio
            var createEndpoint = "/api/espacio";
            var createDto = new CreateEspacioDto
            {
                Nombre = $"Update Test {Guid.NewGuid()}"
            };
            var createResponse = await _client.PostAsJsonAsync(createEndpoint, createDto);
            var createdEspacio = await createResponse.Content.ReadFromJsonAsync<EspacioDto>();
            _createdEspacioIds.Add(createdEspacio.Id);

            // Act: Actualizar espacio
            var updateEndpoint = $"/api/espacio/{createdEspacio.Id}";
            var updateDto = new UpdateEspacioDto
            {
                Nombre = "Nombre Actualizado"
            };
            var updateResponse = await _client.PutAsJsonAsync(updateEndpoint, updateDto);

            // Assert
            Assert.True(updateResponse.IsSuccessStatusCode);
            
            var updatedEspacio = await updateResponse.Content.ReadFromJsonAsync<EspacioDto>();
            Assert.NotNull(updatedEspacio);
            Assert.Equal(createdEspacio.Id, updatedEspacio.Id);
            Assert.Equal("Nombre Actualizado", updatedEspacio.Nombre);
        }

        /// <summary>
        /// Prueba que actualizar espacio inexistente retorna NotFound.
        /// </summary>
        [Fact]
        public async Task PutOverwrite_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var invalidId = "nonexistent-id-" + Guid.NewGuid();
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

        /// <summary>
        /// Prueba actualizar espacio con merge (PUT /merge).
        /// </summary>
        [Fact]
        public async Task PutMerge_WithValidData_ReturnsOkWithMergedContent()
        {
            // Arrange: Crear espacio
            var createEndpoint = "/api/espacio";
            var createDto = new CreateEspacioDto
            {
                Nombre = $"Merge Test {Guid.NewGuid()}"
            };
            var createResponse = await _client.PostAsJsonAsync(createEndpoint, createDto);
            var createdEspacio = await createResponse.Content.ReadFromJsonAsync<EspacioDto>();
            _createdEspacioIds.Add(createdEspacio.Id);

            // Act: Merge update
            var mergeEndpoint = $"/api/espacio/{createdEspacio.Id}/merge";
            var updateDto = new UpdateEspacioDto
            {
                Nombre = "Merged Nombre"
            };
            var mergeResponse = await _client.PutAsJsonAsync(mergeEndpoint, updateDto);

            // Assert
            Assert.True(mergeResponse.IsSuccessStatusCode);
            
            var mergedEspacio = await mergeResponse.Content.ReadFromJsonAsync<EspacioDto>();
            Assert.NotNull(mergedEspacio);
            Assert.Equal("Merged Nombre", mergedEspacio.Nombre);
        }

        /// <summary>
        /// Prueba que merge en espacio inexistente retorna NotFound.
        /// </summary>
        [Fact]
        public async Task PutMerge_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var invalidId = "nonexistent-id-" + Guid.NewGuid();
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

        /// <summary>
        /// Prueba actualizar espacio de forma parcial (PATCH).
        /// </summary>
        [Fact]
        public async Task Patch_WithValidData_ReturnsOkWithPartialUpdate()
        {
            // Arrange: Crear espacio
            var createEndpoint = "/api/espacio";
            var createDto = new CreateEspacioDto
            {
                Nombre = $"Patch Test {Guid.NewGuid()}"
            };
            var createResponse = await _client.PostAsJsonAsync(createEndpoint, createDto);
            var createdEspacio = await createResponse.Content.ReadFromJsonAsync<EspacioDto>();
            _createdEspacioIds.Add(createdEspacio.Id);

            // Act: Patch update
            var patchEndpoint = $"/api/espacio/{createdEspacio.Id}";
            var updateDto = new UpdateEspacioDto
            {
                Nombre = "Patched Nombre"
            };
            var patchResponse = await _client.PatchAsJsonAsync(patchEndpoint, updateDto);

            // Assert
            Assert.True(patchResponse.IsSuccessStatusCode);
            
            var patchedEspacio = await patchResponse.Content.ReadFromJsonAsync<EspacioDto>();
            Assert.NotNull(patchedEspacio);
            Assert.Equal("Patched Nombre", patchedEspacio.Nombre);
        }

        /// <summary>
        /// Prueba que patch en espacio inexistente retorna NotFound.
        /// </summary>
        [Fact]
        public async Task Patch_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var invalidId = "nonexistent-id-" + Guid.NewGuid();
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

        // =============================================
        // PRUEBAS DE ELIMINAR ESPACIO
        // =============================================

        /// <summary>
        /// Prueba eliminar un espacio existente.
        /// </summary>
        [Fact]
        public async Task Delete_WithValidId_ReturnsNoContent()
        {
            // Arrange: Crear espacio
            var createEndpoint = "/api/espacio";
            var createDto = new CreateEspacioDto
            {
                Nombre = $"Delete Test {Guid.NewGuid()}"
            };
            var createResponse = await _client.PostAsJsonAsync(createEndpoint, createDto);
            var createdEspacio = await createResponse.Content.ReadFromJsonAsync<EspacioDto>();

            // Act: Eliminar espacio
            var deleteEndpoint = $"/api/espacio/{createdEspacio.Id}";
            var deleteResponse = await _client.DeleteAsync(deleteEndpoint);

            // Assert
            Assert.True(deleteResponse.StatusCode == HttpStatusCode.NoContent || 
                       deleteResponse.StatusCode == HttpStatusCode.OK);

            // Verificar que no existe más
            var getResponse = await _client.GetAsync(deleteEndpoint);
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        /// <summary>
        /// Prueba que eliminar espacio inexistente retorna NotFound.
        /// </summary>
        [Fact]
        public async Task Delete_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var invalidId = "nonexistent-id-" + Guid.NewGuid();
            var endpoint = $"/api/espacio/{invalidId}";

            // Act
            var response = await _client.DeleteAsync(endpoint);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // =============================================
        // PRUEBAS DE OPERACIONES ESPECIALES
        // =============================================

        /// <summary>
        /// Prueba obtener código de invitación para espacio inexistente retorna NotFound.
        /// </summary>
        [Fact]
        public async Task GetInvitationCode_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var invalidId = "nonexistent-id-" + Guid.NewGuid();
            var endpoint = $"/api/espacio/{invalidId}/getCode";

            // Act
            var response = await _client.GetAsync(endpoint);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        /// <summary>
        /// Prueba unirse a un espacio por código inválido retorna NotFound.
        /// </summary>
        [Fact]
        public async Task JoinSpaceByCode_WithInvalidCode_ReturnsNotFound()
        {
            // Arrange
            var invalidCode = "invalid-code-" + Guid.NewGuid();
            var endpoint = $"/api/espacio/{invalidCode}/usuario";
            var joinDto = new JoinByCodeDto
            {
                UsuarioId = $"test-user-{Guid.NewGuid()}"
            };

            // Act
            var response = await _client.PostAsJsonAsync(endpoint, joinDto);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        /// <summary>
        /// Prueba que unirse a espacio sin ID de usuario retorna BadRequest.
        /// </summary>
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

        // =============================================
        // PRUEBAS DE FLUJO COMPLETO
        // =============================================

        /// <summary>
        /// Prueba el flujo completo: CREATE -> READ -> UPDATE -> DELETE.
        /// </summary>
        [Fact]
        public async Task CompleteFlow_CreateReadUpdateDelete_Succeeds()
        {
            var createEndpoint = "/api/espacio";
            var createDto = new CreateEspacioDto
            {
                Nombre = $"Complete Flow {Guid.NewGuid()}"
            };

            // CREATE
            var createResponse = await _client.PostAsJsonAsync(createEndpoint, createDto);
            Assert.True(createResponse.IsSuccessStatusCode);
            var createdEspacio = await createResponse.Content.ReadFromJsonAsync<EspacioDto>();
            Assert.NotNull(createdEspacio);
            Assert.NotEmpty(createdEspacio.Id);

            // READ
            var readResponse = await _client.GetAsync($"/api/espacio/{createdEspacio.Id}");
            Assert.Equal(HttpStatusCode.OK, readResponse.StatusCode);
            var readEspacio = await readResponse.Content.ReadFromJsonAsync<EspacioDto>();
            Assert.Equal(createdEspacio.Id, readEspacio.Id);

            // UPDATE
            var updateDto = new UpdateEspacioDto
            {
                Nombre = "Updated Name in Flow"
            };
            var updateResponse = await _client.PutAsJsonAsync($"/api/espacio/{createdEspacio.Id}", updateDto);
            Assert.True(updateResponse.IsSuccessStatusCode);
            var updatedEspacio = await updateResponse.Content.ReadFromJsonAsync<EspacioDto>();
            Assert.Equal("Updated Name in Flow", updatedEspacio.Nombre);

            // DELETE
            var deleteResponse = await _client.DeleteAsync($"/api/espacio/{createdEspacio.Id}");
            Assert.True(deleteResponse.IsSuccessStatusCode || 
                       deleteResponse.StatusCode == HttpStatusCode.NoContent);

            // Verify deletion
            var verifyResponse = await _client.GetAsync($"/api/espacio/{createdEspacio.Id}");
            Assert.Equal(HttpStatusCode.NotFound, verifyResponse.StatusCode);
        }
    }
}
