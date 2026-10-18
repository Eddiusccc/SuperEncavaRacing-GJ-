using System.Collections.Generic;
using Framework.Core;
using UnityEngine;

public class PassengerHandler : MonoBehaviour
{
    [SerializeField] private GameObject pass_prefab;
    [SerializeField] private int initialPoolSize = 30;

    public PoolFactory passengerPool;
    public List<Passenger> activePassengers = new();

    BusMotor bus;

    void Awake()
    {
        bus = FindFirstObjectByType<BusMotor>();
        if (pass_prefab != null)
        {
            passengerPool = new PoolFactory(pass_prefab, initialPoolSize, transform);
        }
    }

    void OnEnable()
    {
        EventBus.Subscribe<BusArrivedAtStop>(OnBusArrived);
    }

    void OnDisable()
    {
        EventBus.Unsubscribe<BusArrivedAtStop>(OnBusArrived);
    }

    /// <summary>
    /// Genera peatones visuales en la posición de una parada.
    /// </summary>
    public void SpawnVisualPassengersAtStop(Vector3 stopPosition, int count)
    {
        if (passengerPool == null) return;

        for (int i = 0; i < count; i++)
        {
            GameObject go = passengerPool.GetObject();
            Vector3 offset = new Vector3(Random.Range(-1.5f, 1.5f), 0f, Random.Range(-1.5f, 1.5f));
            go.transform.position = stopPosition + offset;
            go.transform.rotation = Quaternion.identity;

            if (go.TryGetComponent(out Passenger pas))
            {
                activePassengers.Add(pas);
            }
        }
    }

    void OnBusArrived(BusArrivedAtStop e)
    {
        if (bus == null) bus = FindFirstObjectByType<BusMotor>();
        Vector3 busPos = bus != null ? bus.transform.position : transform.position;

        // Animar pasajeros cercanos para que suban al autobús
        for (int i = activePassengers.Count - 1; i >= 0; i--)
        {
            Passenger pas = activePassengers[i];
            if (pas == null) continue;

            if (Vector3.Distance(pas.transform.position, busPos) < 25f)
            {
                activePassengers.RemoveAt(i);
                pas.MoveTowardsBus(busPos, 5f, () =>
                {
                    if (passengerPool != null && pas != null)
                    {
                        passengerPool.ReturnObject(pas.gameObject);
                    }
                });
            }
        }
    }

    public void ClearAll()
    {
        if (passengerPool == null) return;
        foreach (var pas in activePassengers)
        {
            if (pas != null) passengerPool.ReturnObject(pas.gameObject);
        }
        activePassengers.Clear();
    }
}

public class PoolFactory
{
    public GameObject go;
    public int poolSize;
    public Transform parent;
    public List<GameObject> pool = new();

    public PoolFactory(GameObject prefab, int size, Transform parentTransform = null)
    {
        go = prefab;
        poolSize = size;
        parent = parentTransform;
        CreatePool();
    }

    public void CreatePool()
    {
        if (go == null) return;
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Object.Instantiate(go);
            obj.name = $"{go.name}_{i}";
            obj.transform.SetParent(parent, false);
            pool.Add(obj);
            obj.SetActive(false);
        }
    }

    public GameObject GetObject()
    {
        foreach (GameObject obj in pool)
        {
            if (obj != null && !obj.activeInHierarchy)
            {
                obj.SetActive(true);
                return obj;
            }
        }

        // Si no hay objetos inactivos disponibles, expandir pool
        if (go != null)
        {
            GameObject newObj = Object.Instantiate(go);
            newObj.transform.SetParent(parent, false);
            newObj.SetActive(true);
            pool.Add(newObj);
            return newObj;
        }

        return null;
    }

    public void ReturnObject(GameObject obj)
    {
        if (obj == null) return;
        obj.SetActive(false);
        if (parent != null) obj.transform.SetParent(parent, false);
    }
}
