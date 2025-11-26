using Google.Cloud.Firestore;

namespace Convivia.Shared.DTOs
{
    [FirestoreData]
    public class SalaDto
    {
        public SalaDto() { }

        [FirestoreProperty("Id")]
        public string Id { get; set; }
        
        [FirestoreProperty("Nombre")]
        public string Nombre { get; set; }
        
        [FirestoreProperty("Descripcion")]
        public string? Descripcion { get; set; }
        
        [FirestoreProperty("IdEspacio")]
        public string IdEspacio { get; set; }

        // Note: Reservas are not included temporarily. We do in the next Sprint.
    }
}
