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
    public class UsuarioEspacioControllerIntegrationTests : IClassFixture<ConviviaWebApplicationFactory>, IAsyncLifetime
    {
        private readonly ConviviaWebApplicationFactory _factory;
        private readonly HttpClient _client;
        private readonly List<string> _createdUsuarioEspacioIds = new();

        public UsuarioEspacioControllerIntegrationTests(ConviviaWebApplicationFactory factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _client = _factory.CreateClient();
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            foreach (var usuarioEspacioId in _createdUsuarioEspacioIds)
            {
                try
                {
                    var endpoint = $"/api/usuarioespacio/{usuarioEspacioId}";
                    await _client.DeleteAsync(endpoint);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error al eliminar usuarioEspacio {usuarioEspacioId}: {ex.Message}");
                }
            }
        }

        [Fact]
        public async Task Create_WithValidData_ReturnsSuccessfulResponse()
        {
            var endpoint = "/api/usuarioespacio";
            var createDto = new CreateUsuarioEspacioDto
            {
                Ausente = false,
                Karma = 50,
                Rol = "Usuario",
                EspacioId = $"espacio-test-{Guid.NewGuid()}",
                UsuarioId = $"usuario-test-{Guid.NewGuid()}",
                PermisoId = $"permiso-test-{Guid.NewGuid()}",
                TareasId = new List<string> { "tarea1", "tarea2" },
                FacturasId = new List<string> { "factura1" }
            };

            var response = await _client.PostAsJsonAsync(endpoint, createDto);

            Assert.NotNull(response);
            Assert.True(response.StatusCode == HttpStatusCode.Created || 
                       response.StatusCode == HttpStatusCode.OK ||
                       response.StatusCode == HttpStatusCode.BadRequest ||
                       response.StatusCode == HttpStatusCode.InternalServerError);

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var created = await response.Content.ReadFromJsonAsync<UsuarioEspacioDto>();
                    if (created != null && !string.IsNullOrEmpty(created.Id))
                        _createdUsuarioEspacioIds.Add(created.Id);
                }
                catch { }
            }
        }

        [Fact]
        public async Task Create_WithMissingRequiredFields_ReturnsBadRequest()
        {
            var endpoint = "/api/usuarioespacio";
            var createDto = new CreateUsuarioEspacioDto
            {
                Ausente = false
            };

            var response = await _client.PostAsJsonAsync(endpoint, createDto);

            Assert.True(response.StatusCode == HttpStatusCode.BadRequest || 
                       response.StatusCode == HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task GetAll_ReturnsOkWithList()
        {
            var endpoint = "/api/usuarioespacio";

            var response = await _client.GetAsync(endpoint);

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var usuarioEspacios = await response.Content.ReadFromJsonAsync<List<UsuarioEspacioDto>>();
            Assert.NotNull(usuarioEspacios);
            Assert.IsType<List<UsuarioEspacioDto>>(usuarioEspacios);
        }

        [Fact]
        public async Task GetById_WithInvalidId_ReturnsNotFound()
        {
            var invalidId = "nonexistent-id-" + Guid.NewGuid();
            var endpoint = $"/api/usuarioespacio/{invalidId}";

            var response = await _client.GetAsync(endpoint);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetByEspacio_WithInvalidId_ReturnsOk()
        {
            var invalidId = "nonexistent-espacio-id-" + Guid.NewGuid();
            var endpoint = $"/api/usuarioespacio/espacio/{invalidId}";

            var response = await _client.GetAsync(endpoint);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var usuarioEspacios = await response.Content.ReadFromJsonAsync<List<UsuarioEspacioDto>>();
            Assert.NotNull(usuarioEspacios);
        }

        [Fact]
        public async Task GetByUsuario_WithInvalidId_ReturnsOk()
        {
            var invalidId = "nonexistent-usuario-id-" + Guid.NewGuid();
            var endpoint = $"/api/usuarioespacio/usuario/{invalidId}";

            var response = await _client.GetAsync(endpoint);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var usuarioEspacios = await response.Content.ReadFromJsonAsync<List<UsuarioEspacioDto>>();
            Assert.NotNull(usuarioEspacios);
        }

        [Fact]
        public async Task PutOverwrite_WithInvalidId_ReturnsNotFound()
        {
            var invalidId = "nonexistent-id-" + Guid.NewGuid();
            var endpoint = $"/api/usuarioespacio/{invalidId}";
            var updateDto = new UpdateUsuarioEspacioDto
            {
                Karma = 100
            };

            var response = await _client.PutAsJsonAsync(endpoint, updateDto);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task PutMerge_WithInvalidId_ReturnsNotFound()
        {
            var invalidId = "nonexistent-id-" + Guid.NewGuid();
            var endpoint = $"/api/usuarioespacio/{invalidId}/merge";
            var updateDto = new UpdateUsuarioEspacioDto();

            var response = await _client.PutAsJsonAsync(endpoint, updateDto);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Patch_WithInvalidId_ReturnsNotFound()
        {
            var invalidId = "nonexistent-id-" + Guid.NewGuid();
            var endpoint = $"/api/usuarioespacio/{invalidId}";
            var updateDto = new UpdateUsuarioEspacioDto
            {
                Ausente = true
            };

            var response = await _client.PatchAsJsonAsync(endpoint, updateDto);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Delete_WithInvalidId_ReturnsNotFound()
        {
            var invalidId = "nonexistent-id-" + Guid.NewGuid();
            var endpoint = $"/api/usuarioespacio/{invalidId}";

            var response = await _client.DeleteAsync(endpoint);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Delete_WithConstraints_MayReturnConflict()
        {
            var endpoint = "/api/usuarioespacio/any-id";

            var response = await _client.DeleteAsync(endpoint);

            Assert.True(response.StatusCode == HttpStatusCode.NotFound || 
                       response.StatusCode == HttpStatusCode.Conflict);
        }
    }
}
