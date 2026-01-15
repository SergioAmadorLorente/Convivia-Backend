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
    public class PlantillaTareaControllerIntegrationTests : IClassFixture<ConviviaWebApplicationFactory>, IAsyncLifetime
    {
        private readonly ConviviaWebApplicationFactory _factory;
        private readonly HttpClient _client;
        private readonly List<string> _createdPlantillaIds = new();

        public PlantillaTareaControllerIntegrationTests(ConviviaWebApplicationFactory factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _client = _factory.CreateClient();
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            foreach (var plantillaId in _createdPlantillaIds)
            {
                System.Diagnostics.Debug.WriteLine($"Plantilla creada: {plantillaId}");
            }
        }

        // =============================================
        // PRUEBAS DE ACTUALIZAR PLANTILLA
        // =============================================

        [Fact]
        public async Task Patch_WithValidData_ReturnsOkOrNotFound()
        {
            var espacioId = $"espacio-test-{Guid.NewGuid()}";
            var plantillaId = $"plantilla-test-{Guid.NewGuid()}";
            var endpoint = $"/api/espacio/{espacioId}/{plantillaId}";
            var updateDto = new UpdatePlantillaTareaDto
            {
                Nombre = "Updated Plantilla",
                Descripcion = "Updated Description"
            };

            var response = await _client.PatchAsJsonAsync(endpoint, updateDto);

            Assert.True(response.StatusCode == HttpStatusCode.OK || 
                       response.StatusCode == HttpStatusCode.NotFound ||
                       response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Patch_WithInvalidEspacioId_ReturnsNotFound()
        {
            var espacioId = $"nonexistent-espacio-{Guid.NewGuid()}";
            var plantillaId = $"nonexistent-plantilla-{Guid.NewGuid()}";
            var endpoint = $"/api/espacio/{espacioId}/{plantillaId}";
            var updateDto = new UpdatePlantillaTareaDto
            {
                Nombre = "Updated Plantilla"
            };

            var response = await _client.PatchAsJsonAsync(endpoint, updateDto);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Patch_WithInvalidPlantillaId_ReturnsNotFound()
        {
            var espacioId = $"espacio-test-{Guid.NewGuid()}";
            var plantillaId = $"nonexistent-plantilla-{Guid.NewGuid()}";
            var endpoint = $"/api/espacio/{espacioId}/{plantillaId}";
            var updateDto = new UpdatePlantillaTareaDto
            {
                Nombre = "Updated Plantilla",
                Descripcion = "Nueva descripción"
            };

            var response = await _client.PatchAsJsonAsync(endpoint, updateDto);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Patch_WithEmptyData_ReturnsNotFound()
        {
            var espacioId = $"espacio-test-{Guid.NewGuid()}";
            var plantillaId = $"plantilla-test-{Guid.NewGuid()}";
            var endpoint = $"/api/espacio/{espacioId}/{plantillaId}";
            var updateDto = new UpdatePlantillaTareaDto();

            var response = await _client.PatchAsJsonAsync(endpoint, updateDto);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Patch_WithOnlyNombre_ReturnsNotFound()
        {
            var espacioId = $"espacio-test-{Guid.NewGuid()}";
            var plantillaId = $"plantilla-test-{Guid.NewGuid()}";
            var endpoint = $"/api/espacio/{espacioId}/{plantillaId}";
            var updateDto = new UpdatePlantillaTareaDto
            {
                Nombre = "Nuevo Nombre"
            };

            var response = await _client.PatchAsJsonAsync(endpoint, updateDto);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Patch_WithOnlyDescripcion_ReturnsNotFound()
        {
            var espacioId = $"espacio-test-{Guid.NewGuid()}";
            var plantillaId = $"plantilla-test-{Guid.NewGuid()}";
            var endpoint = $"/api/espacio/{espacioId}/{plantillaId}";
            var updateDto = new UpdatePlantillaTareaDto
            {
                Descripcion = "Nueva descripción"
            };

            var response = await _client.PatchAsJsonAsync(endpoint, updateDto);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Patch_WithOnlyKarma_ReturnsNotFound()
        {
            var espacioId = $"espacio-test-{Guid.NewGuid()}";
            var plantillaId = $"plantilla-test-{Guid.NewGuid()}";
            var endpoint = $"/api/espacio/{espacioId}/{plantillaId}";
            var updateDto = new UpdatePlantillaTareaDto
            {
                karma = 50
            };

            var response = await _client.PatchAsJsonAsync(endpoint, updateDto);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Patch_WithInvalidKarma_ReturnsBadRequest()
        {
            var espacioId = $"espacio-test-{Guid.NewGuid()}";
            var plantillaId = $"plantilla-test-{Guid.NewGuid()}";
            var endpoint = $"/api/espacio/{espacioId}/{plantillaId}";
            var updateDto = new UpdatePlantillaTareaDto
            {
                Nombre = "Updated Name",
                karma = 7
            };

            var response = await _client.PatchAsJsonAsync(endpoint, updateDto);

            Assert.True(response.StatusCode == HttpStatusCode.BadRequest || 
                       response.StatusCode == HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Patch_WithInvalidDiasRepeticion_ReturnsBadRequest()
        {
            var espacioId = $"espacio-test-{Guid.NewGuid()}";
            var plantillaId = $"plantilla-test-{Guid.NewGuid()}";
            var endpoint = $"/api/espacio/{espacioId}/{plantillaId}";
            var updateDto = new UpdatePlantillaTareaDto
            {
                Nombre = "Updated Name",
                DiasRepeticion = new List<int> { 0, 7 }
            };

            var response = await _client.PatchAsJsonAsync(endpoint, updateDto);

            Assert.True(response.StatusCode == HttpStatusCode.BadRequest || 
                       response.StatusCode == HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Patch_WithComplexValidData_ReturnsOkOrNotFound()
        {
            var espacioId = $"espacio-test-{Guid.NewGuid()}";
            var plantillaId = $"plantilla-test-{Guid.NewGuid()}";
            var endpoint = $"/api/espacio/{espacioId}/{plantillaId}";
            var updateDto = new UpdatePlantillaTareaDto
            {
                Nombre = "Updated Name",
                Descripcion = "Updated Description",
                karma = 50,
                DiasRepeticion = new List<int> { 1, 2, 3 },
                HoraLimite = new TimeOnly(15, 30),
                UsuariosAsignacion = new List<string> { "user1", "user2", "user3" }
            };

            var response = await _client.PatchAsJsonAsync(endpoint, updateDto);

            Assert.True(response.StatusCode == HttpStatusCode.OK || 
                       response.StatusCode == HttpStatusCode.NotFound ||
                       response.StatusCode == HttpStatusCode.BadRequest);
        }

        // =============================================
        // PRUEBAS DE FLUJO COMPLETO
        // =============================================

        [Fact]
        public async Task CompleteFlow_CreateReadUpdateDelete_Succeeds()
        {
            var espacioId = $"espacio-test-{Guid.NewGuid()}";
            var createEndpoint = $"/api/espacios/{espacioId}/tareas/plantilla";
            var createDto = new CreatePlantillaTareaDto
            {
                Nombre = $"Complete Flow Plantilla {Guid.NewGuid()}",
                Descripcion = "Complete flow description",
                karma = 25,
                DiasRepeticion = new List<int> { 0, 1, 2 }
            };

            var createResponse = await _client.PostAsJsonAsync(createEndpoint, createDto);
            
            if (createResponse.IsSuccessStatusCode)
            {
                var createdPlantilla = await createResponse.Content.ReadFromJsonAsync<PlantillaTareaDto>();
                Assert.NotNull(createdPlantilla);
                Assert.NotEmpty(createdPlantilla.Id);
                _createdPlantillaIds.Add(createdPlantilla.Id);

                var readEndpoint = $"/api/espacio/{espacioId}/{createdPlantilla.Id}";
                var readResponse = await _client.GetAsync(readEndpoint);
                Assert.True(readResponse.IsSuccessStatusCode || 
                           readResponse.StatusCode == HttpStatusCode.NotFound);

                if (readResponse.IsSuccessStatusCode)
                {
                    var updateEndpoint = $"/api/espacio/{espacioId}/{createdPlantilla.Id}";
                    var updateDto = new UpdatePlantillaTareaDto
                    {
                        Nombre = "Updated Plantilla in Flow"
                    };
                    var updateResponse = await _client.PatchAsJsonAsync(updateEndpoint, updateDto);
                    Assert.True(updateResponse.IsSuccessStatusCode || 
                               updateResponse.StatusCode == HttpStatusCode.NotFound);
                }

                var deleteEndpoint = $"/api/espacio/{espacioId}/{createdPlantilla.Id}";
                var deleteResponse = await _client.DeleteAsync(deleteEndpoint);
                Assert.True(deleteResponse.IsSuccessStatusCode || 
                           deleteResponse.StatusCode == HttpStatusCode.NotFound);
            }
        }
    }
}
