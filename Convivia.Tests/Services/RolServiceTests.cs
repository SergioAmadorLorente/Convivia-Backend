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
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Convivia.Tests.Services
{
    public class RolServiceTests
    {
        private readonly Mock<IRolRepository> _rolRepository;
        private readonly Mock<IPermisoRepository> _permisoRepository;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<ILogger<RolService>> _logger;
        private readonly RolService _service;

        public RolServiceTests()
        {
            _rolRepository = new Mock<IRolRepository>();
            _permisoRepository = new Mock<IPermisoRepository>();
            _mapper = new Mock<IMapper>();
            _logger = new Mock<ILogger<RolService>>();

            _service = new RolService(
                _rolRepository.Object,
                _permisoRepository.Object,
                _mapper.Object,
                _logger.Object
            );
        }

        // -------------------------------------------------------------
        // CREAR ROL
        // -------------------------------------------------------------

        [Fact]
        public async Task CrearRolAsync_Throws_WhenDtoIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.CrearRolAsync(null!)
            );
        }

        [Fact]
        public async Task CrearRolAsync_CreatesAdminRoleCorrectly()
        {
            var dto = new CreateRolDto { Nombre = TipoRol.Admin };
            var saved = new Rol { Id = "r1", Nombre = "Admin" };
            var savedDto = new RolDto { Id = "r1", Nombre = TipoRol.Admin };

            _rolRepository.Setup(r => r.AddAsync(It.IsAny<Rol>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync("r1");

            _rolRepository.Setup(r => r.GetByIdAsync("r1", It.IsAny<CancellationToken>()))
                          .ReturnsAsync(saved);

            _mapper.Setup(m => m.Map<RolDto>(saved)).Returns(savedDto);

            var result = await _service.CrearRolAsync(dto);

            Assert.Equal("r1", result.Id);
            Assert.Equal(TipoRol.Admin, result.Nombre);
        }

        // -------------------------------------------------------------
        // OBTENER ROL
        // -------------------------------------------------------------

        [Fact]
        public async Task ObtenerRolAsync_Throws_WhenIdInvalid()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.ObtenerRolAsync("")
            );
        }

        [Fact]
        public async Task ObtenerRolAsync_ReturnsNull_WhenNotFound()
        {
            _rolRepository.Setup(r => r.GetByIdAsync("x", It.IsAny<CancellationToken>()))
                          .ReturnsAsync((Rol)null!);

            var result = await _service.ObtenerRolAsync("x");

            Assert.Null(result);
        }

        [Fact]
        public async Task ObtenerRolAsync_ReturnsMappedDto_WhenFound()
        {
            var domain = new Rol { Id = "r1", Nombre = "Usuario" };
            var dto = new RolDto { Id = "r1", Nombre = TipoRol.Usuario };

            _rolRepository.Setup(r => r.GetByIdAsync("r1", It.IsAny<CancellationToken>()))
                          .ReturnsAsync(domain);

            _mapper.Setup(m => m.Map<RolDto>(domain)).Returns(dto);

            var result = await _service.ObtenerRolAsync("r1");

            Assert.Equal("r1", result!.Id);
        }

        // -------------------------------------------------------------
        // LISTAR TODAS
        // -------------------------------------------------------------

        [Fact]
        public async Task ListarTodasAsync_ReturnsEmpty_WhenRepositoryReturnsNull()
        {
            _rolRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                          .ReturnsAsync((IEnumerable<Rol>)null!);

            var result = await _service.ListarTodasAsync();

            Assert.Empty(result);
        }

        [Fact]
        public async Task ListarTodasAsync_ReturnsMappedList()
        {
            var domainList = new List<Rol>
            {
                new Rol { Id = "1", Nombre = "Usuario" }
            };

            _rolRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                          .ReturnsAsync(domainList);

            _mapper.Setup(m => m.Map<RolDto>(It.IsAny<Rol>()))
                   .Returns(new RolDto { Id = "1", Nombre = TipoRol.Usuario });

            var result = await _service.ListarTodasAsync();

            Assert.Single(result);
            Assert.Equal("1", result[0].Id);
        }

        // -------------------------------------------------------------
        // OBTENER POR NOMBRE
        // -------------------------------------------------------------

        [Fact]
        public async Task ObtenerPorNombreAsync_ReturnsMappedDto()
        {
            var domain = new Rol { Id = "r1", Nombre = "Admin" };
            var dto = new RolDto { Id = "r1", Nombre = TipoRol.Admin };

            _rolRepository.Setup(r => r.GetByNombreAsync(TipoRol.Admin, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(domain);

            _mapper.Setup(m => m.Map<RolDto>(domain)).Returns(dto);

            var result = await _service.ObtenerPorNombreAsync(TipoRol.Admin);

            Assert.Equal("r1", result!.Id);
        }

        // -------------------------------------------------------------
        // ACTUALIZAR COMPLETO
        // -------------------------------------------------------------

        [Fact]
        public async Task ActualizarRolCompletoAsync_Throws_WhenIdInvalid()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.ActualizarRolCompletoAsync("", new UpdateRolDto())
            );
        }

        [Fact]
        public async Task ActualizarRolCompletoAsync_UpdatesCorrectly()
        {
            var dto = new UpdateRolDto
            {
                Permisos = new PermisosRolDto { CrearTarea = true }
            };

            var domain = new Rol { Id = "r1", Nombre = "Usuario" };
            var dtoMapped = new RolDto { Id = "r1", Nombre = TipoRol.Usuario };

            _mapper.Setup(m => m.Map<Rol>(dto)).Returns(domain);

            _rolRepository.Setup(r => r.GetByIdAsync("r1", It.IsAny<CancellationToken>()))
                          .ReturnsAsync(domain);

            _mapper.Setup(m => m.Map<RolDto>(domain)).Returns(dtoMapped);

            var result = await _service.ActualizarRolCompletoAsync("r1", dto);

            Assert.Equal("r1", result!.Id);
        }

        // -------------------------------------------------------------
        // ACTUALIZAR MERGE
        // -------------------------------------------------------------

        [Fact]
        public async Task ActualizarRolMergeAsync_ReturnsNull_WhenNotFound()
        {
            _rolRepository.Setup(r => r.GetByIdAsync("x", It.IsAny<CancellationToken>()))
                          .ReturnsAsync((Rol)null!);

            var result = await _service.ActualizarRolMergeAsync("x", new UpdateRolDto());

            Assert.Null(result);
        }

        [Fact]
        public async Task ActualizarRolMergeAsync_UpdatesCorrectly()
        {
            var existing = new Rol { Id = "r1", Nombre = "Usuario" };
            var dto = new UpdateRolDto
            {
                Permisos = new PermisosRolDto { EditarTarea = true }
            };

            _rolRepository.Setup(r => r.GetByIdAsync("r1", It.IsAny<CancellationToken>()))
                          .ReturnsAsync(existing);

            var updated = new Rol { Id = "r1", Nombre = "Usuario", EditarTarea = true };
            var dtoMapped = new RolDto { Id = "r1", Nombre = TipoRol.Usuario, EditarTarea = true };

            _rolRepository.Setup(r => r.GetByIdAsync("r1", It.IsAny<CancellationToken>()))
                          .ReturnsAsync(updated);

            _mapper.Setup(m => m.Map<RolDto>(updated)).Returns(dtoMapped);

            var result = await _service.ActualizarRolMergeAsync("r1", dto);

            Assert.True(result!.EditarTarea);
        }

        // -------------------------------------------------------------
        // ELIMINAR
        // -------------------------------------------------------------

        [Fact]
        public async Task EliminarRolAsync_ReturnsFalse_WhenNotFound()
        {
            _rolRepository.Setup(r => r.GetByIdAsync("x", It.IsAny<CancellationToken>()))
                          .ReturnsAsync((Rol)null!);

            var result = await _service.EliminarRolAsync("x");

            Assert.False(result);
        }

        [Fact]
        public async Task EliminarRolAsync_Throws_WhenHasAssociatedPermisos()
        {
            var rol = new Rol { Id = "r1", Nombre = "Admin" };

            _rolRepository.Setup(r => r.GetByIdAsync("r1", It.IsAny<CancellationToken>()))
                          .ReturnsAsync(rol);

            _permisoRepository.Setup(r => r.GetByRolAsync(TipoRol.Admin, It.IsAny<CancellationToken>()))
                              .ReturnsAsync(new List<Permiso> { new Permiso() });

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.EliminarRolAsync("r1")
            );
        }

        [Fact]
        public async Task EliminarRolAsync_ReturnsTrue_WhenDeleted()
        {
            var rol = new Rol { Id = "r1", Nombre = "Usuario" };

            _rolRepository.Setup(r => r.GetByIdAsync("r1", It.IsAny<CancellationToken>()))
                          .ReturnsAsync(rol);

            _permisoRepository.Setup(r => r.GetByRolAsync(TipoRol.Usuario, It.IsAny<CancellationToken>()))
                              .ReturnsAsync(new List<Permiso>());

            var result = await _service.EliminarRolAsync("r1");

            _rolRepository.Verify(r => r.DeleteAsync("r1", It.IsAny<CancellationToken>()), Times.Once);
            Assert.True(result);
        }
    }
}
