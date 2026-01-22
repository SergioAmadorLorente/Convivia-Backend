using System;
using System.Collections.Generic;
using System.IO;
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

        public async Task<FacturaDto> CrearFacturaAsync(string espacioId, CreateFacturaDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(espacioId)) throw new ArgumentNullException(nameof(espacioId));
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Nombre)) throw new ArgumentException("Nombre no puede estar vacío", nameof(dto.Nombre));
            if (dto.Precio < 0) throw new ArgumentException("Precio no puede ser negativo", nameof(dto.Precio));
            if (dto.Deudores == null || dto.Deudores.Count == 0)
                throw new ArgumentException("Debe haber al menos un deudor en la factura", nameof(dto.Deudores));
            if (dto.PagoMediano == null) dto.PagoMediano = (float) dto.Precio / dto.Deudores.Count;
            var facturaDomain = _mapper.Map<Factura>(dto);
            var id = await _facturaRepository.AddAsync(espacioId, facturaDomain, ct);

            var createdDomain = await _facturaRepository.GetByIdAsync(espacioId, id, ct);
            if (createdDomain == null)
            {
                return new FacturaDto { Id = id };
            }

            var createdDto = _mapper.Map<FacturaDto>(createdDomain);
            if (string.IsNullOrWhiteSpace(createdDto.Id))
                createdDto.Id = id;

            createdDto.TieneImagen = createdDomain.DocumentoImagen != null && createdDomain.DocumentoImagen.Length > 0;
            return createdDto;
        }

        /// <summary>
        /// Obtiene una factura por id.
        /// </summary>
        public async Task<FacturaDto?> ObtenerFacturaAsync(string espacioId, string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(espacioId)) throw new ArgumentNullException(nameof(espacioId));
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            
            var domain = await _facturaRepository.GetByIdAsync(espacioId, id, ct);
            if (domain == null) return null;
            
            var dto = _mapper.Map<FacturaDto>(domain);
            dto.TieneImagen = domain.DocumentoImagen != null && domain.DocumentoImagen.Length > 0;
            return dto;
        }

        /// <summary>
        /// Lista todas las facturas de un espacio.
        /// </summary>
        public async Task<List<FacturaDto>> ListarTodasAsync(string espacioId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(espacioId)) throw new ArgumentNullException(nameof(espacioId));
            
            var list = await _facturaRepository.GetAllAsync(espacioId, ct);
            var dtos = list?.Select(f =>
            {
                var dto = _mapper.Map<FacturaDto>(f);
                dto.TieneImagen = f.DocumentoImagen != null && f.DocumentoImagen.Length > 0;
                return dto;
            }).ToList() ?? new List<FacturaDto>();
            
            return dtos;
        }

        /// <summary>
        /// Lista todas las facturas de un espacio creadas por un usuario específico.
        /// </summary>
        public async Task<List<FacturaDto>> ListarPorCreadorAsync(string espacioId, string creadorId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(espacioId)) throw new ArgumentNullException(nameof(espacioId));
            if (string.IsNullOrWhiteSpace(creadorId)) throw new ArgumentNullException(nameof(creadorId));

            var list = await _facturaRepository.GetByCreadorAsync(espacioId, creadorId, ct);
            var dtos = list?.Select(f =>
            {
                var dto = _mapper.Map<FacturaDto>(f);
                dto.TieneImagen = f.DocumentoImagen != null && f.DocumentoImagen.Length > 0;
                return dto;
            }).ToList() ?? new List<FacturaDto>();

            return dtos;
        }

        /// <summary>
        /// Lista todas las facturas de un espacio donde un usuario es deudor.
        /// </summary>
        public async Task<List<FacturaDto>> ListarPorDeudorAsync(string espacioId, string deudorId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(espacioId)) throw new ArgumentNullException(nameof(espacioId));
            if (string.IsNullOrWhiteSpace(deudorId)) throw new ArgumentNullException(nameof(deudorId));

            var facturasDeudor = await _facturaRepository.GetByDeudorAsync(espacioId, deudorId, ct);
            var dtos = facturasDeudor?.Select(f =>
            {
                var dto = _mapper.Map<FacturaDto>(f);
                dto.TieneImagen = f.DocumentoImagen != null && f.DocumentoImagen.Length > 0;
                return dto;
            }).ToList() ?? new List<FacturaDto>();

            return dtos;
        }

        /// <summary>
        /// Overwrite completo: reemplaza todo el documento en Firestore.
        /// </summary>
        public async Task<FacturaDto?> ActualizarFacturaCompletaAsync(string espacioId, string id, UpdateFacturaDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(espacioId)) throw new ArgumentNullException(nameof(espacioId));
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            var existing = await _facturaRepository.GetByIdAsync(id, ct);
            if (existing == null) return null;

            var domain = _mapper.Map<Factura>(dto);
            domain.Id = id;

            await _facturaRepository.UpdateAsync(espacioId, id, domain, merge: false, ct);

            var updated = await _facturaRepository.GetByIdAsync(espacioId, id, ct);
            if (updated == null) return null;
            
            var resultDto = _mapper.Map<FacturaDto>(updated);
            resultDto.TieneImagen = updated.DocumentoImagen != null && updated.DocumentoImagen.Length > 0;
            return resultDto;
        }

        /// <summary>
        /// Merge: fusiona los campos del objeto con los del documento existente (SetOptions.MergeAll).
        /// </summary>
        public async Task<FacturaDto?> ActualizarFacturaMergeAsync(string espacioId, string id, UpdateFacturaDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(espacioId)) throw new ArgumentNullException(nameof(espacioId));
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var existing = await _facturaRepository.GetByIdAsync(espacioId, id, ct);
            if (existing == null) return null;

            _mapper.Map(dto, existing);

            await _facturaRepository.UpdateAsync(espacioId, id, existing, merge: true, ct);

            var updated = await _facturaRepository.GetByIdAsync(espacioId, id, ct);
            if (updated == null) return null;
            
            var resultDto = _mapper.Map<FacturaDto>(updated);
            resultDto.TieneImagen = updated.DocumentoImagen != null && updated.DocumentoImagen.Length > 0;
            return resultDto;
        }

        /// <summary>
        /// Parcial / PATCH: construye un diccionario con solo las propiedades no nulas del DTO
        /// y llama a la sobrecarga del repositorio que acepta IDictionary (update parcial).
        /// </summary>
        public async Task<FacturaDto?> ActualizarFacturaParcialAsync(string espacioId, string id, UpdateFacturaDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(espacioId)) throw new ArgumentNullException(nameof(espacioId));
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            var existent = await _facturaRepository.GetByIdAsync(id, ct);
            if (existent == null) return null;

            var updates = ObtenerActualizacionesDesdeDto(dto);
            if (updates.Count == 0)
            {
                var current = await _facturaRepository.GetByIdAsync(espacioId, id, ct);
                if (current == null) return null;
                
                var currentDto = _mapper.Map<FacturaDto>(current);
                currentDto.TieneImagen = current.DocumentoImagen != null && current.DocumentoImagen.Length > 0;
                return currentDto;
            }

            await _facturaRepository.UpdateAsync(espacioId, id, updates, useSetMerge: false, ct);

            var updated = await _facturaRepository.GetByIdAsync(espacioId, id, ct);
            if (updated == null) return null;
            
            var resultDto = _mapper.Map<FacturaDto>(updated);
            resultDto.TieneImagen = updated.DocumentoImagen != null && updated.DocumentoImagen.Length > 0;
            return resultDto;
        }

        public async Task<bool> EliminarFacturaAsync(string espacioId, string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(espacioId)) throw new ArgumentNullException(nameof(espacioId));
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));

            var existing = await _facturaRepository.GetByIdAsync(espacioId, id, ct);
            if (existing == null) return false;

            await _facturaRepository.DeleteAsync(espacioId, id, ct);
            return true;
        }

        // Métodos para gestión de imágenes
        public async Task<byte[]?> ObtenerImagenAsync(string espacioId, string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(espacioId)) throw new ArgumentNullException(nameof(espacioId));
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));

            return await _facturaRepository.GetImagenAsync(espacioId, id, ct);
        }

        public async Task<bool> ActualizarImagenAsync(string espacioId, string id, byte[] imagen, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(espacioId)) throw new ArgumentNullException(nameof(espacioId));
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (imagen == null || imagen.Length == 0) throw new ArgumentException("Imagen no puede estar vacía", nameof(imagen));

            var existing = await _facturaRepository.GetByIdAsync(espacioId, id, ct);
            if (existing == null) return false;

            await _facturaRepository.UpdateImagenAsync(espacioId, id, imagen, ct);
            return true;
        }

        /// <summary>
        /// Elimina una factura.
        /// </summary>
        public async Task<bool> EliminarImagenAsync(string espacioId, string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(espacioId)) throw new ArgumentNullException(nameof(espacioId));
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));

            var existing = await _facturaRepository.GetByIdAsync(espacioId, id, ct);
            if (existing == null) return false;

            await _facturaRepository.DeleteImagenAsync(espacioId, id, ct);
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

            if (dto.Nombre != null) updates["Nombre"] = dto.Nombre;
            if (dto.Precio.HasValue) updates["Precio"] = dto.Precio.Value;
            if (dto.PagoMediano.HasValue) updates["PagoMediano"] = dto.PagoMediano.Value;
            if (dto.Deudores != null && dto.Deudores.Count > 0) updates["Deudores"] = dto.Deudores;
            if (dto.Pagado.HasValue) updates["Pagado"] = dto.Pagado.Value;
            if (dto.CreadorFactura != null) updates["CreadorFactura"] = dto.CreadorFactura;

            return updates;
        }
    }
}
