using Framework.Core;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BusStop : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] int stopIndex;
    [SerializeField] string stopName = "Parada";
    [SerializeField] float maxEnterSpeed = 8f;   // m/s
    [SerializeField] float dwellTime = 0.5f;     // segundos detenido para subir/bajar

    [Header("Indicadores Visuales")]
    [SerializeField] GameObject indicatorPickup;   // Indicador de que aquí hay pasajeros esperando
    [SerializeField] GameObject indicatorDropoff;  // Indicador de que esta parada es destino de llegada

    public int Index
    {
        get => stopIndex;
        set => stopIndex = value;
    }

    public string StopName => stopName;

    BusMotor bus;
    float timer;
    bool servedInThisVisit;

    void Awake()
    {
        SetState(hasPickup: false, hasDropoff: false);
    }

    public void SetState(bool hasPickup, bool hasDropoff)
    {
        if (indicatorPickup != null) indicatorPickup.SetActive(hasPickup);
        if (indicatorDropoff != null) indicatorDropoff.SetActive(hasDropoff);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out BusMotor m))
        {
            bus = m;
            timer = 0f;
            servedInThisVisit = false;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out BusMotor m) && m == bus)
        {
            bus = null;
            timer = 0f;
            servedInThisVisit = false;
        }
    }

    void Update()
    {
        if (bus == null || servedInThisVisit) return;

        // Requiere frenar dentro de la parada
        if (bus.ForwardSpeed > maxEnterSpeed)
        {
            timer = 0f;
            return;
        }

        timer += Time.deltaTime;
        if (timer >= dwellTime)
        {
            servedInThisVisit = true;
            EventBus.Publish(new BusArrivedAtStop(stopIndex));
        }
    }
}
