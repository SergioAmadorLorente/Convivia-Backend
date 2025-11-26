using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Cloud.Firestore;


namespace Convivia.Shared.DTOs
{
    [FirestoreData]
    public class EspacioDto
    {
        [FirestoreProperty("Id")]
        public string Id { get; set; } = Guid.NewGuid().ToString(); // <- ahora con setter

        [FirestoreProperty("Nombre")]
        public string Nombre { get; set; }

        [FirestoreProperty("Direccion")]
        public string? Direccion { get; set; }
    }
}
