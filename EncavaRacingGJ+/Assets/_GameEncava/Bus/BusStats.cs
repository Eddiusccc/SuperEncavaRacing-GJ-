using System.Collections.Generic;

public enum StatType { MaxSpeed, Acceleration, Handling, BoostSpeed, Capacity, FareMultiplier, TimeBonus }

public class BusStats
{
    readonly Dictionary<StatType, Dictionary<object, float>> mods = new();

    public void Set(object source, StatType stat, float multiplier)
    {
        if (!mods.TryGetValue(stat, out var bySource))
            mods[stat] = bySource = new Dictionary<object, float>();
        bySource[source] = multiplier;
    }

    public void Clear(object source)
    {
        foreach (var d in mods.Values) d.Remove(source);
    }

    public float Get(StatType stat, float baseValue)
    {
        float total = baseValue;
        if (mods.TryGetValue(stat, out var bySource))
            foreach (float m in bySource.Values) total *= m;
        return total;
    }
}