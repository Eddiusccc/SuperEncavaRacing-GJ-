using Framework.Core;
using UnityEngine;

public class RunTimer : MonoBehaviour
{
    [SerializeField] GameFlow flow;
    [SerializeField] float startTime = 90f;
    [SerializeField] float crashPenalty = 2f;

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
    }

    void OnDisable()
    {
        EventBus.Unsubscribe<GameStateChanged>(OnState);
        EventBus.Unsubscribe<FareDelivered>(OnDelivered);
        EventBus.Unsubscribe<BusCrashed>(OnCrashed);
    }

    void OnState(GameStateChanged e)
    {
        running = e.Current == GameState.Playing;
        // Reanudar desde pausa NO reinicia el reloj
        if (running && e.Previous != GameState.Paused)
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
