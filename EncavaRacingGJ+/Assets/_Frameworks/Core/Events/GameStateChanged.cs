namespace Framework.Core
{
    public enum GameState
    {
        Menu,
        Playing,
        Paused,
        GameOver
    }

    /// <summary>
    /// Contiene Previous y Current estado. Publicado por HealthChanged y GameFlow.
    /// </summary>
    public readonly struct GameStateChanged
    {
        public readonly GameState Previous;
        public readonly GameState Current;

        public GameStateChanged(GameState previus, GameState current)
        {
            Previous = previus;
            Current = current;
        }
    }
}