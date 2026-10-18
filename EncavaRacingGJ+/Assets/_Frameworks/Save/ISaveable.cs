namespace Framework.Save
{
    public interface ISaveable
    {
        string SaveId { get; }
        string Capture();
        void Restore(string json);
    }
}
