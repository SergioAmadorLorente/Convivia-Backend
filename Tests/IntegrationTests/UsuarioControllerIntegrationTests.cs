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

        public Task InitializeAsync() => Task.CompletedTask;

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

        [Fact]
        public async Task Create_WithValidData_ReturnsCreatedWithValidContent()
        {
            var endpoint = "/api/usuario";
            var createDto = new CreateUsuarioDto
            {
                Nombre = $"Test Usuario {Guid.NewGuid()}",
                Email = $"test-{Guid.NewGuid()}@example.com",
                Password = "password123"
            };

            var response = await _client.PostAsJsonAsync(endpoint, createDto);

            Assert.NotNull(response);
            Assert.True(response.IsSuccessStatusCode, $"Response status: {response.StatusCode}");

            var createdUser = await response.Content.ReadFromJsonAsync<UsuarioDto>();
            Assert.NotNull(createdUser);
            Assert.NotEmpty(createdUser.Id);
            Assert.Equal(createDto.Nombre, createdUser.Nombre);
            Assert.Equal(createDto.Email, createdUser.Email);

            _createdUserIds.Add(createdUser.Id);
        }

        [Fact]
        public async Task Create_WithMultipleValidUsers_AllSucceed()
        {
            var endpoint = "/api/usuario";
            var userCount = 3;

            for (int i = 0; i < userCount; i++)
            {
                var createDto = new CreateUsuarioDto
                {
                    Nombre = $"User {i}-{Guid.NewGuid()}",
                    Email = $"user{i}-{Guid.NewGuid()}@example.com",
                    Password = "password123"
                };
                var response = await _client.PostAsJsonAsync(endpoint, createDto);

                Assert.True(response.IsSuccessStatusCode);
                
                var createdUser = await response.Content.ReadFromJsonAsync<UsuarioDto>();
                Assert.NotNull(createdUser);
                Assert.NotEmpty(createdUser.Id);
                _createdUserIds.Add(createdUser.Id);
            }
        }

        [Fact]
        public async Task Create_WithMissingNombre_ReturnsBadRequest()
        {
            var endpoint = "/api/usuario";
            var createDto = new CreateUsuarioDto
            {
                Nombre = "",
                Email = $"test-{Guid.NewGuid()}@example.com",
                Password = "password123"
            };

            var response = await _client.PostAsJsonAsync(endpoint, createDto);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Create_WithMissingEmail_ReturnsBadRequest()
        {
            var endpoint = "/api/usuario";
            var createDto = new CreateUsuarioDto
            {
                Nombre = "Test Usuario",
                Email = "",
                Password = "password123"
            };

            var response = await _client.PostAsJsonAsync(endpoint, createDto);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Create_WithMissingPassword_ReturnsBadRequest()
        {
            var endpoint = "/api/usuario";
            var createDto = new CreateUsuarioDto
            {
                Nombre = "Test Usuario",
                Email = $"test-{Guid.NewGuid()}@example.com",
                Password = ""
            };

            var response = await _client.PostAsJsonAsync(endpoint, createDto);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetAll_ReturnsOkWithList()
        {
            var endpoint = "/api/usuario";

            var response = await _client.GetAsync(endpoint);

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var usuarios = await response.Content.ReadFromJsonAsync<List<UsuarioDto>>();
            Assert.NotNull(usuarios);
            Assert.IsType<List<UsuarioDto>>(usuarios);
        }

        [Fact]
        public async Task GetById_WithValidId_ReturnsOkWithValidContent()
        {
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

            var getEndpoint = $"/api/usuario/{createdUser.Id}";
            var getResponse = await _client.GetAsync(getEndpoint);

            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            
            var retrievedUser = await getResponse.Content.ReadFromJsonAsync<UsuarioDto>();
            Assert.NotNull(retrievedUser);
            Assert.Equal(createdUser.Id, retrievedUser.Id);
            Assert.Equal(createdUser.Nombre, retrievedUser.Nombre);
            Assert.Equal(createdUser.Email, retrievedUser.Email);
        }

        [Fact]
        public async Task GetById_WithInvalid
