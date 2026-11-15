using Framework.Core;
using UnityEngine;

public class RunTimer : MonoBehaviour
{
    [SerializeField] GameFlow flow;
    [SerializeField] float startTime = 90f;
    [SerializeField] float crashPenalty = 2f;
    [Tooltip("Tiempo extra que se otorga de inmediato al aplicar un perk de bonus de tiempo.")]
    [SerializeField] float bonusTimePerPerk = 4f;

    float remaining;
    bool running;

    void Awake()
    {
        if (flow == null) flow = FindFirstObjectByType<GameFlow>();
    }

    void OnEnable()
    {
        EventBus.Subscribe<GameStateChanged>(OnState);
        EventBus.Subscribe<FareDelivered>(OnDelivered);
        EventBus.Subscribe<BusCrashed>(OnCrashed);
        EventBus.Subscribe<PerkApplied>(OnPerkApplied);
    }

    void OnDisable()
    {
        EventBus.Unsubscribe<GameStateChanged>(OnState);
        EventBus.Unsubscribe<FareDelivered>(OnDelivered);
        EventBus.Unsubscribe<BusCrashed>(OnCrashed);
        EventBus.Unsubscribe<PerkApplied>(OnPerkApplied);
    }

    void OnState(GameStateChanged e)
    {
        running = e.Current == GameState.Playing;
        // Reanudar desde pausa o perks NO reinicia el reloj
        if (running && e.Previous != GameState.Paused && e.Previous != GameState.PerkSelection)
        {
            remaining = startTime;
            EventBus.Publish(new TimeChanged(remaining));
        }
    }

    void OnDelivered(FareDelivered e)
    {
        remaining += e.BonusTime;
        EventBus.Publish(new TimeChanged(Mathf.Max(remaining, 0f)));
    }

    void OnCrashed(BusCrashed _)
    {
        remaining -= crashPenalty;
        EventBus.Publish(new TimeChanged(Mathf.Max(remaining, 0f)));
    }

    /// <summary>
    /// Aplica de inmediato el tiempo extra de un perk de bonus de tiempo al cronómetro de la partida.
    /// </summary>
    void OnPerkApplied(PerkApplied e)
    {
        if (e.Perk.stat == StatType.TimeBonus)
        {
            remaining += bonusTimePerPerk * e.Perk.multiplier;
            EventBus.Publish(new TimeChanged(Mathf.Max(remaining, 0f)));
        }
    }

    void Update()
    {
        if (!running) return;

        remaining -= Time.deltaTime;
        EventBus.Publish(new TimeChanged(Mathf.Max(remaining, 0f)));

        if (remaining <= 0f)
        {
            running = false;
            if (flow != null) flow.RequestGameOver();
        }
    }
}
