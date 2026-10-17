namespace Framework.Core
{
    /// <summary>
    /// Evento que contiene Score (puntuación). Publicado por ScoreChanged interface.
    /// </summary>
    public readonly struct ScoreChanged
    {
        public readonly int Score;
        public ScoreChanged(int score) => Score = score;
    }
}