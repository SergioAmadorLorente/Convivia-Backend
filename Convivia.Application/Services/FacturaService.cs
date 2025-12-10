using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Convivia.Domain.Repositories;
using Convivia.Shared.DTOs;
using Convivia.Shared.Repositories;

namespace Convivia.Application.Services
{
    /// <summary>
    /// Servicio de aplicación que orquesta la lógica de facturas usando IFacturaRepository.
    /// </summary>
    public class FacturaService
    {
        private readonly IFacturaRepository _facturaRepository;

        public FacturaService(IFacturaRepository facturaRepository)
        {
            _facturaRepository = facturaRepository ?? throw new ArgumentNullException(nameof(facturaRepository));
        }

        /// <summary>
        /// Crea una factura y devuelve la factura persistida (con Id y metadatos).
        /// </summary>
        public async Task<FacturaDto> CrearFacturaAsync(CreateFacturaDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Nombre)) throw new ArgumentException("Nombre no puede estar vacío", nameof(dto.Nombre));
            if (dto.Precio < 0) throw new ArgumentException("Precio no puede ser negativo", nameof(dto.Precio));

            // Guardar y obtener id generado
            var id = await _facturaRepository.AddAsync(dto);

            // Intentar recuperar el objeto persistido
            var created = await _facturaRepository.GetByIdAsync(id);

            // Si el mapeo no rellenó el Id (inconsistencia en nombres de campos), asegurarlo aquí
            if (created == null)
            {
                // devolver DTO mínimo con el id para evitar errores de generación de rutas
                return new FacturaDto
                {
                    IdFactura = id
                };
            }

            if (string.IsNullOrWhiteSpace(created.IdFactura))
            {
                created.IdFactura = id;
            }

            return created;
        }

        /// <summary>
        /// Obtiene una factura por id.
        /// </summary>
        public async Task<FacturaDto?> ObtenerFacturaAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            return await _facturaRepository.GetByIdAsync(id);
        }

        /// <summary>
        /// Lista todas las facturas.
        /// </summary>
        public async Task<List<FacturaDto>> ListarTodasAsync()
        {
            var all = await _facturaRepository.GetAllAsync();
            return all?.ToList() ?? new List<FacturaDto>();
        }

        /// <summary>
        /// Actualiza una factura parcialmente (delegando la lógica de merge al repositorio/mapper).
        /// Devuelve la factura actualizada.
        /// </summary>
        public async Task<FacturaDto?> ActualizarFacturaAsync(string id, UpdateFacturaDto dto)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            await _facturaRepository.UpdateAsync(id, dto);
            return await _facturaRepository.GetByIdAsync(id);
        }

        /// <summary>
        /// Elimina una factura.
        /// </summary>
        public async Task EliminarFacturaAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            await _facturaRepository.DeleteAsync(id);
        }
    }
}
