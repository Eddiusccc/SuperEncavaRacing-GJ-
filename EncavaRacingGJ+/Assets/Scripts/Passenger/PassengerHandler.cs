using System.Collections.Generic;
using UnityEngine;

public class PassengerHandler : MonoBehaviour
{
    [SerializeField] private GameObject pass_prefab;
    public PoolFactory passengerPool;
    public List<Passenger> activePassengers = new List<Passenger>();

    float distanceCheck = 50f;

    public SphereCollider busCollider;
    public Transform currentParada;


    public float enterPassengers = 0f;
    public float exitPassengers = 0f;
    
    private void Awake()
    {
        passengerPool = new PoolFactory(pass_prefab, 50);
    }


    public void CheckDistance()
    {
        //Si el autobus esta a cierta distancia de la parada, se spawnean los pasajeros en la parada

        if (Vector3.Distance(busCollider.transform.position, currentParada.position) < distanceCheck)
        {
            int spawnQuantity = Random.Range(10, 30); //A cambiar
            for (int i = 0; i < spawnQuantity; i++)
            {
                GameObject go = passengerPool.GetObject();
                Passenger pas = go.GetComponent<Passenger>();
                activePassengers.Add(pas);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "ParadaZone")
        {
            //Los pasajeros de esa parada deben empezar a moverse al autobus
            foreach (var item in activePassengers)
            {
                //item.MoverABus();
            }
        }

        if(other.tag == "Passenger")
        {
            //Se presiona un trigger para aceptar la bienvenida a los pasajeros
            if(enterPassengers > 0f)
            {
                Passenger pas = other.gameObject.GetComponent<Passenger>();
                passengerPool.ReturnObject(other.gameObject);
                activePassengers.Remove(pas);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //Se resetea
    }
}
public class PoolFactory
{
    public GameObject go;
    public int poolSize;
    public Transform parent;
    public List<GameObject> pool = new List<GameObject>();

    public PoolFactory(GameObject prefab, int size, Transform parentTransform = null)
    {
        go = prefab;
        poolSize = size;
        parent = parentTransform;
        CreatePool();
    }

    public void CreatePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Object.Instantiate(go);
            obj.name = obj.name + " " + i;
            obj.transform.SetParent(parent, false);
            pool.Add(obj);
            obj.gameObject.SetActive(false);
        }
    }
    public GameObject GetObject()
    {
        foreach (GameObject obj in pool)
        {
            if (!obj.activeInHierarchy)
            {
                obj.SetActive(true);
                return obj;
            }
        }
        // If no inactive object is found, create a new one
        GameObject newObj = Object.Instantiate(go);
        newObj.SetActive(true);
        pool.Add(newObj);
        return newObj;
    }

    public void ReturnObject(GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.SetParent(parent, false);
    }
}

