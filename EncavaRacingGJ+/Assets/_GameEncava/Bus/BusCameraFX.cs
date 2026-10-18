using Framework.Core;
using Unity.Cinemachine;
using UnityEngine;

/// <summary>
/// Gestiona efectos dinámicos de cámara vinculados a la velocidad del autobús.
/// Ajusta progresivamente el campo de visión (FOV) de la CinemachineCamera según la velocidad actual.
/// </summary>
public class BusCameraFX : MonoBehaviour
{
    [Tooltip("Cámara Cinemachine a controlar. Si no se asigna, intentará obtenerla del mismo GameObject.")]
    [SerializeField] CinemachineCamera cam;

    [Tooltip("Cantidad de FOV adicional que se sumará al alcanzar la velocidad máxima.")]
    [SerializeField] float extraFov = 14f;

    float baseFov, target;

    void Awake()
    {
        if (cam == null) cam = GetComponent<CinemachineCamera>();
        if (cam != null)
        {
            baseFov = cam.Lens.FieldOfView;
            target = baseFov;
        }
    }

    void OnEnable()  => EventBus.Subscribe<BusSpeedChanged>(OnSpeed);
    void OnDisable() => EventBus.Unsubscribe<BusSpeedChanged>(OnSpeed);

    /// <summary>
    /// Manejador de evento que calcula el FOV objetivo proporcional a la relación de velocidad actual vs máxima.
    /// </summary>
    /// <param name="e">Datos del evento de velocidad del autobús.</param>
    void OnSpeed(BusSpeedChanged e)
    {
        if (cam == null) return;
        float ratio = e.Max > 0.01f ? Mathf.Clamp01(e.Speed / e.Max) : 0f;
        target = baseFov + extraFov * ratio;
    }

    /// <summary>
    /// Suaviza la transición del campo de visión (FOV) hacia el valor objetivo de forma independiente del timescale.
    /// </summary>
    void Update()
    {
        if (cam == null) return;
        var lens = cam.Lens;
        lens.FieldOfView = Mathf.Lerp(lens.FieldOfView, target, 1f - Mathf.Exp(-6f * Time.unscaledDeltaTime));
        cam.Lens = lens;
    }
}
