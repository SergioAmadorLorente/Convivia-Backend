using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Convivia.Infrastructure.Models
{
    [FirestoreData]
    public class FireStoreSala
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


        // Constructor por defecto (necesario para deserialización)
        public FireStoreSala() { }

        // Constructor práctico que acepta id de espacio como string
        public FireStoreSala(string nombre, string idEspacio, string? descripcion = null)
        {
            Nombre = nombre ?? string.Empty;
            Id_Espacio = idEspacio ?? string.Empty;
            Descripcion = descripcion;
        }

    }
}