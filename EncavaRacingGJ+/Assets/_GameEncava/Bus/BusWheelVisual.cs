using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Ruedas visuales del autobús para el sistema cinemático (<see cref="BusMotor"/>).
/// Gira los meshes de las ruedas según la velocidad real y orienta las delanteras
/// según el ángulo de dirección. No usa física: las ruedas no deben tener colliders.
/// </summary>
public class BusWheelVisual : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Motor del autobús del que se leen la velocidad y el ángulo de dirección.")]
    [SerializeField] BusMotor bus;

    [Tooltip("Todas las ruedas visuales del modelo (delanteras y traseras).")]
    [SerializeField] Transform[] wheels;

    [Tooltip("Ruedas que giran con la dirección (normalmente las delanteras).")]
    [SerializeField] Transform[] steerWheels;

    [Header("Ajustes")]
    [Tooltip("Radio real de la rueda en metros. Se usa para calcular la velocidad de giro.")]
    [SerializeField] float wheelRadius = 0.45f;

    [Tooltip("Suavizado del ángulo de dirección visual.")]
    [SerializeField] float steerSmooth = 12f;

    readonly Dictionary<Transform, Quaternion> baseRotations = new();
    readonly HashSet<Transform> steerSet = new();
    float spinAngle;
    float currentSteer;

    void Awake()
    {
        // Guarda la rotación inicial de cada rueda para no perder su orientación original.
        if (wheels != null)
        {
            foreach (var w in wheels)
                if (w != null) baseRotations[w] = w.localRotation;
        }

        if (steerWheels != null)
        {
            foreach (var w in steerWheels)
            {
                if (w == null) continue;
                steerSet.Add(w);
                if (!baseRotations.ContainsKey(w)) baseRotations[w] = w.localRotation;
            }
        }
    }

    void LateUpdate()
    {
        if (bus == null) return;

        // Dirección visual suavizada.
        float targetSteer = bus.SteerAngle;
        currentSteer = Mathf.Lerp(currentSteer, targetSteer, 1f - Mathf.Exp(-steerSmooth * Time.deltaTime));

        // Giro de rodadura según la distancia recorrida.
        float delta = bus.ForwardSpeed * Time.deltaTime;
        spinAngle = Mathf.Repeat(spinAngle + (delta / Mathf.Max(wheelRadius, 0.01f)) * Mathf.Rad2Deg, 360f);

        // Ruedas sin dirección.
        if (wheels != null)
        {
            foreach (var w in wheels)
            {
                if (w == null || steerSet.Contains(w)) continue;
                w.localRotation = GetBase(w) * Quaternion.Euler(spinAngle, 0f, 0f);
            }
        }

        // Ruedas con dirección.
        if (steerWheels != null)
        {
            foreach (var w in steerWheels)
            {
                if (w == null) continue;
                w.localRotation = GetBase(w) * Quaternion.Euler(spinAngle, currentSteer, 0f);
            }
        }
    }

    Quaternion GetBase(Transform t) => baseRotations.TryGetValue(t, out var q) ? q : t.localRotation;
}
