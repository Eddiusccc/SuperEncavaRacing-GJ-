using System.Collections.Generic;
using Framework.Core;
using UnityEngine;

public class CityRouteManager : MonoBehaviour
{
    [Header("Configuración de Ciudad")]
    [SerializeField] List<BusStop> cityStops = new();
    [SerializeField] FareBoard fareBoard;
    [SerializeField] int activePickupCalls = 2; // Cantidad de paradas llamando a la vez en la ciudad
    [SerializeField] float parSpeed = 16f;     // m/s velocidad media esperada en ciudad

    readonly Dictionary<int, BusStop> stopsByIndex = new();

    void Awake()
    {
        if (fareBoard == null) fareBoard = FindFirstObjectByType<FareBoard>();

        // Si la lista está vacía, buscar todas las paradas en la escena automáticamente
        if (cityStops.Count == 0)
        {
            cityStops.AddRange(FindObjectsByType<BusStop>(FindObjectsSortMode.None));
        }

        // Asignar y registrar índices únicos
        for (int i = 0; i < cityStops.Count; i++)
        {
            if (cityStops[i] == null) continue;
            cityStops[i].Index = i + 1;
            stopsByIndex[cityStops[i].Index] = cityStops[i];
        }
    }

    void OnEnable()
    {
        EventBus.Subscribe<BusArrivedAtStop>(OnBusArrived);
        EventBus.Subscribe<FareDelivered>(OnFareDelivered);
        EventBus.Subscribe<GameStateChanged>(OnGameState);
    }

    void OnDisable()
    {
        EventBus.Unsubscribe<BusArrivedAtStop>(OnBusArrived);
        EventBus.Unsubscribe<FareDelivered>(OnFareDelivered);
        EventBus.Unsubscribe<GameStateChanged>(OnGameState);
    }

    void Start()
    {
        InitCityRoutes();
    }

    void OnGameState(GameStateChanged e)
    {
        if (e.Current == GameState.Playing && e.Previous != GameState.Paused)
        {
            InitCityRoutes();
        }
    }

    public void InitCityRoutes()
    {
        if (cityStops.Count < 2) return;

        // Resetear visuales de todas las paradas
        foreach (var stop in cityStops)
        {
            if (stop != null) stop.SetState(false, false);
        }

        if (fareBoard != null) fareBoard.ClearAll();

        // Generar llamadas iniciales en la ciudad
        for (int i = 0; i < activePickupCalls; i++)
        {
            GenerateNewCall();
        }

        UpdateWaypointObjective();
    }

    public void GenerateNewCall()
    {
        if (cityStops.Count < 2 || fareBoard == null) return;

        // Elegir parada origen al azar
        int originIdx = Random.Range(0, cityStops.Count);
        BusStop originStop = cityStops[originIdx];

        // Elegir parada destino diferente
        int destIdx = Random.Range(0, cityStops.Count);
        while (destIdx == originIdx)
        {
            destIdx = Random.Range(0, cityStops.Count);
        }
        BusStop destStop = cityStops[destIdx];

        float dist = Vector3.Distance(originStop.transform.position, destStop.transform.position);
        // Factor de curvas urbanas (la distancia por carretera suele ser 1.3x la línea recta)
        dist *= 1.3f;
        float parTime = dist / Mathf.Max(parSpeed, 1f);

        int passengers = Random.Range(1, 4);

        fareBoard.AddWaiting(new Fare
        {
            Origin = originStop.Index,
            Destination = destStop.Index,
            Passengers = passengers,
            Distance = dist,
            ParTime = parTime
        });

        // Activar indicador visual de recogida en la parada origen
        originStop.SetState(hasPickup: true, hasDropoff: false);
    }

    void OnBusArrived(BusArrivedAtStop e)
    {
        if (stopsByIndex.TryGetValue(e.StopIndex, out var currentStop))
        {
            // Ya no tiene llamada de recogida si los pasajeros subieron
            currentStop.SetState(hasPickup: false, hasDropoff: false);
        }

        // Si tenemos pasajeros a bordo, encender el indicador de llegada en sus paradas destino
        RefreshStopVisuals();
        UpdateWaypointObjective();
    }

    void OnFareDelivered(FareDelivered e)
    {
        // Al bajarse los pasajeros, generar una nueva llamada en otra parte de la ciudad
        GenerateNewCall();
        RefreshStopVisuals();
        UpdateWaypointObjective();
    }

    void RefreshStopVisuals()
    {
        if (fareBoard == null) return;

        var waitingOrigins = new HashSet<int>();
        foreach (var f in fareBoard.WaitingFares) waitingOrigins.Add(f.Origin);

        var onboardDests = new HashSet<int>();
        foreach (var f in fareBoard.OnboardFares) onboardDests.Add(f.Destination);

        foreach (var kv in stopsByIndex)
        {
            int idx = kv.Key;
            BusStop stop = kv.Value;
            if (stop == null) continue;

            bool isPickup = waitingOrigins.Contains(idx);
            bool isDropoff = onboardDests.Contains(idx);
            stop.SetState(isPickup, isDropoff);
        }
    }

    public void UpdateWaypointObjective()
    {
        if (fareBoard == null) return;

        // 1) Prioridad: Si llevamos pasajeros a bordo, el objetivo es su parada de bajada (Drop-off)
        if (fareBoard.OnboardFares.Count > 0)
        {
            var firstOnboard = fareBoard.OnboardFares[0];
            if (stopsByIndex.TryGetValue(firstOnboard.Destination, out var destStop))
            {
                EventBus.Publish(new TargetDestinationChanged(
                    destStop.Index,
                    destStop.StopName,
                    destStop.transform.position,
                    hasTarget: true,
                    isDropOff: true
                ));
                return;
            }
        }

        // 2) Si el bus está vacío: el objetivo es la parada de recogida activa más cercana o prioritaria
        if (fareBoard.WaitingFares.Count > 0)
        {
            var firstWait = fareBoard.WaitingFares[0];
            if (stopsByIndex.TryGetValue(firstWait.Origin, out var originStop))
            {
                EventBus.Publish(new TargetDestinationChanged(
                    originStop.Index,
                    originStop.StopName,
                    originStop.transform.position,
                    hasTarget: true,
                    isDropOff: false
                ));
                return;
            }
        }

        // Sin objetivo activo
        EventBus.Publish(new TargetDestinationChanged(0, "Sin Ruta", Vector3.zero, false, false));
    }
}
