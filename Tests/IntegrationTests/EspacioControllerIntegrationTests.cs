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

        public Task InitializeAsync() => Task.CompletedTask;

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

        [Fact]
        public async Task Create_WithValidData_ReturnsCreatedWithValidContent()
        {
            var endpoint = "/api/espacio";
            var createDto = new CreateEspacioDto { Nombre = $"Test Espacio {Guid.NewGuid()}" };
            var response = await _client.PostAsJsonAsync(endpoint, createDto);
            Assert.NotNull(response);
            Assert.True(response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK);
            var createdEspacio = await response.Content.ReadFromJsonAsync<EspacioDto>();
            Assert.NotNull(createdEspacio);
            Assert.NotEmpty(createdEspacio.Id);
            Assert.Equal(createDto.Nombre, createdEspacio.Nombre);
            _createdEspacioIds.Add(createdEspacio.Id);
        }

        [Fact]
        public async Task Create_WithMultipleEspacios_AllSucceed()
        {
            var endpoint = "/api/espacio";
            for (int i = 0; i < 3; i++)
            {
                var createDto = new CreateEspacioDto { Nombre = $"Espacio {i}-{Guid.NewGuid()}" };
                var response = await _client.PostAsJsonAsync(endpoint, createDto);
                Assert.True(response.IsSuccessStatusCode);
                var createdEspacio = await response.Content.ReadFromJsonAsync<EspacioDto>();
                Assert.NotNull(createdEspacio);
                Assert.NotEmpty(createdEspacio.Id);
                _createdEspacioIds.Add(createdEspacio.Id);
            }
        }

        [Fact]
        public async Task GetAll_ReturnsOkWithList()
        {
            var endpoint = "/api/espacio";
            var response = await _client.GetAsync(endpoint);
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var espacios = await response.Content.ReadFromJsonAsync<List<EspacioDto>>();
            Assert.NotNull(espacios);
            Assert.IsType<List<EspacioDto>>(espacios);
        }

        [Fact]
        public async Task GetById_WithValidId_ReturnsOkWithValidContent()
        {
            var createEndpoint = "/api/espacio";
            var createDto = new CreateEspacioDto { Nombre = $"GetByID Test {Guid.NewGuid()}" };
            var createResponse = await _client.PostAsJsonAsync(createEndpoint, createDto);
            var createdEspacio = await createResponse.Content.ReadFromJsonAsync<EspacioDto>();
            _createdEspacioIds.Add(createdEspacio.Id);
            var getEndpoint = $"/api/espacio/{createdEspacio.Id}";
            var getResponse = await _client.GetAsync(getEndpoint);
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var retrievedEspacio = await getResponse.Content.ReadFromJsonAsync<EspacioDto>();
            Assert.NotNull(retrievedEspacio);
            Assert.Equal(createdEspacio.Id, retrievedEspacio.Id);
            Assert.Equal(createdEspacio.Nombre, retrievedEspacio.Nombre);
        }

        [Fact]
        public async Task GetById_WithInvalid
