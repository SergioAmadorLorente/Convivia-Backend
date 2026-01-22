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

public class FacturaServiceTests
{
    private readonly Mock<IFacturaRepository> _repoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<FacturaService>> _loggerMock;
    private readonly FacturaService _service;
    private const string EspacioId = "espacio1";

    public FacturaServiceTests()
    {
        _repoMock = new Mock<IFacturaRepository>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<FacturaService>>();

        _service = new FacturaService(
            _repoMock.Object,
            _mapperMock.Object,
            _loggerMock.Object
        );
    }

    // -------------------------------------------------------------
    // CREAR FACTURA
    // -------------------------------------------------------------
    [Fact]
    public async Task CrearFacturaAsync_CreaCorrectamente()
    {
        // Arrange
        var dto = new CreateFacturaDto
        {
            Nombre = "Factura Test",
            Precio = 50
        };

        var facturaDomain = new Factura { Id = "abc123", Nombre = "Factura Test", Precio = 50 };
        var facturaCreated = new Factura { Id = "abc123", Nombre = "Factura Test", Precio = 50 };
        var facturaDto = new FacturaDto { Id = "abc123", Nombre = "Factura Test", Precio = 50 };

        _mapperMock.Setup(m => m.Map<Factura>(dto)).Returns(facturaDomain);
        _repoMock.Setup(r => r.AddAsync(EspacioId, facturaDomain, It.IsAny<CancellationToken>()))
                 .ReturnsAsync("abc123");
        _repoMock.Setup(r => r.GetByIdAsync(EspacioId, "abc123", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(facturaCreated);
        _mapperMock.Setup(m => m.Map<FacturaDto>(facturaCreated)).Returns(facturaDto);

        // Act
        var result = await _service.CrearFacturaAsync(EspacioId, dto);

        // Assert
        Assert.Equal("abc123", result.Id);
        Assert.Equal("Factura Test", result.Nombre);
        Assert.Equal(50, result.Precio);
    }

    [Fact]
    public async Task CrearFacturaAsync_DevuelveSoloIdSiNoEncuentraEntidad()
    {
        var dto = new CreateFacturaDto { Nombre = "Test", Precio = 10 };
        var facturaDomain = new Factura { Id = "xyz" };

        _mapperMock.Setup(m => m.Map<Factura>(dto)).Returns(facturaDomain);
        _repoMock.Setup(r => r.AddAsync(EspacioId, facturaDomain, It.IsAny<CancellationToken>()))
                 .ReturnsAsync("xyz");
        _repoMock.Setup(r => r.GetByIdAsync(EspacioId, "xyz", It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Factura)null);

        var result = await _service.CrearFacturaAsync(EspacioId, dto);

        Assert.Equal("xyz", result.Id);
    }

    // -------------------------------------------------------------
    // OBTENER FACTURA
    // -------------------------------------------------------------
    [Fact]
    public async Task ObtenerFacturaAsync_DevuelveFactura()
    {
        var factura = new Factura { Id = "1", Nombre = "Test" };
        var facturaDto = new FacturaDto { Id = "1", Nombre = "Test" };

        _repoMock.Setup(r => r.GetByIdAsync(EspacioId, "1", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(factura);
        _mapperMock.Setup(m => m.Map<FacturaDto>(factura)).Returns(facturaDto);

        var result = await _service.ObtenerFacturaAsync(EspacioId, "1");

        Assert.NotNull(result);
        Assert.Equal("1", result.Id);
    }

    [Fact]
    public async Task ObtenerFacturaAsync_DevuelveNullSiNoExiste()
    {
        _repoMock.Setup(r => r.GetByIdAsync(EspacioId, "1", It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Factura)null);

        var result = await _service.ObtenerFacturaAsync(EspacioId, "1");

        Assert.Null(result);
    }

    // -------------------------------------------------------------
    // LISTAR TODAS
    // -------------------------------------------------------------
    [Fact]
    public async Task ListarTodasAsync_DevuelveListaMapeada()
    {
        var lista = new List<Factura>
        {
            new Factura { Id = "1", Nombre = "A" },
            new Factura { Id = "2", Nombre = "B" }
        };

        _repoMock.Setup(r => r.GetAllAsync(EspacioId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(lista);

        _mapperMock.Setup(m => m.Map<FacturaDto>(It.IsAny<Factura>()))
                   .Returns<Factura>(f => new FacturaDto { Id = f.Id, Nombre = f.Nombre });

        var result = await _service.ListarTodasAsync(EspacioId);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, f => f.Id == "1");
        Assert.Contains(result, f => f.Id == "2");
    }

    // -------------------------------------------------------------
    // ACTUALIZAR COMPLETA
    // -------------------------------------------------------------
    [Fact]
    public async Task ActualizarFacturaCompletaAsync_ActualizaCorrectamente()
    {
        var dto = new UpdateFacturaDto { Nombre = "Nuevo" };
        var domainMapped = new Factura { Id = "1", Nombre = "Nuevo" };
        var updated = new Factura { Id = "1", Nombre = "Nuevo" };
        var updatedDto = new FacturaDto { Id = "1", Nombre = "Nuevo" };

        _mapperMock.Setup(m => m.Map<Factura>(dto)).Returns(domainMapped);
        _repoMock.Setup(r => r.GetByIdAsync(EspacioId, "1", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(updated);
        _mapperMock.Setup(m => m.Map<FacturaDto>(updated)).Returns(updatedDto);

        var result = await _service.ActualizarFacturaCompletaAsync(EspacioId, "1", dto);

        Assert.Equal("1", result.Id);
        Assert.Equal("Nuevo", result.Nombre);
    }

    // -------------------------------------------------------------
    // ACTUALIZAR MERGE
    // -------------------------------------------------------------
    [Fact]
    public async Task ActualizarFacturaMergeAsync_DevuelveNullSiNoExiste()
    {
        _repoMock.Setup(r => r.GetByIdAsync(EspacioId, "1", It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Factura)null);

        var result = await _service.ActualizarFacturaMergeAsync(EspacioId, "1", new UpdateFacturaDto());

        Assert.Null(result);
    }

    [Fact]
    public async Task ActualizarFacturaMergeAsync_ActualizaCorrectamente()
    {
        var existing = new Factura { Id = "1", Nombre = "Viejo" };
        var updated = new Factura { Id = "1", Nombre = "Nuevo" };
        var updatedDto = new FacturaDto { Id = "1", Nombre = "Nuevo" };

        _repoMock.Setup(r => r.GetByIdAsync(EspacioId, "1", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing);
        _mapperMock.Setup(m => m.Map(It.IsAny<UpdateFacturaDto>(), existing))
                   .Callback<UpdateFacturaDto, Factura>((dto, f) => f.Nombre = "Nuevo");
        _repoMock.Setup(r => r.GetByIdAsync(EspacioId, "1", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(updated);
        _mapperMock.Setup(m => m.Map<FacturaDto>(updated)).Returns(updatedDto);

        var result = await _service.ActualizarFacturaMergeAsync(EspacioId, "1", new UpdateFacturaDto());

        Assert.Equal("Nuevo", result.Nombre);
    }

    // -------------------------------------------------------------
    // ACTUALIZAR PARCIAL
    // -------------------------------------------------------------
    [Fact]
    public async Task ActualizarFacturaParcialAsync_SinCambios_DevuelveActual()
    {
        var existing = new Factura { Id = "1", Nombre = "A" };
        var dto = new UpdateFacturaDto(); // todo null

        _repoMock.Setup(r => r.GetByIdAsync(EspacioId, "1", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing);
        _mapperMock.Setup(m => m.Map<FacturaDto>(existing))
                   .Returns(new FacturaDto { Id = "1", Nombre = "A" });

        var result = await _service.ActualizarFacturaParcialAsync(EspacioId, "1", dto);

        Assert.Equal("1", result.Id);
        Assert.Equal("A", result.Nombre);
    }

    // -------------------------------------------------------------
    // ELIMINAR
    // -------------------------------------------------------------
    [Fact]
    public async Task EliminarFacturaAsync_DevuelveFalseSiNoExiste()
    {
        _repoMock.Setup(r => r.GetByIdAsync(EspacioId, "1", It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Factura)null);

        var result = await _service.EliminarFacturaAsync(EspacioId, "1");

        Assert.False(result);
    }

    [Fact]
    public async Task EliminarFacturaAsync_EliminaCorrectamente()
    {
        _repoMock.Setup(r => r.GetByIdAsync(EspacioId, "1", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new Factura { Id = "1" });

        var result = await _service.EliminarFacturaAsync(EspacioId, "1");

        Assert.True(result);
        _repoMock.Verify(r => r.DeleteAsync(EspacioId, "1", It.IsAny<CancellationToken>()), Times.Once);
    }
}
