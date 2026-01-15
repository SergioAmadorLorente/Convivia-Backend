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

        public Task InitializeAsync() => Task.CompletedTask;

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

        [Fact]
        public async Task Create_WithValidData_ReturnsCreatedWithValidContent()
        {
            var endpoint = "/api/factura";
            var createDto = new CreateFacturaDto
            {
                Nombre = $"Test Factura {Guid.NewGuid()}",
                Precio = 100.50m,
                Pagado = false,
                Reparto = new Dictionary<string, float> { { "user1", 100.50f } }
            };

            var response = await _client.PostAsJsonAsync(endpoint, createDto);

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

        [Fact]
        public async Task Create_WithMultipleValidFacturas_AllSucceed()
        {
            var endpoint = "/api/factura";

            for (int i = 0; i < 3; i++)
            {
                var createDto = new CreateFacturaDto
                {
                    Nombre = $"Factura {i}-{Guid.NewGuid()}",
                    Precio = 50.0m + i * 10,
                    Pagado = i % 2 == 0,
                    Reparto = new Dictionary<string, float> { { $"user{i}", 50.0f + i * 10 } }
                };
                var response = await _client.PostAsJsonAsync(endpoint, createDto);

                Assert.True(response.IsSuccessStatusCode);
                
                var createdFactura = await response.Content.ReadFromJsonAsync<FacturaDto>();
                Assert.NotNull(createdFactura);
                Assert.NotEmpty(createdFactura.Id);
                _createdFacturaIds.Add(createdFactura.Id);
            }
        }

        [Fact]
        public async Task Create_WithMissingNombre_ReturnsBadRequest()
        {
            var endpoint = "/api/factura";
            var createDto = new CreateFacturaDto { Nombre = "", Precio = 100.50m };
            var response = await _client.PostAsJsonAsync(endpoint, createDto);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Create_WithNegativePrice_ReturnsBadRequest()
        {
            var endpoint = "/api/factura";
            var createDto = new CreateFacturaDto { Nombre = "Test Factura", Precio = -50m };
            var response = await _client.PostAsJsonAsync(endpoint, createDto);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Create_WithZeroPrice_Succeeds()
        {
            var endpoint = "/api/factura";
            var createDto = new CreateFacturaDto { Nombre = $"Zero Price Factura {Guid.NewGuid()}", Precio = 0m, Reparto = new Dictionary<string, float>() };
            var response = await _client.PostAsJsonAsync(endpoint, createDto);
            Assert.True(response.IsSuccessStatusCode);
            var createdFactura = await response.Content.ReadFromJsonAsync<FacturaDto>();
            Assert.NotNull(createdFactura);
            _createdFacturaIds.Add(createdFactura.Id);
        }

        [Fact]
        public async Task GetAll_ReturnsOkWithList()
        {
            var endpoint = "/api/factura";
            var response = await _client.GetAsync(endpoint);
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var facturas = await response.Content.ReadFromJsonAsync<List<FacturaDto>>();
            Assert.NotNull(facturas);
            Assert.IsType<List<FacturaDto>>(facturas);
        }

        [Fact]
        public async Task GetById_WithValidId_ReturnsOkWithValidContent()
        {
            var createEndpoint = "/api/factura";
            var createDto = new CreateFacturaDto { Nombre = $"GetByID Test {Guid.NewGuid()}", Precio = 200.75m, Pagado = true, Reparto = new Dictionary<string, float> { { "user1", 200.75f } } };
            var createResponse = await _client.PostAsJsonAsync(createEndpoint, createDto);
            var createdFactura = await createResponse.Content.ReadFromJsonAsync<FacturaDto>();
            _createdFacturaIds.Add(createdFactura.Id);

            var getEndpoint = $"/api/factura/{createdFactura.Id}";
            var getResponse = await _client.GetAsync(getEndpoint);
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var retrievedFactura = await getResponse.Content.ReadFromJsonAsync<FacturaDto>();
            Assert.NotNull(retrievedFactura);
            Assert.Equal(createdFactura.Id, retrievedFactura.Id);
            Assert.Equal(createdFactura.Nombre, retrievedFactura.Nombre);
            Assert.Equal(createdFactura.Precio, retrievedFactura.Precio);
        }

        [Fact]
        public async Task GetById_WithInvalid
