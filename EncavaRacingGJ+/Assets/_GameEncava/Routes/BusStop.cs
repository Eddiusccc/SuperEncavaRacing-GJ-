using Framework.Core;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BusStop : MonoBehaviour
{
    [SerializeField] float maxEnterSpeed = 8f;   // m/s
    [SerializeField] float dwellTime = 0.5f;     // segundos detenido
    [SerializeField] GameObject indicatorActive;

    public int Index { get; set; }

    BusMotor bus;
    float timer;
    bool served;

    void OnEnable()
    {
        served = false;
        timer = 0f;
        bus = null;
        if (indicatorActive != null) indicatorActive.SetActive(true);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out BusMotor m))
        {
            bus = m;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out BusMotor m) && m == bus)
        {
            bus = null;
            timer = 0f;
        }
    }

    void Update()
    {
        if (bus == null || served) return;

        if (bus.ForwardSpeed > maxEnterSpeed)
        {
            timer = 0f;
            return;
        }

        timer += Time.deltaTime;
        if (timer >= dwellTime)
        {
            served = true;
            if (indicatorActive != null) indicatorActive.SetActive(false);
            EventBus.Publish(new BusArrivedAtStop(Index));
        }
    }
}
