using System;
using System.Collections.Generic;
using Framework.Core;
using UnityEngine;

namespace Framework.Save
{
    [Serializable]
    public class SaveContainer
    {
        public List<string> keys = new();
        public List<string> payloads = new();
    }

    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

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

        public void ClearSave(int slot = 0)
        {
            PlayerPrefs.DeleteKey(saveKeyPrefix + slot);
            PlayerPrefs.Save();
        }
    }
}
