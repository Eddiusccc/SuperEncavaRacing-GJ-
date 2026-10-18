using System;
using System.Collections.Generic;
using Framework.Core;
using Framework.Save;
using UnityEngine;

public class UpgradeService : MonoBehaviour, ISaveable
{
    [Serializable]
    class SaveData
    {
        public string[] ids;
        public int[] levels;
    }

    [SerializeField] List<UpgradeData> catalog = new();
    [SerializeField] BusMotor bus;
    [SerializeField] Wallet wallet;

    readonly Dictionary<string, int> levels = new();

    public IReadOnlyList<UpgradeData> Catalog => catalog;
    public string SaveId => "game.upgrades";

    void Awake()
    {
        if (bus == null) bus = FindFirstObjectByType<BusMotor>();
        if (wallet == null) wallet = FindFirstObjectByType<Wallet>();
    }

    void OnEnable()  => SaveRegistry.Register(this);
    void OnDisable() => SaveRegistry.Unregister(this);
    void Start()     => ApplyAll();

    public int Level(UpgradeData u)
    {
        if (u == null || string.IsNullOrEmpty(u.id)) return 0;
        return levels.TryGetValue(u.id, out int l) ? l : 0;
    }

    public bool TryBuy(UpgradeData u)
    {
        if (u == null || wallet == null || bus == null) return false;
        int lvl = Level(u);
        if (lvl >= u.maxLevel) return false;

        int cost = u.CostFor(lvl);
        if (!wallet.TrySpend(cost)) return false;

        int nextLevel = lvl + 1;
        levels[u.id] = nextLevel;
        bus.Stats.Set(u, u.stat, u.MultiplierFor(nextLevel));
        EventBus.Publish(new UpgradeBought(u, nextLevel));
        return true;
    }

    public void ApplyAll()
    {
        if (bus == null) return;
        foreach (var u in catalog)
        {
            if (u == null) continue;
            int lvl = Level(u);
            bus.Stats.Set(u, u.stat, u.MultiplierFor(lvl));
        }
    }

    public string Capture()
    {
        var data = new SaveData
        {
            ids = new string[levels.Count],
            levels = new int[levels.Count]
        };
        int i = 0;
        foreach (var kv in levels)
        {
            data.ids[i] = kv.Key;
            data.levels[i] = kv.Value;
            i++;
        }
        return JsonUtility.ToJson(data);
    }

    public void Restore(string json)
    {
        if (string.IsNullOrEmpty(json)) return;
        var data = JsonUtility.FromJson<SaveData>(json);
        levels.Clear();
        if (data?.ids != null)
        {
            for (int i = 0; i < data.ids.Length; i++)
            {
                levels[data.ids[i]] = data.levels[i];
            }
        }
        ApplyAll();
    }
}
