using UnityEngine;

/// <summary>
/// Sistema de detección de contacto con el suelo mediante raycasts en las ruedas del autobús.
/// Calcula el estado de contacto, la normal promedio de la superficie, el ángulo de pendiente
/// y el cabeceo del terreno entre ejes delantero y trasero.
/// </summary>
public class BusGroundCheck : MonoBehaviour
{
    [Header("Ruedas (puntos de contacto con el suelo)")]
    [Tooltip("Puntos de origen en cada rueda (delanteras y traseras; mínimo 2 requeridas).")]
    [SerializeField] Transform[] probes;

    [Header("Detección")]
    [Tooltip("Máscara de capas considerada como superficie transitable.")]
    [SerializeField] LayerMask groundMask = ~0;

    [Tooltip("Distancia por encima de la rueda desde donde se dispara el raycast hacia abajo.")]
    [SerializeField] float startHeight = 0.5f;

    [Tooltip("Distancia por debajo de la rueda que aún se considera contacto con el suelo.")]
    [SerializeField] float reach = 0.25f;

    [Tooltip("Ángulo de pendiente máximo admisible para considerar el suelo como transitable.")]
    [SerializeField] float maxSlopeAngle = 45f;

    [Tooltip("Número mínimo de ruedas que deben estar en contacto para considerarse en suelo.")]
    [SerializeField, Range(1, 4)] int minWheelsGrounded = 2;

    [Tooltip("Tiempo de gracia en segundos tras perder contacto para tolerar baches sin despegar de inmediato.")]
    [SerializeField] float groundedGrace = 0.1f;

    bool[] wheelHit;
    float graceTimer;

    /// <summary>Indica si el vehículo se considera actualmente apoyado sobre el suelo.</summary>
    public bool IsGrounded { get; private set; }

    /// <summary>Normal promedio de las superficies donde hacen contacto las ruedas.</summary>
    public Vector3 Normal { get; private set; } = Vector3.up;

    /// <summary>Ángulo de inclinación del terreno en grados con respecto a la vertical.</summary>
    public float SlopeAngle { get; private set; }

    /// <summary>Cantidad de ruedas actualmente en contacto físico con el suelo.</summary>
    public int GroundedWheels { get; private set; }

    /// <summary>Ángulo de cabeceo del terreno en grados formado entre los ejes delantero y trasero (+ = morro arriba).</summary>
    public float GroundPitch { get; private set; }

    void Awake()
    {
        if (probes == null || probes.Length == 0)
        {
            Debug.LogError("BusGroundCheck: asigna al menos 2 puntos de rueda en 'Probes'.", this);
            probes = new Transform[0];
        }
        wheelHit = new bool[probes.Length];
    }

    /// <summary>
    /// Realiza los raycasts de cada rueda y recalcula normales, pendientes y estado de contacto.
    /// Diseñado para invocarse periódicamente desde el motor en FixedUpdate.
    /// </summary>
    public void Refresh()
    {
        Vector3 normalSum = Vector3.zero, frontSum = Vector3.zero, rearSum = Vector3.zero;
        int hits = 0, frontHits = 0, rearHits = 0;
        float length = startHeight + reach;

        for (int i = 0; i < probes.Length; i++)
        {
            Vector3 origin = probes[i].position + Vector3.up * startHeight;
            wheelHit[i] = Physics.Raycast(origin, Vector3.down, out RaycastHit hit, length,
                                        groundMask, QueryTriggerInteraction.Ignore);
            if (!wheelHit[i]) continue;

            normalSum += hit.normal;
            hits++;

            // Determina si la rueda se encuentra por delante o detrás del origen local del vehículo
            if (transform.InverseTransformPoint(probes[i].position).z > 0f) { frontSum += hit.point; frontHits++; }
            else { rearSum += hit.point; rearHits++; }
        }

        GroundedWheels = hits;

        if (hits > 0)
        {
            Normal = normalSum.normalized;               // promedio de las normales de las ruedas
            SlopeAngle = Vector3.Angle(Normal, Vector3.up);
        }

        // Pendiente del terreno entre el eje delantero y el trasero
        GroundPitch = 0f;
        if (frontHits > 0 && rearHits > 0)
        {
            Vector3 d = frontSum / frontHits - rearSum / rearHits;
            float flat = new Vector2(d.x, d.z).magnitude;
            if (flat > 0.01f) GroundPitch = Mathf.Atan2(d.y, flat) * Mathf.Rad2Deg;
        }

        // Tolerancia: no pierde el suelo por un bache de un instante
        bool raw = hits >= minWheelsGrounded && SlopeAngle <= maxSlopeAngle;
        graceTimer = raw ? groundedGrace : graceTimer - Time.fixedDeltaTime;
        IsGrounded = graceTimer > 0f;

        if (!IsGrounded) { Normal = Vector3.up; SlopeAngle = 0f; }
    }

    void OnDrawGizmosSelected()
    {
        if (probes == null) return;
        float length = startHeight + reach;

        for (int i = 0; i < probes.Length; i++)
        {
            if (probes[i] == null) continue;
            bool hit = Application.isPlaying && wheelHit != null && i < wheelHit.Length && wheelHit[i];
            Gizmos.color = hit ? Color.green : Color.red;

            Vector3 origin = probes[i].position + Vector3.up * startHeight;
            Gizmos.DrawLine(origin, origin + Vector3.down * length);
            Gizmos.DrawWireSphere(probes[i].position, 0.1f);
        }
    }
}