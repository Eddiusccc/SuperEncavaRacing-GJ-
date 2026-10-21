using UnityEngine;

/// <summary>
/// Configuración de físicas, maniobrabilidad y comportamiento visual del autobús.
/// Define las velocidades, aceleraciones, modelo de dirección tipo bicicleta y límites de impacto.
/// </summary>
[CreateAssetMenu(fileName = "BusConfig", menuName = "GameEncava/Bus Config")]
public class BusConfig : ScriptableObject
{
    [Header("Velocidad (m/s; x3.6 = km/h)")]
    [Tooltip("Velocidad máxima estándar de avance en m/s (ej. 25 m/s = 90 km/h).")]
    public float maxSpeed = 25f;

    [Tooltip("Velocidad máxima alcanzable con nitro/turbo en m/s (ej. 38 m/s = 137 km/h).")]
    public float boostSpeed = 38f;

    [Tooltip("Tasa de aceleración estándar en m/s².")]
    public float acceleration = 10f;

    [Tooltip("Tasa de frenado activo en m/s².")]
    public float brakeDeceleration = 30f;

    [Tooltip("Desaceleración pasiva por inercia/fricción al soltar el acelerador en m/s².")]
    public float coastDeceleration = 3f;

    [Header("Marcha atrás")]
    [Tooltip("Velocidad máxima en reversa en m/s.")]
    public float reverseSpeed = 8f;

    [Tooltip("Tasa de aceleración en reversa en m/s².")]
    public float reverseAcceleration = 6f;

    [Header("Nitro")]
    [Tooltip("Duración activa del impulso en segundos.")]
    public float boostDuration = 1.5f;

    [Tooltip("Tiempo de recarga antes de poder usar nitro nuevamente en segundos.")]
    public float boostCooldown = 4f;

    [Header("Aceleración")]
    [Tooltip("Modificador de aceleración según el porcentaje de velocidad actual (0 a 1). Permite simular pérdida de potencia a altas velocidades.")]
    public AnimationCurve accelerationBySpeed =
        new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(0.6f, 0.55f), new Keyframe(1f, 0.15f));

    [Header("Dirección (modelo bicicleta)")]
    [Tooltip("Distancia entre ejes en metros. A mayor longitud, giros más amplios y estables.")]
    public float wheelBase = 5.5f;

    [Tooltip("Ángulo de giro máximo de las ruedas a baja velocidad en grados.")]
    public float maxSteerAngle = 28f;

    [Tooltip("Ángulo de giro máximo de las ruedas cuando se viaja a velocidad tope en grados.")]
    public float maxSteerAngleAtTopSpeed = 7f;

    [Tooltip("Velocidad de giro del volante en grados por segundo.")]
    public float steerAngleRate = 60f;

    [Tooltip("Velocidad de retorno del volante a posición neutra al soltar la dirección en grados por segundo.")]
    public float steerReturnRate = 90f;

    [Tooltip("Aceleración centrípeta máxima que soportan los neumáticos antes de comenzar a derrapar (m/s²).")]
    public float gripAcceleration = 40f;

    [Header("Giro")]
    [Tooltip("Tasa angular de respuesta en rad/s² para alcanzar el giro deseado.")]
    public float yawResponse = 6f;

    [Header("Terreno")]
    [Tooltip("Velocidad de suavizado con la que el cuerpo físico se alinea con la pendiente del terreno. A mayor valor, más rápida la respuesta.")]
    public float terrainAlignSpeed = 10f;

    [Header("Choque")]
    [Tooltip("Velocidad de impacto mínima en m/s para registrar una colisión como choque crítico.")]
    public float crashImpactThreshold = 6f;

    [Tooltip("Tiempo de invulnerabilidad tras un choque en segundos.")]
    public float invulnerability = 1.2f;

    [Header("Cuerpo (visual)")]
    [Tooltip("Grados de inclinación lateral (roll) generados por cada m/s² de aceleración lateral.")]
    public float rollPerAccel = 0.6f;

    [Tooltip("Grados de inclinación longitudinal (pitch) generados por cada m/s² de aceleración longitudinal.")]
    public float pitchPerAccel = 0.4f;
}