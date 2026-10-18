using Framework.Core;
using Unity.Cinemachine;
using UnityEngine;

public class BusCameraFX : MonoBehaviour
{
    [SerializeField] CinemachineCamera cam;
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

    void OnSpeed(BusSpeedChanged e)
    {
        if (cam == null) return;
        float ratio = e.Max > 0.01f ? Mathf.Clamp01(e.Speed / e.Max) : 0f;
        target = baseFov + extraFov * ratio;
    }

    void Update()
    {
        if (cam == null) return;
        var lens = cam.Lens;
        lens.FieldOfView = Mathf.Lerp(lens.FieldOfView, target, 1f - Mathf.Exp(-6f * Time.unscaledDeltaTime));
        cam.Lens = lens;
    }
}
