using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Convivia.Application.Repositories;
using Convivia.Application.Services;
using Convivia.Domain.Entities;
using Convivia.Shared.DTOs;
using MapsterMapper;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Convivia.Tests.Services
{
    public class PermisoServiceTests
    {
        private readonly Mock<IPermisoRepository> _permisoRepository;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<ILogger<PermisoService>> _logger;
        private readonly PermisoService _service;

        public PermisoServiceTests()
        {
            _permisoRepository = new Mock<IPermisoRepository>();
            _mapper = new Mock<IMapper>();
            _logger = new Mock<ILogger<PermisoService>>();

            _service = new PermisoService(
                _permisoRepository.Object,
                _mapper.Object,
                _logger.Object
            );
        }

        // -------------------------------------------------------------
        // CREAR PERMISO
        // -------------------------------------------------------------

        [Fact]
        public async Task CrearPermisoAsync_Throws_WhenDtoIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.CrearPermisoAsync(null!)
            );
        }

        [Fact]
        public async Task CrearPermisoAsync_ReturnsCreatedPermiso()
        {
            var dto = new CreatePermisoDto { Rol = TipoRol.Usuario };
            var domain = new Permiso();
            var saved = new Permiso { Id = "p1" };
            var savedDto = new PermisoDto { Id = "p1", Rol = TipoRol.Usuario };

            _mapper.Setup(m => m.Map<Permiso>(dto)).Returns(domain);
            _permisoRepository.Setup(r => r.AddAsync(domain, It.IsAny<CancellationToken>()))
                              .ReturnsAsync("p1");
            _permisoRepository.Setup(r => r.GetByIdAsync("p1", It.IsAny<CancellationToken>()))
                              .ReturnsAsync(saved);
            _mapper.Setup(m => m.Map<PermisoDto>(saved)).Returns(savedDto);

            var result = await _service.CrearPermisoAsync(dto);

            Assert.Equal("p1", result.Id);
            Assert.Equal(TipoRol.Usuario, result.Rol);
        }

        // -------------------------------------------------------------
        // OBTENER PERMISO
        // -------------------------------------------------------------

        [Fact]
        public async Task ObtenerPermisoAsync_Throws_WhenIdInvalid()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.ObtenerPermisoAsync("")
            );
        }

        [Fact]
        public async Task ObtenerPermisoAsync_ReturnsNull_WhenNotFound()
        {
            _permisoRepository.Setup(r => r.GetByIdAsync("x", It.IsAny<CancellationToken>()))
                              .ReturnsAsync((Permiso)null!);

            var result = await _service.ObtenerPermisoAsync("x");

            Assert.Null(result);
        }

        [Fact]
        public async Task ObtenerPermisoAsync_ReturnsMappedDto_WhenFound()
        {
            var domain = new Permiso { Id = "p1" };
            var dto = new PermisoDto { Id = "p1" };

            _permisoRepository.Setup(r => r.GetByIdAsync("p1", It.IsAny<CancellationToken>()))
                              .ReturnsAsync(domain);
            _mapper.Setup(m => m.Map<PermisoDto>(domain)).Returns(dto);

            var result = await _service.ObtenerPermisoAsync("p1");

            Assert.Equal("p1", result!.Id);
        }

        // -------------------------------------------------------------
        // OBTENER POR ROL
        // -------------------------------------------------------------

        [Fact]
        public async Task ObtenerPorRolAsync_ReturnsMappedList()
        {
            var domainList = new List<Permiso>
            {
                new Permiso { Id = "1" },
                new Permiso { Id = "2" }
            };

            _permisoRepository.Setup(r => r.GetByRolAsync(TipoRol.Admin, It.IsAny<CancellationToken>()))
                              .ReturnsAsync(domainList);

            _mapper.Setup(m => m.Map<PermisoDto>(It.IsAny<Permiso>()))
                   .Returns<Permiso>(p => new PermisoDto { Id = p.Id });

            var result = await _service.ObtenerPorRolAsync(TipoRol.Admin);

            Assert.Collection(result,
                p => Assert.Equal("1", p.Id),
                p => Assert.Equal("2", p.Id)
            );
        }

        // -------------------------------------------------------------
        // LISTAR TODAS
        // -------------------------------------------------------------

        [Fact]
        public async Task ListarTodasAsync_ReturnsEmpty_WhenRepositoryReturnsNull()
        {
            _permisoRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                              .ReturnsAsync((IEnumerable<Permiso>)null!);

            var result = await _service.ListarTodasAsync();

            Assert.Empty(result);
        }

        [Fact]
        public async Task ListarTodasAsync_ReturnsMappedList()
        {
            var domainList = new List<Permiso>
            {
                new Permiso { Id = "1" }
            };

            _permisoRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                              .ReturnsAsync(domainList);

            _mapper.Setup(m => m.Map<PermisoDto>(It.IsAny<Permiso>()))
                   .Returns(new PermisoDto { Id = "1" });

            var result = await _service.ListarTodasAsync();

            Assert.Single(result);
            Assert.Equal("1", result[0].Id);
        }

        // -------------------------------------------------------------
        // ACTUALIZAR COMPLETA
        // -------------------------------------------------------------

        [Fact]
        public async Task ActualizarPermisoCompletaAsync_Throws_WhenIdInvalid()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.ActualizarPermisoCompletaAsync("", new UpdatePermisoDto())
            );
        }

        [Fact]
        public async Task ActualizarPermisoCompletaAsync_ReturnsNull_WhenNotFound()
        {
            _permisoRepository.Setup(r => r.GetByIdAsync("x", It.IsAny<CancellationToken>()))
                              .ReturnsAsync((Permiso)null!);

            var result = await _service.ActualizarPermisoCompletaAsync("x", new UpdatePermisoDto());

            Assert.Null(result);
        }

        // -------------------------------------------------------------
        // ACTUALIZAR MERGE
        // -------------------------------------------------------------

        [Fact]
        public async Task ActualizarPermisoMergeAsync_UpdatesFieldsCorrectly()
        {
            var existing = new Permiso
            {
                Id = "p1",
                Rol = new Rol()
            };

            var dto = new UpdatePermisoDto
            {
                CrearTarea = true,
                EliminarUsuario = true
            };

            _permisoRepository.Setup(r => r.GetByIdAsync("p1", It.IsAny<CancellationToken>()))
                              .ReturnsAsync(existing);

            var updated = new Permiso { Id = "p1", Rol = existing.Rol };
            _permisoRepository.Setup(r => r.GetByIdAsync("p1", It.IsAny<CancellationToken>()))
                              .ReturnsAsync(updated);

            _mapper.Setup(m => m.Map<PermisoDto>(updated))
                   .Returns(new PermisoDto
                   {
                       Id = "p1",
                       CrearTarea = true,
                       EliminarUsuario = true
                   });

            var result = await _service.ActualizarPermisoMergeAsync("p1", dto);

            Assert.True(result!.CrearTarea);
            Assert.True(result.EliminarUsuario);
        }

        // -------------------------------------------------------------
        // ACTUALIZAR PARCIAL
        // -------------------------------------------------------------

        [Fact]
        public async Task ActualizarPermisoParcialAsync_ReturnsCurrent_WhenNoUpdates()
        {
            var existing = new Permiso { Id = "p1" };

            _permisoRepository.Setup(r => r.GetByIdAsync("p1", It.IsAny<CancellationToken>()))
                              .ReturnsAsync(existing);

            _mapper.Setup(m => m.Map<PermisoDto>(existing))
                   .Returns(new PermisoDto { Id = "p1" });

            var result = await _service.ActualizarPermisoParcialAsync("p1", new UpdatePermisoDto());

            Assert.Equal("p1", result!.Id);
        }

        // -------------------------------------------------------------
        // ELIMINAR
        // -------------------------------------------------------------

        [Fact]
        public async Task EliminarPermisoAsync_ReturnsFalse_WhenNotFound()
        {
            _permisoRepository.Setup(r => r.GetByIdAsync("x", It.IsAny<CancellationToken>()))
                              .ReturnsAsync((Permiso)null!);

            var result = await _service.EliminarPermisoAsync("x");

            Assert.False(result);
        }

        [Fact]
        public async Task EliminarPermisoAsync_ReturnsTrue_WhenDeleted()
        {
            var permiso = new Permiso { Id = "p1" };

            _permisoRepository.Setup(r => r.GetByIdAsync("p1", It.IsAny<CancellationToken>()))
                              .ReturnsAsync(permiso);

            var result = await _service.EliminarPermisoAsync("p1");

            _permisoRepository.Verify(r => r.DeleteAsync("p1", It.IsAny<CancellationToken>()), Times.Once);
            Assert.True(result);
        }
    }
}
