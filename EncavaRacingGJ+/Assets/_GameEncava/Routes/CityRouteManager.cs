using System.Collections.Generic;
using Framework.Core;
using UnityEngine;

/// <summary>
/// Gestiona la red de rutas urbanas: llamadas de recogida en paradas,
/// asignación de paradas de entrega, visuales y publicación de métricas para la UI.
/// </summary>
public class CityRouteManager : MonoBehaviour
{
    [Header("Configuración de Ciudad")]
    [SerializeField] List<BusStop> cityStops = new();
    [SerializeField] FareBoard fareBoard;
    [SerializeField] int initialActiveCalls = 2; // Cantidad inicial de paradas llamando
    [SerializeField] int maxActiveCalls = 6;     // Límite máximo incremental de llamadas simultáneas
    [SerializeField] float parSpeed = 16f;     // m/s velocidad media esperada en ciudad

    [Header("Pasajeros por Viaje")]
    [SerializeField] int minPassengers = 1;
    [SerializeField] int maxPassengers = 4;

    [Header("RNG e Incremental")]
    [Tooltip("Tiempo mínimo y máximo (en segundos) de espera aleatoria antes de generar una nueva llamada.")]
    [SerializeField] Vector2 spawnIntervalRange = new Vector2(3f, 8f);
    [Tooltip("Cada cuántos segundos de juego aumenta la dificultad/frecuencia de llamadas.")]
    [SerializeField] float difficultyInterval = 60f;

    readonly Dictionary<int, BusStop> stopsByIndex = new();
    float spawnTimer;
    float nextSpawnTime;
    float totalPlayTime;
    int currentMaxCalls;

    void Awake()
    {
        if (fareBoard == null) fareBoard = FindFirstObjectByType<FareBoard>();

        // Si la lista está vacía, buscar todas las paradas en la escena automáticamente
        if (cityStops.Count == 0)
        {
            cityStops.AddRange(FindObjectsByType<BusStop>(FindObjectsSortMode.None));
        }

        // Limpiar nulos y referencias duplicadas
        var uniqueStops = new HashSet<BusStop>();
        for (int i = cityStops.Count - 1; i >= 0; i--)
        {
            if (cityStops[i] == null || !uniqueStops.Add(cityStops[i]))
            {
                cityStops.RemoveAt(i);
            }
        }

        // Asignar y registrar índices únicos garantizados
        stopsByIndex.Clear();
        for (int i = 0; i < cityStops.Count; i++)
        {
            cityStops[i].Index = i + 1;
            stopsByIndex[cityStops[i].Index] = cityStops[i];
        }

        if (cityStops.Count < 2)
        {
            Debug.LogWarning("CityRouteManager: Se necesitan al menos 2 paradas en la ciudad para que el origen y destino sean distintos.");
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

        totalPlayTime = 0f;
        currentMaxCalls = initialActiveCalls;
        ScheduleNextSpawn();

        // Resetear visuales de todas las paradas
        foreach (var stop in cityStops)
        {
            if (stop != null) stop.SetState(hasPickup: false, hasDropoff: false);
        }

        if (fareBoard != null) 
        {
            fareBoard.ClearAll();
        }

        // Generar llamadas iniciales en la ciudad
        for (int i = 0; i < initialActiveCalls; i++)
        {
            GenerateNewCall();
        }

        RefreshStopVisuals();
        UpdateWaypointObjective();
        PublishCounts();
    }

    void Update()
    {
        totalPlayTime += Time.deltaTime;

        // Incrementar dificultad progresivamente cada X segundos
        int targetCalls = initialActiveCalls + Mathf.FloorToInt(totalPlayTime / difficultyInterval);
        currentMaxCalls = Mathf.Min(targetCalls, maxActiveCalls);

        // Control de generación aleatoria por tiempo
        if (fareBoard != null && fareBoard.WaitingFares.Count < currentMaxCalls)
        {
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= nextSpawnTime)
            {
                GenerateNewCall();
                ScheduleNextSpawn();
            }
        }
    }

    void ScheduleNextSpawn()
    {
        spawnTimer = 0f;
        nextSpawnTime = Random.Range(spawnIntervalRange.x, spawnIntervalRange.y);
    }

    /// <summary>
    /// Genera una solicitud de viaje garantizando que la parada de destino sea distinta a la de recogida.
    /// </summary>
    public void GenerateNewCall()
    {
        if (cityStops.Count < 2 || fareBoard == null) return;

        // 1) Elegir parada origen al azar
        int originIdx = Random.Range(0, cityStops.Count);
        BusStop originStop = cityStops[originIdx];

        // 2) Elegir parada destino ESTRICTAMENTE DIFERENTE
        int destIdx = Random.Range(0, cityStops.Count);
        int attempts = 0;
        while ((destIdx == originIdx || cityStops[destIdx].Index == originStop.Index || cityStops[destIdx] == originStop) && attempts < 50)
        {
            destIdx = Random.Range(0, cityStops.Count);
            attempts++;
        }

        // Búsqueda lineal de seguridad si el azar coincidió
        if (cityStops[destIdx].Index == originStop.Index)
        {
            for (int i = 0; i < cityStops.Count; i++)
            {
                if (cityStops[i].Index != originStop.Index)
                {
                    destIdx = i;
                    break;
                }
            }
        }

        BusStop destStop = cityStops[destIdx];

        // Validación definitiva: jamás pueden ser iguales
        if (destStop == null || destStop.Index == originStop.Index)
        {
            Debug.LogError("CityRouteManager: No se pudo encontrar una parada de destino diferente a la de origen.");
            return;
        }

        float dist = Vector3.Distance(originStop.transform.position, destStop.transform.position);
        // Factor de trazado vial urbano (las curvas y manzanas suelen ser ~1.3x la distancia en línea recta)
        dist *= 1.3f;
        float parTime = dist / Mathf.Max(parSpeed, 1f);

        int passengers = Random.Range(minPassengers, maxPassengers + 1);

        fareBoard.AddWaiting(new Fare
        {
            Origin = originStop.Index,
            Destination = destStop.Index,
            Passengers = passengers,
            Distance = dist,
            ParTime = parTime
        });

        RefreshStopVisuals();
        PublishCounts();
    }

    void OnBusArrived(BusArrivedAtStop e)
    {
        // Al llegar el autobús: se procesa la bajada y subida en FareBoard.
        // Actualizamos los estados visuales y las métricas de la ciudad.
        RefreshStopVisuals();
        UpdateWaypointObjective();
        PublishCounts();
    }

    void OnFareDelivered(FareDelivered e)
    {
        // Al bajarse los pasajeros y completarse la entrega:
        RefreshStopVisuals();
        UpdateWaypointObjective();
        PublishCounts();
    }

    /// <summary>
    /// Actualiza el estado visual (luces/indicadores) de cada parada de la ciudad:
    /// - hasPickup = true si hay pasajeros esperando en esa parada para subir.
    /// - hasDropoff = true si en el autobús van pasajeros que tienen como destino esa parada.
    /// </summary>
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
            stop.SetState(hasPickup: isPickup, hasDropoff: isDropoff);
        }
    }

    /// <summary>
    /// Publica los eventos con los conteos exactos de paradas esperando recogida y paradas esperando entrega.
    /// </summary>
    void PublishCounts()
    {
        if (fareBoard == null) return;

        var waitingOriginStops = new HashSet<int>();
        int totalWaitingPassengers = 0;
        foreach (var f in fareBoard.WaitingFares)
        {
            waitingOriginStops.Add(f.Origin);
            totalWaitingPassengers += f.Passengers;
        }

        var pendingDestStops = new HashSet<int>();
        int totalOnboardPassengers = 0;
        foreach (var f in fareBoard.OnboardFares)
        {
            pendingDestStops.Add(f.Destination);
            totalOnboardPassengers += f.Passengers;
        }

        // Publicar eventos específicos
        EventBus.Publish(new PickupStopsCountChanged(waitingOriginStops.Count, totalWaitingPassengers));
        EventBus.Publish(new DropoffStopsCountChanged(pendingDestStops.Count, totalOnboardPassengers));
    }

    /// <summary>
    /// Actualiza el destino de navegación de la flecha brújula (WaypointArrow):
    /// 1. Prioridad: Si hay pasajeros a bordo, apunta a la parada de bajada (Drop-off).
    /// 2. Si el bus está vacío: apunta a la parada de recogida activa (Pickup).
    /// </summary>
    public void UpdateWaypointObjective()
    {
        if (fareBoard == null) return;

        // 1) Si llevamos pasajeros a bordo, el objetivo es su parada de bajada (Drop-off)
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

        // 2) Si el bus está vacío: el objetivo es la parada de recogida activa
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