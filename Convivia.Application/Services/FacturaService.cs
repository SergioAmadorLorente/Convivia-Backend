using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Convivia.Shared.DTOs;
using Convivia.Domain.Entities;
using MapsterMapper;
using Microsoft.Extensions.Logging;
using Convivia.Application.Repositories;

namespace Convivia.Application.Services
{
    /// <summary>
    /// Servicio de aplicación que orquesta la lógica de facturas usando IFacturaRepository.
    /// </summary>
    public class FacturaService
    {
        private readonly IFacturaRepository _facturaRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<FacturaService> _logger;

        public FacturaService(IFacturaRepository facturaRepository, IMapper mapper, ILogger<FacturaService> logger)
        {
            _facturaRepository = facturaRepository ?? throw new ArgumentNullException(nameof(facturaRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Crea una factura y devuelve la factura persistida (con Id y metadatos).
        /// </summary>
        public async Task<FacturaDto> CrearFacturaAsync(CreateFacturaDto dto, CancellationToken ct = default)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Nombre)) throw new ArgumentException("Nombre no puede estar vacío", nameof(dto.Nombre));
            if (dto.Precio < 0) throw new ArgumentException("Precio no puede ser negativo", nameof(dto.Precio));

            // DTO -> Domain
            var facturaDomain = _mapper.Map<Factura>(dto);

            // Persistir y obtener id           
            var id = await _facturaRepository.AddAsync(facturaDomain, ct);

            // Recuperar entidad guardada y devolver DTO consistente
            var createdDomain = await _facturaRepository.GetByIdAsync(id, ct);
            if (createdDomain == null)
            {
                // devolver DTO mínimo con id para evitar fallos en rutas
                return new FacturaDto { Id = id };
            }

            var createdDto = _mapper.Map<FacturaDto>(createdDomain);
            if (string.IsNullOrWhiteSpace(createdDto.Id))
                createdDto.Id = id;

            return createdDto;
        }

        /// <summary>
        /// Obtiene una factura por id.
        /// </summary>
        public async Task<FacturaDto?> ObtenerFacturaAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            var domain = await _facturaRepository.GetByIdAsync(id, ct);
            return domain == null ? null : _mapper.Map<FacturaDto>(domain);
        }

        /// <summary>
        /// Lista todas las facturas.
        /// </summary>
        public async Task<List<FacturaDto>> ListarTodasAsync(CancellationToken ct = default)
        {
            var list = await _facturaRepository.GetAllAsync(ct);
            return list?.Select(f => _mapper.Map<FacturaDto>(f)).ToList() ?? new List<FacturaDto>();
        }

        /// <summary>
        /// Overwrite completo: reemplaza todo el documento en Firestore.
        /// </summary>
        public async Task<FacturaDto?> ActualizarFacturaCompletaAsync(string id, UpdateFacturaDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            // Mapear DTO -> Domain (nuevo objeto completo)
            var domain = _mapper.Map<Factura>(dto);

            // Asegurar que el Id de dominio coincide con el id pasado
            domain.Id = id;

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




        /// <summary>
        /// Elimina una factura.
        /// </summary>
        public async Task<bool> EliminarFacturaAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));

            var existing = await _facturaRepository.GetByIdAsync(id, ct);
            if (existing == null) return false;

            await _facturaRepository.DeleteAsync(id, ct);
            return true;
        }

        /// <summary>
        /// Mapeo manual para PATCH y actualizaciones parciales.
        /// 
        /// Razonamiento(para que MARC SASTRE no me mate xD):
        /// - PATCH debe enviar únicamente los campos que cambian; Mapster por sí solo puede generar objetos
        ///   con valores por defecto o nulls que provocarían sobrescrituras no deseadas en Firestore.
        /// - Aquí construimos explícitamente un IDictionary<string, object> con las claves exactas de Firestore
        ///   y solo añadimos propiedades no nulas/validadas, evitando borrar datos accidentalmente.
        /// - Usamos Mapster para operaciones FULL o MERGE (cuando mapeamos DTO sobre la entidad existente
        ///   con IgnoreNullValues), pero para PATCH preferimos este enfoque explícito por seguridad, control
        ///   de nombres de campo, y eficiencia (no requiere leer/escribir todo el documento).
        /// 
        /// Instrucciones para compañeros:
        /// - Si necesitáis añadir un campo nuevo, actualizar también la clave usada en este diccionario.
        /// - Validar y filtrar aquí cualquier campo sensible (p. ej. FechaCreacion, campos de auditoría).
        /// - Si preferís automatizar, podéis adaptar el patrón semi-automático (Adapt + filtrar nulos),
        ///   pero revisad cuidadosamente nombres y conversiones antes de enviar a Firestore.
        ///   
        /// Desarrollaré asi todos los services con un helper manual, me parece mucho más seguro, se que puede parecer ineficiente, 
        /// pero al tenner controlados las entidadaes que existen y al tener acceso a la bd nosotros, de esta manera es mejor y más seguro
        /// </summary>
        private IDictionary<string, object> ObtenerActualizacionesDesdeDto(UpdateFacturaDto dto)
        {
            var updates = new Dictionary<string, object>();

            // Mapeo explícito por propiedad (seguro y claro)
            if (dto.Nombre != null) updates["Nombre"] = dto.Nombre;
            if (dto.Precio.HasValue) updates["Precio"] = dto.Precio.Value;
            if (dto.Reparto != null && dto.Reparto.Count > 0) updates["Reparto"] = dto.Reparto;
            if (dto.Pagado.HasValue) updates["Pagado"] = dto.Pagado.Value;
            if (dto.DocumentoUrl != null) updates["DocumentoUrl"] = dto.DocumentoUrl;
            if (!string.IsNullOrWhiteSpace(dto.TareaId)) updates["TareaId"] = dto.TareaId;

            return updates;
        }
    }
}
