using UnityEngine;
using Framework.Input;
using Framework.Core;

/// <summary>
/// Controlador estable de WheelColliders para autobús (masa ~2).
/// Publica BusSpeedChanged para BusCameraFX y expone API compatible con BusMotor.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class BusWheelController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] WheelCollider[] wheels;
    [SerializeField] Transform[] wheelVisuals;
    [SerializeField] Transform centerOfMass;
    [SerializeField] InputReader input;
    [SerializeField] BusGroundCheck groundCheck; // opcional: para Normal, SlopeAngle, GroundPitch

    [Header("Motor / Freno")]
    [SerializeField] float maxMotorTorque = 1500f;
    [SerializeField] float maxBrakeTorque = 3000f;
    [SerializeField] float maxSteerAngle = 35f;
    [SerializeField] bool rearWheelDrive = true;

    [Header("Boost (desde BusConfig)")]
    [SerializeField] BusConfig config;

    [Header("Freno de mano (solo ruedas traseras)")]
    [SerializeField] float handbrakeTorque = 5000f;  // Nm por rueda trasera  // para boostDuration, boostCooldown, boostSpeed

    [Header("Estabilidad (anti-vuelo)")]
    [SerializeField] float antiRollForce = 5000f;
    [SerializeField] float downforceAtSpeed = 200f;
    [SerializeField] float maxSpeedForDownforce = 30f;

    [Header("Suspensión (ajustada para masa 2)")]
    [SerializeField] float springRate = 40000f;
    [SerializeField] float damperRate = 4000f;
    [SerializeField] float suspensionDistance = 0.2f;
    [SerializeField] float forceAppPointDist = 0.15f;

    [Header("Fricción (curvas conservadoras)")]
    [SerializeField] WheelFrictionCurve forwardFriction = new() {
        extremumSlip = 0.3f, extremumValue = 1.8f,
        asymptoteSlip = 0.6f, asymptoteValue = 1.0f,
        stiffness = 1f
    };
    [SerializeField] WheelFrictionCurve sidewaysFriction = new() {
        extremumSlip = 0.2f, extremumValue = 1.5f,
        asymptoteSlip = 0.5f, asymptoteValue = 0.8f,
        stiffness = 1f
    };

    // --- API compatible con BusMotor ---
    public bool IsGrounded { get; private set; }
    public float ForwardSpeed { get; private set; }
    public float LateralSpeed { get; private set; }
    public float SteerAngle { get; private set; }
    public float YawAngle => Mathf.DeltaAngle(0f, rb.rotation.eulerAngles.y);
    public float BodyRoll { get; private set; }
    public float BodyPitch { get; private set; }
    public bool IsBoosting => boostTimer > 0f;
    public bool IsReversing => ForwardSpeed < -0.1f;

    // Delegados a BusGroundCheck si existe
    public Vector3 GroundNormal => groundCheck ? groundCheck.Normal : Vector3.up;
    public float SlopeAngle => groundCheck ? groundCheck.SlopeAngle : 0f;
    public float GroundPitch => groundCheck ? groundCheck.GroundPitch : 0f;
    public int GroundedWheels => groundCheck ? groundCheck.GroundedWheels : (grounded ? wheels.Length : 0);

    Rigidbody rb;
    float steerInput, throttleInput, brakeInput;
    float boostTimer, boostCooldown;
    float lastSpeed, lastPublishedSpeed;
    float normalMaxSpeed = 25f;
    BusStats stats; // para modificadores // fallback, sobrescrito por BusConfig si existe

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = centerOfMass ? centerOfMass.localPosition : new Vector3(0, -0.6f, 0);
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints = RigidbodyConstraints.FreezeRotationZ;

        if (input != null)
            input.DashPressed += OnBoostPressed;

        // BusConfig (para boostDuration, boostCooldown, maxSpeed)
        if (config == null) config = GetComponent<BusConfig>();
        if (config)
        {
            normalMaxSpeed = config.maxSpeed;
        }

        // BusStats (clase plain, no MonoBehaviour)
        stats = new BusStats();

        ConfigureWheels();
    }

    void OnDestroy()
    {
        if (input != null)
            input.DashPressed -= OnBoostPressed;
    }

    void OnBoostPressed()
    {
        if (IsBoosting || boostCooldown > 0f) return;
        if (config == null) return;

        boostTimer = config.boostDuration;
        float cdMod = stats.Get(StatType.BoostCooldown, 1f);
        boostCooldown = config.boostCooldown / Mathf.Max(cdMod, 0.1f);
    }

    void ConfigureWheels()
    {
        foreach (var w in wheels)
        {
            w.mass = 25f;                           // Masa por rueda (no del vehículo)
            w.radius = 0.45f;                       // Radio real en metros
            w.suspensionDistance = suspensionDistance;
            w.forceAppPointDistance = forceAppPointDist;

            var spring = w.suspensionSpring;
            spring.spring = springRate;
            spring.damper = damperRate;
            spring.targetPosition = 0.5f;
            w.suspensionSpring = spring;

            w.forwardFriction = forwardFriction;
            w.sidewaysFriction = sidewaysFriction;

            w.ConfigureVehicleSubsteps(5f, 12, 12); // Substeps para estabilidad a alta velocidad
        }
    }

    bool handbrakeHeld;
    bool reversing; // para lógica de reversa estilo BusMotor

    void Update()
    {
        if (input == null) return;

        // Input analógico: Move.y > 0 = gas, Move.y < 0 = freno/reversa
        float moveY = input.Move.y;
        throttleInput = Mathf.Max(moveY, 0f);
        brakeInput  = Mathf.Max(-moveY, 0f);
        steerInput  = input.Move.x;

        // Freno de mano: Crouch (C / Gamepad B) - bloquea ruedas traseras
        handbrakeHeld = input.CrouchHeld;

        // Reversa estilo BusMotor: Move.y < -0.1f
        // Si velocidad > 0.5f → frena a 0; si <= 0.5f → motor negativo (reversa)
        reversing = moveY < -0.1f;

        // Update visual wheels
        for (int i = 0; i < wheels.Length && i < wheelVisuals.Length; i++)
            UpdateWheelVisual(wheels[i], wheelVisuals[i]);
    }

void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        // Grounded: prioridad a BusGroundCheck (más robusto), fallback a WheelColliders
        bool wcGrounded = CheckGrounded();
        grounded = groundCheck ? groundCheck.IsGrounded : wcGrounded;
        IsGrounded = grounded;

        // Refresh ground check si existe (hace raycasts)
        if (groundCheck) groundCheck.Refresh();

        boostTimer -= dt; boostCooldown -= dt;

        // --- Downforce por velocidad ---
        float speed = rb.linearVelocity.magnitude;
        float downforce = Mathf.Clamp01(speed / maxSpeedForDownforce) * downforceAtSpeed;
        rb.AddForce(Vector3.down * downforce);

        // --- Anti-roll bar (estabilidad lateral) ---
        ApplyAntiRoll();

        // --- Leer velocidades reales (para evento + visual) ---
        Vector3 vel = rb.linearVelocity;
        Vector3 planar = new Vector3(vel.x, 0f, vel.z);
        Vector3 fwd = rb.rotation * Vector3.forward; fwd.y = 0f; fwd.Normalize();
        Vector3 right = new Vector3(fwd.z, 0f, -fwd.x);

        ForwardSpeed = Vector3.Dot(planar, fwd);
        LateralSpeed = Vector3.Dot(planar, right);

        // --- Body roll/pitch (para visual) ---
        float lateralAccel = ForwardSpeed * rb.angularVelocity.y;
        float longAccel = (ForwardSpeed - lastSpeed) / dt;
        lastSpeed = ForwardSpeed;
        float smooth = 1f - Mathf.Exp(-6f * dt);
        BodyRoll  = Mathf.Lerp(BodyRoll,  -lateralAccel * 0.02f, smooth);
        // BodyPitch: combina aceleración longitudinal + pendiente del terreno (GroundPitch)
        float targetPitch = -longAccel * 0.02f + (groundCheck ? groundCheck.GroundPitch * Mathf.Deg2Rad * 0.5f : 0f);
        BodyPitch = Mathf.Lerp(BodyPitch, targetPitch, smooth);

        // --- Motor / Freno / Dirección ---
        float motor = throttleInput * maxMotorTorque;
        float brake = brakeInput * maxBrakeTorque;
        float steer = steerInput * maxSteerAngle;
        SteerAngle = steer;

        // Boost: duplica torque mientras boostTimer > 0
        if (IsBoosting) motor *= 2.5f;

        // Reversa estilo BusMotor
        if (reversing)
        {
            if (ForwardSpeed > 0.5f)
            {
                brake = maxBrakeTorque * 2f;
                motor = 0f;
            }
            else
            {
                motor = -config.reverseAcceleration * maxMotorTorque * 0.5f;
                brake = 0f;
            }
        }

        for (int i = 0; i < wheels.Length; i++)
        {
            var w = wheels[i];
            bool isFront = transform.InverseTransformPoint(w.transform.position).z > 0f;
            bool isRear = !isFront;
            bool driven = rearWheelDrive ? isRear : isFront;

            if (isFront) w.steerAngle = steer;

            if (driven && grounded)
                w.motorTorque = motor / (rearWheelDrive ? CountRear() : CountFront());
            else
                w.motorTorque = 0f;

            // Freno normal en todas
            w.brakeTorque = brake;

            // Freno de mano: SOLO ruedas traseras
            if (handbrakeHeld && isRear)
                w.brakeTorque += handbrakeTorque;
        }

        // --- Publicar BusSpeedChanged (para BusCameraFX) ---
        float shown = Mathf.Abs(ForwardSpeed);
        float maxSpeed = IsBoosting ? (config ? config.boostSpeed : normalMaxSpeed * 1.5f) : normalMaxSpeed;
        if (Mathf.Abs(shown - lastPublishedSpeed) > 0.25f)
        {
            lastPublishedSpeed = shown;
            EventBus.Publish(new BusSpeedChanged(shown, maxSpeed));
        }
    }

    bool grounded;

bool CheckGrounded()
    {
        int count = 0;
        foreach (var w in wheels) if (w.isGrounded) count++;
        return count >= 3;
    }

    void ApplyAntiRoll()
    {
        // Parejas: FL-FR, RL-RR (y RRL-RRR si 6 ruedas)
        for (int i = 0; i < wheels.Length; i += 2)
        {
            if (i + 1 >= wheels.Length) break;
            WheelCollider L = wheels[i], R = wheels[i + 1];
            if (!L.isGrounded || !R.isGrounded) continue;

            float travelL = 1f - L.suspensionSpring.targetPosition; // aprox
            float travelR = 1f - R.suspensionSpring.targetPosition;
            // Mejor: usa suspensionDistance y raycast real
            float compressionL = GetCompression(L);
            float compressionR = GetCompression(R);

            float diff = (compressionL - compressionR) * antiRollForce;
            Vector3 upForce = transform.up * diff;
            rb.AddForceAtPosition( upForce, L.transform.position);
            rb.AddForceAtPosition(-upForce, R.transform.position);
        }
    }

    float GetCompression(WheelCollider w)
    {
        // Raycast manual para compresión real
        if (Physics.Raycast(w.transform.position, -transform.up, out RaycastHit hit,
                            w.suspensionDistance + w.radius, ~0, QueryTriggerInteraction.Ignore))
        {
            float dist = hit.distance - w.radius;
            return 1f - Mathf.Clamp01(dist / w.suspensionDistance);
        }
        return 0f; // extendida
    }

    void UpdateWheelVisual(WheelCollider col, Transform visual)
    {
        col.GetWorldPose(out Vector3 pos, out Quaternion rot);
        visual.SetPositionAndRotation(pos, rot);
    }

    int CountFront()
    {
        int c = 0;
        foreach (var w in wheels) if (transform.InverseTransformPoint(w.transform.position).z > 0f) c++;
        return Mathf.Max(c, 1);
    }
    int CountRear() => wheels.Length - CountFront();

    void OnDrawGizmosSelected()
    {
        if (wheels == null) return;
        Gizmos.color = Color.yellow;
        foreach (var w in wheels)
        {
            if (!w) continue;
            Gizmos.DrawWireSphere(w.transform.position, w.radius);
            Gizmos.DrawLine(w.transform.position, w.transform.position - transform.up * (w.suspensionDistance + w.radius));
        }
        if (centerOfMass) Gizmos.DrawSphere(centerOfMass.position, 0.15f);
    }
}