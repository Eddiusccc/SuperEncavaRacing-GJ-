using UnityEngine;

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

public readonly struct TargetDestinationChanged
{
    public readonly int StopIndex;
    public readonly string StopName;
    public readonly Vector3 Position;
    public readonly bool HasTarget;
    public readonly bool IsDropOff; // true = destino de bajada, false = parada de recogida

    public TargetDestinationChanged(int stopIndex, string stopName, Vector3 position, bool hasTarget, bool isDropOff)
    {
        StopIndex = stopIndex;
        StopName = stopName;
        Position = position;
        HasTarget = hasTarget;
        IsDropOff = isDropOff;
    }
}

/// <summary>
/// Evento que notifica la cantidad de paradas de la ciudad que están esperando recogida.
/// </summary>
public readonly struct PickupStopsCountChanged
{
    public readonly int WaitingStopsCount;
    public readonly int TotalWaitingPassengers;

    public PickupStopsCountChanged(int waitingStopsCount, int totalWaitingPassengers)
    {
        WaitingStopsCount = waitingStopsCount;
        TotalWaitingPassengers = totalWaitingPassengers;
    }
}

/// <summary>
/// Evento que notifica la cantidad de paradas destino pendientes de entrega según los pasajeros a bordo.
/// </summary>
public readonly struct DropoffStopsCountChanged
{
    public readonly int PendingDropoffStopsCount;
    public readonly int TotalOnboardPassengers;

    public DropoffStopsCountChanged(int pendingDropoffStopsCount, int totalOnboardPassengers)
    {
        PendingDropoffStopsCount = pendingDropoffStopsCount;
        TotalOnboardPassengers = totalOnboardPassengers;
    }
}

/// <summary>
/// Evento de conveniencia que reporta el total de llamadas en espera.
/// </summary>
public readonly struct CityCallsUpdated
{
    public readonly int WaitingCalls;
    public CityCallsUpdated(int waitingCalls) => WaitingCalls = waitingCalls;
}
