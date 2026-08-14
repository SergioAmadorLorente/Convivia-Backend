using Convivia.Application.Repositories;
using Convivia.Application.Services;
using Convivia.Domain.Entities;
using Convivia.Shared.DTOs;
using MapsterMapper;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Convivia.Tests.Services
{
    public class UsuarioEspacioServiceTests
    {
        private readonly Mock<IUsuarioEspacioRepository> _usuarioEspacioRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<UsuarioEspacioService>> _loggerMock;
        private readonly Mock<IFacturaRepository> _facturaRepoMock;
        private readonly Mock<ITareaRepository> _tareaRepoMock;
        private readonly Mock<IKarmaEstadisticasRepository> _karmaRepoMock;
        private readonly UsuarioEspacioService _sut;

        public UsuarioEspacioServiceTests()
        {
            _usuarioEspacioRepoMock = new Mock<IUsuarioEspacioRepository>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<UsuarioEspacioService>>();
            _facturaRepoMock = new Mock<IFacturaRepository>();
            _tareaRepoMock = new Mock<ITareaRepository>();
            _karmaRepoMock = new Mock<IKarmaEstadisticasRepository>();

            _sut = new UsuarioEspacioService(
                _usuarioEspacioRepoMock.Object,
                _mapperMock.Object,
                _loggerMock.Object,
                _facturaRepoMock.Object,
                _tareaRepoMock.Object,
                _karmaRepoMock.Object);
        }

        [Fact]
        public async Task ObtenerUsuarioEspacioAsync_EnrichesKarmaFromKarmaEstadisticas()
        {
            // Arrange
            var ueId = "ue-123";
            var espacioId = "espacio-456";
            var domain = new UsuarioEspacio
            {
                Id = ueId,
                EspacioId = espacioId,
                UsuarioId = "user-789",
                Karma = 0
            };
            var dto = new UsuarioEspacioDto
            {
                Id = ueId,
                EspacioId = espacioId,
                UsuarioId = "user-789",
                Karma = 0
            };
            var karmaStats = new KarmaEstadisticas
            {
                Id = "k-1",
                UsuarioEspacioId = ueId,
                KarmaTotal = 45,
                KarmaSemanal = 15,
                KarmaMensual = 30
            };

            _usuarioEspacioRepoMock.Setup(r => r.GetByIdAsync(ueId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(domain);
            _mapperMock.Setup(m => m.Map<UsuarioEspacioDto>(domain))
                .Returns(dto);
            _karmaRepoMock.Setup(r => r.GetByUsuarioEspacioIdAsync(espacioId, ueId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(karmaStats);

            // Act
            var result = await _sut.ObtenerUsuarioEspacioAsync(ueId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(45, result.Karma);
        }

        [Fact]
        public async Task ObtenerPorEspacioAsync_EnrichesAllUsersKarmaFromKarmaEstadisticas()
        {
            // Arrange
            var espacioId = "espacio-1";
            var domainList = new List<UsuarioEspacio>
            {
                new UsuarioEspacio { Id = "ue1", EspacioId = espacioId, Karma = 0 },
                new UsuarioEspacio { Id = "ue2", EspacioId = espacioId, Karma = 0 }
            };

            _usuarioEspacioRepoMock.Setup(r => r.GetByEspacioIdAsync(espacioId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(domainList);

            _mapperMock.Setup(m => m.Map<UsuarioEspacioDto>(It.Is<UsuarioEspacio>(u => u.Id == "ue1")))
                .Returns(new UsuarioEspacioDto { Id = "ue1", EspacioId = espacioId, Karma = 0 });
            _mapperMock.Setup(m => m.Map<UsuarioEspacioDto>(It.Is<UsuarioEspacio>(u => u.Id == "ue2")))
                .Returns(new UsuarioEspacioDto { Id = "ue2", EspacioId = espacioId, Karma = 0 });

            var karmaStatsList = new List<KarmaEstadisticas>
            {
                new KarmaEstadisticas { UsuarioEspacioId = "ue1", KarmaTotal = 100 },
                new KarmaEstadisticas { UsuarioEspacioId = "ue2", KarmaTotal = 50 }
            };

            _karmaRepoMock.Setup(r => r.GetAllByEspacioIdAsync(espacioId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(karmaStatsList);

            // Act
            var result = (await _sut.ObtenerPorEspacioAsync(espacioId)).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(100, result.First(u => u.Id == "ue1").Karma);
            Assert.Equal(50, result.First(u => u.Id == "ue2").Karma);
        }

        [Fact]
        public async Task ObtenerUsuarioEspacioAsync_WhenKarmaStatsNull_KeepsOriginalKarma()
        {
            // Arrange
            var ueId = "ue-123";
            var espacioId = "espacio-456";
            var domain = new UsuarioEspacio
            {
                Id = ueId,
                EspacioId = espacioId,
                UsuarioId = "user-789",
                Karma = 10
            };
            var dto = new UsuarioEspacioDto
            {
                Id = ueId,
                EspacioId = espacioId,
                UsuarioId = "user-789",
                Karma = 10
            };

            _usuarioEspacioRepoMock.Setup(r => r.GetByIdAsync(ueId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(domain);
            _mapperMock.Setup(m => m.Map<UsuarioEspacioDto>(domain))
                .Returns(dto);
            _karmaRepoMock.Setup(r => r.GetByUsuarioEspacioIdAsync(espacioId, ueId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((KarmaEstadisticas?)null);

            // Act
            var result = await _sut.ObtenerUsuarioEspacioAsync(ueId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(10, result.Karma);
        }
    }
}
