using Framework.Core;
using Framework.Input;
using UnityEngine;

/// <summary>
/// Evento que se publica cuando la velocidad del autobús cambia.
/// </summary>
public readonly struct BusSpeedChanged
{
    /// <summary>La velocidad actual del autobús.</summary>
    public readonly float Speed;
    /// <summary>La velocidad máxima actual del autobús.</summary>
    public readonly float Max;
    /// <summary>
    /// Constructor para <see cref="BusSpeedChanged"/>.
    /// </summary>
    /// <param name="speed">Velocidad actual.</param>
    /// <param name="max">Velocidad máxima.</param>
    public BusSpeedChanged(float speed, float max) { Speed = speed; Max = max; }
}

/// <summary>
/// Evento que se publica cuando el autobús colisiona significativamente.
/// </summary>
public readonly struct BusCrashed
{
    /// <summary>La magnitud del impacto de la colisión en m/s.</summary>
    public readonly float Impact;      // m/s de la colisión
    /// <summary>
    /// Constructor para <see cref="BusCrashed"/>.
    /// </summary>
    /// <param name="impact">Magnitud del impacto.</param>
    public BusCrashed(float impact) => Impact = impact;
}

/// <summary>
/// Componente que gestiona el movimiento físico y las interacciones del autobús.
/// Requiere un Rigidbody en el mismo GameObject.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class BusMotor : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] BusConfig config;
    [SerializeField] InputReader input;
    [SerializeField] BusGroundCheck ground;

    /// <summary>Indica si el autobús está tocando el suelo.</summary>
    public bool IsGrounded => ground.IsGrounded;
    /// <summary>El ángulo de inclinación del terreno bajo el autobús (en grados).
    /// Positivo si el morro está hacia arriba, negativo si está hacia abajo.</summary>
    public float GroundPitch => ground.GroundPitch;    // grados: + = morro hacia arriba
    Rigidbody rb;
    float speed;            // REAL, a lo largo del chasis: + adelante, - reversa
    float lateral;          // REAL, deslizamiento lateral
    float steerAngle;       // grados de las ruedas delanteras
    float lastSpeed;
    float boostTimer, boostCooldown, invulnTimer, lastPublished;

    /// <summary>Las estadísticas actuales del autobús, obtenidas de <see cref="BusStats"/>.</summary>
    public BusStats Stats { get; } = new();
    /// <summary>Velocidad del autobús hacia adelante (a lo largo de su eje Z local).</summary>
    public float ForwardSpeed => speed;
    /// <summary>Velocidad de deslizamiento lateral del autobús (a lo largo de su eje X local).</summary>
    public float LateralSpeed => lateral;
    /// <summary>Ángulo de dirección actual de las ruedas delanteras (en grados).</summary>
    public float SteerAngle => steerAngle;
    /// <summary>Ángulo de guiñada (yaw) del autobús en el eje Y (rotación horizontal).</summary>
    public float YawAngle => Mathf.DeltaAngle(0f, rb.rotation.eulerAngles.y);
    /// <summary>Rotación del cuerpo del autobús en el eje X (balanceo).</summary>
    public float BodyRoll { get; private set; }
    /// <summary>Rotación del cuerpo del autobús en el eje Z (cabeceo).</summary>
    public float BodyPitch { get; private set; }
    /// <summary>Indica si el autobús está actualmente en modo boost.</summary>
    public bool IsBoosting => boostTimer > 0f;
    /// <summary>Indica si el autobús se está moviendo en reversa.</summary>
    public bool IsReversing => speed < -0.1f;
    /// <summary>Configuración actual del autobús.</summary>
    public BusConfig Config => config;

    /// <summary>
    /// Método llamado cuando el script se carga o el Inspector restablece un valor.
    /// Asigna automáticamente el componente <see cref="BusGroundCheck"/> si existe.
    /// </summary>
    void Reset() => ground = GetComponent<BusGroundCheck>();

    /// <summary>
    /// Se llama cuando el script se carga.
    /// Inicializa el Rigidbody y configura sus restricciones físicas y modo de detección de colisiones.
    /// </summary>
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // Solo congelamos X y Z: el bus puede girar en Y (rumbo) y la física resuelve los choques
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    /// <summary>
    /// Se suscribe al evento de entrada de "Dash" cuando el componente se habilita.
    /// </summary>
    void OnEnable()  => input.DashPressed += TryBoost;
    /// <summary>
    /// Se desuscribe del evento de entrada de "Dash" cuando el componente se deshabilita.
    /// </summary>
    void OnDisable() => input.DashPressed -= TryBoost;

    /// <summary>
    /// Intenta activar el boost del autobús si no está activo y el cooldown ha terminado.
    /// </summary>
    void TryBoost()
    {
        if (IsBoosting || boostCooldown > 0f) return;
        boostTimer = config.boostDuration;
        float cdMod = Stats.Get(StatType.BoostCooldown, 1f);
        boostCooldown = config.boostCooldown / Mathf.Max(cdMod, 0.1f);
    }

    /// <summary>
    /// Se llama en un intervalo de tiempo fijo para actualizaciones de física.
    /// Esta función es el corazón del sistema de movimiento del autobús, gestionando:
    /// <list type="number">
    ///     <item>La lectura de la velocidad y el deslizamiento real del Rigidbody.</item>
    ///     <item>La aplicación del motor, freno y agarre de las ruedas, solo cuando el autobús está en el suelo.</item>
    ///     <item>El cálculo del ángulo de dirección y la aplicación del giro del chasis.</item>
    ///     <item>La aplicación final de la velocidad lineal al Rigidbody.</item>
    ///     <item>El cálculo de los datos para la inclinación visual del cuerpo (balanceo y cabeceo).</item>
    ///     <item>La publicación de eventos de cambio de velocidad.</item>
    /// </list>
    /// </summary>
    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;
        ground.Refresh();
        boostTimer -= dt; boostCooldown -= dt; invulnTimer -= dt;
        bool grounded = ground.IsGrounded;

        // ----- 0) Leer la realidad: la física ya aplicó choques, paredes, etc. -----
        // Calcula la velocidad hacia adelante (speed) y el deslizamiento lateral (lateral)
        // del autobús basándose en la velocidad lineal del Rigidbody y su orientación actual.
        Vector3 vel = rb.linearVelocity;
        Vector3 planar = new Vector3(vel.x, 0f, vel.z);
        Vector3 fwd = rb.rotation * Vector3.forward; fwd.y = 0f; fwd.Normalize();
        Vector3 right = new Vector3(fwd.z, 0f, -fwd.x);        // derecha del bus

        speed = Vector3.Dot(planar, fwd);
        lateral = Vector3.Dot(planar, right);

        float normalMax = Stats.Get(StatType.MaxSpeed, config.maxSpeed);
        float max = IsBoosting ? Stats.Get(StatType.BoostSpeed, config.boostSpeed) : normalMax;

        // ----- 1) Motor, freno y agarre: solo con las ruedas en el suelo -----
        // Aplica la aceleración, el freno y corrige el deslizamiento lateral
        // si el autobús está en el suelo, utilizando las entradas del jugador
        // y las estadísticas del autobús.
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
        // Calcula el ángulo de dirección de las ruedas delanteras y lo utiliza
        // para determinar la velocidad angular deseada del Rigidbody (guiñada).
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
        // Combina la velocidad hacia adelante, lateral y vertical para establecer
        // la velocidad lineal final del Rigidbody. Si está en el suelo, ajusta
        // la velocidad vertical para seguir la superficie.
        Vector3 newPlanar = fwd * speed + right * lateral;
        Vector3 v = new Vector3(newPlanar.x, vel.y, newPlanar.z);   // en el aire: gravedad normal
        if (grounded)
        {
            Vector3 n = ground.Normal;
            v.y = (-n.x * v.x - n.z * v.z) / n.y;                   // sigue rampas
        }
        rb.linearVelocity = v;

        // ----- 4) Datos para el cuerpo visual -----
        // Calcula el balanceo (roll) y cabeceo (pitch) del cuerpo del autobús
        // para fines visuales, basándose en la aceleración lateral y longitudinal.
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

    /// <summary>
    /// Se llama cuando este collider/rigidbody ha comenzado a tocar otro Rigidbody/Collider.
    /// Gestiona los impactos del autobús y publica el evento <see cref="BusCrashed"/> si el impacto supera un umbral.
    /// </summary>
    /// <param name="c">Información sobre la colisión.</param>
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