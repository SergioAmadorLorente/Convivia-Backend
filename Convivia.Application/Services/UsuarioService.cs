using Convivia.Application.Mappers;
using Convivia.Shared.DTOs;
using Convivia.Shared.Helpers;
using Convivia.Shared.Repositories;
using Mapster;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Convivia.Domain.Repositories;

namespace Convivia.Application.Services
{
    public class UsuarioService
    {
        private readonly IUsuarioRepository _repo;
        private readonly IUsuarioEspacioRepository _usuarioEspacioRepo;
        private readonly ILogger<UsuarioService> _logger;

        public UsuarioService(IUsuarioRepository repo, ILogger<UsuarioService> logger, IUsuarioEspacioRepository usuarioEspacioRepo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _usuarioEspacioRepo = usuarioEspacioRepo ?? throw new ArgumentNullException(nameof(usuarioEspacioRepo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> CrearAsync(CreateUsuarioDto dto, CancellationToken ct = default)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Nombre)) throw new ArgumentException("Nombre requerido");
            if (string.IsNullOrWhiteSpace(dto.Email)) throw new ArgumentException("Email requerido");

            // Usar Mapster para convertir CreateUsuarioDto a Usuario
            var usuarioDomain = dto.Adapt<Convivia.Domain.Entities.Usuario>();
            var usuarioDto = usuarioDomain.Adapt<UsuarioDto>();



            try
            {
                return await _repo.AddAsync(usuarioDto, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando usuario");
                throw;
            }
        }

        public async Task<UsuarioDto?> ObtenerPorIdAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            try
            {
                return await _repo.GetByIdAsync(id, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ObtenerPorId {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<UsuarioDto>> GetByFullNameInvitadoAsync(string Nombre, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(Nombre)) return Array.Empty<UsuarioDto>();
            try
            {
                return await _repo.GetByFullNameInvitadoAsync(Nombre, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ObtenerPorUsuarioFullName {Usuario}", Nombre);
                throw;
            }
        }

        public async Task<bool> EliminarAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Id requerido", nameof(id));

            var usuario = await _repo.GetByIdAsync(id, ct).ConfigureAwait(false);
            if (usuario == null)
                return false;

            var usuarioEspacio = await _usuarioEspacioRepo
                .GetByUsuarioIdAsync(usuario.Id, ct)
                .ConfigureAwait(false);

            if (usuarioEspacio != null)
            {
                await _usuarioEspacioRepo
                    .DeleteAsync(usuarioEspacio.Id_UsuarioEspacio, ct)
                    .ConfigureAwait(false);
            }

            await _repo.DeleteAsync(id, ct).ConfigureAwait(false);

            return true;
        }
    }
}