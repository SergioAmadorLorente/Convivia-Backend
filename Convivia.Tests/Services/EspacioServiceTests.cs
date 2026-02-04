using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Convivia.Application.Repositories;
using Convivia.Application.Services;
using Convivia.Domain.Entities;
using Convivia.Shared.DTOs;
using MapsterMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Convivia.Tests.Services
{
    public class EspacioServiceTests
    {
        private readonly Mock<IEspacioRepository> _espacioRepo;
        private readonly Mock<IPlantillaTareaRepository> _plantillaRepo;
        private readonly Mock<IUsuarioEspacioRepository> _usuarioEspacioRepo;
        private readonly Mock<IUsuarioRepository> _usuarioRepo;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<ILogger<EspacioService>> _logger;
        private readonly Mock<IUsuarioEspacioService> _usuarioEspacioService;
        private readonly Mock<TareaService> _tareaService;
        private readonly IMemoryCache _cache;

        private readonly EspacioService _service;

        public EspacioServiceTests()
        {
            _espacioRepo = new Mock<IEspacioRepository>();
            _plantillaRepo = new Mock<IPlantillaTareaRepository>();
            _usuarioEspacioRepo = new Mock<IUsuarioEspacioRepository>();
            _usuarioRepo = new Mock<IUsuarioRepository>();
            _mapper = new Mock<IMapper>();
            _logger = new Mock<ILogger<EspacioService>>();
            _usuarioEspacioService = new Mock<IUsuarioEspacioService>();
            _tareaService = new Mock<TareaService>(
                MockBehavior.Loose,
                Mock.Of<ITareaRepository>(),
                Mock.Of<IMapper>(),
                Mock.Of<ILogger<TareaService>>()
            );

            _cache = new MemoryCache(new MemoryCacheOptions());

            _service = new EspacioService(
                _plantillaRepo.Object,
                _usuarioEspacioRepo.Object,
                _logger.Object,
                _espacioRepo.Object,
                _mapper.Object,
                _usuarioRepo.Object,
                _cache,
                _usuarioEspacioService.Object,
                _tareaService.Object
            );
        }

        // -------------------------------------------------------------
        // CREAR ESPACIO
        // -------------------------------------------------------------
        [Fact]
        public async Task CrearEspacioAsync_Throws_WhenDtoNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.CrearEspacioAsync(null));
        }

        [Fact]
        public async Task CrearEspacioAsync_Throws_WhenNombreEmpty()
        {
            var dto = new CreateEspacioDto { Nombre = "" };
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CrearEspacioAsync(dto));
        }

        [Fact]
        public async Task CrearEspacioAsync_ReturnsDto_WhenCreated()
        {
            var dto = new CreateEspacioDto { Nombre = "Casa" };
            var domain = new Espacio("Casa");

            _mapper.Setup(m => m.Map<Espacio>(dto)).Returns(domain);
            _espacioRepo.Setup(r => r.AddAsync(domain, It.IsAny<CancellationToken>()))
                        .ReturnsAsync("abc123");

            var saved = new Espacio("Casa") { Id = "abc123" };
            _espacioRepo.Setup(r => r.GetByIdAsync("abc123", It.IsAny<CancellationToken>()))
                        .ReturnsAsync(saved);

            var dtoMapped = new EspacioDto { Id = "abc123", Nombre = "Casa" };
            _mapper.Setup(m => m.Map<EspacioDto>(saved)).Returns(dtoMapped);

            var result = await _service.CrearEspacioAsync(dto);

            Assert.Equal("abc123", result.Id);
            Assert.Equal("Casa", result.Nombre);
        }

        // -------------------------------------------------------------
        // OBTENER ESPACIO
        // -------------------------------------------------------------
        [Fact]
        public async Task ObtenerEspacioAsync_Throws_WhenIdEmpty()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.ObtenerEspacioAsync(""));
        }

        [Fact]
        public async Task ObtenerEspacioAsync_ReturnsNull_WhenNotFound()
        {
            _espacioRepo.Setup(r => r.GetByIdAsync("x", It.IsAny<CancellationToken>()))
                        .ReturnsAsync((Espacio)null);

            var result = await _service.ObtenerEspacioAsync("x");

            Assert.Null(result);
        }

        [Fact]
        public async Task ObtenerEspacioAsync_ReturnsDto_WhenFound()
        {
            var domain = new Espacio("Casa") { Id = "abc" };
            _espacioRepo.Setup(r => r.GetByIdAsync("abc", It.IsAny<CancellationToken>()))
                        .ReturnsAsync(domain);

            var dto = new EspacioDto { Id = "abc", Nombre = "Casa" };
            _mapper.Setup(m => m.Map<EspacioDto>(domain)).Returns(dto);

            var result = await _service.ObtenerEspacioAsync("abc");

            Assert.Equal("abc", result.Id);
        }

        // -------------------------------------------------------------
        // LISTAR TODAS
        // -------------------------------------------------------------
        [Fact]
        public async Task ListarTodasAsync_ReturnsEmpty_WhenRepoNull()
        {
            _espacioRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                        .ReturnsAsync((IEnumerable<Espacio>)null);

            var result = await _service.ListarTodasAsync();

            Assert.Empty(result);
        }

        [Fact]
        public async Task ListarTodasAsync_MapsAll()
        {
            var list = new List<Espacio>
            {
                new Espacio("A"){Id="1"},
                new Espacio("B"){Id="2"}
            };

            _espacioRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                        .ReturnsAsync(list);

            _mapper.Setup(m => m.Map<EspacioDto>(It.IsAny<Espacio>()))
                   .Returns<Espacio>(e => new EspacioDto { Id = e.Id, Nombre = e.Nombre });

            var result = await _service.ListarTodasAsync();

            Assert.Equal(2, result.Count);
        }

        // -------------------------------------------------------------
        // OBTENER POR DIRECCION
        // -------------------------------------------------------------
        [Fact]
        public async Task ObtenerPorDireccionAsync_ReturnsEmpty_WhenDireccionEmpty()
        {
            var result = await _service.ObtenerPorDireccionAsync("");
            Assert.Empty(result);
        }

        [Fact]
        public async Task ObtenerPorDireccionAsync_Throws_WhenRepoFails()
        {
            _espacioRepo.Setup(r => r.GetByDireccionAsync("x", It.IsAny<CancellationToken>()))
                        .ThrowsAsync(new Exception("fail"));

            await Assert.ThrowsAsync<Exception>(() =>
                _service.ObtenerPorDireccionAsync("x"));
        }

        // -------------------------------------------------------------
        // ACTUALIZAR COMPLETO
        // -------------------------------------------------------------
        [Fact]
        public async Task ActualizarEspacioCompletoAsync_Throws_WhenIdEmpty()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.ActualizarEspacioCompletoAsync("", new UpdateEspacioDto()));
        }

        [Fact]
        public async Task ActualizarEspacioCompletoAsync_Throws_WhenDtoNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.ActualizarEspacioCompletoAsync("1", null));
        }

        [Fact]
        public async Task ActualizarEspacioCompletoAsync_ReturnsDto_WhenUpdated()
        {
            var dto = new UpdateEspacioDto { Nombre = "Nuevo" };
            var domain = new Espacio("Nuevo") { Id = "1" };

            _mapper.Setup(m => m.Map<Espacio>(dto)).Returns(domain);

            var updated = new Espacio("Nuevo") { Id = "1" };
            _espacioRepo.Setup(r => r.GetByIdAsync("1", It.IsAny<CancellationToken>()))
                        .ReturnsAsync(updated);

            var dtoMapped = new EspacioDto { Id = "1", Nombre = "Nuevo" };
            _mapper.Setup(m => m.Map<EspacioDto>(updated)).Returns(dtoMapped);

            var result = await _service.ActualizarEspacioCompletoAsync("1", dto);

            Assert.Equal("1", result.Id);
        }

        // -------------------------------------------------------------
        // ACTUALIZAR MERGE
        // -------------------------------------------------------------
        [Fact]
        public async Task ActualizarEspacioMergeAsync_ReturnsNull_WhenNotFound()
        {
            _espacioRepo.Setup(r => r.GetByIdAsync("1", It.IsAny<CancellationToken>()))
                        .ReturnsAsync((Espacio)null);

            var result = await _service.ActualizarEspacioMergeAsync("1", new UpdateEspacioDto());

            Assert.Null(result);
        }

        // -------------------------------------------------------------
        // ACTUALIZAR PARCIAL
        // -------------------------------------------------------------
        [Fact]
        public async Task ActualizarEspacioParcialAsync_ReturnsCurrent_WhenNoUpdates()
        {
            var dto = new UpdateEspacioDto(); // sin campos
            var domain = new Espacio("Casa") { Id = "1" };

            _espacioRepo.Setup(r => r.GetByIdAsync("1", It.IsAny<CancellationToken>()))
                        .ReturnsAsync(domain);

            _mapper.Setup(m => m.Map<EspacioDto>(domain))
                   .Returns(new EspacioDto { Id = "1", Nombre = "Casa" });

            var result = await _service.ActualizarEspacioParcialAsync("1", dto);

            Assert.Equal("1", result.Id);
        }

        // -------------------------------------------------------------
        // ELIMINAR ESPACIO
        // -------------------------------------------------------------
        [Fact]
        public async Task EliminarEspacioAsync_Throws_WhenIdEmpty()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.EliminarEspacioAsync(""));
        }

        [Fact]
        public async Task EliminarEspacioAsync_Throws_WhenUsuariosExist()
        {
            _usuarioEspacioRepo.Setup(r => r.ExistsByEspacioIdAsync("1", It.IsAny<CancellationToken>()))
                               .ReturnsAsync(true);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.EliminarEspacioAsync("1"));
        }

        [Fact]
        public async Task EliminarEspacioAsync_ReturnsFalse_WhenNotFound()
        {
            _usuarioEspacioRepo.Setup(r => r.ExistsByEspacioIdAsync("1", It.IsAny<CancellationToken>()))
                               .ReturnsAsync(false);

            _espacioRepo.Setup(r => r.GetByIdAsync("1", It.IsAny<CancellationToken>()))
                        .ReturnsAsync((Espacio)null);

            var result = await _service.EliminarEspacioAsync("1");

            Assert.False(result);
        }

        // -------------------------------------------------------------
        // CHANGE ESPACIO ID CASCADE
        // -------------------------------------------------------------
        [Fact]
        public async Task ChangeEspacioIdCascadeAsync_Throws_WhenOldIdEmpty()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.ChangeEspacioIdCascadeAsync("", "new"));
        }

        [Fact]
        public async Task ChangeEspacioIdCascadeAsync_Throws_WhenNotFound()
        {
            _espacioRepo.Setup(r => r.GetByIdAsync("old", It.IsAny<CancellationToken>()))
                        .ReturnsAsync((Espacio)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.ChangeEspacioIdCascadeAsync("old", "new"));
        }

        // -------------------------------------------------------------
        // GENERAR CODIGO INVITACION
        // -------------------------------------------------------------
        [Fact]
        public async Task GenerarCodigoInvitacionAsync_Throws_WhenIdEmpty()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.GenerarCodigoInvitacionAsync(""));
        }

        [Fact]
        public async Task GenerarCodigoInvitacionAsync_Throws_WhenNotFound()
        {
            _espacioRepo.Setup(r => r.GetByIdAsync("1", It.IsAny<CancellationToken>()))
                        .ReturnsAsync((Espacio)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.GenerarCodigoInvitacionAsync("1"));
        }

        [Fact]
        public async Task GenerarCodigoInvitacionAsync_ReturnsExisting_WhenCached()
        {
            _cache.Set("EspacioId_1", "999999");

            _espacioRepo.Setup(r => r.GetByIdAsync("1", It.IsAny<CancellationToken>()))
                        .ReturnsAsync(new Espacio("Casa") { Id = "1" });

            var result = await _service.GenerarCodigoInvitacionAsync("1");

            Assert.Equal("999999", result);
        }

        // -------------------------------------------------------------
        // UNIR USUARIO POR CODIGO
        // -------------------------------------------------------------
        [Fact]
        public async Task UnirUsuarioPorCodigoAsync_ReturnsNull_WhenCodigoInvalid()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.UnirUsuarioPorCodigoAsync("", "u1")
            );
        }

        [Fact]
        public async Task UnirUsuarioPorCodigoAsync_ReturnsNull_WhenCacheMiss()
        {
            var result = await _service.UnirUsuarioPorCodigoAsync("123456", "u1");
            Assert.Null(result);
        }

        [Fact]
        public async Task UnirUsuarioPorCodigoAsync_ReturnsNull_WhenEspacioNotFound()
        {
            _cache.Set("InvitacionCode_123456", "1");

            _espacioRepo.Setup(r => r.GetByIdAsync("1", It.IsAny<CancellationToken>()))
                        .ReturnsAsync((Espacio)null);

            var result = await _service.UnirUsuarioPorCodigoAsync("123456", "u1");

            Assert.Null(result);
        }

[Fact]
public async Task UnirUsuarioPorCodigoAsync_ReturnsNull_WhenUserAlreadyInEspacio()
{
    _cache.Set("InvitacionCode_123456", "1");

    _espacioRepo.Setup(r => r.GetByIdAsync("1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Espacio("Casa") { Id = "1" });

    _usuarioEspacioService
        .Setup(s => s.ObtenerPorEspacioAsync("1", It.IsAny<CancellationToken>()))
        .ReturnsAsync(new List<UsuarioEspacioDto>
        {
            new UsuarioEspacioDto { UsuarioId = "u1" }
        });

    var result = await _service.UnirUsuarioPorCodigoAsync("123456", "u1");

    Assert.Null(result);
}


    }
}
