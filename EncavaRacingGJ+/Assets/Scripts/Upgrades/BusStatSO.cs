using System;
using UnityEngine;

[CreateAssetMenu(fileName = "BusStat", menuName = "BusStat")]
public class BusStatSO : ScriptableObject
{
    public Stat[] stats = new Stat[Enum.GetValues(typeof(BusStatsLegacy)).Length];

    private void OnValidate()
    {
        if (stats == null || stats.Length != Enum.GetValues(typeof(BusStatsLegacy)).Length)
        {
            stats = new Stat[Enum.GetValues(typeof(BusStatsLegacy)).Length];
        }

        for (int i = 0; i < stats.Length; i++)
        {
            stats[i].stat = (BusStatsLegacy)i;
            stats[i].statName = stats[i].stat.ToString();
        }
    }

    public float GetValue(BusStatsLegacy type)
    {
        if (stats == null) return 0f;
        for (int i = 0; i < stats.Length; i++)
        {
            if (stats[i].stat == type) return stats[i].value;
        }
        return 0f;
    }

    public void SetValue(BusStatsLegacy type, float value)
    {
        if (stats == null) return;
        for (int i = 0; i < stats.Length; i++)
        {
            if (stats[i].stat == type)
            {
                stats[i].value = value;
                return;
            }
        }
    }

    /// <summary>
    /// Aplica los valores de este ScriptableObject al sistema unificado de modificadores BusStats.
    /// </summary>
    public void ApplyToBusStats(BusStats targetStats, object source = null)
    {
        if (targetStats == null || stats == null) return;
        source ??= this;

        for (int i = 0; i < stats.Length; i++)
        {
            if (stats[i].stat.TryConvertToStatType(out StatType statType))
            {
                if (stats[i].value > 0f)
                {
                    targetStats.Set(source, statType, stats[i].value);
                }
            }
        }
    }
}

[Serializable]
public struct Stat
{
    public string statName;
    public float value;
    public BusStatsLegacy stat;

    public void AddValue(float add)
    {
        value += add;
    }
}
