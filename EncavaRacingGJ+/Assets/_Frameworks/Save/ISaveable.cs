namespace Framework.Save
{
    public interface ISaveable
    {
        string SaveId { get; }
        string Capture();
        void Restore(string json);
        /// <summary>
        /// Resetea el objeto a su estado por defecto (sin depender de datos guardados).
        /// Se usa al borrar una partida para que no queden valores viejos en memoria.
        /// </summary>
        void Reset();
    }
}
