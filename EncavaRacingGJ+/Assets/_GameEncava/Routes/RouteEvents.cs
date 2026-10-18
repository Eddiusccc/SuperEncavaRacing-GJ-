public readonly struct BusArrivedAtStop
{
    public readonly int StopIndex;
    public BusArrivedAtStop(int stopIndex) => StopIndex = stopIndex;
}

public readonly struct FareDelivered
{
    public readonly int OriginStop;
    public readonly int DestinationStop;
    public readonly int Passengers;
    public readonly float Reward;
    public readonly float BonusTime;
    public readonly float TimeRatio; // > 1 a tiempo / antes, < 1 con retraso

    public FareDelivered(int originStop, int destinationStop, int passengers, float reward, float bonusTime, float timeRatio)
    {
        OriginStop = originStop;
        DestinationStop = destinationStop;
        Passengers = passengers;
        Reward = reward;
        BonusTime = bonusTime;
        TimeRatio = timeRatio;
    }
}

public readonly struct FaresChanged
{
    public readonly int Onboard;
    public readonly int Capacity;
    public FaresChanged(int onboard, int capacity)
    {
        Onboard = onboard;
        Capacity = capacity;
    }
}

public readonly struct StopReputationChanged
{
    public readonly int StopIndex;
    public readonly int Level;
    public readonly float Reputation;

    public StopReputationChanged(int stopIndex, int level, float reputation)
    {
        StopIndex = stopIndex;
        Level = level;
        Reputation = reputation;
    }
}
