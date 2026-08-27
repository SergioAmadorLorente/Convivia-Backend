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
            if (string.IsNullOrWhiteSpace(dto.Nombre)) throw new ArgumentException("Nombre no puede estar vac�o", nameof(dto.Nombre));
            if (dto.Precio < 0) throw new ArgumentException("Precio no puede ser negativo", nameof(dto.Precio));
            if (dto.Deudores == null || dto.Deudores.Count == 0)
                throw new ArgumentException("Debe haber al menos un deudor en la factura", nameof(dto.Deudores));
            if (dto.PagoMediano == null) dto.PagoMediano = (float) dto.Precio / dto.Deudores.Count;
            var facturaDomain = _mapper.Map<Factura>(dto);
            if (facturaDomain.Pagado && facturaDomain.FechaPago == null)
            {
                facturaDomain.FechaPago = DateTime.UtcNow;
            }
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
        private const int DiasExpiracionFacturaPagada = 15;

        private bool EstaFacturaExpirada(Factura f)
        {
            if (!f.Pagado) return false;
            var fechaRef = f.FechaPago ?? f.FechaCreacion;
            return fechaRef <= DateTime.UtcNow.AddDays(-DiasExpiracionFacturaPagada);
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

            if (EstaFacturaExpirada(domain))
            {
                _ = _facturaRepository.DeleteAsync(espacioId, id, ct);
                return null;
            }
            
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
            if (list == null) return new List<FacturaDto>();

            var validas = new List<FacturaDto>();
            foreach (var f in list)
            {
                if (EstaFacturaExpirada(f))
                {
                    _ = _facturaRepository.DeleteAsync(espacioId, f.Id, ct);
                    continue;
                }

                var dto = _mapper.Map<FacturaDto>(f);
                dto.TieneImagen = f.DocumentoImagen != null && f.DocumentoImagen.Length > 0;
                validas.Add(dto);
            }
            
            return validas;
        }

        /// <summary>
        /// Lista todas las facturas de un espacio creadas por un usuario específico.
        /// </summary>
        public async Task<List<FacturaDto>> ListarPorCreadorAsync(string espacioId, string creadorId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(espacioId)) throw new ArgumentNullException(nameof(espacioId));
            if (string.IsNullOrWhiteSpace(creadorId)) throw new ArgumentNullException(nameof(creadorId));

            var list = await _facturaRepository.GetByCreadorAsync(espacioId, creadorId, ct);
            if (list == null) return new List<FacturaDto>();

            var validas = new List<FacturaDto>();
            foreach (var f in list)
            {
                if (EstaFacturaExpirada(f))
                {
                    _ = _facturaRepository.DeleteAsync(espacioId, f.Id, ct);
                    continue;
                }

                var dto = _mapper.Map<FacturaDto>(f);
                dto.TieneImagen = f.DocumentoImagen != null && f.DocumentoImagen.Length > 0;
                validas.Add(dto);
            }

            return validas;
        }

        /// <summary>
        /// Lista todas las facturas de un espacio donde un usuario es deudor.
        /// </summary>
        public async Task<List<FacturaDto>> ListarPorDeudorAsync(string espacioId, string deudorId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(espacioId)) throw new ArgumentNullException(nameof(espacioId));
            if (string.IsNullOrWhiteSpace(deudorId)) throw new ArgumentNullException(nameof(deudorId));

            var facturasDeudor = await _facturaRepository.GetByDeudorAsync(espacioId, deudorId, ct);
            if (facturasDeudor == null) return new List<FacturaDto>();

            var validas = new List<FacturaDto>();
            foreach (var f in facturasDeudor)
            {
                if (EstaFacturaExpirada(f))
                {
                    _ = _facturaRepository.DeleteAsync(espacioId, f.Id, ct);
                    continue;
                }

                var dto = _mapper.Map<FacturaDto>(f);
                dto.TieneImagen = f.DocumentoImagen != null && f.DocumentoImagen.Length > 0;
                validas.Add(dto);
            }

            return validas;
        }

        /// <summary>
        /// Overwrite completo: reemplaza todo el documento en Firestore.
        /// </summary>
        public async Task<FacturaDto?> ActualizarFacturaCompletaAsync(string espacioId, string id, UpdateFacturaDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(espacioId)) throw new ArgumentNullException(nameof(espacioId));
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            var existing = await _facturaRepository.GetByIdAsync(espacioId, id, ct);
            if (existing == null) return null;

            var domain = _mapper.Map<Factura>(dto);
            domain.Id = id;
            if (domain.Pagado && domain.FechaPago == null)
            {
                domain.FechaPago = DateTime.UtcNow;
            }

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
            if (existing.Pagado && existing.FechaPago == null)
            {
                existing.FechaPago = DateTime.UtcNow;
            }

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
            var existent = await _facturaRepository.GetByIdAsync(espacioId, id, ct);
            if (existent == null) return null;

            var updates = ObtenerActualizacionesDesdeDto(dto);

            // Si se está marcando como pagada y aún no tiene FechaPago, establecerla automáticamente.
            // Es el campo que usa el job de limpieza para saber cuándo borrar la factura (15 días después).
            if (dto.Pagado == true && existent.FechaPago == null && !updates.ContainsKey("FechaPago"))
                updates["FechaPago"] = DateTime.UtcNow;

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

        // M�todos para gesti�n de im�genes
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
            if (imagen == null || imagen.Length == 0) throw new ArgumentException("Imagen no puede estar vac�a", nameof(imagen));

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
        /// - PATCH debe enviar �nicamente los campos que cambian; Mapster por s� solo puede generar objetos
        ///   con valores por defecto o nulls que provocar�an sobrescrituras no deseadas en Firestore.
        /// - Aqu� construimos expl�citamente un IDictionary<string, object> con las claves exactas de Firestore
        ///   y solo a�adimos propiedades no nulas/validadas, evitando borrar datos accidentalmente.
        /// - Usamos Mapster para operaciones FULL o MERGE (cuando mapeamos DTO sobre la entidad existente
        ///   con IgnoreNullValues), pero para PATCH preferimos este enfoque expl�cito por seguridad, control
        ///   de nombres de campo, y eficiencia (no requiere leer/escribir todo el documento).
        /// 
        /// Instrucciones para compa�eros:
        /// - Si necesit�is a�adir un campo nuevo, actualizar tambi�n la clave usada en este diccionario.
        /// - Validar y filtrar aqu� cualquier campo sensible (p. ej. FechaCreacion, campos de auditor�a).
        /// - Si prefer�s automatizar, pod�is adaptar el patr�n semi-autom�tico (Adapt + filtrar nulos),
        ///   pero revisad cuidadosamente nombres y conversiones antes de enviar a Firestore.
        ///   
        /// Desarrollar� asi todos los services con un helper manual, me parece mucho m�s seguro, se que puede parecer ineficiente, 
        /// pero al tenner controlados las entidadaes que existen y al tener acceso a la bd nosotros, de esta manera es mejor y m�s seguro
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
            if (dto.FechaPago.HasValue) updates["FechaPago"] = DateTime.SpecifyKind(dto.FechaPago.Value, DateTimeKind.Utc);

            return updates;
        }
    }
}

