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

public class UsuarioServiceTests
{
    private readonly Mock<IUsuarioRepository> _repoMock;
    private readonly Mock<IUsuarioEspacioRepository> _usuarioEspacioRepoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<UsuarioService>> _loggerMock;
    private readonly UsuarioService _service;

    public UsuarioServiceTests()
    {
        _repoMock = new Mock<IUsuarioRepository>();
        _usuarioEspacioRepoMock = new Mock<IUsuarioEspacioRepository>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<UsuarioService>>();

        _service = new UsuarioService(
            _repoMock.Object,
            _mapperMock.Object,
            _loggerMock.Object,
            _usuarioEspacioRepoMock.Object
        );
    }

    // -------------------------------------------------------------
    // CREAR USUARIO
    // -------------------------------------------------------------
    [Fact]
    public async Task CrearUsuarioAsync_CreaCorrectamente()
    {
        var dto = new CreateUsuarioDto
        {
            Nombre = "Marc",
            Email = "marc@test.com",
            Password = "1234"
        };

        var domain = new Usuario { Id = "abc123", Nombre = "Marc", Email = "marc@test.com" };
        var createdDomain = new Usuario { Id = "abc123", Nombre = "Marc", Email = "marc@test.com" };
        var dtoResult = new UsuarioDto { Id = "abc123", Nombre = "Marc", Email = "marc@test.com" };

        _mapperMock.Setup(m => m.Map<Usuario>(dto)).Returns(domain);
        _repoMock.Setup(r => r.AddAsync(domain, It.IsAny<CancellationToken>()))
                 .ReturnsAsync("abc123");
        _repoMock.Setup(r => r.GetByIdAsync("abc123", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(createdDomain);
        _mapperMock.Setup(m => m.Map<UsuarioDto>(createdDomain)).Returns(dtoResult);

        var result = await _service.CrearUsuarioAsync(dto);

        Assert.Equal("abc123", result.Id);
        Assert.Equal("Marc", result.Nombre);
    }

    [Fact]
    public async Task CrearUsuarioAsync_DevuelveSoloIdSiNoEncuentraEntidad()
    {
        var dto = new CreateUsuarioDto
        {
            Nombre = "Marc",
            Email = "marc@test.com",
            Password = "1234"
        };

        var domain = new Usuario { Id = "xyz" };

        _mapperMock.Setup(m => m.Map<Usuario>(dto)).Returns(domain);
        _repoMock.Setup(r => r.AddAsync(domain, It.IsAny<CancellationToken>()))
                 .ReturnsAsync("xyz");
        _repoMock.Setup(r => r.GetByIdAsync("xyz", It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Usuario)null);

        var result = await _service.CrearUsuarioAsync(dto);

        Assert.Equal("xyz", result.Id);
    }

    // -------------------------------------------------------------
    // OBTENER USUARIO
    // -------------------------------------------------------------
    [Fact]
    public async Task ObtenerUsuarioAsync_DevuelveUsuario()
    {
        var domain = new Usuario { Id = "1", Nombre = "Marc" };
        var dto = new UsuarioDto { Id = "1", Nombre = "Marc" };

        _repoMock.Setup(r => r.GetByIdAsync("1", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(domain);
        _mapperMock.Setup(m => m.Map<UsuarioDto>(domain)).Returns(dto);

        var result = await _service.ObtenerUsuarioAsync("1");

        Assert.NotNull(result);
        Assert.Equal("1", result.Id);
    }

    [Fact]
    public async Task ObtenerUsuarioAsync_DevuelveNullSiNoExiste()
    {
        _repoMock.Setup(r => r.GetByIdAsync("1", It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Usuario)null);

        var result = await _service.ObtenerUsuarioAsync("1");

        Assert.Null(result);
    }

    // -------------------------------------------------------------
    // LISTAR TODOS
    // -------------------------------------------------------------
    [Fact]
    public async Task ListarTodasAsync_DevuelveListaMapeada()
    {
        var lista = new List<Usuario>
        {
            new Usuario { Id = "1", Nombre = "A" },
            new Usuario { Id = "2", Nombre = "B" }
        };

        _repoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                 .ReturnsAsync(lista);

        _mapperMock.Setup(m => m.Map<UsuarioDto>(It.IsAny<Usuario>()))
                   .Returns<Usuario>(u => new UsuarioDto { Id = u.Id, Nombre = u.Nombre });

        var result = await _service.ListarTodasAsync();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, u => u.Id == "1");
        Assert.Contains(result, u => u.Id == "2");
    }

    // -------------------------------------------------------------
    // OBTENER POR EMAIL
    // -------------------------------------------------------------
    [Fact]
    public async Task ObtenerPorEmailAsync_DevuelveUsuario()
    {
        var dto = new UsuarioDto { Id = "1", Email = "test@test.com" };

        _repoMock.Setup(r => r.GetByEmailAsync("test@test.com", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(dto);

        var result = await _service.ObtenerPorEmailAsync("test@test.com");

        Assert.NotNull(result);
        Assert.Equal("1", result.Id);
    }

    [Fact]
    public async Task ObtenerPorEmailAsync_DevuelveNullSiEmailVacio()
    {
        var result = await _service.ObtenerPorEmailAsync("");

        Assert.Null(result);
    }

    // -------------------------------------------------------------
    // ACTUALIZAR COMPLETO
    // -------------------------------------------------------------
    [Fact]
    public async Task ActualizarUsuarioCompletoAsync_ActualizaCorrectamente()
    {
        var dto = new UpdateUsuarioDto { Nombre = "Nuevo" };
        var domain = new Usuario { Id = "1", Nombre = "Nuevo" };
        var updated = new Usuario { Id = "1", Nombre = "Nuevo" };
        var dtoUpdated = new UsuarioDto { Id = "1", Nombre = "Nuevo" };

        _mapperMock.Setup(m => m.Map<Usuario>(dto)).Returns(domain);
        _repoMock.Setup(r => r.GetByIdAsync("1", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(updated);
        _mapperMock.Setup(m => m.Map<UsuarioDto>(updated)).Returns(dtoUpdated);

        var result = await _service.ActualizarUsuarioCompletoAsync("1", dto);

        Assert.Equal("Nuevo", result.Nombre);
    }

    // -------------------------------------------------------------
    // ACTUALIZAR MERGE
    // -------------------------------------------------------------
    [Fact]
    public async Task ActualizarUsuarioMergeAsync_DevuelveNullSiNoExiste()
    {
        _repoMock.Setup(r => r.GetByIdAsync("1", It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Usuario)null);

        var result = await _service.ActualizarUsuarioMergeAsync("1", new UpdateUsuarioDto());

        Assert.Null(result);
    }

    [Fact]
    public async Task ActualizarUsuarioMergeAsync_ActualizaCorrectamente()
    {
        var existing = new Usuario { Id = "1", Nombre = "Viejo" };
        var updated = new Usuario { Id = "1", Nombre = "Nuevo" };
        var dtoUpdated = new UsuarioDto { Id = "1", Nombre = "Nuevo" };

        _repoMock.Setup(r => r.GetByIdAsync("1", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing);

        _mapperMock.Setup(m => m.Map(It.IsAny<UpdateUsuarioDto>(), existing))
                   .Callback<UpdateUsuarioDto, Usuario>((dto, u) => u.Nombre = "Nuevo");

        _repoMock.Setup(r => r.GetByIdAsync("1", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(updated);

        _mapperMock.Setup(m => m.Map<UsuarioDto>(updated)).Returns(dtoUpdated);

        var result = await _service.ActualizarUsuarioMergeAsync("1", new UpdateUsuarioDto());

        Assert.Equal("Nuevo", result.Nombre);
    }

    // -------------------------------------------------------------
    // ACTUALIZAR PARCIAL
    // -------------------------------------------------------------
    [Fact]
    public async Task ActualizarUsuarioParcialAsync_SinCambios_DevuelveActual()
    {
        var existing = new Usuario { Id = "1", Nombre = "A" };
        var dto = new UpdateUsuarioDto(); // todo null

        _repoMock.Setup(r => r.GetByIdAsync("1", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing);

        _mapperMock.Setup(m => m.Map<UsuarioDto>(existing))
                   .Returns(new UsuarioDto { Id = "1", Nombre = "A" });

        var result = await _service.ActualizarUsuarioParcialAsync("1", dto);

        Assert.Equal("A", result.Nombre);
    }

    // -------------------------------------------------------------
    // ELIMINAR
    // -------------------------------------------------------------
    [Fact]
    public async Task EliminarUsuarioAsync_DevuelveFalseSiNoExiste()
    {
        _repoMock.Setup(r => r.GetByIdAsync("1", It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Usuario)null);

        var result = await _service.EliminarUsuarioAsync("1");

        Assert.False(result);
    }

    [Fact]
    public async Task EliminarUsuarioAsync_EliminaCorrectamenteSinUsuarioEspacios()
    {
        _repoMock.Setup(r => r.GetByIdAsync("1", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new Usuario { Id = "1" });

        _usuarioEspacioRepoMock.Setup(r => r.GetByUsuarioIdAsync("1", It.IsAny<CancellationToken>()))
                               .ReturnsAsync(new List<UsuarioEspacio>());

        var result = await _service.EliminarUsuarioAsync("1");

        Assert.True(result);
        _repoMock.Verify(r => r.DeleteAsync("1", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EliminarUsuarioAsync_EliminaUsuarioEspaciosAntes()
    {
        var usuarioEspacios = new List<UsuarioEspacio>
        {
            new UsuarioEspacio { Id = "UE1" },
            new UsuarioEspacio { Id = "UE2" }
        };

        _repoMock.Setup(r => r.GetByIdAsync("1", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new Usuario { Id = "1" });

        _usuarioEspacioRepoMock.Setup(r => r.GetByUsuarioIdAsync("1", It.IsAny<CancellationToken>()))
                               .ReturnsAsync(usuarioEspacios);

        var result = await _service.EliminarUsuarioAsync("1");

        Assert.True(result);

        _usuarioEspacioRepoMock.Verify(r => r.DeleteAsync("UE1", It.IsAny<CancellationToken>()), Times.Once);
        _usuarioEspacioRepoMock.Verify(r => r.DeleteAsync("UE2", It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.DeleteAsync("1", It.IsAny<CancellationToken>()), Times.Once);
    }
}