using Convivia.Application.Mappers;
using Convivia.Shared.DTOs;
using Convivia.Shared.Helpers;
using Mapster;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Convivia.Shared.DTOs;
using Microsoft.Extensions.Logging;
using Convivia.Domain.Entities;
using Convivia.Application.Repositories;
using Mapster;
using MapsterMapper;

namespace Convivia.Application.Services
{
    public class UsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IMapper _mapper;
        private readonly IUsuarioEspacioRepository _usuarioEspacioRepo;
        private readonly ILogger<UsuarioService> _logger;

        public UsuarioService(IUsuarioRepository usuarioRepository,IMapper mapper, ILogger<UsuarioService> logger, IUsuarioEspacioRepository usuarioEspacioRepo)
        {
            _usuarioRepository = usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _usuarioEspacioRepo = usuarioEspacioRepo ?? throw new ArgumentNullException(nameof(usuarioEspacioRepo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<UsuarioDto> CrearUsuarioAsync(CreateUsuarioDto dto, CancellationToken ct = default)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Id)) dto.Id = Guid.NewGuid().ToString("N");
            if (string.IsNullOrEmpty(dto.Nombre)) throw new ArgumentNullException("Nombre no puede estar vacio", nameof(dto.Nombre));
            if (string.IsNullOrWhiteSpace(dto.Email)) throw new ArgumentNullException("Correo no puede estar vacio", nameof(dto.Email));
            if (string.IsNullOrWhiteSpace(dto.Password)) throw new ArgumentNullException("Contrasenya no puede estar vacio", nameof(dto.Password));

            // DTO -> domain
            var usuarioDomain = _mapper.Map<Usuario>(dto);

            // Persistir y obtener id
            var id = await _usuarioRepository.AddAsync(usuarioDomain, ct);

            // Recuperar entidad guardad y dwevolver DTO consistente
            var createdDomain = await _usuarioRepository.GetByIdAsync(id, ct);
            if (createdDomain == null)
            {
                // Devolver dto mínimo con id evitar fallos en rutas 
                return new UsuarioDto { Id = id };
            }

            var createdDto = _mapper.Map<UsuarioDto>(createdDomain);
            if (string.IsNullOrWhiteSpace(createdDto.Id))
                createdDto.Id = id;

            return createdDto;
        }

        public async Task<UsuarioDto?> ObtenerUsuarioAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            var domain = await _usuarioRepository.GetByIdAsync(id, ct);
            return domain == null ? null : _mapper.Map<UsuarioDto>(domain);
        }

        public async Task<List<UsuarioDto>> ListarTodasAsync(CancellationToken ct = default)
        {
            var list = await _usuarioRepository.GetAllAsync(ct);
            return list?.Select(f => _mapper.Map<UsuarioDto>(f)).ToList() ?? new List<UsuarioDto>();
        }
        public async Task<UsuarioDto?> ObtenerPorEmailAsync(string email, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            try
            {
                return await _usuarioRepository.GetByEmailAsync(email, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ObtenerPorEmail {Email}", email);
                throw;
            }
        }
        /// <summary>
        /// Overwrite completo: reemplaza todo el documento en Firestore.
        /// </summary>
        public async Task<UsuarioDto?> ActualizarUsuarioCompletoAsync(string id, UpdateUsuarioDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            // Mapear DTO -> Domain (nuevo objeto completo)
            var domain = _mapper.Map<Usuario>(dto);

            // Asegurar que el Id de dominio coincide con el id pasado
            domain.Id = id;

            // Persistir como overwrite (merge = false)
            await _usuarioRepository.UpdateAsync(id, domain, merge: false, ct);

            var updated = await _usuarioRepository.GetByIdAsync(id, ct);
            return updated == null ? null : _mapper.Map<UsuarioDto>(updated);
        }

        /// <summary>
        /// Merge: fusiona los campos del objeto con los del documento existente (SetOptions.MergeAll).
        /// </summary>
        public async Task<UsuarioDto?> ActualizarUsuarioMergeAsync(string id, UpdateUsuarioDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var existing = await _usuarioRepository.GetByIdAsync(id, ct);
            if (existing == null) return null;

            // Mapear DTO sobre la entidad existente (Mapster configurado para IgnoreNullValues)
            _mapper.Map(dto, existing);

            // Persistir con merge para evitar sobrescribir campos no mapeados
            await _usuarioRepository.UpdateAsync(id, existing, merge: true, ct);

            var updated = await _usuarioRepository.GetByIdAsync(id, ct);
            return updated == null ? null : _mapper.Map<UsuarioDto>(updated);
        }

        /// <summary>
        /// Parcial / PATCH: construye un diccionario con solo las propiedades no nulas del DTO
        /// y llama a la sobrecarga del repositorio que acepta IDictionary (update parcial).
        /// </summary>
        public async Task<UsuarioDto?> ActualizarUsuarioParcialAsync(string id, UpdateUsuarioDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var updates = ObtenerActualizacionesDesdeDto(dto);
            if (updates.Count == 0)
            {
                // Nada que actualizar: devolver la entidad actual
                var current = await _usuarioRepository.GetByIdAsync(id, ct);
                return current == null ? null : _mapper.Map<UsuarioDto>(current);
            }

            // useSetMerge: false -> UpdateAsync estricto (fallará si no existe)
            await _usuarioRepository.UpdateAsync(id, updates, useSetMerge: false, ct);

            var updated = await _usuarioRepository.GetByIdAsync(id, ct);
            return updated == null ? null : _mapper.Map<UsuarioDto>(updated);
        }
        public async Task<bool> EliminarUsuarioAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));

            var existing = await _usuarioRepository.GetByIdAsync(id, ct);
            if (existing == null) return false;

            var usuariosEspacio = await _usuarioEspacioRepo
                .GetByUsuarioIdAsync(id, ct)
                .ConfigureAwait(false);

            if (usuariosEspacio != null && usuariosEspacio.Any())
            {
                foreach (var usuarioEspacio in usuariosEspacio)
                {
                    await _usuarioEspacioRepo
                        .DeleteAsync(usuarioEspacio.Id, ct)
                        .ConfigureAwait(false);
                }
            }
            
            await _usuarioRepository.DeleteAsync(id, ct);
            return true;
        }
        private IDictionary<string, object> ObtenerActualizacionesDesdeDto(UpdateUsuarioDto dto)
        {
            var updates = new Dictionary<string, object>();

            if (dto.Nombre != null) updates["Nombre"] = dto.Nombre;
            if (dto.Telefono != null) updates["Telefono"] = dto.Telefono;
            if (dto.Email != null) updates["Email"] = dto.Email;
            if (dto.Password != null) updates["Password"] = dto.Password;
            if (dto.Premium != null) updates["Premium"] = dto.Premium;

            return updates;
        }
    }
}