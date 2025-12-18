using System;
using System.Collections.Generic;
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
        private readonly ILogger<UsuarioService> _logger;

        public UsuarioService(IUsuarioRepository usuarioRepository,IMapper mapper, ILogger<UsuarioService> logger)
        {
            _usuarioRepository = usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<UsuarioDto> CrearUsuarioAsync(CreateUsuarioDto dto, CancellationToken ct = default)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
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
                return new UsuarioDto { IdUsuario = id };
            }

            var createdDto = _mapper.Map<UsuarioDto>(createdDomain);
            if (string.IsNullOrWhiteSpace(createdDto.IdUsuario))
                createdDto.IdUsuario = id;

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

        /// <summary>
        /// Overwrite completo: reemplaza todo el documento en Firestore.
        /// </summary>
        public async Task<UsuarioDto?> ActualizarUsuarioCompletaAsync(string id, UpdateUsuarioDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            // Mapear DTO -> Domain (nuevo objeto completo)
            var domain = _mapper.Map<Usuario>(dto);

            // Asegurar que el Id de dominio coincide con el id pasado
            domain.Id_Factura = id;

            // Persistir como overwrite (merge = false)
            await _facturaRepository.UpdateAsync(id, domain, merge: false, ct);

            var updated = await _facturaRepository.GetByIdAsync(id, ct);
            return updated == null ? null : _mapper.Map<FacturaDto>(updated);
        }

        /// <summary>
        /// Merge: fusiona los campos del objeto con los del documento existente (SetOptions.MergeAll).
        /// </summary>
        public async Task<FacturaDto?> ActualizarFacturaMergeAsync(string id, UpdateFacturaDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var existing = await _facturaRepository.GetByIdAsync(id, ct);
            if (existing == null) return null;

            // Mapear DTO sobre la entidad existente (Mapster configurado para IgnoreNullValues)
            _mapper.Map(dto, existing);

            // Persistir con merge para evitar sobrescribir campos no mapeados
            await _facturaRepository.UpdateAsync(id, existing, merge: true, ct);

            var updated = await _facturaRepository.GetByIdAsync(id, ct);
            return updated == null ? null : _mapper.Map<FacturaDto>(updated);
        }

        /// <summary>
        /// Parcial / PATCH: construye un diccionario con solo las propiedades no nulas del DTO
        /// y llama a la sobrecarga del repositorio que acepta IDictionary (update parcial).
        /// </summary>
        public async Task<FacturaDto?> ActualizarFacturaParcialAsync(string id, UpdateFacturaDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var updates = ObtenerActualizacionesDesdeDto(dto);
            if (updates.Count == 0)
            {
                // Nada que actualizar: devolver la entidad actual
                var current = await _facturaRepository.GetByIdAsync(id, ct);
                return current == null ? null : _mapper.Map<FacturaDto>(current);
            }

            // useSetMerge: false -> UpdateAsync estricto (fallará si no existe)
            await _facturaRepository.UpdateAsync(id, updates, useSetMerge: false, ct);

            var updated = await _facturaRepository.GetByIdAsync(id, ct);
            return updated == null ? null : _mapper.Map<FacturaDto>(updated);
        }
        public async Task<bool> EliminarAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) return false;
            try
            {
                await _usuarioRepository.DeleteAsync(id, ct);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error EliminarUsuario {Id}", id);
                throw;
            }
        }
    }
}