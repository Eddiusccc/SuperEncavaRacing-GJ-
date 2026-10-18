using Framework.Core;
using Framework.Input;
using UnityEngine;

public readonly struct BusSpeedChanged
{
    public readonly float Speed, Max;
    public BusSpeedChanged(float speed, float max) { Speed = speed; Max = max; }
}

public readonly struct BusCrashed
{
    public readonly float Impact;      // m/s de la colisión
    public BusCrashed(float impact) => Impact = impact;
}

[RequireComponent(typeof(Rigidbody))]
public class BusMotor : MonoBehaviour
{
    [SerializeField] BusConfig config;
    [SerializeField] InputReader input;
    [SerializeField] BusGroundCheck ground;

    public bool IsGrounded => ground.IsGrounded;
    public float GroundPitch => ground.GroundPitch;    // grados: + = morro hacia arriba
    Rigidbody rb;
    float speed;            // REAL, a lo largo del chasis: + adelante, - reversa
    float lateral;          // REAL, deslizamiento lateral
    float steerAngle;       // grados de las ruedas delanteras
    float lastSpeed;
    float boostTimer, boostCooldown, invulnTimer, lastPublished;

    public BusStats Stats { get; } = new();
    public float ForwardSpeed => speed;
    public float LateralSpeed => lateral;
    public float SteerAngle => steerAngle;
    public float YawAngle => Mathf.DeltaAngle(0f, rb.rotation.eulerAngles.y);
    public float BodyRoll { get; private set; }
    public float BodyPitch { get; private set; }
    public bool IsBoosting => boostTimer > 0f;
    public bool IsReversing => speed < -0.1f;
    public BusConfig Config => config;

    void Reset() => ground = GetComponent<BusGroundCheck>();

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // Solo congelamos X y Z: el bus puede girar en Y (rumbo) y la física resuelve los choques
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    void OnEnable()  => input.DashPressed += TryBoost;
    void OnDisable() => input.DashPressed -= TryBoost;

    void TryBoost()
    {
        if (IsBoosting || boostCooldown > 0f) return;
        boostTimer = config.boostDuration;
        float cdMod = Stats.Get(StatType.BoostCooldown, 1f);
        boostCooldown = config.boostCooldown / Mathf.Max(cdMod, 0.1f);
    }

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;
        ground.Refresh();
        boostTimer -= dt; boostCooldown -= dt; invulnTimer -= dt;
        bool grounded = ground.IsGrounded;

        // ----- 0) Leer la realidad: la física ya aplicó choques, paredes, etc. -----
        Vector3 vel = rb.linearVelocity;
        Vector3 planar = new Vector3(vel.x, 0f, vel.z);
        Vector3 fwd = rb.rotation * Vector3.forward; fwd.y = 0f; fwd.Normalize();
        Vector3 right = new Vector3(fwd.z, 0f, -fwd.x);        // derecha del bus

        speed = Vector3.Dot(planar, fwd);
        lateral = Vector3.Dot(planar, right);

        float normalMax = Stats.Get(StatType.MaxSpeed, config.maxSpeed);
        float max = IsBoosting ? Stats.Get(StatType.BoostSpeed, config.boostSpeed) : normalMax;

        // ----- 1) Motor, freno y agarre: solo con las ruedas en el suelo -----
        if (grounded)
        {
            float ratio = Mathf.Clamp01(Mathf.Max(speed, 0f) / max);
            float accel = Stats.Get(StatType.Acceleration, config.acceleration)
                          * config.accelerationBySpeed.Evaluate(ratio);

            float throttle = input.Move.y;
            float brakeDecel = config.brakeDeceleration * Stats.Get(StatType.BrakeForce, 1f);
            if (speed > max)
                speed = Mathf.MoveTowards(speed, max, config.coastDeceleration * 2f * dt);
            else if (throttle > 0.1f)
                speed = Mathf.MoveTowards(speed, max, (speed < 0f ? brakeDecel : accel) * dt);
            else if (throttle < -0.1f)
                speed = speed > 0.5f
                    ? Mathf.MoveTowards(speed, 0f, brakeDecel * dt)
                    : Mathf.MoveTowards(speed, -config.reverseSpeed, config.reverseAcceleration * dt);
            else
                speed = Mathf.MoveTowards(speed, 0f, config.coastDeceleration * dt);

            // El agarre elimina el deslizamiento lateral
            float grip = config.gripAcceleration * Stats.Get(StatType.Handling, 1f);
            lateral = Mathf.MoveTowards(lateral, 0f, grip * dt);
        }

        // ----- 2) Dirección: ángulo de ruedas -> giro del chasis -----
        float angleLimit = Mathf.Lerp(config.maxSteerAngle, config.maxSteerAngleAtTopSpeed,
                                    Mathf.Clamp01(Mathf.Abs(speed) / normalMax));
        angleLimit *= Stats.Get(StatType.Handling, 1f);

        float steerInput = input.Move.x;
        float angleRate = Mathf.Abs(steerInput) > 0.05f ? config.steerAngleRate : config.steerReturnRate;
        steerAngle = Mathf.MoveTowards(steerAngle, steerInput * angleLimit, angleRate * dt);

        // Modelo de bicicleta (rad/s). En reversa el giro se invierte solo
        float targetYaw = grounded
            ? speed / config.wheelBase * Mathf.Tan(steerAngle * Mathf.Deg2Rad)
            : rb.angularVelocity.y;

        Vector3 av = rb.angularVelocity;
        av.y = Mathf.MoveTowards(av.y, targetYaw, config.yawResponse * dt);   // los choques pueden perturbarlo
        rb.angularVelocity = av;

        // ----- 3) Aplicar: UNA sola escritura de velocidad -----
        Vector3 newPlanar = fwd * speed + right * lateral;
        Vector3 v = new Vector3(newPlanar.x, vel.y, newPlanar.z);   // en el aire: gravedad normal
        if (grounded)
        {
            Vector3 n = ground.Normal;
            v.y = (-n.x * v.x - n.z * v.z) / n.y;                   // sigue rampas
        }
        rb.linearVelocity = v;

        // ----- 4) Datos para el cuerpo visual -----
        float lateralAccel = speed * rb.angularVelocity.y;           // v·ω
        float longAccel = (speed - lastSpeed) / dt;
        lastSpeed = speed;
        float smooth = 1f - Mathf.Exp(-6f * dt);
        BodyRoll  = Mathf.Lerp(BodyRoll,  -lateralAccel * config.rollPerAccel,  smooth);
        BodyPitch = Mathf.Lerp(BodyPitch, -longAccel * config.pitchPerAccel, smooth);

        float shown = Mathf.Abs(speed);
        if (Mathf.Abs(shown - lastPublished) > 0.25f)
        {
            lastPublished = shown;
            EventBus.Publish(new BusSpeedChanged(shown, normalMax));
        }
    }

    void OnCollisionEnter(Collision c)
    {
        if (invulnTimer > 0f) return;
        if (c.contactCount > 0 && c.GetContact(0).normal.y > 0.7f) return;   // es el suelo, no un choque

        float impact = c.relativeVelocity.magnitude;
        float threshold = config.crashImpactThreshold * Stats.Get(StatType.Armor, 1f);
        if (impact < threshold) return;

        invulnTimer = config.invulnerability;
        EventBus.Publish(new BusCrashed(impact));
    }
}