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
    /// Pruebas de integración para PlantillaTareaController.
    /// Valida operaciones de actualización de plantillas de tareas.
    /// </summary>
    public class PlantillaTareaControllerIntegrationTests : IClassFixture<ConviviaWebApplicationFactory>
    {
        private readonly ConviviaWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public PlantillaTareaControllerIntegrationTests(ConviviaWebApplicationFactory factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _client = _factory.CreateClient();
        }

        // =============================================
        // PRUEBAS DE ACTUALIZAR PLANTILLA
        // =============================================

        /// <summary>
        /// Prueba que actualizar plantilla de tarea con espacio inexistente retorna NotFound.
        /// </summary>
        [Fact]
        public async Task Update_WithInvalidEspacioId_ReturnsNotFound()
        {
            // Arrange
            var espacioId = $"nonexistent-espacio-{Guid.NewGuid()}";
            var plantillaId = $"nonexistent-plantilla-{Guid.NewGuid()}";
            var endpoint = $"/api/espacio/{espacioId}/{plantillaId}";
            var updateDto = new UpdatePlantillaTareaDto
            {
                Nombre = "Updated Plantilla"
            };

            // Act
            var response = await _client.PatchAsJsonAsync(endpoint, updateDto);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        /// <summary>
        /// Prueba que actualizar plantilla inexistente retorna NotFound.
        /// </summary>
        [Fact]
        public async Task Update_WithInvalidPlantillaId_ReturnsNotFound()
        {
            // Arrange
            var espacioId = $"espacio-test-{Guid.NewGuid()}";
            var plantillaId = $"nonexistent-plantilla-{Guid.NewGuid()}";
            var endpoint = $"/api/espacio/{espacioId}/{plantillaId}";
            var updateDto = new UpdatePlantillaTareaDto
            {
                Nombre = "Updated Plantilla",
                Descripcion = "Nueva descripción"
            };

            // Act
            var response = await _client.PatchAsJsonAsync(endpoint, updateDto);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        /// <summary>
        /// Prueba que actualizar plantilla sin datos no causa error (patch vacío).
        /// </summary>
        [Fact]
        public async Task Update_WithEmptyData_ReturnsNotFound()
        {
            // Arrange
            var espacioId = $"espacio-test-{Guid.NewGuid()}";
            var plantillaId = $"plantilla-test-{Guid.NewGuid()}";
            var endpoint = $"/api/espacio/{espacioId}/{plantillaId}";
            var updateDto = new UpdatePlantillaTareaDto();

            // Act
            var response = await _client.PatchAsJsonAsync(endpoint, updateDto);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        /// <summary>
        /// Prueba patch con solo nombre (actualización parcial).
        /// </summary>
        [Fact]
        public async Task Update_WithOnlyNombre_ReturnsNotFound()
        {
            // Arrange
            var espacioId = $"espacio-test-{Guid.NewGuid()}";
            var plantillaId = $"plantilla-test-{Guid.NewGuid()}";
            var endpoint = $"/api/espacio/{espacioId}/{plantillaId}";
            var updateDto = new UpdatePlantillaTareaDto
            {
                Nombre = "Nuevo Nombre"
            };

            // Act
            var response = await _client.PatchAsJsonAsync(endpoint, updateDto);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        /// <summary>
        /// Prueba patch con solo descripción (actualización parcial).
        /// </summary>
        [Fact]
        public async Task Update_WithOnlyDescripcion_ReturnsNotFound()
        {
            // Arrange
            var espacioId = $"espacio-test-{Guid.NewGuid()}";
            var plantillaId = $"plantilla-test-{Guid.NewGuid()}";
            var endpoint = $"/api/espacio/{espacioId}/{plantillaId}";
            var updateDto = new UpdatePlantillaTareaDto
            {
                Descripcion = "Nueva descripción"
            };

            // Act
            var response = await _client.PatchAsJsonAsync(endpoint, updateDto);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        /// <summary>
        /// Prueba patch con datos válidos pero planilla inexistente.
        /// </summary>
        [Fact]
        public async Task Update_WithValidDataButInvalidPlantilla_ReturnsNotFound()
        {
            // Arrange
            var espacioId = $"espacio-test-{Guid.NewGuid()}";
            var plantillaId = $"plantilla-test-{Guid.NewGuid()}";
            var endpoint = $"/api/espacio/{espacioId}/{plantillaId}";
            var updateDto = new UpdatePlantillaTareaDto
            {
                Nombre = "Updated Name",
                Descripcion = "Updated Description",
                karma = 100,
                DiasRepeticion = new List<int> { 1, 2, 3 }
            };

            // Act
            var response = await _client.PatchAsJsonAsync(endpoint, updateDto);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
