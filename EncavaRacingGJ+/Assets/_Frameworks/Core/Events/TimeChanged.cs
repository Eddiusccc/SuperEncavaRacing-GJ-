namespace Framework.Core
{
    /// <summary>
    /// Evento publicado cada segundo/frame con el tiempo restante de la run.
    /// </summary>
    public readonly struct TimeChanged
    {
        public readonly float Remaining;
        public TimeChanged(float remaining) => Remaining = remaining;
    }
}
