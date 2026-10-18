using System.Collections.Generic;

namespace Framework.Save
{
    /// <summary>
    /// Gestiona el registro y anulación del registro de objetos que implementan la interfaz <see cref="ISaveable"/>.
    /// </summary>
    public static class SaveRegistry
    {
        static readonly HashSet<ISaveable> saveables = new();

        /// <summary>
        /// Obtiene todos los objetos <see cref="ISaveable"/> registrados.
        /// </summary>
        public static IEnumerable<ISaveable> All => saveables;

        /// <summary>
        /// Registra un objeto <see cref="ISaveable"/>.
        /// </summary>
        /// <param name="saveable">El objeto a registrar.</param>
        public static void Register(ISaveable saveable)
        {
            if (saveable != null) saveables.Add(saveable);
        }

        /// <summary>
        /// Anula el registro de un objeto <see cref="ISaveable"/>.
        /// </summary>
        /// <param name="saveable">El objeto a anular el registro.</param>
        public static void Unregister(ISaveable saveable)
        {
            if (saveable != null) saveables.Remove(saveable);
        }
    }
}
