using AuthApiDemo.DTOs;
using AuthApiDemo.Models;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace AuthApiDemo.Mappers
{
    public static class SalaMapper
    {
        public static SalaDto ToDto(SalaPersist persist)
        {
            if (persist == null) throw new ArgumentNullException(nameof(persist));
            return new SalaDto
            {
                IdSala = persist.IdSala,
                Nombre = persist.Nombre,
                IdEspacio = persist.IdEspacio,
                Descripcion = persist.Descripcion,
                Reservas = persist.Reservas ?? new List<Reserva>()
            };
        }

        // Convierte persist -> modelo de dominio de forma segura (no crea DocumentReference aquí)
        public static Sala ToModel(SalaPersist persist, Espacio? espacio = null)
        {
            if (persist == null) throw new ArgumentNullException(nameof(persist));

            // Usamos el constructor por defecto para no depender de overloads que requieran DocumentReference
            var sala = new Sala();

            // Asignaciones básicas
            sala.Nombre = persist.Nombre;
            sala.Descripcion = persist.Descripcion;
            sala.Id_Sala = string.IsNullOrWhiteSpace(persist.IdSala) ? Guid.NewGuid().ToString() : persist.IdSala;
            sala.Espacio = espacio;

            // Intentar asignar Id_Espacio solo si el tipo de la propiedad es compatible con el valor persistente.
            var prop = typeof(Sala).GetProperty("Id_Espacio", BindingFlags.Public | BindingFlags.Instance);
            if (prop != null && persist.IdEspacio != null)
            {
                // Si la propiedad en Sala es string y el persist contiene string -> asignar
                if (prop.PropertyType == typeof(string) && persist.IdEspacio is string s)
                {
                    prop.SetValue(sala, s);
                }
                // Si la propiedad en Sala es DocumentReference y el persist guarda ya un DocumentReference -> asignar
                else if (prop.PropertyType.FullName == "Google.Cloud.Firestore.DocumentReference" && prop.PropertyType.IsInstanceOfType(persist.IdEspacio))
                {
                    prop.SetValue(sala, persist.IdEspacio);
                }
                // En otros casos no hacemos nada: la conversión string->DocumentReference debe realizarse en el servicio con FirestoreDb.
            }

            // Copiar reservas y mantener la referencia a la sala en cada reserva
            if (persist.Reservas != null && persist.Reservas.Count > 0)
            {
                sala.reservas = new List<Reserva>(persist.Reservas);
                foreach (var r in sala.reservas)
                    r.sala = sala;
            }

            return sala;
        }

        public static SalaPersist ToPersist(CreateSalaDto dto, string idSala)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(idSala)) throw new ArgumentNullException(nameof(idSala));

            return new SalaPersist
            {
                IdSala = idSala,
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                IdEspacio = dto.IdEspacio,
                Reservas = dto.Reservas ?? new List<Reserva>()
            };
        }
    }
}