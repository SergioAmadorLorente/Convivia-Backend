using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Convivia.Domain.Models
{
    [FirestoreData]
    public class Sala
    {
        [FirestoreProperty]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [FirestoreProperty]
        public string Nombre { get; set; } = string.Empty;

        [FirestoreProperty]
        public string? Descripcion { get; set; } // Puede ser null

        // Usamos string para almacenar sólo el id del espacio (coherente con Espacio.Id_Espacio)
        [FirestoreProperty]
        public string Id_Espacio { get; set; } = string.Empty;

        public List<Reserva> reservas = new List<Reserva>();

        [JsonIgnore]
        public Espacio? Espacio { get; set; } // Opcional: objeto completo

        // Constructor por defecto (necesario para deserialización)
        public Sala() { }

        // Constructor práctico que acepta id de espacio como string
        public Sala(string nombre, string idEspacio, string? descripcion = null)
        {
            Nombre = nombre ?? string.Empty;
            Id_Espacio = idEspacio ?? string.Empty;
            Descripcion = descripcion;
        }

        public void listarReservas()
        {
            foreach (var reserva in reservas)
            {
                Console.WriteLine($"Reserva ID: {reserva.Id_Reserva}, Fecha Inicial: {reserva.FechaInicial}, Fecha Final: {reserva.FechaFinal}, Usuario: {reserva.usuario?.Nombre}");
            }
        }

        public bool esDisponible(DateTime fechaInicio, DateTime fechaFin)
        {
            foreach (var reserva in reservas)
            {
                if (fechaInicio < reserva.FechaFinal && fechaFin > reserva.FechaInicial)
                {
                    Console.WriteLine("No disponible");
                    return false;
                }
            }
            return true;
        }

        public bool crearReserva(DateTime fechaInicio, DateTime fechaFin, Usuario usuario)
        {
            if (esDisponible(fechaInicio, fechaFin))
            {
                var nuevaReserva = new Reserva
                {
                    Id_Reserva = Guid.NewGuid().ToString(),
                    FechaInicial = fechaInicio,
                    FechaFinal = fechaFin,
                    sala = this,
                    usuario = usuario
                };
                reservas.Add(nuevaReserva);
                return true;
            }
            Console.WriteLine("No se pudo crear la reserva, el espacio no está disponible en las fechas solicitadas.");
            return false;
        }

        public bool eliminarReserva(string idReserva)
        {
            var reserva = reservas.FirstOrDefault(r => r.Id_Reserva == idReserva);
            if (reserva != null)
            {
                reservas.Remove(reserva);
                return true;
            }
            Console.WriteLine("Reserva no encontrada.");
            return false;
        }

        public bool eliminarReserva(Reserva reserva)
        {
            if (reservas.Contains(reserva))
            {
                reservas.Remove(reserva);
                return true;
            }
            Console.WriteLine("Reserva no encontrada.");
            return false;
        }

        public Reserva? buscarReserva(string idReserva)
        {
            return reservas.FirstOrDefault(r => r.Id_Reserva == idReserva);
        }
    }
}