using Framework.Core;
using UnityEngine;

public class RunResetter : MonoBehaviour
{
    [SerializeField] BusMotor bus;
    [SerializeField] FareBoard fareBoard;
    [SerializeField] CityRouteManager cityRouteManager;
    [SerializeField] RoadStreamer roadStreamer;
    [SerializeField] PerkService perkService;
    [SerializeField] Vector3 startPosition = Vector3.zero;

    void Awake()
    {
        if (bus == null) bus = FindFirstObjectByType<BusMotor>();
        if (fareBoard == null) fareBoard = FindFirstObjectByType<FareBoard>();
        if (cityRouteManager == null) cityRouteManager = FindFirstObjectByType<CityRouteManager>();
        if (roadStreamer == null) roadStreamer = FindFirstObjectByType<RoadStreamer>();
        if (perkService == null) perkService = FindFirstObjectByType<PerkService>();
    }

    void OnEnable()  => EventBus.Subscribe<GameStateChanged>(OnStateChanged);
    void OnDisable() => EventBus.Unsubscribe<GameStateChanged>(OnStateChanged);

    void OnStateChanged(GameStateChanged e)
    {
        if (e.Current == GameState.Playing && e.Previous != GameState.Paused)
        {
            ResetRun();
        }
    }

    public void ResetRun()
    {
        if (bus != null)
        {
            var rb = bus.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.position = startPosition;
                //rb.rotation = Quaternion.identity;
            }
            else
            {
                bus.transform.position = startPosition;
                bus.transform.rotation = Quaternion.identity;
            }
        }

        if (fareBoard != null) fareBoard.ClearAll();
        if (cityRouteManager != null) cityRouteManager.InitCityRoutes();
        if (roadStreamer != null) roadStreamer.InitRoad();
        if (perkService != null) perkService.ResetRunPerks();
    }
}
