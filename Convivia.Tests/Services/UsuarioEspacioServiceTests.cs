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

public class UsuarioEspacioServiceTests
{
    private readonly Mock<IUsuarioEspacioRepository> _repoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<UsuarioEspacioService>> _loggerMock;
    private readonly Mock<IFacturaRepository> _facturaRepoMock;
    private readonly Mock<ITareaRepository> _tareaRepoMock;

    private readonly UsuarioEspacioService _service;

    public UsuarioEspacioServiceTests()
    {
        _repoMock = new Mock<IUsuarioEspacioRepository>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<UsuarioEspacioService>>();
        _facturaRepoMock = new Mock<IFacturaRepository>();
        _tareaRepoMock = new Mock<ITareaRepository>();

        _service = new UsuarioEspacioService(
            _repoMock.Object,
            _mapperMock.Object,
            _loggerMock.Object,
            _facturaRepoMock.Object,
            _tareaRepoMock.Object
        );
    }

    // -------------------------------------------------------------
    // CREAR
    // -------------------------------------------------------------
    [Fact]
    public async Task CrearUsuarioEspacioAsync_CreaCorrectamente()
    {
        var dto = new CreateUsuarioEspacioDto
        {
            Ausente = false,
            Karma = 10,
            Rol = "Admin",
            EspacioId = "E1",
            UsuarioId = "U1",
            TareasId = new List<string> { "T1" }
        };

        var domain = new UsuarioEspacio { Id = "UE1" };
        var createdDomain = new UsuarioEspacio { Id = "UE1" };
        var dtoResult = new UsuarioEspacioDto { Id = "UE1" };

        _mapperMock.Setup(m => m.Map<UsuarioEspacio>(dto)).Returns(domain);
        _repoMock.Setup(r => r.AddAsync(domain, It.IsAny<CancellationToken>()))
                 .ReturnsAsync("UE1");
        _repoMock.Setup(r => r.GetByIdAsync("UE1", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(createdDomain);
        _mapperMock.Setup(m => m.Map<UsuarioEspacioDto>(createdDomain)).Returns(dtoResult);

        var result = await _service.CrearUsuarioEspacioAsync(dto);

        Assert.Equal("UE1", result.Id);
    }

    [Fact]
    public async Task CrearUsuarioEspacioAsync_DevuelveSoloIdSiNoEncuentraEntidad()
    {
        var dto = new CreateUsuarioEspacioDto
        {
            Ausente = false,
            Karma = 0,
            Rol = "User",
            EspacioId = "E1",
            UsuarioId = "U1",
            TareasId = new List<string>()
        };

        var domain = new UsuarioEspacio { Id = "X" };

        _mapperMock.Setup(m => m.Map<UsuarioEspacio>(dto)).Returns(domain);
        _repoMock.Setup(r => r.AddAsync(domain, It.IsAny<CancellationToken>()))
                 .ReturnsAsync("X");
        _repoMock.Setup(r => r.GetByIdAsync("X", It.IsAny<CancellationToken>()))
                 .ReturnsAsync((UsuarioEspacio)null);

        var result = await _service.CrearUsuarioEspacioAsync(dto);

        Assert.Equal("X", result.Id);
    }

    // -------------------------------------------------------------
    // OBTENER POR ID
    // -------------------------------------------------------------
    [Fact]
    public async Task ObtenerUsuarioEspacioAsync_DevuelveUsuarioEspacio()
    {
        var domain = new UsuarioEspacio { Id = "UE1" };
        var dto = new UsuarioEspacioDto { Id = "UE1" };

        _repoMock.Setup(r => r.GetByIdAsync("UE1", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(domain);
        _mapperMock.Setup(m => m.Map<UsuarioEspacioDto>(domain)).Returns(dto);

        var result = await _service.ObtenerUsuarioEspacioAsync("UE1");

        Assert.NotNull(result);
        Assert.Equal("UE1", result.Id);
    }

    [Fact]
    public async Task ObtenerUsuarioEspacioAsync_DevuelveNullSiNoExiste()
    {
        _repoMock.Setup(r => r.GetByIdAsync("UE1", It.IsAny<CancellationToken>()))
                 .ReturnsAsync((UsuarioEspacio)null);

        var result = await _service.ObtenerUsuarioEspacioAsync("UE1");

        Assert.Null(result);
    }

    // -------------------------------------------------------------
    // OBTENER POR ESPACIO
    // -------------------------------------------------------------
    [Fact]
    public async Task ObtenerPorEspacioAsync_DevuelveLista()
    {
        var lista = new List<UsuarioEspacio>
        {
            new UsuarioEspacio { Id = "1" },
            new UsuarioEspacio { Id = "2" }
        };

        _repoMock.Setup(r => r.GetByEspacioIdAsync("E1", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(lista);

        _mapperMock.Setup(m => m.Map<UsuarioEspacioDto>(It.IsAny<UsuarioEspacio>()))
                   .Returns<UsuarioEspacio>(u => new UsuarioEspacioDto { Id = u.Id });

        var result = await _service.ObtenerPorEspacioAsync("E1");

        Assert.Equal(2, result.Count());
    }

    // -------------------------------------------------------------
    // OBTENER POR USUARIO
    // -------------------------------------------------------------
    [Fact]
    public async Task ObtenerPorUsuarioAsync_DevuelveLista()
    {
        var lista = new List<UsuarioEspacio>
        {
            new UsuarioEspacio { Id = "1" },
            new UsuarioEspacio { Id = "2" }
        };

        _repoMock.Setup(r => r.GetByUsuarioIdAsync("U1", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(lista);

        _mapperMock.Setup(m => m.Map<UsuarioEspacioDto>(It.IsAny<UsuarioEspacio>()))
                   .Returns<UsuarioEspacio>(u => new UsuarioEspacioDto { Id = u.Id });

        var result = await _service.ObtenerPorUsuarioAsync("U1");

        Assert.Equal(2, result.Count());
    }

    // -------------------------------------------------------------
    // LISTAR TODOS
    // -------------------------------------------------------------
    [Fact]
    public async Task ListarTodasAsync_DevuelveListaMapeada()
    {
        var lista = new List<UsuarioEspacio>
        {
            new UsuarioEspacio { Id = "1" },
            new UsuarioEspacio { Id = "2" }
        };

        _repoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                 .ReturnsAsync(lista);

        _mapperMock.Setup(m => m.Map<UsuarioEspacioDto>(It.IsAny<UsuarioEspacio>()))
                   .Returns<UsuarioEspacio>(u => new UsuarioEspacioDto { Id = u.Id });

        var result = await _service.ListarTodasAsync();

        Assert.Equal(2, result.Count());
    }

    // -------------------------------------------------------------
    // ACTUALIZAR COMPLETO
    // -------------------------------------------------------------
    [Fact]
    public async Task ActualizarUsuarioEspacioCompletoAsync_ActualizaCorrectamente()
    {
        var dto = new UpdateUsuarioEspacioDto { Rol = "Nuevo" };
        var domain = new UsuarioEspacio { Id = "UE1", Rol = "Nuevo" };
        var updated = new UsuarioEspacio { Id = "UE1", Rol = "Nuevo" };
        var dtoUpdated = new UsuarioEspacioDto { Id = "UE1", Rol = "Nuevo" };

        _mapperMock.Setup(m => m.Map<UsuarioEspacio>(dto)).Returns(domain);
        _repoMock.Setup(r => r.GetByIdAsync("UE1", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(updated);
        _mapperMock.Setup(m => m.Map<UsuarioEspacioDto>(updated)).Returns(dtoUpdated);

        var result = await _service.ActualizarUsuarioEspacioCompletoAsync("UE1", dto);

        Assert.Equal("Nuevo", result.Rol);
    }

    // -------------------------------------------------------------
    // ACTUALIZAR MERGE
    // -------------------------------------------------------------
    [Fact]
    public async Task ActualizarUsuarioEspacioMergeAsync_DevuelveNullSiNoExiste()
    {
        _repoMock.Setup(r => r.GetByIdAsync("UE1", It.IsAny<CancellationToken>()))
                 .ReturnsAsync((UsuarioEspacio)null);

        var result = await _service.ActualizarUsuarioEspacioMergeAsync("UE1", new UpdateUsuarioEspacioDto());

        Assert.Null(result);
    }

    [Fact]
    public async Task ActualizarUsuarioEspacioMergeAsync_ActualizaCorrectamente()
    {
        var existing = new UsuarioEspacio { Id = "UE1", Rol = "Viejo" };
        var updated = new UsuarioEspacio { Id = "UE1", Rol = "Nuevo" };
        var dtoUpdated = new UsuarioEspacioDto { Id = "UE1", Rol = "Nuevo" };

        _repoMock.Setup(r => r.GetByIdAsync("UE1", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing);

        _mapperMock.Setup(m => m.Map(It.IsAny<UpdateUsuarioEspacioDto>(), existing))
                   .Callback<UpdateUsuarioEspacioDto, UsuarioEspacio>((dto, u) => u.Rol = "Nuevo");

        _repoMock.Setup(r => r.GetByIdAsync("UE1", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(updated);

        _mapperMock.Setup(m => m.Map<UsuarioEspacioDto>(updated)).Returns(dtoUpdated);

        var result = await _service.ActualizarUsuarioEspacioMergeAsync("UE1", new UpdateUsuarioEspacioDto());

        Assert.Equal("Nuevo", result.Rol);
    }

    // -------------------------------------------------------------
    // ACTUALIZAR PARCIAL
    // -------------------------------------------------------------
    [Fact]
    public async Task ActualizarUsuarioEspacioParcialAsync_SinCambios_DevuelveActual()
    {
        var existing = new UsuarioEspacio { Id = "UE1", Rol = "A" };
        var dto = new UpdateUsuarioEspacioDto(); // todo null

        _repoMock.Setup(r => r.GetByIdAsync("UE1", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing);

        _mapperMock.Setup(m => m.Map<UsuarioEspacioDto>(existing))
                   .Returns(new UsuarioEspacioDto { Id = "UE1", Rol = "A" });

        var result = await _service.ActualizarUsuarioEspacioParcialAsync("UE1", dto);

        Assert.Equal("A", result.Rol);
    }

    // -------------------------------------------------------------
    // ELIMINAR
    // -------------------------------------------------------------
    [Fact]
    public async Task EliminarUsuarioEspacioAsync_LanzaExcepcionSiHayFacturas()
    {
        _facturaRepoMock.Setup(r => r.ExistsByUsuarioEspacioIdAsync("UE1", It.IsAny<CancellationToken>()))
                        .ReturnsAsync(true);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.EliminarUsuarioEspacioAsync("UE1"));
    }

    [Fact]
    public async Task EliminarUsuarioEspacioAsync_LimpiaTareasAntes()
    {
        var tareas = new List<Tarea>
        {
            new Tarea { Id = "T1", UsuarioEspacioId = "UE1" },
            new Tarea { Id = "T2", UsuarioEspacioId = "UE1" }
        };

        _facturaRepoMock.Setup(r => r.ExistsByUsuarioEspacioIdAsync("UE1", It.IsAny<CancellationToken>()))
                        .ReturnsAsync(false);

        _tareaRepoMock.Setup(r => r.GetByUsuarioEspacioIdAsync("UE1", It.IsAny<CancellationToken>()))
                      .ReturnsAsync(tareas);

        _repoMock.Setup(r => r.GetByIdAsync("UE1", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new UsuarioEspacio { Id = "UE1" });

        var result = await _service.EliminarUsuarioEspacioAsync("UE1");

        Assert.True(result);

        _tareaRepoMock.Verify(r => r.UpdateAsync("T1", It.Is<Tarea>(t => t.UsuarioEspacioId == null), It.IsAny<CancellationToken>()), Times.Once);
        _tareaRepoMock.Verify(r => r.UpdateAsync("T2", It.Is<Tarea>(t => t.UsuarioEspacioId == null), It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.DeleteAsync("UE1", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EliminarUsuarioEspacioAsync_DevuelveFalseSiNoExiste()
    {
        _facturaRepoMock.Setup(r => r.ExistsByUsuarioEspacioIdAsync("UE1", It.IsAny<CancellationToken>()))
                        .ReturnsAsync(false);

        _tareaRepoMock.Setup(r => r.GetByUsuarioEspacioIdAsync("UE1", It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new List<Tarea>());

        _repoMock.Setup(r => r.GetByIdAsync("UE1", It.IsAny<CancellationToken>()))
                 .ReturnsAsync((UsuarioEspacio)null);

        var result = await _service.EliminarUsuarioEspacioAsync("UE1");

        Assert.False(result);
    }
}