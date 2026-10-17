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
public enum GameState
{
    Gameplay, UpgradeTree, Pause
}
public enum BusStats
{
    TopSpeed, Acceleration, BrakeForce, TractionForce, SuspensionForce,
    SteeringForce, NitroCooldownReduction, TotalCapacity, Tips,
    Armor,JumpCooldownReduction, ScoreMultiplier, ScoreBonus, GasolineTime
}
