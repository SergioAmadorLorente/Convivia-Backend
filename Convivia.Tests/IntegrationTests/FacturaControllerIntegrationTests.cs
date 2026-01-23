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
    /// Valida flujos CRUD completos, validación de contenido real y casos de error.
    /// Se conecta a Firestore real (BD de prueba: convivia-testing).
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

        // =============================================
        // PRUEBAS DE CREAR FACTURA
        // =============================================

        /// <summary>
        /// Prueba crear una factura con datos válidos y validar la respuesta completa.
        /// </summary>
        [Fact]
        public async Task Create_WithValidData_ReturnsCreatedWithValidContent()
        {
            // Arrange
            var endpoint = "/api/factura";
            var createDto = new CreateFacturaDto
            {
                Nombre = $"Test Factura {Guid.NewGuid()}",
                Precio = 100.50m,
                Pagado = false,
                // Reparto = new Dictionary<string, float> { { "user1", 100.50f } }
            };

            // Act
            var response = await _client.PostAsJsonAsync(endpoint, createDto);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.IsSuccessStatusCode, $"Response status: {response.StatusCode}");

            var createdFactura = await response.Content.ReadFromJsonAsync<FacturaDto>();
            Assert.NotNull(createdFactura);
            Assert.NotEmpty(createdFactura.Id);
            Assert.Equal(createDto.Nombre, createdFactura.Nombre);
            Assert.Equal(100.50f, createdFactura.Precio);
            Assert.False(createdFactura.Pagado);

            _createdFacturaIds.Add(createdFactura.Id);
        }

        /// <summary>
        /// Prueba crear múltiples facturas con datos válidos diferentes.
        /// </summary>
        [Fact]
        public async Task Create_WithMultipleValidFacturas_AllSucceed()
        {
            // Arrange
            var endpoint = "/api/factura";

            for (int i = 0; i < 3; i++)
            {
                // Act
                var createDto = new CreateFacturaDto
                {
                    Nombre = $"Factura {i}-{Guid.NewGuid()}",
                    Precio = 50.0m + i * 10,
                    Pagado = i % 2 == 0,
                    // Reparto = new Dictionary<string, float> { { $"user{i}", 50.0f + i * 10 } }
                };
                var response = await _client.PostAsJsonAsync(endpoint, createDto);

                // Assert
                Assert.True(response.IsSuccessStatusCode);
                
                var createdFactura = await response.Content.ReadFromJsonAsync<FacturaDto>();
                Assert.NotNull(createdFactura);
                Assert.NotEmpty(createdFactura.Id);
                _createdFacturaIds.Add(createdFactura.Id);
            }
        }

        /// <summary>
        /// Prueba que crear factura sin nombre retorna BadRequest.
        /// </summary>
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

        /// <summary>
        /// Prueba que crear factura con precio negativo retorna BadRequest.
        /// </summary>
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

        /// <summary>
        /// Prueba crear factura con precio cero (caso borde).
        /// </summary>
        [Fact]
        public async Task Create_WithZeroPrice_Succeeds()
        {
            // Arrange
            var endpoint = "/api/factura";
            var createDto = new CreateFacturaDto
            {
                Nombre = $"Zero Price Factura {Guid.NewGuid()}",
                Precio = 0m,
                // Reparto = new Dictionary<string, float>()
            };

            // Act
            var response = await _client.PostAsJsonAsync(endpoint, createDto);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            
            var createdFactura = await response.Content.ReadFromJsonAsync<FacturaDto>();
            Assert.NotNull(createdFactura);
            _createdFacturaIds.Add(createdFactura.Id);
        }

        // =============================================
        // PRUEBAS DE OBTENER FACTURAS
        // =============================================

        /// <summary>
        /// Prueba obtener todas las facturas retorna OK.
        /// </summary>
        [Fact]
        public async Task GetAll_ReturnsOkWithList()
        {
            // Arrange
            var endpoint = "/api/factura";

            // Act
            var response = await _client.GetAsync(endpoint);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var facturas = await response.Content.ReadFromJsonAsync<List<FacturaDto>>();
            Assert.NotNull(facturas);
            Assert.IsType<List<FacturaDto>>(facturas);
        }

        /// <summary>
        /// Prueba obtener una factura por ID válido después de crearla.
        /// </summary>
        [Fact]
        public async Task GetById_WithValidId_ReturnsOkWithValidContent()
        {
            // Arrange: Crear factura
            var createEndpoint = "/api/factura";
            var createDto = new CreateFacturaDto
            {
                Nombre = $"GetByID Test {Guid.NewGuid()}",
                Precio = 200.75m,
                Pagado = true,
                // Reparto = new Dictionary<string, float> { { "user1", 200.75f } }
            };
            var createResponse = await _client.PostAsJsonAsync(createEndpoint, createDto);
            var createdFactura = await createResponse.Content.ReadFromJsonAsync<FacturaDto>();
            _createdFacturaIds.Add(createdFactura.Id);

            // Act
            var getEndpoint = $"/api/factura/{createdFactura.Id}";
            var getResponse = await _client.GetAsync(getEndpoint);

            // Assert
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            
            var retrievedFactura = await getResponse.Content.ReadFromJsonAsync<FacturaDto>();
            Assert.NotNull(retrievedFactura);
            Assert.Equal(createdFactura.Id, retrievedFactura.Id);
            Assert.Equal(createdFactura.Nombre, retrievedFactura.Nombre);
            Assert.Equal(createdFactura.Precio, retrievedFactura.Precio);
        }

        /// <summary>
        /// Prueba que obtener factura con ID inexistente retorna NotFound.
        /// </summary>
        [Fact]
        public async Task GetById_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var invalidId = "nonexistent-id-" + Guid.NewGuid();
            var endpoint = $"/api/factura/{invalidId}";

            // Act
            var response = await _client.GetAsync(endpoint);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // =============================================
        // PRUEBAS DE ACTUALIZAR FACTURA
        // =============================================

        /// <summary>
        /// Prueba actualizar una factura de forma completa (PUT).
        /// </summary>
        [Fact]
        public async Task PutOverwrite_WithValidData_ReturnsOkWithUpdatedContent()
        {
            // Arrange: Crear factura
            var createEndpoint = "/api/factura";
            var createDto = new CreateFacturaDto
            {
                Nombre = $"Update Test {Guid.NewGuid()}",
                Precio = 150.00m,
                Pagado = false,
                // Reparto = new Dictionary<string, float> { { "user1", 150.0f } }
            };
            var createResponse = await _client.PostAsJsonAsync(createEndpoint, createDto);
            var createdFactura = await createResponse.Content.ReadFromJsonAsync<FacturaDto>();
            _createdFacturaIds.Add(createdFactura.Id);

            // Act: Actualizar factura
            var updateEndpoint = $"/api/factura/{createdFactura.Id}";
            var updateDto = new UpdateFacturaDto
            {
                Nombre = "Nombre Actualizado",
                Precio = 250.00m,
                Pagado = true
            };
            var updateResponse = await _client.PutAsJsonAsync(updateEndpoint, updateDto);

            // Assert
            Assert.True(updateResponse.IsSuccessStatusCode);
            
            var updatedFactura = await updateResponse.Content.ReadFromJsonAsync<FacturaDto>();
            Assert.NotNull(updatedFactura);
            Assert.Equal(createdFactura.Id, updatedFactura.Id);
            Assert.Equal("Nombre Actualizado", updatedFactura.Nombre);
            Assert.Equal(250.00f, updatedFactura.Precio);
            Assert.True(updatedFactura.Pagado);
        }

        /// <summary>
        /// Prueba que actualizar factura inexistente retorna NotFound.
        /// </summary>
        [Fact]
        public async Task PutOverwrite_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var invalidId = "nonexistent-id-" + Guid.NewGuid();
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

        /// <summary>
        /// Prueba actualizar factura con merge (PUT /merge).
        /// </summary>
        [Fact]
        public async Task PutMerge_WithValidData_ReturnsOkWithMergedContent()
        {
            // Arrange: Crear factura
            var createEndpoint = "/api/factura";
            var createDto = new CreateFacturaDto
            {
                Nombre = $"Merge Test {Guid.NewGuid()}",
                Precio = 175.50m,
                Pagado = false,
                // Reparto = new Dictionary<string, float> { { "user1", 175.50f } }
            };
            var createResponse = await _client.PostAsJsonAsync(createEndpoint, createDto);
            var createdFactura = await createResponse.Content.ReadFromJsonAsync<FacturaDto>();
            _createdFacturaIds.Add(createdFactura.Id);

            // Act: Merge update (solo Pagado)
            var mergeEndpoint = $"/api/factura/{createdFactura.Id}/merge";
            var updateDto = new UpdateFacturaDto
            {
                Pagado = true
            };
            var mergeResponse = await _client.PutAsJsonAsync(mergeEndpoint, updateDto);

            // Assert
            Assert.True(mergeResponse.IsSuccessStatusCode);
            
            var mergedFactura = await mergeResponse.Content.ReadFromJsonAsync<FacturaDto>();
            Assert.NotNull(mergedFactura);
            Assert.True(mergedFactura.Pagado);
            Assert.Equal(createdFactura.Nombre, mergedFactura.Nombre); // Nombre no debe cambiar
        }

        /// <summary>
        /// Prueba que merge en factura inexistente retorna NotFound.
        /// </summary>
        [Fact]
        public async Task PutMerge_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var invalidId = "nonexistent-id-" + Guid.NewGuid();
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

        /// <summary>
        /// Prueba actualizar factura de forma parcial (PATCH).
        /// </summary>
        [Fact]
        public async Task Patch_WithValidData_ReturnsOkWithPartialUpdate()
        {
            // Arrange: Crear factura
            var createEndpoint = "/api/factura";
            var originalNombre = $"Patch Test {Guid.NewGuid()}";
            var createDto = new CreateFacturaDto
            {
                Nombre = originalNombre,
                Precio = 300.00m,
                Pagado = false,
                // Reparto = new Dictionary<string, float> { { "user1", 300.0f } }
            };
            var createResponse = await _client.PostAsJsonAsync(createEndpoint, createDto);
            var createdFactura = await createResponse.Content.ReadFromJsonAsync<FacturaDto>();
            _createdFacturaIds.Add(createdFactura.Id);

            // Act: Patch update (solo Pagado)
            var patchEndpoint = $"/api/factura/{createdFactura.Id}";
            var updateDto = new UpdateFacturaDto
            {
                Pagado = true
            };
            var patchResponse = await _client.PatchAsJsonAsync(patchEndpoint, updateDto);

            // Assert
            Assert.True(patchResponse.IsSuccessStatusCode);
            
            var patchedFactura = await patchResponse.Content.ReadFromJsonAsync<FacturaDto>();
            Assert.NotNull(patchedFactura);
            Assert.True(patchedFactura.Pagado);
            Assert.Equal(originalNombre, patchedFactura.Nombre); // Nombre no debe cambiar
        }

        /// <summary>
        /// Prueba que patch en factura inexistente retorna NotFound.
        /// </summary>
        [Fact]
        public async Task Patch_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var invalidId = "nonexistent-id-" + Guid.NewGuid();
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

        // =============================================
        // PRUEBAS DE ELIMINAR FACTURA
        // =============================================

        /// <summary>
        /// Prueba eliminar una factura existente.
        /// </summary>
        [Fact]
        public async Task Delete_WithValidId_ReturnsNoContent()
        {
            // Arrange: Crear factura
            var createEndpoint = "/api/factura";
            var createDto = new CreateFacturaDto
            {
                Nombre = $"Delete Test {Guid.NewGuid()}",
                Precio = 99.99m,
                // Reparto = new Dictionary<string, float> { { "user1", 99.99f } }
            };
            var createResponse = await _client.PostAsJsonAsync(createEndpoint, createDto);
            var createdFactura = await createResponse.Content.ReadFromJsonAsync<FacturaDto>();

            // Act: Eliminar factura
            var deleteEndpoint = $"/api/factura/{createdFactura.Id}";
            var deleteResponse = await _client.DeleteAsync(deleteEndpoint);

            // Assert
            Assert.True(deleteResponse.StatusCode == HttpStatusCode.NoContent || 
                       deleteResponse.StatusCode == HttpStatusCode.OK);

            // Verificar que no existe más
            var getResponse = await _client.GetAsync(deleteEndpoint);
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        /// <summary>
        /// Prueba que eliminar factura inexistente retorna NotFound.
        /// </summary>
        [Fact]
        public async Task Delete_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var invalidId = "nonexistent-id-" + Guid.NewGuid();
            var endpoint = $"/api/factura/{invalidId}";

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
            var createEndpoint = "/api/factura";
            var createDto = new CreateFacturaDto
            {
                Nombre = $"Complete Flow {Guid.NewGuid()}",
                Precio = 500.50m,
                Pagado = false,
                // Reparto = new Dictionary<string, float> { { "user1", 500.50f } }
            };

            // CREATE
            var createResponse = await _client.PostAsJsonAsync(createEndpoint, createDto);
            Assert.True(createResponse.IsSuccessStatusCode);
            var createdFactura = await createResponse.Content.ReadFromJsonAsync<FacturaDto>();
            Assert.NotNull(createdFactura);
            Assert.NotEmpty(createdFactura.Id);

            // READ
            var readResponse = await _client.GetAsync($"/api/factura/{createdFactura.Id}");
            Assert.Equal(HttpStatusCode.OK, readResponse.StatusCode);
            var readFactura = await readResponse.Content.ReadFromJsonAsync<FacturaDto>();
            Assert.Equal(createdFactura.Id, readFactura.Id);

            // UPDATE
            var updateDto = new UpdateFacturaDto
            {
                Precio = 750.75m,
                Pagado = true
            };
            var updateResponse = await _client.PutAsJsonAsync($"/api/factura/{createdFactura.Id}", updateDto);
            Assert.True(updateResponse.IsSuccessStatusCode);
            var updatedFactura = await updateResponse.Content.ReadFromJsonAsync<FacturaDto>();
            Assert.Equal(750.75f, updatedFactura.Precio);
            Assert.True(updatedFactura.Pagado);

            // DELETE
            var deleteResponse = await _client.DeleteAsync($"/api/factura/{createdFactura.Id}");
            Assert.True(deleteResponse.IsSuccessStatusCode || 
                       deleteResponse.StatusCode == HttpStatusCode.NoContent);

            // Verify deletion
            var verifyResponse = await _client.GetAsync($"/api/factura/{createdFactura.Id}");
            Assert.Equal(HttpStatusCode.NotFound, verifyResponse.StatusCode);
        }
    }
}
