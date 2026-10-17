using UnityEngine;

public class BusGroundCheck : MonoBehaviour
{
    [Header("Ruedas (puntos de contacto con el suelo)")]
    [SerializeField] Transform[] probes;           // delanteras y traseras; al menos 2

    [Header("Detección")]
    [SerializeField] LayerMask groundMask = ~0;
    [SerializeField] float startHeight = 0.5f;     // el rayo parte este tanto por encima de la rueda
    [SerializeField] float reach = 0.25f;          // cuánto por debajo de la rueda aún cuenta como suelo
    [SerializeField] float maxSlopeAngle = 45f;
    [SerializeField, Range(1, 4)] int minWheelsGrounded = 2;
    [SerializeField] float groundedGrace = 0.1f;   // segundos de tolerancia en baches

    bool[] wheelHit;
    float graceTimer;

    public bool IsGrounded { get; private set; }
    public Vector3 Normal { get; private set; } = Vector3.up;
    public float SlopeAngle { get; private set; }
    public int GroundedWheels { get; private set; }
    public float GroundPitch { get; private set; }   // grados del terreno: + = morro hacia arriba

    void Awake()
    {
        if (probes == null || probes.Length == 0)
        {
            Debug.LogError("BusGroundCheck: asigna al menos 2 puntos de rueda en 'Probes'.", this);
            probes = new Transform[0];
        }
        wheelHit = new bool[probes.Length];
    }

    // Lo llama el motor una vez por FixedUpdate
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

            // ¿La rueda está delante o detrás del centro del bus?
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