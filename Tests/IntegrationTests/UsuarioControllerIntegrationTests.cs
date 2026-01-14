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
    /// Valida flujos CRUD completos, validación de contenido real y casos de error.
    /// Se conecta a Firestore real (BD de prueba: convivia-testing).
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
        /// Inicialización antes de cada clase de tests.
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
                    System.Diagnostics.Debug.WriteLine($"Error al eliminar usuario {userId}: {ex.Message}");
                }
            }
        }

        // =============================================
        // PRUEBAS DE CREAR USUARIO
        // =============================================

        /// <summary>
        /// Prueba crear un usuario con datos válidos y valida la respuesta completa.
        /// </summary>
        [Fact]
        public async Task Create_WithValidData_ReturnsCreatedWithValidContent()
        {
            // Arrange
            var endpoint = "/api/usuario";
            var createDto = new CreateUsuarioDto
            {
                Nombre = $"Test Usuario {Guid.NewGuid()}",
                Email = $"test-{Guid.NewGuid()}@example.com",
                Password = "password123"
            };

            // Act
            var response = await _client.PostAsJsonAsync(endpoint, createDto);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.IsSuccessStatusCode, $"Response status: {response.StatusCode}");

            var createdUser = await response.Content.ReadFromJsonAsync<UsuarioDto>();
            Assert.NotNull(createdUser);
            Assert.NotEmpty(createdUser.Id);
            Assert.Equal(createDto.Nombre, createdUser.Nombre);
            Assert.Equal(createDto.Email, createdUser.Email);

            _createdUserIds.Add(createdUser.Id);
        }

        /// <summary>
        /// Prueba crear usuarios con diferentes datos válidos.
        /// </summary>
        [Fact]
        public async Task Create_WithMultipleValidUsers_AllSucceed()
        {
            // Arrange
            var endpoint = "/api/usuario";
            var userCount = 3;

            for (int i = 0; i < userCount; i++)
            {
                // Act
                var createDto = new CreateUsuarioDto
                {
                    Nombre = $"User {i}-{Guid.NewGuid()}",
                    Email = $"user{i}-{Guid.NewGuid()}@example.com",
                    Password = "password123"
                };
                var response = await _client.PostAsJsonAsync(endpoint, createDto);

                // Assert
                Assert.True(response.IsSuccessStatusCode);
                
                var createdUser = await response.Content.ReadFromJsonAsync<UsuarioDto>();
                Assert.NotNull(createdUser);
                Assert.NotEmpty(createdUser.Id);
                _createdUserIds.Add(createdUser.Id);
            }
        }

        /// <summary>
        /// Prueba que crear usuario sin nombre retorna BadRequest.
        /// </summary>
        [Fact]
        public async Task Create_WithMissingNombre_ReturnsBadRequest()
        {
            // Arrange
            var endpoint = "/api/usuario";
            var createDto = new CreateUsuarioDto
            {
                Nombre = "",
                Email = $"test-{Guid.NewGuid()}@example.com",
                Password = "password123"
            };

            // Act
            var response = await _client.PostAsJsonAsync(endpoint, createDto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        /// <summary>
        /// Prueba que crear usuario sin email retorna BadRequest.
        /// </summary>
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

        /// <summary>
        /// Prueba que crear usuario sin password retorna BadRequest.
        /// </summary>
        [Fact]
        public async Task Create_WithMissingPassword_ReturnsBadRequest()
        {
            // Arrange
            var endpoint = "/api/usuario";
            var createDto = new CreateUsuarioDto
            {
                Nombre = "Test Usuario",
                Email = $"test-{Guid.NewGuid()}@example.com",
                Password = ""
            };

            // Act
            var response = await _client.PostAsJsonAsync(endpoint, createDto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // =============================================
        // PRUEBAS DE OBTENER USUARIOS
        // =============================================

        /// <summary>
        /// Prueba obtener todos los usuarios retorna OK.
        /// </summary>
        [Fact]
        public async Task GetAll_ReturnsOkWithList()
        {
            // Arrange
            var endpoint = "/api/usuario";

            // Act
            var response = await _client.GetAsync(endpoint);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var usuarios = await response.Content.ReadFromJsonAsync<List<UsuarioDto>>();
            Assert.NotNull(usuarios);
            Assert.IsType<List<UsuarioDto>>(usuarios);
        }

        /// <summary>
        /// Prueba obtener un usuario por ID válido después de crearlo.
        /// </summary>
        [Fact]
        public async Task GetById_WithValidId_ReturnsOkWithValidContent()
        {
            // Arrange: Crear usuario
            var createEndpoint = "/api/usuario";
            var createDto = new CreateUsuarioDto
            {
                Nombre = $"GetByID Test {Guid.NewGuid()}",
                Email = $"getbyid-{Guid.NewGuid()}@example.com",
                Password = "password123"
            };
            var createResponse = await _client.PostAsJsonAsync(createEndpoint, createDto);
            var createdUser = await createResponse.Content.ReadFromJsonAsync<UsuarioDto>();
            _createdUserIds.Add(createdUser.Id);

            // Act
            var getEndpoint = $"/api/usuario/{createdUser.Id}";
            var getResponse = await _client.GetAsync(getEndpoint);

            // Assert
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            
            var retrievedUser = await getResponse.Content.ReadFromJsonAsync<UsuarioDto>();
            Assert.NotNull(retrievedUser);
            Assert.Equal(createdUser.Id, retrievedUser.Id);
            Assert.Equal(createdUser.Nombre, retrievedUser.Nombre);
            Assert.Equal(createdUser.Email, retrievedUser.Email);
        }

        /// <summary>
        /// Prueba que obtener usuario con ID inexistente retorna NotFound.
        /// </summary>
        [Fact]
        public async Task GetById_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var invalidId = "nonexistent-id-" + Guid.NewGuid();
            var endpoint = $"/api/usuario/{invalidId}";

            // Act
            var response = await _client.GetAsync(endpoint);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        /// <summary>
        /// Prueba obtener usuario por email válido después de crearlo.
        /// </summary>
        [Fact]
        public async Task GetByEmail_WithValidEmail_ReturnsOkWithValidContent()
        {
            // Arrange: Crear usuario
            var createEndpoint = "/api/usuario";
            var email = $"getbyemail-{Guid.NewGuid()}@example.com";
            var createDto = new CreateUsuarioDto
            {
                Nombre = $"GetByEmail Test {Guid.NewGuid()}",
                Email = email,
                Password = "password123"
            };
            var createResponse = await _client.PostAsJsonAsync(createEndpoint, createDto);
            var createdUser = await createResponse.Content.ReadFromJsonAsync<UsuarioDto>();
            _createdUserIds.Add(createdUser.Id);

            // Act
            var getEndpoint = $"/api/usuario/correo/{email}";
            var getResponse = await _client.GetAsync(getEndpoint);

            // Assert
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            
            var retrievedUser = await getResponse.Content.ReadFromJsonAsync<UsuarioDto>();
            Assert.NotNull(retrievedUser);
            Assert.Equal(email, retrievedUser.Email);
        }

        /// <summary>
        /// Prueba que obtener usuario con email inexistente retorna NotFound.
        /// </summary>
        [Fact]
        public async Task GetByEmail_WithInvalidEmail_ReturnsNotFound()
        {
            // Arrange
            var invalidEmail = $"nonexistent-{Guid.NewGuid()}@example.com";
            var endpoint = $"/api/usuario/correo/{invalidEmail}";

            // Act
            var response = await _client.GetAsync(endpoint);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        /// <summary>
        /// Prueba que obtener usuario con email vacío retorna BadRequest.
        /// </summary>
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

        // =============================================
        // PRUEBAS DE ACTUALIZAR USUARIO
        // =============================================

        /// <summary>
        /// Prueba actualizar un usuario de forma completa (PUT).
        /// </summary>
        [Fact]
        public async Task PutOverwrite_WithValidData_ReturnsOkWithUpdatedContent()
        {
            // Arrange: Crear usuario
            var createEndpoint = "/api/usuario";
            var createDto = new CreateUsuarioDto
            {
                Nombre = $"Update Test {Guid.NewGuid()}",
                Email = $"update-{Guid.NewGuid()}@example.com",
                Password = "password123"
            };
            var createResponse = await _client.PostAsJsonAsync(createEndpoint, createDto);
            var createdUser = await createResponse.Content.ReadFromJsonAsync<UsuarioDto>();
            _createdUserIds.Add(createdUser.Id);

            // Act: Actualizar usuario
            var updateEndpoint = $"/api/usuario/{createdUser.Id}";
            var updateDto = new UpdateUsuarioDto
            {
                Nombre = "Nombre Actualizado"
            };
            var updateResponse = await _client.PutAsJsonAsync(updateEndpoint, updateDto);

            // Assert
            Assert.True(updateResponse.IsSuccessStatusCode);
            
            var updatedUser = await updateResponse.Content.ReadFromJsonAsync<UsuarioDto>();
            Assert.NotNull(updatedUser);
            Assert.Equal(createdUser.Id, updatedUser.Id);
            Assert.Equal("Nombre Actualizado", updatedUser.Nombre);
        }

        /// <summary>
        /// Prueba que actualizar usuario inexistente retorna NotFound.
        /// </summary>
        [Fact]
        public async Task PutOverwrite_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var invalidId = "nonexistent-id-" + Guid.NewGuid();
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

        /// <summary>
        /// Prueba actualizar usuario con merge (PUT /merge).
        /// </summary>
        [Fact]
        public async Task PutMerge_WithValidData_ReturnsOkWithMergedContent()
        {
            // Arrange: Crear usuario
            var createEndpoint = "/api/usuario";
            var createDto = new CreateUsuarioDto
            {
                Nombre = $"Merge Test {Guid.NewGuid()}",
                Email = $"merge-{Guid.NewGuid()}@example.com",
                Password = "password123"
            };
            var createResponse = await _client.PostAsJsonAsync(createEndpoint, createDto);
            var createdUser = await createResponse.Content.ReadFromJsonAsync<UsuarioDto>();
            _createdUserIds.Add(createdUser.Id);

            // Act: Merge update
            var mergeEndpoint = $"/api/usuario/{createdUser.Id}/merge";
            var updateDto = new UpdateUsuarioDto
            {
                Nombre = "Merged Nombre"
            };
            var mergeResponse = await _client.PutAsJsonAsync(mergeEndpoint, updateDto);

            // Assert
            Assert.True(mergeResponse.IsSuccessStatusCode);
            
            var mergedUser = await mergeResponse.Content.ReadFromJsonAsync<UsuarioDto>();
            Assert.NotNull(mergedUser);
            Assert.Equal("Merged Nombre", mergedUser.Nombre);
        }

        /// <summary>
        /// Prueba que merge en usuario inexistente retorna NotFound.
        /// </summary>
        [Fact]
        public async Task PutMerge_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var invalidId = "nonexistent-id-" + Guid.NewGuid();
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

        /// <summary>
        /// Prueba actualizar usuario de forma parcial (PATCH).
        /// </summary>
        [Fact]
        public async Task Patch_WithValidData_ReturnsOkWithPartialUpdate()
        {
            // Arrange: Crear usuario
            var createEndpoint = "/api/usuario";
            var originalEmail = $"patch-{Guid.NewGuid()}@example.com";
            var createDto = new CreateUsuarioDto
            {
                Nombre = $"Patch Test {Guid.NewGuid()}",
                Email = originalEmail,
                Password = "password123"
            };
            var createResponse = await _client.PostAsJsonAsync(createEndpoint, createDto);
            var createdUser = await createResponse.Content.ReadFromJsonAsync<UsuarioDto>();
            _createdUserIds.Add(createdUser.Id);

            // Act: Patch update (solo nombre)
            var patchEndpoint = $"/api/usuario/{createdUser.Id}";
            var updateDto = new UpdateUsuarioDto
            {
                Nombre = "Patched Nombre"
            };
            var patchResponse = await _client.PatchAsJsonAsync(patchEndpoint, updateDto);

            // Assert
            Assert.True(patchResponse.IsSuccessStatusCode);
            
            var patchedUser = await patchResponse.Content.ReadFromJsonAsync<UsuarioDto>();
            Assert.NotNull(patchedUser);
            Assert.Equal("Patched Nombre", patchedUser.Nombre);
            Assert.Equal(originalEmail, patchedUser.Email); // Email no debe cambiar
        }

        /// <summary>
        /// Prueba que patch en usuario inexistente retorna NotFound.
        /// </summary>
        [Fact]
        public async Task Patch_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var invalidId = "nonexistent-id-" + Guid.NewGuid();
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

        // =============================================
        // PRUEBAS DE ELIMINAR USUARIO
        // =============================================

        /// <summary>
        /// Prueba eliminar un usuario existente.
        /// </summary>
        [Fact]
        public async Task Delete_WithValidId_ReturnsNoContent()
        {
            // Arrange: Crear usuario
            var createEndpoint = "/api/usuario";
            var createDto = new CreateUsuarioDto
            {
                Nombre = $"Delete Test {Guid.NewGuid()}",
                Email = $"delete-{Guid.NewGuid()}@example.com",
                Password = "password123"
            };
            var createResponse = await _client.PostAsJsonAsync(createEndpoint, createDto);
            var createdUser = await createResponse.Content.ReadFromJsonAsync<UsuarioDto>();

            // Act: Eliminar usuario
            var deleteEndpoint = $"/api/usuario/{createdUser.Id}";
            var deleteResponse = await _client.DeleteAsync(deleteEndpoint);

            // Assert
            Assert.True(deleteResponse.StatusCode == HttpStatusCode.NoContent || 
                       deleteResponse.StatusCode == HttpStatusCode.OK);

            // Verificar que no existe más
            var getResponse = await _client.GetAsync(deleteEndpoint);
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        /// <summary>
        /// Prueba que eliminar usuario inexistente retorna NotFound.
        /// </summary>
        [Fact]
        public async Task Delete_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var invalidId = "nonexistent-id-" + Guid.NewGuid();
            var endpoint = $"/api/usuario/{invalidId}";

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
            var createEndpoint = "/api/usuario";
            var email = $"complete-{Guid.NewGuid()}@example.com";
            var createDto = new CreateUsuarioDto
            {
                Nombre = $"Complete Flow Test {Guid.NewGuid()}",
                Email = email,
                Password = "password123"
            };

            // CREATE
            var createResponse = await _client.PostAsJsonAsync(createEndpoint, createDto);
            Assert.True(createResponse.IsSuccessStatusCode);
            var createdUser = await createResponse.Content.ReadFromJsonAsync<UsuarioDto>();
            Assert.NotNull(createdUser);
            Assert.NotEmpty(createdUser.Id);

            // READ by ID
            var readByIdResponse = await _client.GetAsync($"/api/usuario/{createdUser.Id}");
            Assert.Equal(HttpStatusCode.OK, readByIdResponse.StatusCode);
            var readByIdUser = await readByIdResponse.Content.ReadFromJsonAsync<UsuarioDto>();
            Assert.Equal(createdUser.Id, readByIdUser.Id);

            // READ by Email
            var readByEmailResponse = await _client.GetAsync($"/api/usuario/correo/{email}");
            Assert.Equal(HttpStatusCode.OK, readByEmailResponse.StatusCode);
            var readByEmailUser = await readByEmailResponse.Content.ReadFromJsonAsync<UsuarioDto>();
            Assert.Equal(email, readByEmailUser.Email);

            // UPDATE
            var updateDto = new UpdateUsuarioDto
            {
                Nombre = "Updated Name in Flow"
            };
            var updateResponse = await _client.PutAsJsonAsync($"/api/usuario/{createdUser.Id}", updateDto);
            Assert.True(updateResponse.IsSuccessStatusCode);
            var updatedUser = await updateResponse.Content.ReadFromJsonAsync<UsuarioDto>();
            Assert.Equal("Updated Name in Flow", updatedUser.Nombre);

            // DELETE
            var deleteResponse = await _client.DeleteAsync($"/api/usuario/{createdUser.Id}");
            Assert.True(deleteResponse.IsSuccessStatusCode || 
                       deleteResponse.StatusCode == HttpStatusCode.NoContent);

            // Verify deletion
            var verifyResponse = await _client.GetAsync($"/api/usuario/{createdUser.Id}");
            Assert.Equal(HttpStatusCode.NotFound, verifyResponse.StatusCode);
        }
    }
}
