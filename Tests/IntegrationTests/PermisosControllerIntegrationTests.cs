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
    /// Valida flujos CRUD completos, validación de contenido real y casos de error.
    /// </summary>
    public class PermisosControllerIntegrationTests : IClassFixture<ConviviaWebApplicationFactory>, IAsyncLifetime
    {
        private readonly ConviviaWebApplicationFactory _factory;
        private readonly HttpClient _client;
        private readonly List<string> _createdPermisoIds = new();

        public PermisosControllerIntegrationTests(ConviviaWebApplicationFactory factory)
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
        /// Elimina los permisos creados durante las pruebas.
        /// </summary>
        public async Task DisposeAsync()
        {
            foreach (var permisoId in _createdPermisoIds)
            {
                try
                {
                    var endpoint = $"/api/permisos/{permisoId}";
                    await _client.DeleteAsync(endpoint);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error al eliminar permiso {permisoId}: {ex.Message}");
                }
            }
        }

        // =============================================
        // PRUEBAS DE CREAR PERMISO
        // =============================================

        /// <summary>
        /// Prueba crear un permiso con datos válidos.
        /// </summary>
        [Fact]
        public async Task Create_WithValidData_ReturnsCreatedWithValidContent()
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

            var createdPermiso = await response.Content.ReadFromJsonAsync<PermisoDto>();
            Assert.NotNull(createdPermiso);
            Assert.NotEmpty(createdPermiso.Id);
            Assert.Equal(TipoRol.Usuario, createdPermiso.Rol);

            _createdPermisoIds.Add(createdPermiso.Id);
        }

        /// <summary>
        /// Prueba crear múltiples permisos con diferentes roles.
        /// </summary>
        [Fact]
        public async Task Create_WithMultipleRoles_AllSucceed()
        {
            // Arrange
            var endpoint = "/api/permisos";
            var roles = new[] { TipoRol.Usuario, TipoRol.Admin, TipoRol.Moderador };

            foreach (var role in roles)
            {
                // Act
                var createDto = new CreatePermisoDto { Rol = role };
                var response = await _client.PostAsJsonAsync(endpoint, createDto);

                // Assert
                Assert.True(response.IsSuccessStatusCode);
                
                var createdPermiso = await response.Content.ReadFromJsonAsync<PermisoDto>();
                Assert.NotNull(createdPermiso);
                Assert.Equal(role, createdPermiso.Rol);
                _createdPermisoIds.Add(createdPermiso.Id);
            }
        }

        // =============================================
        // PRUEBAS DE OBTENER PERMISOS
        // =============================================

        /// <summary>
        /// Prueba obtener todos los permisos retorna OK.
        /// </summary>
        [Fact]
        public async Task GetAll_ReturnsOkWithList()
        {
            // Arrange
            var endpoint = "/api/permisos";

            // Act
            var response = await _client.GetAsync(endpoint);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var permisos = await response.Content.ReadFromJsonAsync<List<PermisoDto>>();
            Assert.NotNull(permisos);
            Assert.IsType<List<PermisoDto>>(permisos);
        }

        /// <summary>
        /// Prueba obtener permiso por ID válido después de crearlo.
        /// </summary>
        [Fact]
        public async Task GetById_WithValidId_ReturnsOkWithValidContent()
        {
            // Arrange: Crear permiso
            var createEndpoint = "/api/permisos";
            var createDto = new CreatePermisoDto { Rol = TipoRol.Admin };
            var createResponse = await _client.PostAsJsonAsync(createEndpoint, createDto);
            var createdPermiso = await createResponse.Content.ReadFromJsonAsync<PermisoDto>();
            _createdPermisoIds.Add(createdPermiso.Id);

            // Act
            var getEndpoint = $"/api/permisos/{createdPermiso.Id}";
            var getResponse = await _client.GetAsync(getEndpoint);

            // Assert
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            
            var retrievedPermiso = await getResponse.Content.ReadFromJsonAsync<PermisoDto>();
            Assert.NotNull(retrievedPermiso);
            Assert.Equal(createdPermiso.Id, retrievedPermiso.Id);
            Assert.Equal(TipoRol.Admin, retrievedPermiso.Rol);
        }

        /// <summary>
        /// Prueba que obtener permiso inexistente retorna NotFound.
        /// </summary>
        [Fact]
        public async Task GetById_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var invalidId = "nonexistent-id-" + Guid.NewGuid();
            var endpoint = $"/api/permisos/{invalidId}";

            // Act
            var response = await _client.GetAsync(endpoint);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        /// <summary>
        /// Prueba obtener permisos por rol válido.
        /// </summary>
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

            var permisos = await response.Content.ReadFromJsonAsync<List<PermisoDto>>();
            Assert.NotNull(permisos);
        }

        // =============================================
        // PRUEBAS DE ACTUALIZAR PERMISO
        // =============================================

        /// <summary>
        /// Prueba actualizar un permiso de forma completa (PUT).
        /// </summary>
        [Fact]
        public async Task PutOverwrite_WithValidData_ReturnsOkWithUpdatedContent()
        {
            // Arrange: Crear permiso
            var createEndpoint = "/api/permisos";
            var createDto = new CreatePermisoDto { Rol = TipoRol.Usuario };
            var createResponse = await _client.PostAsJsonAsync(createEndpoint, createDto);
            var createdPermiso = await createResponse.Content.ReadFromJsonAsync<PermisoDto>();
            _createdPermisoIds.Add(createdPermiso.Id);

            // Act: Actualizar permiso
            var updateEndpoint = $"/api/permisos/{createdPermiso.Id}";
            var updateDto = new UpdatePermisoDto
            {
                Rol = TipoRol.Admin,
                CrearTarea = true
            };
            var updateResponse = await _client.PutAsJsonAsync(updateEndpoint, updateDto);

            // Assert
            Assert.True(updateResponse.IsSuccessStatusCode);
            
            var updatedPermiso = await updateResponse.Content.ReadFromJsonAsync<PermisoDto>();
            Assert.NotNull(updatedPermiso);
            Assert.Equal(createdPermiso.Id, updatedPermiso.Id);
        }

        /// <summary>
        /// Prueba que actualizar permiso inexistente retorna NotFound.
        /// </summary>
        [Fact]
        public async Task PutOverwrite_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var invalidId = "nonexistent-id-" + Guid.NewGuid();
            var endpoint = $"/api/permisos/{invalidId}";
            var updateDto = new UpdatePermisoDto();

            // Act
            var response = await _client.PutAsJsonAsync(endpoint, updateDto);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        /// <summary>
        /// Prueba actualizar permiso con merge (PUT /merge).
        /// </summary>
        [Fact]
        public async Task PutMerge_WithValidData_ReturnsOkWithMergedContent()
        {
            // Arrange: Crear permiso
            var createEndpoint = "/api/permisos";
            var createDto = new CreatePermisoDto { Rol = TipoRol.Moderador };
            var createResponse = await _client.PostAsJsonAsync(createEndpoint, createDto);
            var createdPermiso = await createResponse.Content.ReadFromJsonAsync<PermisoDto>();
            _createdPermisoIds.Add(createdPermiso.Id);

            // Act: Merge update
            var mergeEndpoint = $"/api/permisos/{createdPermiso.Id}/merge";
            var updateDto = new UpdatePermisoDto
            {
                EliminarTarea = true
            };
            var mergeResponse = await _client.PutAsJsonAsync(mergeEndpoint, updateDto);

            // Assert
            Assert.True(mergeResponse.IsSuccessStatusCode);
            
            var mergedPermiso = await mergeResponse.Content.ReadFromJsonAsync<PermisoDto>();
            Assert.NotNull(mergedPermiso);
        }

        /// <summary>
        /// Prueba que merge en permiso inexistente retorna NotFound.
        /// </summary>
        [Fact]
        public async Task PutMerge_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var invalidId = "nonexistent-id-" + Guid.NewGuid();
            var endpoint = $"/api/permisos/{invalidId}/merge";
            var updateDto = new UpdatePermisoDto();

            // Act
            var response = await _client.PutAsJsonAsync(endpoint, updateDto);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        /// <summary>
        /// Prueba actualizar permiso de forma parcial (PATCH).
        /// </summary>
        [Fact]
        public async Task Patch_WithValidData_ReturnsOkWithPartialUpdate()
        {
            // Arrange: Crear permiso
            var createEndpoint = "/api/permisos";
            var createDto = new CreatePermisoDto { Rol = TipoRol.Usuario };
            var createResponse = await _client.PostAsJsonAsync(createEndpoint, createDto);
            var createdPermiso = await createResponse.Content.ReadFromJsonAsync<PermisoDto>();
            _createdPermisoIds.Add(createdPermiso.Id);

            // Act: Patch update
            var patchEndpoint = $"/api/permisos/{createdPermiso.Id}";
            var updateDto = new UpdatePermisoDto
            {
                AsignarTarea = true
            };
            var patchResponse = await _client.PatchAsJsonAsync(patchEndpoint, updateDto);

            // Assert
            Assert.True(patchResponse.IsSuccessStatusCode);
            
            var patchedPermiso = await patchResponse.Content.ReadFromJsonAsync<PermisoDto>();
            Assert.NotNull(patchedPermiso);
        }

        /// <summary>
        /// Prueba que patch en permiso inexistente retorna NotFound.
        /// </summary>
        [Fact]
        public async Task Patch_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var invalidId = "nonexistent-id-" + Guid.NewGuid();
            var endpoint = $"/api/permisos/{invalidId}";
            var updateDto = new UpdatePermisoDto();

            // Act
            var response = await _client.PatchAsJsonAsync(endpoint, updateDto);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // =============================================
        // PRUEBAS DE ELIMINAR PERMISO
        // =============================================

        /// <summary>
        /// Prueba eliminar un permiso existente.
        /// </summary>
        [Fact]
        public async Task Delete_WithValidId_ReturnsNoContent()
        {
            // Arrange: Crear permiso
            var createEndpoint = "/api/permisos";
            var createDto = new CreatePermisoDto { Rol = TipoRol.Usuario };
            var createResponse = await _client.PostAsJsonAsync(createEndpoint, createDto);
            var createdPermiso = await createResponse.Content.ReadFromJsonAsync<PermisoDto>();

            // Act: Eliminar permiso
            var deleteEndpoint = $"/api/permisos/{createdPermiso.Id}";
            var deleteResponse = await _client.DeleteAsync(deleteEndpoint);

            // Assert
            Assert.True(deleteResponse.StatusCode == HttpStatusCode.NoContent || 
                       deleteResponse.StatusCode == HttpStatusCode.OK);

            // Verificar que no existe más
            var getResponse = await _client.GetAsync(deleteEndpoint);
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        /// <summary>
        /// Prueba que eliminar permiso inexistente retorna NotFound.
        /// </summary>
        [Fact]
        public async Task Delete_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var invalidId = "nonexistent-id-" + Guid.NewGuid();
            var endpoint = $"/api/permisos/{invalidId}";

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
            var createEndpoint = "/api/permisos";
            var createDto = new CreatePermisoDto { Rol = TipoRol.Admin };

            // CREATE
            var createResponse = await _client.PostAsJsonAsync(createEndpoint, createDto);
            Assert.True(createResponse.IsSuccessStatusCode);
            var createdPermiso = await createResponse.Content.ReadFromJsonAsync<PermisoDto>();
            Assert.NotNull(createdPermiso);
            Assert.NotEmpty(createdPermiso.Id);

            // READ
            var readResponse = await _client.GetAsync($"/api/permisos/{createdPermiso.Id}");
            Assert.Equal(HttpStatusCode.OK, readResponse.StatusCode);
            var readPermiso = await readResponse.Content.ReadFromJsonAsync<PermisoDto>();
            Assert.Equal(createdPermiso.Id, readPermiso.Id);

            // UPDATE
            var updateDto = new UpdatePermisoDto
            {
                Rol = TipoRol.Moderador,
                CrearTarea = true
            };
            var updateResponse = await _client.PutAsJsonAsync($"/api/permisos/{createdPermiso.Id}", updateDto);
            Assert.True(updateResponse.IsSuccessStatusCode);

            // DELETE
            var deleteResponse = await _client.DeleteAsync($"/api/permisos/{createdPermiso.Id}");
            Assert.True(deleteResponse.IsSuccessStatusCode || 
                       deleteResponse.StatusCode == HttpStatusCode.NoContent);

            // Verify deletion
            var verifyResponse = await _client.GetAsync($"/api/permisos/{createdPermiso.Id}");
            Assert.Equal(HttpStatusCode.NotFound, verifyResponse.StatusCode);
        }
    }
}
