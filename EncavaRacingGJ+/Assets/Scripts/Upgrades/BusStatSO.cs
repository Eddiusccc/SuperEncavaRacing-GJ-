using System;
using UnityEngine;

[CreateAssetMenu(fileName = "BusStat", menuName = "BusStat")]
public class BusStatSO : ScriptableObject
{
    public Stat[] stats = new Stat[Enum.GetValues(typeof(BusStatsLegacy)).Length];



    private void OnValidate()
    {
        for (int i = 0; i < stats.Length; i++)
        {
            stats[i].stat = (BusStatsLegacy)i;

            stats[i].statName = stats[i].stat.ToString();
        }
    }
}
[Serializable]
public struct Stat
{
    public string statName;
    public float value;
    public BusStatsLegacy stat;

    public void AddValue(float value)
    {
        this.value += value;
    }
}
