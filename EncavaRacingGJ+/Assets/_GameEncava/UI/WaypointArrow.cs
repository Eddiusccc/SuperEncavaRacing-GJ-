using Framework.Core;
using TMPro;
using UnityEngine;

public class WaypointArrow : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] Transform busTransform;
    [SerializeField] Transform arrowVisual;
    [SerializeField] TMP_Text labelDistance;
    [SerializeField] TMP_Text labelStopName;

    [Header("Ajustes")]
    [SerializeField] float heightOffset = 3.5f;
    [SerializeField] Color pickupColor = Color.green;
    [SerializeField] Color dropoffColor = Color.yellow;
    [SerializeField] MeshRenderer arrowRenderer;

    Vector3 targetPosition;
    bool hasTarget;
    bool isDropOff;

    void Awake()
    {
        if (busTransform == null)
        {
            var motor = FindFirstObjectByType<BusMotor>();
            if (motor != null) busTransform = motor.transform;
        }
    }

    void OnEnable()  => EventBus.Subscribe<TargetDestinationChanged>(OnTargetChanged);
    void OnDisable() => EventBus.Unsubscribe<TargetDestinationChanged>(OnTargetChanged);

    void OnTargetChanged(TargetDestinationChanged e)
    {
        hasTarget = e.HasTarget;
        targetPosition = e.Position;
        isDropOff = e.IsDropOff;

        if (arrowVisual != null) arrowVisual.gameObject.SetActive(hasTarget);
        if (labelStopName != null)
        {
            labelStopName.text = hasTarget
                ? (isDropOff ? $"DESTINO: {e.StopName}" : $"PASAJEROS EN: {e.StopName}")
                : "";
        }

        if (arrowRenderer != null)
        {
            arrowRenderer.material.color = isDropOff ? dropoffColor : pickupColor;
        }
    }

    void LateUpdate()
    {
        if (busTransform == null || !hasTarget)
        {
            if (arrowVisual != null) arrowVisual.gameObject.SetActive(false);
            return;
        }

        // Posicionar sobre el techo del autobús
        transform.position = busTransform.position + Vector3.up * heightOffset;

        // Apuntar horizontalmente hacia la parada objetivo en la ciudad
        Vector3 dir = targetPosition - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(dir);
        }

        // Mostrar distancia en metros
        if (labelDistance != null)
        {
            float dist = Vector3.Distance(busTransform.position, targetPosition);
            labelDistance.text = $"{dist:F0} m";
        }
    }
}
