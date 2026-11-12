namespace Framework.Core
{
    /// <summary>
    /// Evento publicado cuando cambia el minuto del reloj de la run.
    /// Lo consume un widget de HUD para mostrar la hora del día
    /// (por defecto la run va de las 5:00 AM a las 7:00 PM).
    /// </summary>
    public readonly struct DayTimeChanged
    {
        /// <summary>Hora del día en formato 24 h con decimales (13.5 = 1:30 PM).</summary>
        public readonly float Hours;

        /// <summary>Hora en formato 24 h, de 0 a 23.</summary>
        public readonly int Hour24;

        /// <summary>Minutos, de 0 a 59.</summary>
        public readonly int Minute;

        public DayTimeChanged(float hours, int hour24, int minute)
        {
            Hours = hours;
            Hour24 = hour24;
            Minute = minute;
        }

        /// <summary>Hora en formato 12 h, de 1 a 12.</summary>
        public int Hour12
        {
            get
            {
                int hour = Hour24 % 12;
                return hour == 0 ? 12 : hour;
            }
        }

        /// <summary>Verdadero desde el mediodía hasta antes de la medianoche.</summary>
        public bool IsPm => Hour24 >= 12;

        /// <summary>
        /// Texto listo para el HUD. Por defecto usa 12 h ("5:30 AM");
        /// con <paramref name="use24Hour"/> usa 24 h ("05:30").
        /// </summary>
        public string ToClockString(bool use24Hour = false)
        {
            if (use24Hour)
                return $"{Hour24:D2}:{Minute:D2}";

            return $"{Hour12}:{Minute:D2} {(IsPm ? "PM" : "AM")}";
        }
    }
}
