using System;

namespace Convivia.Domain.Entities
{
    /// <summary>
    /// Estadísticas de karma para un UsuarioEspacio específico.
    /// Se resetea automáticamente cada semana y cada mes.
    /// </summary>
    public class KarmaEstadisticas
    {
        /// <summary>
        /// ID único de las estadísticas (usa el mismo ID que UsuarioEspacio)
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        /// <summary>
        /// ID del UsuarioEspacio al que pertenecen estas estadísticas.
        /// UsuarioEspacio ya contiene la referencia al Usuario y al Espacio.
        /// </summary>
        public string UsuarioEspacioId { get; set; } = string.Empty;

        /// <summary>
        /// Karma total acumulado global (nunca se resetea)
        /// </summary>
        public int KarmaTotal { get; set; }

        /// <summary>
        /// Karma conseguido durante la semana actual (se resetea cada semana)
        /// </summary>
        public int KarmaSemanal { get; set; }

        /// <summary>
        /// Karma conseguido durante el mes actual (se resetea cada mes)
        /// </summary>
        public int KarmaMensual { get; set; }

        /// <summary>
        /// Identificador de la última semana en formato YYYYWW (ej: 202453)
        /// Usado para detectar cambios de semana
        /// </summary>
        public int? UltimaSemana { get; set; }

        /// <summary>
        /// Identificador del último mes en formato YYYYMM (ej: 202412)
        /// Usado para detectar cambios de mes
        /// </summary>
        public int? UltimoMes { get; set; }

        /// <summary>
        /// Fecha de última actualización
        /// </summary>
        public DateTime UltimaActualizacion { get; set; } = DateTime.UtcNow;

        public KarmaEstadisticas()
        {
        }

        public KarmaEstadisticas(string usuarioEspacioId)
        {
            Id = usuarioEspacioId; // Usar el mismo ID que UsuarioEspacio para facilitar búsqueda
            UsuarioEspacioId = usuarioEspacioId;
            KarmaTotal = 0;
            KarmaSemanal = 0;
            KarmaMensual = 0;
            UltimaSemana = GetCurrentWeek();
            UltimoMes = GetCurrentMonth();
            UltimaActualizacion = DateTime.UtcNow;
        }

        private static int GetCurrentWeek()
        {
            var date = DateTime.UtcNow;
            int year = System.Globalization.ISOWeek.GetYear(date);
            int weekNum = System.Globalization.ISOWeek.GetWeekOfYear(date);
            return year * 100 + weekNum;
        }

        private static int GetCurrentMonth()
        {
            var date = DateTime.UtcNow;
            return date.Year * 100 + date.Month;
        }
    }
}
