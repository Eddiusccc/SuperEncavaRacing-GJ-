namespace Framework.Core
{
    /// <summary>
    /// Evento que contiene Current (salud actual) y Max (salud máxima). 
    /// Publicado por Health y HealthChanged interfaces.
    /// </summary>
    public readonly struct HealthChanged
    {
        public readonly int Current;
        public readonly int Max;

        public HealthChanged(int current, int max)
        {
            Current = current;
            Max = max;
        }
    }
}