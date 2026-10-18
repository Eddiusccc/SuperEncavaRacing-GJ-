using UnityEngine;

[CreateAssetMenu(fileName = "NewUpgrade", menuName = "GameEncava/Upgrade")]
public class UpgradeData : ScriptableObject
{
    public string id;                      // único y fijo (se guarda)
    public string displayName;
    [TextArea] public string description;
    public StatType stat;
    public float bonusPerLevel = 0.08f;    // +8% por nivel
    [Min(1)] public int maxLevel = 6;
    public int baseCost = 100;
    public float costGrowth = 1.6f;

    public int CostFor(int level) => Mathf.RoundToInt(baseCost * Mathf.Pow(costGrowth, level));
    public float MultiplierFor(int level) => 1f + bonusPerLevel * level;
}

public readonly struct UpgradeBought
{
    public readonly UpgradeData Upgrade;
    public readonly int Level;
    public UpgradeBought(UpgradeData u, int level)
    {
        Upgrade = u;
        Level = level;
    }
}
