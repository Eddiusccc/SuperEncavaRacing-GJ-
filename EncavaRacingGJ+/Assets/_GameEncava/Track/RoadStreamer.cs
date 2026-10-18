using System.Collections.Generic;
using UnityEngine;

public class RoadStreamer : MonoBehaviour
{
    [Header("Bus Target")]
    [SerializeField] Transform bus;

    [Header("Carretera")]
    [SerializeField] GameObject roadSegmentPrefab;
    [SerializeField] int totalSegments = 6;
    [SerializeField] float segmentLength = 60f;

    [Header("Paradas")]
    [SerializeField] GameObject busStopPrefab;
    [SerializeField] FareBoard fareBoard;
    [SerializeField] float stopSpacing = 300f;
    [SerializeField] float parSpeed = 20f; // m/s base para calcular ParTime

    [Header("Tráfico")]
    [SerializeField] GameObject trafficCarPrefab;
    [SerializeField] int trafficPoolSize = 15;
    [SerializeField] float trafficSpawnInterval = 2.5f;
    [SerializeField] float trafficSpawnDistance = 150f;
    [SerializeField] float[] lanePositionsX = new float[] { -3.5f, 0f, 3.5f };

    readonly List<GameObject> activeSegments = new();
    readonly List<GameObject> trafficPool = new();
    readonly List<GameObject> activeTraffic = new();
    readonly List<BusStop> activeStops = new();

    float nextSpawnZ;
    float nextStopZ;
    int nextStopIndex;
    float trafficTimer;

    void Awake()
    {
        if (bus == null)
        {
            var motor = FindFirstObjectByType<BusMotor>();
            if (motor != null) bus = motor.transform;
        }
        if (fareBoard == null) fareBoard = FindFirstObjectByType<FareBoard>();

        InitTrafficPool();
    }

    void Start()
    {
        InitRoad();
    }

    void InitTrafficPool()
    {
        if (trafficCarPrefab == null) return;
        for (int i = 0; i < trafficPoolSize; i++)
        {
            GameObject car = Instantiate(trafficCarPrefab, transform);
            car.SetActive(false);
            trafficPool.Add(car);
        }
    }

    public void InitRoad()
    {
        // Limpiar segmentos previos si existen
        foreach (var seg in activeSegments)
        {
            if (seg != null) Destroy(seg);
        }
        activeSegments.Clear();

        foreach (var stop in activeStops)
        {
            if (stop != null) Destroy(stop.gameObject);
        }
        activeStops.Clear();

        // Limpiar tráfico activo
        foreach (var car in activeTraffic)
        {
            car.SetActive(false);
            trafficPool.Add(car);
        }
        activeTraffic.Clear();

        nextSpawnZ = 0f;
        nextStopZ = stopSpacing;
        nextStopIndex = 1;
        trafficTimer = 0f;

        if (roadSegmentPrefab == null) return;

        for (int i = 0; i < totalSegments; i++)
        {
            GameObject seg = Instantiate(roadSegmentPrefab, new Vector3(0f, 0f, nextSpawnZ), Quaternion.identity, transform);
            activeSegments.Add(seg);
            nextSpawnZ += segmentLength;
        }
    }

    void Update()
    {
        if (bus == null) return;

        float busZ = bus.position.z;

        // 1) Reciclar segmentos de carretera
        if (activeSegments.Count > 0)
        {
            GameObject first = activeSegments[0];
            if (busZ - first.transform.position.z > segmentLength * 1.5f)
            {
                activeSegments.RemoveAt(0);
                first.transform.position = new Vector3(0f, 0f, nextSpawnZ);
                activeSegments.Add(first);
                nextSpawnZ += segmentLength;
            }
        }

        // 2) Colocar paradas periódicas
        if (busZ + (totalSegments * segmentLength * 0.8f) >= nextStopZ)
        {
            PlaceBusStop(nextStopIndex, nextStopZ);
            nextStopIndex++;
            nextStopZ += stopSpacing;
        }

        // 3) Gestionar tráfico (spawning y retorno al pool)
        trafficTimer += Time.deltaTime;
        if (trafficTimer >= trafficSpawnInterval)
        {
            trafficTimer = 0f;
            TrySpawnTraffic(busZ);
        }

        RecyclePassedTraffic(busZ);
    }

    void PlaceBusStop(int index, float z)
    {
        if (busStopPrefab == null) return;

        // Colocar la parada junto a la vía (por ejemplo, carril derecho)
        Vector3 stopPos = new Vector3(lanePositionsX[lanePositionsX.Length - 1] + 1.5f, 0f, z);
        GameObject stopGo = Instantiate(busStopPrefab, stopPos, Quaternion.identity, transform);
        var stopComp = stopGo.GetComponent<BusStop>();
        if (stopComp != null)
        {
            stopComp.Index = index;
            activeStops.Add(stopComp);
        }

        // Generar pasajeros para esta parada
        if (fareBoard != null)
        {
            int groups = Random.Range(1, 4); // 1 a 3 grupos
            for (int g = 0; g < groups; g++)
            {
                int dest = index + Random.Range(1, 4);
                float dist = (dest - index) * stopSpacing;
                fareBoard.AddWaiting(new Fare
                {
                    Origin = index,
                    Destination = dest,
                    Passengers = Random.Range(1, 4),
                    Distance = dist,
                    ParTime = dist / Mathf.Max(parSpeed, 1f)
                });
            }
        }
    }

    void TrySpawnTraffic(float busZ)
    {
        if (trafficPool.Count == 0 || lanePositionsX == null || lanePositionsX.Length == 0) return;

        // Elegir 1 o 2 carriles al azar (NUNCA todos para dejar paso libre)
        int maxLanesToBlock = Mathf.Max(1, lanePositionsX.Length - 1);
        int lanesToBlock = Random.Range(1, maxLanesToBlock + 1);

        var availableLanes = new List<float>(lanePositionsX);
        for (int i = 0; i < lanesToBlock; i++)
        {
            if (availableLanes.Count == 0 || trafficPool.Count == 0) break;

            int laneIdx = Random.Range(0, availableLanes.Count);
            float laneX = availableLanes[laneIdx];
            availableLanes.RemoveAt(laneIdx);

            GameObject car = trafficPool[trafficPool.Count - 1];
            trafficPool.RemoveAt(trafficPool.Count - 1);

            float spawnZ = busZ + trafficSpawnDistance + Random.Range(-5f, 15f);
            car.transform.position = new Vector3(laneX, 0f, spawnZ);
            car.transform.rotation = Quaternion.identity;
            car.SetActive(true);
            activeTraffic.Add(car);
        }
    }

    void RecyclePassedTraffic(float busZ)
    {
        for (int i = activeTraffic.Count - 1; i >= 0; i--)
        {
            GameObject car = activeTraffic[i];
            if (car == null || busZ - car.transform.position.z > 30f)
            {
                activeTraffic.RemoveAt(i);
                if (car != null)
                {
                    car.SetActive(false);
                    trafficPool.Add(car);
                }
            }
        }
    }
}
