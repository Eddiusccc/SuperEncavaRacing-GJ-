namespace Framework.Core
{
    /// <summary>
    /// Promt = null o vacio significa " no hay nada interactuable cerca"
    /// </summary>
    public readonly struct InteractPromptChanged
    {
        public readonly string Prompt;
        public InteractPromptChanged(string prompt) => Prompt = prompt;
    }
}