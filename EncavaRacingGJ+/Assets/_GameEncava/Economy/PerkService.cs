using System;
using System.Collections.Generic;
using Framework.Core;
using UnityEngine;

public readonly struct PerkOffered
{
    public readonly PerkData[] Choices;
    public PerkOffered(PerkData[] choices) => Choices = choices;
}

public class PerkService : MonoBehaviour
{
    [SerializeField] List<PerkData> perkPool = new();
    [SerializeField] BusMotor bus;
    [SerializeField] int deliveriesPerPerk = 3;

    readonly List<PerkData> activePerks = new();
    int deliveriesSinceLastPerk;

    public IReadOnlyList<PerkData> ActivePerks => activePerks;

    void Awake()
    {
        if (bus == null) bus = FindFirstObjectByType<BusMotor>();
    }

    void OnEnable()  => EventBus.Subscribe<FareDelivered>(OnDelivered);
    void OnDisable() => EventBus.Unsubscribe<FareDelivered>(OnDelivered);

    void OnDelivered(FareDelivered _)
    {
        deliveriesSinceLastPerk++;
        if (deliveriesSinceLastPerk >= deliveriesPerPerk)
        {
            deliveriesSinceLastPerk = 0;
            OfferPerks();
        }
    }

    void OfferPerks()
    {
        if (perkPool.Count == 0) return;

        // Seleccionar hasta 3 perks al azar
        int count = Mathf.Min(3, perkPool.Count);
        var shuffled = new List<PerkData>(perkPool);
        for (int i = 0; i < shuffled.Count; i++)
        {
            int r = UnityEngine.Random.Range(i, shuffled.Count);
            (shuffled[i], shuffled[r]) = (shuffled[r], shuffled[i]);
        }

        var choices = shuffled.GetRange(0, count).ToArray();
        EventBus.Publish(new PerkOffered(choices));
    }

    public void SelectPerk(PerkData perk)
    {
        if (perk == null || bus == null) return;
        activePerks.Add(perk);
        bus.Stats.Set(perk, perk.stat, perk.multiplier);
    }

    public void ResetRunPerks()
    {
        deliveriesSinceLastPerk = 0;
        if (bus != null)
        {
            foreach (var p in activePerks)
            {
                bus.Stats.Clear(p);
            }
        }
        activePerks.Clear();
    }
}
