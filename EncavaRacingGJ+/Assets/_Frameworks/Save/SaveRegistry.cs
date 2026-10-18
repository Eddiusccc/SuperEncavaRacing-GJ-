using System.Collections.Generic;

namespace Framework.Save
{
    public static class SaveRegistry
    {
        static readonly HashSet<ISaveable> saveables = new();

        public static IEnumerable<ISaveable> All => saveables;

        public static void Register(ISaveable saveable)
        {
            if (saveable != null) saveables.Add(saveable);
        }

        public static void Unregister(ISaveable saveable)
        {
            if (saveable != null) saveables.Remove(saveable);
        }
    }
}
