using UnityEngine;

public static class Data
{
    public static float MaxOfThree(float a, float b, float c)
    {
        return Mathf.Max(a, Mathf.Max(b, c));
    }

    public static float Map(float value, float inMin, float inMax, float outMin, float outMax)
    {
        return (value - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;
    }
}

/// <summary>
/// Renombrado a LegacyGameState para evitar colisión de nombres con Framework.Core.GameState.
/// El gameloop principal del juego utiliza Framework.Core.GameState (Menu, Playing, Paused, GameOver, Shop).
/// </summary>
public enum LegacyGameState
{
    Gameplay, UpgradeTree, Pause
}

public enum BusStatsLegacy
{
    TopSpeed, Acceleration, BrakeForce, TractionForce, SuspensionForce,
    SteeringForce, NitroCooldownReduction, TotalCapacity, Tips,
    Armor, JumpCooldownReduction, ScoreMultiplier, ScoreBonus, GasolineTime
}

public static class BusStatsLegacyExtensions
{
    /// <summary>
    /// Convierte y mapea un stat propuesto por el compañero al sistema unificado StatType de BusStats.
    /// </summary>
    public static bool TryConvertToStatType(this BusStatsLegacy legacy, out StatType statType)
    {
        switch (legacy)
        {
            case BusStatsLegacy.TopSpeed:
                statType = StatType.MaxSpeed;
                return true;
            case BusStatsLegacy.Acceleration:
                statType = StatType.Acceleration;
                return true;
            case BusStatsLegacy.BrakeForce:
                statType = StatType.BrakeForce;
                return true;
            case BusStatsLegacy.TractionForce:
            case BusStatsLegacy.SteeringForce:
            case BusStatsLegacy.SuspensionForce:
                statType = StatType.Handling;
                return true;
            case BusStatsLegacy.NitroCooldownReduction:
                statType = StatType.BoostCooldown;
                return true;
            case BusStatsLegacy.TotalCapacity:
                statType = StatType.Capacity;
                return true;
            case BusStatsLegacy.Tips:
            case BusStatsLegacy.ScoreMultiplier:
            case BusStatsLegacy.ScoreBonus:
                statType = StatType.FareMultiplier;
                return true;
            case BusStatsLegacy.GasolineTime:
                statType = StatType.TimeBonus;
                return true;
            case BusStatsLegacy.Armor:
                statType = StatType.Armor;
                return true;
            case BusStatsLegacy.JumpCooldownReduction:
            default:
                statType = default;
                return false;
        }
    }
}
