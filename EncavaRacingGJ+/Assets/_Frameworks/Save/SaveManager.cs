using System;
using System.Collections.Generic;
using Framework.Core;
using UnityEngine;

namespace Framework.Save
{
    /// <summary>
    /// Contenedor serializable que almacena los pares de identificadores y datos serializados (payloads) de guardado.
    /// </summary>
    [Serializable]
    public class SaveContainer
    {
        /// <summary>
        /// Lista de identificadores únicos correspondientes a los objetos guardados.
        /// </summary>
        public List<string> keys = new();

        /// <summary>
        /// Lista de datos serializados (payloads) en formato JSON u otro string asociados a cada clave.
        /// </summary>
        public List<string> payloads = new();
    }

    /// <summary>
    /// Administrador del sistema de guardado y carga persistente del juego mediante PlayerPrefs.
    /// Funciona como un singleton persistente entre escenas.
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        /// <summary>
        /// Instancia única del <see cref="SaveManager"/>.
        /// </summary>
        public static SaveManager Instance { get; private set; }

        [Tooltip("Prefijo utilizado para generar la clave de guardado en PlayerPrefs.")]
        [SerializeField] string saveKeyPrefix = "SuperBusRacing_SaveSlot_";

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Captura el estado actual de todos los objetos registrados en <see cref="SaveRegistry"/> y los guarda en el slot especificado.
        /// </summary>
        /// <param name="slot">Número de ranura de guardado (por defecto 0).</param>
        public void Save(int slot = 0)
        {
            var container = new SaveContainer();
            foreach (var saveable in SaveRegistry.All)
            {
                if (saveable == null || string.IsNullOrEmpty(saveable.SaveId)) continue;
                container.keys.Add(saveable.SaveId);
                container.payloads.Add(saveable.Capture());
            }

            string json = JsonUtility.ToJson(container);
            PlayerPrefs.SetString(saveKeyPrefix + slot, json);
            PlayerPrefs.Save();

            EventBus.Publish(new GameSaved(slot));
        }

        /// <summary>
        /// Carga los datos del slot especificado y restaura el estado de todos los objetos registrados en <see cref="SaveRegistry"/>.
        /// </summary>
        /// <param name="slot">Número de ranura de guardado a cargar (por defecto 0).</param>
        /// <returns><c>true</c> si la carga fue exitosa; en caso contrario, <c>false</c>.</returns>
        public bool Load(int slot = 0)
        {
            string key = saveKeyPrefix + slot;
            if (!PlayerPrefs.HasKey(key)) return false;

            string json = PlayerPrefs.GetString(key);
            if (string.IsNullOrEmpty(json)) return false;

            var container = JsonUtility.FromJson<SaveContainer>(json);
            if (container == null) return false;

            var map = new Dictionary<string, string>();
            for (int i = 0; i < container.keys.Count; i++)
            {
                if (i < container.payloads.Count)
                    map[container.keys[i]] = container.payloads[i];
            }

            foreach (var saveable in SaveRegistry.All)
            {
                if (saveable != null && map.TryGetValue(saveable.SaveId, out var payload))
                {
                    saveable.Restore(payload);
                }
            }

            EventBus.Publish(new GameLoaded(slot));
            return true;
        }

        /// <summary>
        /// Elimina los datos de guardado asociados al slot especificado en PlayerPrefs.
        /// </summary>
        /// <param name="slot">Número de ranura de guardado a eliminar (por defecto 0).</param>
        public void ClearSave(int slot = 0)
        {
            PlayerPrefs.DeleteKey(saveKeyPrefix + slot);
            PlayerPrefs.Save();
        }
    }
}
