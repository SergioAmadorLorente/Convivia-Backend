// Services/InvitacionService.cs

using System;

using System.Collections.Generic;

using System.Linq;

using System.Threading.Tasks;

using AuthApiDemo.DTOs;

using AuthApiDemo.Mappers;

using AuthApiDemo.Models;

namespace AuthApiDemo.Services

{

    public class InvitacionService

    {

        private readonly IFirebaseService _firebase;

        private const string COLLECTION = "invitaciones"; // coincide con tu JSON

        public InvitacionService(IFirebaseService firebase)

        {

            _firebase = firebase ?? throw new ArgumentNullException(nameof(firebase));

        }

        // Helper: si el valor viene como "coleccion/id", usa esa colección; si es solo "id",

        // usa fallbackCollection (en minúsculas).

        private async Task<T?> ResolveRefAsync<T>(string refOrId, string fallbackCollection) where T : class

        {

            if (string.IsNullOrWhiteSpace(refOrId)) return null;

            if (refOrId.Contains('/'))

            {

                var parts = refOrId.Split('/', 2, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 2)

                {

                    var coll = parts[0]; // tal como aparece en Firestore (p.e. "usuarioespacios")

                    var id = parts[1];

                    return await _firebase.GetAsync<T>(coll, id);

                }

            }

            // fallback (colección por defecto en minúsculas)

            return await _firebase.GetAsync<T>(fallbackCollection, refOrId);

        }

        public async Task<InvitacionDto> CrearInvitacionAsync(CreateInvitacionDto dto)

        {

            if (dto == null) throw new ArgumentNullException(nameof(dto));

            if (string.IsNullOrWhiteSpace(dto.UsuarioSolicitanteId) ||

                string.IsNullOrWhiteSpace(dto.EspacioId))

                throw new ArgumentException("UsuarioSolicitanteId y EspacioId son requeridos.");

            // Evitar duplicados (la persistencia usa campo "espacio" y "usuariosolicitante")

            var posibles = await _firebase.QueryMultipleConditionsAsync<InvitacionPersist>(COLLECTION,
                new (string field, object val)[] {
                    ("Espacio", dto.EspacioId),
                    ("UsuarioSolicitante", dto.UsuarioSolicitanteId)
                });

            var existePendiente = posibles.Any(p => string.Equals(p.Estado, "pendiente", StringComparison.OrdinalIgnoreCase));

            if (existePendiente)

                throw new InvalidOperationException("Ya existe una invitación pendiente para este solicitante y espacio.");

            // Resolver solicitante (puede venir como "usuarioespacios/ue1" o "ue1")

            var solicitante = await ResolveRefAsync<UsuarioEspacio>(dto.UsuarioSolicitanteId, "usuarioespacios")

                             ?? throw new ArgumentException("UsuarioSolicitante no encontrado.");

            Usuario? invitado = null;

            if (!string.IsNullOrWhiteSpace(dto.UsuarioInvitadoId))

            {

                invitado = await ResolveRefAsync<Usuario>(dto.UsuarioInvitadoId, "usuarios")

                          ?? throw new ArgumentException("UsuarioInvitado no encontrado.");

            }

            var espacio = await ResolveRefAsync<Espacio>(dto.EspacioId, "espacios")

                        ?? throw new ArgumentException("Espacio no encontrado.");

            // Construir modelo

            var invitacion = new Invitacion

            {

                Id = Guid.NewGuid().ToString(),

                UsuarioSolicitante = solicitante,

                UsuarioInvitado = invitado,

                Espacio = espacio,

                Mensaje = dto.Mensaje ?? string.Empty,

                Fecha = DateTime.UtcNow,

                Estado = "pendiente"

            };

            // Mapear a persistencia y guardar (los nombres de campo están anotados en InvitacionPersist)

            var persist = InvitacionMapper.ToPersist(invitacion);

            await _firebase.AddAsync(COLLECTION, persist.Id, persist);

            return InvitacionMapper.ToDto(invitacion);

        }

        public async Task<InvitacionDto?> GetInvitacionAsync(string id)

        {

            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));

            var persist = await _firebase.GetAsync<InvitacionPersist>(COLLECTION, id);
            if (persist == null) return null;

            var solicitante = await ResolveRefAsync<UsuarioEspacio>(persist.UsuarioSolicitante, "usuarioespacios");
            var invitado = await ResolveRefAsync<Usuario>(persist.UsuarioInvitado, "usuarios");
            var espacio = await ResolveRefAsync<Espacio>(persist.Espacio, "espacios");

            var model = InvitacionMapper.ToModel(persist, solicitante, invitado, espacio);
            return InvitacionMapper.ToDto(model);
        }

        public async Task<List<InvitacionDto>> GetAllInvitacionesAsync()

        {

            var persistList = await _firebase.QueryAsync<InvitacionPersist>(COLLECTION, "estado", "pendiente");

            var result = new List<InvitacionDto>(persistList.Count);

            foreach (var persist in persistList)

            {

                var solicitante = await ResolveRefAsync<UsuarioEspacio>(persist.UsuarioSolicitante, "usuarioespacios");

                var invitado = await ResolveRefAsync<Usuario>(persist.UsuarioInvitado, "usuarios");

                var espacio = await ResolveRefAsync<Espacio>(persist.Espacio, "espacios");

                var modelo = InvitacionMapper.ToModel(persist, solicitante, invitado, espacio);

                result.Add(InvitacionMapper.ToDto(modelo));

            }

            return result;

        }

        public async Task<bool> CambiarEstadoAsync(string id, StateChangeDto dto)

        {

            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));

            if (dto == null || string.IsNullOrWhiteSpace(dto.NuevoEstado)) throw new ArgumentException("NuevoEstado requerido.");

            var persist = await _firebase.GetAsync<InvitacionPersist>(COLLECTION, id);

            if (persist == null) return false;

            // Usar las propiedades correctas de InvitacionPersist
            var solicitante = await ResolveRefAsync<UsuarioEspacio>(persist.UsuarioSolicitante, "usuarioespacios");
            var invitado = await ResolveRefAsync<Usuario>(persist.UsuarioInvitado, "usuarios");
            var espacio = await ResolveRefAsync<Espacio>(persist.Espacio, "espacios");

            // Comprobar nulos para evitar advertencias CS8604
            if (solicitante == null || espacio == null)
                return false; // O lanza una excepción si lo prefieres

            var model = InvitacionMapper.ToModel(persist, solicitante, invitado, espacio);

            var nuevo = dto.NuevoEstado.Trim().ToLowerInvariant();
            switch (nuevo)
            {
                case "aceptada": model.Aceptar(); break;
                case "rechazada": model.Rechazar(); break;
                case "pendiente": model.Pendiente(); break;
                case "cancelada": model.Cancelar(); break;
                default: throw new ArgumentException("Estado no válido.");
            }

            var updatedPersist = InvitacionMapper.ToPersist(model);
            await _firebase.UpdateAsync(COLLECTION, updatedPersist.Id, updatedPersist);
            return true;

        }

        public async Task<bool> DeleteInvitacionAsync(string id)

        {

            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));

            var exist = await _firebase.GetAsync<InvitacionPersist>(COLLECTION, id);

            if (exist == null) return false;

            await _firebase.DeleteAsync(COLLECTION, id);

            return true;

        }

    }

}








