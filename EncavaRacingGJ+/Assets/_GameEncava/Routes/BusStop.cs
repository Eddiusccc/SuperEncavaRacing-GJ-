using Framework.Core;
using UnityEngine;

/// <summary>
/// Representa una parada de autobús a lo largo de una ruta.
/// Detecta la llegada del autobús, verifica que baje la velocidad y permanezca
/// detenido el tiempo necesario antes de notificar el evento de llegada (<see cref="BusArrivedAtStop"/>).
/// Controla también los indicadores visuales de recogida y bajada de pasajeros.
/// </summary>
[RequireComponent(typeof(Collider))]
public class BusStop : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Índice de la parada en la secuencia de la ruta.")]
    [SerializeField] int stopIndex;

    [Tooltip("Nombre descriptivo de la parada para UI o depuración.")]
    [SerializeField] string stopName = "Parada";

    [Tooltip("Velocidad máxima hacia adelante (en m/s) a la que debe ir el bus para empezar a contar el tiempo de parada.")]
    [SerializeField] float maxEnterSpeed = 8f;

    [Tooltip("Tiempo mínimo en segundos que el bus debe permanecer por debajo de la velocidad máxima para completar la parada.")]
    [SerializeField] float dwellTime = 0.5f;

    [Header("Indicadores Visuales")]
    [Tooltip("Objeto visual que se activa cuando hay pasajeros esperando ser recogidos en esta parada.")]
    [SerializeField] GameObject indicatorPickup;

    [Tooltip("Objeto visual que se activa cuando esta parada es el destino para dejar pasajeros.")]
    [SerializeField] GameObject indicatorDropoff;

    /// <summary>
    /// Obtiene o establece el índice numérico de la parada dentro de la ruta.
    /// </summary>
    public int Index
    {
        get => stopIndex;
        set => stopIndex = value;
    }

    /// <summary>
    /// Nombre descriptivo asignado a esta parada.
    /// </summary>
    public string StopName => stopName;

    BusMotor bus;
    float timer;
    bool servedInThisVisit;

    void Awake()
    {
        SetState(hasPickup: false, hasDropoff: false);
    }

    /// <summary>
    /// Actualiza la visibilidad de los indicadores de recogida y bajada de pasajeros.
    /// </summary>
    /// <param name="hasPickup">Indica si hay pasajeros esperando para subir.</param>
    /// <param name="hasDropoff">Indica si hay pasajeros a bordo con destino a esta parada.</param>
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

    /// <summary>
    /// Verifica si el autobús dentro del área de la parada mantiene una velocidad suficientemente baja
    /// durante el tiempo configurado (<see cref="dwellTime"/>) para publicar el evento de llegada.
    /// </summary>
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
