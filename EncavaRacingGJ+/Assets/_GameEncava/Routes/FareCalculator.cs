using UnityEngine;

public static class FareCalculator
{
    public const float RatePerMeterPerPassenger = 0.1f;

    public static float Reward(Fare f, float now, BusStats stats, float reputationMultiplier = 1f)
    {
        float elapsed = Mathf.Max(now - f.PickedAt, 0.01f);
        // ratio > 1 = llegaste antes del tiempo justo
        float timeRatio = f.ParTime / elapsed;
        float timeFactor = Mathf.Clamp(timeRatio, 0.4f, 1.6f);
        float basePay = f.Distance * f.Passengers * RatePerMeterPerPassenger;
        float statMult = stats != null ? stats.Get(StatType.FareMultiplier, 1f) : 1f;

        return basePay * timeFactor * statMult * reputationMultiplier;
    }

    public static float CalculateTimeRatio(Fare f, float now)
    {
        float elapsed = Mathf.Max(now - f.PickedAt, 0.01f);
        return f.ParTime / elapsed;
    }
}
