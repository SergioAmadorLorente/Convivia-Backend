using System;

namespace Convivia.Shared.DTOs
{
    /// <summary>
    /// Estadísticas de karma de un UsuarioEspacio
    /// </summary>
    public class KarmaEstadisticasDto
    {
        /// <summary>
        /// Karma total acumulado (nunca se resetea)
        /// </summary>
        public int KarmaTotal { get; set; }

        /// <summary>
        /// Karma conseguido durante la semana actual
        /// </summary>
        public int KarmaSemanal { get; set; }

        /// <summary>
        /// Karma conseguido durante el mes actual
        /// </summary>
        public int KarmaMensual { get; set; }
    }
}
