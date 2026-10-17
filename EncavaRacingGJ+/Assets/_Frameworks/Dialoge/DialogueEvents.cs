namespace Framework.Dialogue
{
    // Alguien (un NPC) pide iniciar un dialogo
    public readonly struct DialogueRequested
    {
        public readonly DialogueData Data;
        public DialogueRequested(DialogueData data) => Data = data;
    }

    public readonly struct DialogueStarted { }
    public readonly struct DialogueLineShown
    {
        public readonly string Speaker;
        public readonly string Text;
        public readonly string[] Choices; // vacio si no hay opciones

        public DialogueLineShown(string speaker, string text, string[] choices)
        {
            Speaker = speaker;
            Text = text;
            Choices = choices;
        }
    }

    public readonly struct DialogueEnded { }

    // La UI avisa al runner
    public readonly struct DialogueAdvanceRequested { }
    public readonly struct DialogueChoiceSelected
    {
        public readonly int Index;
        public DialogueChoiceSelected(int index) => Index = index;
    }
}