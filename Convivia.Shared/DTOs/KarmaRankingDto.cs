namespace Convivia.Shared.DTOs
{
    /// <summary>
    /// Representa un usuario en el ranking de karma
    /// </summary>
    public class KarmaRankingDto
    {
        /// <summary>
        /// ID del UsuarioEspacio
        /// </summary>
        public string UsuarioId { get; set; } = string.Empty;

        /// <summary>
        /// Karma total acumulado
        /// </summary>
        public int KarmaTotal { get; set; }

        /// <summary>
        /// Karma semanal
        /// </summary>
        public int KarmaSemanal { get; set; }

        /// <summary>
        /// Karma mensual
        /// </summary>
        public int KarmaMensual { get; set; }

        /// <summary>
        /// Posición en el ranking (1 = primero)
        /// </summary>
        public int Posicion { get; set; }
    }
}
