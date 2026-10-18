using System.Collections.Generic;
using Framework.Core;
using UnityEngine;

public class FareBoard : MonoBehaviour
{
    [SerializeField] BusMotor bus;
    [SerializeField] ReputationService reputation;
    [SerializeField] int baseCapacity = 6;
    [SerializeField] float baseBonusTime = 4f;

    readonly List<Fare> waiting = new();
    readonly List<Fare> onboard = new();

    public IReadOnlyList<Fare> WaitingFares => waiting;
    public IReadOnlyList<Fare> OnboardFares => onboard;

    public int Capacity => bus != null && bus.Stats != null
        ? Mathf.RoundToInt(bus.Stats.Get(StatType.Capacity, baseCapacity))
        : baseCapacity;

    public int OnboardCount => Count(onboard);

    void Awake()
    {
        if (bus == null) bus = FindFirstObjectByType<BusMotor>();
        if (reputation == null) reputation = FindFirstObjectByType<ReputationService>();
    }

    void OnEnable()  => EventBus.Subscribe<BusArrivedAtStop>(OnArrived);
    void OnDisable() => EventBus.Unsubscribe<BusArrivedAtStop>(OnArrived);

    public void AddWaiting(Fare f)
    {
        if (f != null) waiting.Add(f);
    }

    public void ClearAll()
    {
        waiting.Clear();
        onboard.Clear();
        Publish();
    }

    void OnArrived(BusArrivedAtStop e)
    {
        // 1) BAJADA: Pasajeros que llegaron a su destino en esta parada
        for (int i = onboard.Count - 1; i >= 0; i--)
        {
            Fare f = onboard[i];
            if (f.Destination != e.StopIndex) continue;

            // En este momento exacto se contabiliza el pago, la reputación y el tiempo
            float repMult = reputation != null ? reputation.GetReputationMultiplier(f.Origin) : 1f;
            float reward = FareCalculator.Reward(f, Time.time, bus != null ? bus.Stats : null, repMult);
            float timeBonusMod = bus != null && bus.Stats != null ? bus.Stats.Get(StatType.TimeBonus, 1f) : 1f;
            float bonus = baseBonusTime * timeBonusMod;
            float ratio = FareCalculator.CalculateTimeRatio(f, Time.time);

            onboard.RemoveAt(i);
            EventBus.Publish(new FareDelivered(f.Origin, f.Destination, f.Passengers, reward, bonus, ratio));
        }

        // 2) SUBIDA: Pasajeros que están esperando en esta parada de entrada
        int free = Capacity - Count(onboard);
        for (int i = 0; i < waiting.Count; i++)
        {
            Fare f = waiting[i];
            if (f.Origin != e.StopIndex || f.Passengers > free) continue;

            f.PickedAt = Time.time;
            onboard.Add(f);
            waiting.RemoveAt(i--);
            free -= f.Passengers;
        }

        Publish();
    }

    static int Count(List<Fare> list)
    {
        int n = 0;
        foreach (var f in list) n += f.Passengers;
        return n;
    }

    void Publish() => EventBus.Publish(new FaresChanged(Count(onboard), Capacity));
}
