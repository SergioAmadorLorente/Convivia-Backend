using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Convivia.Application.Repositories;
using Convivia.Infrastructure.Repositories;
using Convivia.Application.Services;
using Convivia.Domain.Entities;
using Convivia.Shared.DTOs;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Convivia.Infrastructure.Models; // o el namespace donde está FirestorePlantillaTarea

namespace Convivia.Application.Tests.Services
{
    public class TareaServiceTests
    {
        private readonly Mock<ITareaRepository> _tareaRepositoryMock;
        private readonly Mock<IUsuarioEspacioRepository> _usuarioEspacioRepositoryMock;
        private readonly Mock<PlantillaTareaRepository> _plantillaTareaRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<PlantillaTareaService>> _plantillaLoggerMock;
        private readonly Mock<ILogger<TareaService>> _tareaLoggerMock;
        private readonly Mock<ITareaRepository> _tareaRepoForPlantillaMock;

        private readonly PlantillaTareaService _plantillaService;
        private readonly TareaService _sut; // System Under Test

        public TareaServiceTests()
        {
            _tareaRepositoryMock = new Mock<ITareaRepository>(MockBehavior.Strict);
            _usuarioEspacioRepositoryMock = new Mock<IUsuarioEspacioRepository>(MockBehavior.Strict);
            _plantillaTareaRepositoryMock = new Mock<PlantillaTareaRepository>(MockBehavior.Strict);
            _mapperMock = new Mock<IMapper>(MockBehavior.Strict);
            _plantillaLoggerMock = new Mock<ILogger<PlantillaTareaService>>();
            _tareaLoggerMock = new Mock<ILogger<TareaService>>();
            _tareaRepoForPlantillaMock = new Mock<ITareaRepository>();

            _plantillaService = new PlantillaTareaService(
                _plantillaTareaRepositoryMock.Object,
                _mapperMock.Object,
                _tareaRepoForPlantillaMock.Object,
                _plantillaLoggerMock.Object
            );

            _sut = new TareaService(
                _tareaRepositoryMock.Object,
                _mapperMock.Object,
                _plantillaService,
                _usuarioEspacioRepositoryMock.Object,
                _tareaLoggerMock.Object
            );
        }

        #region Helpers

        private static CreateTareaDto CreateValidPuntualDto(
            int karma = 5,
            string? userId = "user1")
        {
            return new CreateTareaDto
            {
                Nombre = "Tarea puntual",
                Descripcion = "Desc",
                karma = karma,
                HoraLimite = new TimeOnly(10, 0),
                FechaLimite = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1),
                UsuariosAsignacion = userId != null
                    ? new List<string> { userId }
                    : null
            };
        }

        private static CreateTareaDto CreateValidRecurrenteDto(
            int karma = 5,
            List<int>? dias = null,
            List<string>? usuarios = null)
        {
            return new CreateTareaDto
            {
                Nombre = "Tarea recurrente",
                Descripcion = "Desc",
                karma = karma,
                HoraLimite = new TimeOnly(10, 0),
                DiasRepeticion = dias ?? new List<int> { 1, 3 }, // Lunes, Miércoles (según tu mapping)
                UsuariosAsignacion = usuarios
            };
        }

        private PlantillaTarea CreatePlantilla(
            string id = "p1",
            string tzId = null,
            int? graceMinutes = null,
            List<string>? tareasId = null,
            List<int>? diasRep = null)
        {
            return new PlantillaTarea
            {
                Id = id,
                Nombre = "Plantilla",
                Descripcion = "Desc",
                karma = 10,
                TimeZoneId = tzId ?? TimeZoneInfo.Local.Id,
                TareasId = tareasId ?? new List<string>(),
                DiasRepeticion = diasRep ?? new List<int>()
            };
        }

        private Tarea CreateTarea(
            string id = "t1",
            string plantillaId = "p1",
            int diaSemana = -1,
            TareaEstado estado = TareaEstado.Pendiente,
            DateTime? fechaRealizacion = null,
            TimeOnly? horaLimite = null,
            DateTime? fechaLimite = null,
            string? usuarioEspacioId = null)
        {
            return new Tarea
            {
                Id = id,
                PlantillaId = plantillaId,
                DiaSemana = diaSemana,
                Estado = estado,
                FechaRealizacion = fechaRealizacion,
                HoraLimite = horaLimite,
                UsuarioEspacioId = usuarioEspacioId
            };
        }

        #endregion

        #region AddAsync – validaciones

        [Fact]
        public async Task AddAsync_ShouldThrow_WhenDtoIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _sut.AddAsync("espacio1", null!));
        }

        [Fact]
        public async Task AddAsync_ShouldThrow_WhenEspacioIdIsNullOrWhitespace()
        {
            var dto = CreateValidPuntualDto();

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _sut.AddAsync(" ", dto));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(7)]
        [InlineData(10)]
        public async Task AddAsync_ShouldThrow_WhenKarmaNotAllowed(int karma)
        {
            var dto = CreateValidPuntualDto(karma: karma);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.AddAsync("espacio1", dto));
        }

        [Fact]
        public async Task AddAsync_ShouldThrow_WhenHoraLimiteDefault()
        {
            var dto = CreateValidPuntualDto();
            dto.HoraLimite = default;

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.AddAsync("espacio1", dto));
        }

        [Fact]
        public async Task AddAsync_ShouldThrow_WhenPuntualWithoutFechaLimite()
        {
            var dto = CreateValidPuntualDto();
            dto.FechaLimite = null;

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.AddAsync("espacio1", dto));
        }

        [Fact]
        public async Task AddAsync_ShouldThrow_WhenRecurrente_DiaRepeticionFueraDeRango()
        {
            var dto = CreateValidRecurrenteDto(dias: new List<int> { 0, 7 });

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.AddAsync("espacio1", dto));
        }

        [Fact]
        public async Task AddAsync_ShouldThrow_WhenRecurrente_DiasDuplicados()
        {
            var dto = CreateValidRecurrenteDto(dias: new List<int> { 1, 1 });

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.AddAsync("espacio1", dto));
        }

        [Fact]
        public async Task AddAsync_ShouldThrow_WhenPuntualConMasDeUnUsuario()
        {
            var dto = CreateValidPuntualDto();
            dto.UsuariosAsignacion = new List<string> { "user1", "user2" };

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.AddAsync("espacio1", dto));
        }

        [Fact]
        public async Task AddAsync_ShouldThrow_WhenRecurrente_UsuariosCountNoCoincide()
        {
            var dto = CreateValidRecurrenteDto(
                dias: new List<int> { 1, 3, 5 },
                usuarios: new List<string> { "user1", "user2" });

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.AddAsync("espacio1", dto));
        }

        #endregion

        #region AddAsync – comportamiento puntual

        [Fact]
        public async Task AddAsync_Puntual_ShouldCreatePlantillaAndSingleTarea_WithCorrectFields()
        {
            var dto = CreateValidPuntualDto();

            var plantillaCreateDto = new CreatePlantillaTareaDto
            {
                DiasRepeticion = new List<int>(),
                UsuariosAsignacion = new List<string>(),
                TareasId = new List<string>()
            };

            _mapperMock
                .Setup(m => m.Map<CreatePlantillaTareaDto>(dto))
                .Returns(plantillaCreateDto);

            _mapperMock
                .Setup(m => m.Map<Tarea>(dto))
                .Returns(new Tarea());

            _plantillaTareaRepositoryMock
                .Setup(r => r.AddAsync(It.IsAny<FirestorePlantillaTarea>(), "espacio1", It.IsAny<CancellationToken>()))
                .ReturnsAsync("plantilla-123")
                .Callback<PlantillaTarea, string, CancellationToken>((p, e, ct) =>
                {
                    // comprobación básica de mapping (nombre, karma, etc.) si quieres
                });

            List<Tarea>? tareasGuardadas = null;

            _tareaRepositoryMock
                .Setup(r => r.AddAsyncList(It.IsAny<List<Tarea>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<string> { "tarea-1" })
                .Callback<IEnumerable<Tarea>, CancellationToken>((t, ct) =>
                {
                    tareasGuardadas = t.ToList();
                });

            var result = await _sut.AddAsync("espacio1", dto);

            Assert.Equal("plantilla-123", result);
            Assert.NotNull(tareasGuardadas);
            Assert.Single(tareasGuardadas);
            var t1 = tareasGuardadas![0];
            Assert.Equal(-1, t1.DiaSemana);
            Assert.Equal(TareaEstado.Pendiente, t1.Estado);
            Assert.Equal(dto.HoraLimite, t1.HoraLimite);
            Assert.Equal("user1", t1.UsuarioEspacioId);
            Assert.False(string.IsNullOrWhiteSpace(t1.Id));
            Assert.Equal("plantilla-123", t1.PlantillaId);
            Assert.Contains(t1.Id, plantillaCreateDto.TareasId);
        }

        #endregion

        #region AddAsync – comportamiento recurrente

        [Fact]
        public async Task AddAsync_Recurrente_SinUsuarios_CreaTareasSinUsuario()
        {
            var dto = CreateValidRecurrenteDto(dias: new List<int> { 1, 3 });

            var plantillaCreateDto = new CreatePlantillaTareaDto
            {
                DiasRepeticion = new List<int>(),
                UsuariosAsignacion = new List<string>(),
                TareasId = new List<string>()
            };

            _mapperMock
                .Setup(m => m.Map<CreatePlantillaTareaDto>(dto))
                .Returns(plantillaCreateDto);

            _mapperMock
                .Setup(m => m.Map<Tarea>(dto))
                .Returns(new Tarea());

            _plantillaTareaRepositoryMock
                .Setup(r => r.AddAsync(It.IsAny<FirestorePlantillaTarea>(), "espacio1", It.IsAny<CancellationToken>()))
                .ReturnsAsync("plantilla-123");

            List<Tarea>? tareasGuardadas = null;

            _tareaRepositoryMock
                .Setup(r => r.AddAsyncList(It.IsAny<List<Tarea>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<string> { "t1", "t2" })
                .Callback<IEnumerable<Tarea>, CancellationToken>((t, ct) =>
                {
                    tareasGuardadas = t.ToList();
                });

            var result = await _sut.AddAsync("espacio1", dto);

            Assert.Equal("plantilla-123", result);
            Assert.NotNull(tareasGuardadas);
            Assert.Equal(2, tareasGuardadas!.Count);

            Assert.All(tareasGuardadas, t => Assert.Equal(TareaEstado.Pendiente, t.Estado));
            Assert.All(tareasGuardadas, t => Assert.Null(t.UsuarioEspacioId));
            Assert.All(tareasGuardadas, t => Assert.Equal("plantilla-123", t.PlantillaId));
        }

        [Fact]
        public async Task AddAsync_Recurrente_UsuarioUnico_AsignadoSoloAPrimeraTarea()
        {
            var dto = CreateValidRecurrenteDto(
                dias: new List<int> { 1, 3 },
                usuarios: new List<string> { "userX" });

            var plantillaCreateDto = new CreatePlantillaTareaDto
            {
                DiasRepeticion = new List<int>(),
                UsuariosAsignacion = new List<string>(),
                TareasId = new List<string>()
            };

            _mapperMock
                .Setup(m => m.Map<CreatePlantillaTareaDto>(dto))
                .Returns(plantillaCreateDto);

            _mapperMock
                .Setup(m => m.Map<Tarea>(dto))
                .Returns(new Tarea());

            _plantillaTareaRepositoryMock
                .Setup(r => r.AddAsync(It.IsAny<FirestorePlantillaTarea>(), "espacio1", It.IsAny<CancellationToken>()))
                .ReturnsAsync("plantilla-123");

            List<Tarea>? tareasGuardadas = null;

            _tareaRepositoryMock
                .Setup(r => r.AddAsyncList(It.IsAny<List<Tarea>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<string> { "t1", "t2" })
                .Callback<IEnumerable<Tarea>, CancellationToken>((t, ct) =>
                {
                    tareasGuardadas = t.ToList();
                });

            var result = await _sut.AddAsync("espacio1", dto);

            Assert.Equal("plantilla-123", result);
            Assert.NotNull(tareasGuardadas);
            Assert.Equal(2, tareasGuardadas!.Count);

            Assert.Equal("userX", tareasGuardadas[0].UsuarioEspacioId);
            Assert.Null(tareasGuardadas[1].UsuarioEspacioId);
        }

        [Fact]
        public async Task AddAsync_Recurrente_VariosUsuarios_AsignacionUnoAUno()
        {
            var dto = CreateValidRecurrenteDto(
                dias: new List<int> { 1, 3, 5 },
                usuarios: new List<string> { "u1", "u2", "u3" });

            var plantillaCreateDto = new CreatePlantillaTareaDto
            {
                DiasRepeticion = new List<int>(),
                UsuariosAsignacion = new List<string>(),
                TareasId = new List<string>()
            };

            // pequeño truco: como tu código usa Map<Tarea>(dto) en bucle,
            // devolvemos una NUEVA instancia cada vez
            _mapperMock
                .Setup(m => m.Map<CreatePlantillaTareaDto>(dto))
                .Returns(plantillaCreateDto);

            _mapperMock
                .SetupSequence(m => m.Map<Tarea>(dto))
                .Returns(new Tarea())
                .Returns(new Tarea())
                .Returns(new Tarea());

            _plantillaTareaRepositoryMock
                .Setup(r => r.AddAsync(It.IsAny<FirestorePlantillaTarea>(), "espacio1", It.IsAny<CancellationToken>()))
                .ReturnsAsync("plantilla-123");

            List<Tarea>? tareasGuardadas = null;

            _tareaRepositoryMock
                .Setup(r => r.AddAsyncList(It.IsAny<List<Tarea>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<string> { "t1", "t2", "t3" })
                .Callback<IEnumerable<Tarea>, CancellationToken>((t, ct) =>
                {
                    tareasGuardadas = t.ToList();
                });

            var result = await _sut.AddAsync("espacio1", dto);

            Assert.Equal("plantilla-123", result);
            Assert.NotNull(tareasGuardadas);
            Assert.Equal(3, tareasGuardadas!.Count);

            Assert.Equal("u1", tareasGuardadas[0].UsuarioEspacioId);
            Assert.Equal("u2", tareasGuardadas[1].UsuarioEspacioId);
            Assert.Equal("u3", tareasGuardadas[2].UsuarioEspacioId);
        }

        #endregion

        #region GetAllByEspacioAsync / GetByEspacioAndIdAsync (ya teníamos, más algún caso)

        [Fact]
        public async Task GetAllByEspacioAsync_ShouldThrow_WhenEspacioIdNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _sut.GetAllByEspacioAsync(" "));
        }

        [Fact]
        public async Task GetByEspacioAndIdAsync_ShouldThrow_WhenEspacioOrIdInvalid()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _sut.GetByEspacioAndIdAsync(" ", "id"));

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _sut.GetByEspacioAndIdAsync("esp", " "));
        }

        [Fact]
        public async Task GetByEspacioAndIdAsync_ShouldThrow_WhenPlantillaNotFound()
        {
            _plantillaTareaRepositoryMock
                .Setup(r => r.GetByEspacioAndIdAsync("esp", "p1", It.IsAny<CancellationToken>()))
                .ReturnsAsync((PlantillaTarea?)null);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.GetByEspacioAndIdAsync("esp", "p1"));
        }

        [Fact]
        public async Task GetByEspacioAndIdAsync_ShouldReturnMappedDto()
        {
            var plantilla = CreatePlantilla(id: "p1");

            _plantillaTareaRepositoryMock
                .Setup(r => r.GetByEspacioAndIdAsync("esp", "p1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(plantilla);

            _mapperMock
                .Setup(m => m.Map<PlantillaTareaDto>(plantilla))
                .Returns(new PlantillaTareaDto { Id = "p1", Nombre = "Plantilla 1" });

            var result = await _sut.GetByEspacioAndIdAsync("esp", "p1");

            Assert.NotNull(result);
            Assert.Equal("p1", result.Id);
        }

        #endregion

        #region DeleteAsync – casos extra

        [Fact]
        public async Task DeleteAsync_ShouldThrow_WhenIdsInvalid()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _sut.DeleteAsync(" ", "p1"));

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _sut.DeleteAsync("esp", " "));
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnFalse_WhenPlantillaNotFound()
        {
            _plantillaTareaRepositoryMock
                .Setup(r => r.GetByEspacioAndIdAsync("esp", "p1", It.IsAny<CancellationToken>()))
                .ReturnsAsync((PlantillaTarea?)null);

            var result = await _sut.DeleteAsync("esp", "p1");

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteOnlyPlantilla_WhenNoTareas()
        {
            var plantilla = CreatePlantilla(
                id: "p1",
                tareasId: new List<string>());

            _plantillaTareaRepositoryMock
                .Setup(r => r.GetByEspacioAndIdAsync("esp", "p1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(plantilla);

            _plantillaTareaRepositoryMock
                .Setup(r => r.DeleteAsync("p1", It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await _sut.DeleteAsync("esp", "p1");

            Assert.True(result);

            _tareaRepositoryMock.Verify(r =>
                r.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteTareasAndPlantilla()
        {
            var plantilla = CreatePlantilla(
                id: "p1",
                tareasId: new List<string> { "t1", "t2" });

            _plantillaTareaRepositoryMock
                .Setup(r => r.GetByEspacioAndIdAsync("esp", "p1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(plantilla);

            _tareaRepositoryMock
                .Setup(r => r.DeleteAsync("t1", It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _tareaRepositoryMock
                .Setup(r => r.DeleteAsync("t2", It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _plantillaTareaRepositoryMock
                .Setup(r => r.DeleteAsync("p1", It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await _sut.DeleteAsync("esp", "p1");

            Assert.True(result);

            _tareaRepositoryMock.Verify(r =>
                r.DeleteAsync("t1", It.IsAny<CancellationToken>()), Times.Once);
            _tareaRepositoryMock.Verify(r =>
                r.DeleteAsync("t2", It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region FilterAsync / GetByEstadoAsync / GetByDiaAndEstadoAsync – validaciones básicas

        [Fact]
        public async Task FilterAsync_ShouldThrow_WhenEspacioIdInvalid()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _sut.FilterAsync(" ", null, null, null));
        }

        [Fact]
        public async Task FilterAsync_ShouldThrow_WhenDiaSemanaInvalid()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.FilterAsync("esp", -1, null, null));

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.FilterAsync("esp", 7, null, null));
        }

        [Fact]
        public async Task FilterAsync_ShouldThrow_WhenEstadoInvalid()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.FilterAsync("esp", null, "NO_EXISTE", null));
        }

        [Fact]
        public async Task GetByEstadoAsync_ShouldThrow_WhenEspacioIdInvalid()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _sut.GetByEstadoAsync(" ", TareaEstado.Pendiente));
        }

        [Fact]
        public async Task GetByDiaAndEstadoAsync_ShouldThrow_WhenEspacioIdInvalid()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _sut.GetByDiaAndEstadoAsync(" ", 1, TareaEstado.Pendiente));
        }

        [Fact]
        public async Task GetByDiaAndEstadoAsync_ShouldThrow_WhenDiaSemanaInvalid()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.GetByDiaAndEstadoAsync("esp", -1, TareaEstado.Pendiente));

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.GetByDiaAndEstadoAsync("esp", 7, TareaEstado.Pendiente));
        }

        #endregion

        #region UpdateCompleteAsync – casos de lógica de estado y karma

        [Fact]
        public async Task UpdateCompleteAsync_ShouldThrow_WhenDtoNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _sut.UpdateCompleteAsync("esp", "p1", "t1", null!));
        }

        [Fact]
        public async Task UpdateCompleteAsync_ShouldReturnNull_WhenPlantillaNotFound()
        {
            var dto = new UpdateTareaDto { Estado = "Completada" };

            _plantillaTareaRepositoryMock
                .Setup(r => r.GetByEspacioAndIdAsync("esp", "p1", It.IsAny<CancellationToken>()))
                .ReturnsAsync((PlantillaTarea?)null);

            var result = await _sut.UpdateCompleteAsync("esp", "p1", "t1", dto);

            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateCompleteAsync_ShouldReturnNull_WhenTareaNotFound()
        {
            var dto = new UpdateTareaDto { Estado = "Completada" };
            var plantilla = CreatePlantilla(id: "p1");

            _plantillaTareaRepositoryMock
                .Setup(r => r.GetByEspacioAndIdAsync("esp", "p1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(plantilla);

            _tareaRepositoryMock
                .Setup(r => r.GetInstanciaAsync("p1", "t1", It.IsAny<CancellationToken>()))
                .ReturnsAsync((Tarea?)null);

            var result = await _sut.UpdateCompleteAsync("esp", "p1", "t1", dto);

            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateCompleteAsync_ShouldThrow_WhenEstadoPendienteYOverdue()
        {
            var dto = new UpdateTareaDto { Estado = "Pendiente" };

            var plantilla = CreatePlantilla(id: "p1");

            var tarea = CreateTarea(
                id: "t1",
                plantillaId: "p1",
                estado: TareaEstado.Pendiente,
                diaSemana: -1,
                fechaLimite: DateTime.UtcNow.AddDays(-2),
                horaLimite: new TimeOnly(10, 0));

            _plantillaTareaRepositoryMock
                .Setup(r => r.GetByEspacioAndIdAsync("esp", "p1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(plantilla);

            _tareaRepositoryMock
                .Setup(r => r.GetInstanciaAsync("p1", "t1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(tarea);

            _mapperMock
                .Setup(m => m.Map<Tarea>(dto))
                .Returns(new Tarea { Id = "t1", PlantillaId = "p1" });

            _tareaRepositoryMock
                .Setup(r => r.UpdateAsync("t1", It.IsAny<Tarea>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _tareaRepositoryMock
                .Setup(r => r.GetInstanciaAsync("p1", "t1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(tarea);

            _mapperMock
                .Setup(m => m.Map<TareaDto>(It.IsAny<Tarea>()))
                .Returns(new TareaDto());

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.UpdateCompleteAsync("esp", "p1", "t1", dto));
        }

        [Fact]
        public async Task UpdateCompleteAsync_CompletarTarea_ShouldSumarKarmaSiNoEstabaCompletada()
        {
            var dto = new UpdateTareaDto
            {
                Estado = "Completada",
                FechaRealizacion = DateTime.UtcNow
            };

            var plantilla = CreatePlantilla(id: "p1");
            plantilla.karma = 15;

            var existing = CreateTarea(
                id: "t1",
                plantillaId: "p1",
                estado: TareaEstado.Pendiente,
                diaSemana: -1,
                fechaLimite: DateTime.UtcNow.AddDays(1),
                horaLimite: new TimeOnly(23, 59),
                usuarioEspacioId: "userX");

            _plantillaTareaRepositoryMock
                .Setup(r => r.GetByEspacioAndIdAsync("esp", "p1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(plantilla);

            _tareaRepositoryMock
                .Setup(r => r.GetInstanciaAsync("p1", "t1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing);

            _mapperMock
                .Setup(m => m.Map<Tarea>(dto))
                .Returns(new Tarea { Id = "t1", PlantillaId = "p1", Estado = TareaEstado.Completada });

            _tareaRepositoryMock
                .Setup(r => r.UpdateAsync("t1", It.IsAny<Tarea>(), false, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var updated = existing.Adapt<Tarea>();
            updated.Estado = TareaEstado.Completada;

            _tareaRepositoryMock
                .Setup(r => r.GetInstanciaAsync("p1", "t1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(updated);

            _mapperMock
                .Setup(m => m.Map<TareaDto>(updated))
                .Returns(new TareaDto());

            _usuarioEspacioRepositoryMock
                .Setup(r => r.UpdateKarmaAsync("userX", 15, It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var res = await _sut.UpdateCompleteAsync("esp", "p1", "t1", dto);

            Assert.NotNull(res);
            _usuarioEspacioRepositoryMock.Verify(r =>
                r.UpdateKarmaAsync("userX", 15, It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region UpdateMergeAsync / UpdatePartialAsync / UpdateAsync – validaciones y flujo mínimo

        [Fact]
        public async Task UpdateMergeAsync_ShouldThrow_WhenDtoNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _sut.UpdateMergeAsync("esp", "p1", "t1", null!));
        }

        [Fact]
        public async Task UpdatePartialAsync_ShouldThrow_WhenDtoNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _sut.UpdatePartialAsync("esp", "p1", "t1", null!));
        }

        [Fact]
        public async Task UpdateAsync_ShouldDelegateToUpdatePartialAsync()
        {
            var dto = new UpdateTareaDto
            {
                Estado = "Completada"
            };

            var plantilla = CreatePlantilla(id: "p1");
            var tarea = CreateTarea(
                id: "t1",
                plantillaId: "p1",
                estado: TareaEstado.Pendiente,
                diaSemana: -1,
                fechaLimite: DateTime.UtcNow.AddDays(1),
                horaLimite: new TimeOnly(10, 0));

            _plantillaTareaRepositoryMock
                .Setup(r => r.GetByEspacioAndIdAsync("esp", "p1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(plantilla);

            _tareaRepositoryMock
                .Setup(r => r.GetInstanciaAsync("p1", "t1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(tarea);

            _mapperMock
                .Setup(m => m.Map<Tarea>(dto))
                .Returns(new Tarea { Id = "t1", PlantillaId = "p1", Estado = TareaEstado.Completada });

            _tareaRepositoryMock
                .Setup(r => r.UpdateAsync("t1", It.IsAny<Tarea>(), false, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var updated = tarea.Adapt<Tarea>();
            updated.Estado = TareaEstado.Completada;

            _tareaRepositoryMock
                .Setup(r => r.GetInstanciaAsync("p1", "t1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(updated);

            _mapperMock
                .Setup(m => m.Map<TareaDto>(updated))
                .Returns(new TareaDto());

            var result = await _sut.UpdateAsync("esp", "p1", "t1", dto);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task UpdatePartialAsync_WithoutChanges_ShouldReturnCurrentDto()
        {
            var dto = new UpdateTareaDto(); // sin campos

            var plantilla = CreatePlantilla(id: "p1");
            var existing = CreateTarea(
                id: "t1",
                plantillaId: "p1",
                estado: TareaEstado.Pendiente,
                diaSemana: -1,
                fechaLimite: DateTime.UtcNow.AddDays(1),
                horaLimite: new TimeOnly(10, 0));

            _plantillaTareaRepositoryMock
                .Setup(r => r.GetByEspacioAndIdAsync("esp", "p1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(plantilla);

            _tareaRepositoryMock
                .Setup(r => r.GetInstanciaAsync("p1", "t1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing);

            _mapperMock
                .Setup(m => m.Map<TareaDto>(existing))
                .Returns(new TareaDto { Nombre = "X" });

            var result = await _sut.UpdatePartialAsync("esp", "p1", "t1", dto);

            Assert.NotNull(result);
            Assert.Equal("X", result!.Nombre);

            _tareaRepositoryMock.Verify(r =>
                r.UpdateAsync(It.IsAny<string>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        #endregion
    }
}