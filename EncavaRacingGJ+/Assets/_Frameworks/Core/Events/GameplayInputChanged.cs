namespace Framework.Core
{
    /// <summary>
    /// Se publica cuando el input de gameplay se activa o desactiva
    /// </summary>
    public readonly struct GameplayInputChanged
    {
        public readonly bool Enable;
        public GameplayInputChanged(bool enable) => Enable = enable;
    }
}