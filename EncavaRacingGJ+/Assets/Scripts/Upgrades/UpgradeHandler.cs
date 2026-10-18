using UnityEngine;

public class UpgradeHandler : MonoBehaviour
{
    public static UpgradeHandler instance;

    public BusStatSO busStats;

    #region UPGRADE STAT

    public Stat GetStat(BusStatsLegacy type)
    {
        if (busStats == null || busStats.stats == null) return new Stat();

        foreach (var item in busStats.stats)
        {
            if (item.stat == type)
            {
                return item;
            }
        }
        return new Stat();
    }

    public void UpgradeBusStat(BusStatsLegacy type, float value, bool isMulti = false)
    {
        if (busStats == null || busStats.stats == null) return;

        for (int i = 0; i < busStats.stats.Length; i++)
        {
            if (busStats.stats[i].stat == type)
            {
                float valueToAdd = value;
                if (isMulti)
                {
                    valueToAdd = busStats.stats[i].value * (value - 1f);
                }

                // Corrección del bug de struct: modificamos directamente el elemento del array
                busStats.stats[i].value += valueToAdd;

                // Propagar al sistema de BusStats del chasis en tiempo de ejecución
                if (type.TryConvertToStatType(out StatType statType))
                {
                    var bus = FindFirstObjectByType<BusMotor>();
                    if (bus != null && bus.Stats != null)
                    {
                        bus.Stats.Set(this, statType, busStats.stats[i].value);
                    }
                }
                break;
            }
        }
    }

    #endregion

    private void Awake()
    {
        if (instance == null) instance = this;
        else { Destroy(gameObject); }
    }
}
