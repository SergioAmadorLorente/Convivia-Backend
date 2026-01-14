using Convivia.Application.Repositories;
using Convivia.Application.Services;
using Convivia.Domain.Entities;
using Convivia.Shared.DTOs;
using MapsterMapper;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Convivia.Application.Tests.Services
{
    public class PlantillaTareaServiceTests
    {
        private readonly Mock<IPlantillaTareaRepository> _repoMock;
        private readonly Mock<ITareaRepository> _tareaRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<PlantillaTareaService>> _loggerMock;

        private readonly PlantillaTareaService _sut;

        public PlantillaTareaServiceTests()
        {
            _repoMock = new Mock<IPlantillaTareaRepository>(MockBehavior.Strict);
            _tareaRepoMock = new Mock<ITareaRepository>(MockBehavior.Strict);
            _mapperMock = new Mock<IMapper>(MockBehavior.Strict);
            _loggerMock = new Mock<ILogger<PlantillaTareaService>>();

            _sut = new PlantillaTareaService(
                _repoMock.Object,
                _mapperMock.Object,
                _tareaRepoMock.Object,
                _loggerMock.Object
            );
        }

        // -------------------------------------------------------------
        // ADD
        // -------------------------------------------------------------
        [Fact]
        public async Task AddAsync_ShouldThrow_WhenDtoNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _sut.AddAsync(null!, "esp1"));
        }

        [Fact]
        public async Task AddAsync_ShouldThrow_WhenEspacioIdInvalid()
        {
            var dto = new CreatePlantillaTareaDto { Nombre = "X" };

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _sut.AddAsync(dto, " "));
        }

        [Fact]
        public async Task AddAsync_ShouldCallRepository_AndReturnId()
        {
            var dto = new CreatePlantillaTareaDto
            {
                Nombre = "Plantilla",
                Descripcion = "Desc",
                karma = 5
            };

            var entity = new PlantillaTarea { Nombre = "Plantilla" };

            _mapperMock
                .Setup(m => m.Map<PlantillaTarea>(dto))
                .Returns(entity);

            _repoMock
                .Setup(r => r.AddAsync(entity, It.IsAny<CancellationToken>()))
                .ReturnsAsync("p123");

            var result = await _sut.AddAsync(dto, "esp1");

            Assert.Equal("p123", result);
        }

        // -------------------------------------------------------------
        // GET ALL BY ESPACIO
        // -------------------------------------------------------------
        [Fact]
        public async Task GetAllByEspacioAsync_ShouldThrow_WhenEspacioIdInvalid()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _sut.GetAllByEspacioAsync(" "));
        }

        [Fact]
        public async Task GetAllByEspacioAsync_ShouldReturnFilteredPlantillas()
        {
            var all = new List<PlantillaTarea>
            {
                new PlantillaTarea { Id = "p1", EspacioId = "esp1" },
                new PlantillaTarea { Id = "p2", EspacioId = "esp2" }
            };

            _repoMock
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(all);

            var result = await _sut.GetAllByEspacioAsync("esp1");

            Assert.Single(result);
            Assert.Equal("p1", result.First().Id);
        }

        // -------------------------------------------------------------
        // GET BY ESPACIO + ID
        // -------------------------------------------------------------
        [Fact]
        public async Task GetByEspacioAndIdAsync_ShouldThrow_WhenIdsInvalid()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _sut.GetByEspacioAndIdAsync(" ", "p1"));

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _sut.GetByEspacioAndIdAsync("esp1", " "));
        }

        [Fact]
        public async Task GetByEspacioAndIdAsync_ShouldReturnNull_WhenNotFound()
        {
            _repoMock
                .Setup(r => r.GetByEspacioAndIdAsync("esp1", "p1", It.IsAny<CancellationToken>()))
                .ReturnsAsync((PlantillaTarea?)null);

            var result = await _sut.GetByEspacioAndIdAsync("esp1", "p1");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByEspacioAndIdAsync_ShouldReturnDto_WhenFound()
        {
            var entity = new PlantillaTarea { Id = "p1", Nombre = "Plantilla" };

            _repoMock
                .Setup(r => r.GetByEspacioAndIdAsync("esp1", "p1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity);

            _mapperMock
                .Setup(m => m.Map<PlantillaTareaDto>(entity))
                .Returns(new PlantillaTareaDto { Id = "p1", Nombre = "Plantilla" });

            var result = await _sut.GetByEspacioAndIdAsync("esp1", "p1");

            Assert.NotNull(result);
            Assert.Equal("p1", result!.Id);
        }

        // -------------------------------------------------------------
        // DELETE
        // -------------------------------------------------------------
        [Fact]
        public async Task DeleteAsync_ShouldThrow_WhenIdsInvalid()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _sut.DeleteAsync(" ", "p1"));

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _sut.DeleteAsync("esp1", " "));
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnFalse_WhenPlantillaNotFound()
        {
            _repoMock
                .Setup(r => r.GetByEspacioAndIdAsync("esp1", "p1", It.IsAny<CancellationToken>()))
                .ReturnsAsync((PlantillaTarea?)null);

            var result = await _sut.DeleteAsync("esp1", "p1");

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeletePlantilla()
        {
            var entity = new PlantillaTarea { Id = "p1", EspacioId = "esp1" };

            _repoMock
                .Setup(r => r.GetByEspacioAndIdAsync("esp1", "p1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity);

            _repoMock
                .Setup(r => r.DeleteAsync("p1", It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await _sut.DeleteAsync("esp1", "p1");

            Assert.True(result);
        }

        // -------------------------------------------------------------
        // UPDATE (FULL)
        // -------------------------------------------------------------
        [Fact]
        public async Task UpdateAsync_ShouldThrow_WhenDtoNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _sut.UpdateAsync("esp1", "p1", null!));
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnNull_WhenPlantillaNotFound()
        {
            _repoMock
                .Setup(r => r.GetByEspacioAndIdAsync("esp1", "p1", It.IsAny<CancellationToken>()))
                .ReturnsAsync((PlantillaTarea?)null);

            var result = await _sut.UpdateAsync("esp1", "p1", new UpdatePlantillaTareaDto());

            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdatePlantilla()
        {
            var entity = new PlantillaTarea { Id = "p1", EspacioId = "esp1" };
            var dto = new UpdatePlantillaTareaDto { Nombre = "Nuevo" };

            _repoMock
                .Setup(r => r.GetByEspacioAndIdAsync("esp1", "p1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity);

            _mapperMock
                .Setup(m => m.Map(dto, entity))
                .Returns(entity);

            _repoMock
                .Setup(r => r.UpdateAsync("p1", entity, false, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mapperMock
                .Setup(m => m.Map<PlantillaTareaDto>(entity))
                .Returns(new PlantillaTareaDto { Id = "p1", Nombre = "Nuevo" });

            var result = await _sut.UpdateAsync("esp1", "p1", dto);

            Assert.NotNull(result);
            Assert.Equal("Nuevo", result!.Nombre);
        }
    }
}