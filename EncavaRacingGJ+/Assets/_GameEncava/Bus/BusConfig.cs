using UnityEngine;

[CreateAssetMenu(fileName = "BusConfig", menuName = "GameEncava/Bus Config")]
public class BusConfig : ScriptableObject
{
    [Header("Velocidad (m/s; x3.6 = km/h)")]
    public float maxSpeed = 25f;           // 90 km/h
    public float boostSpeed = 38f;         // 137 km/h
    public float acceleration = 10f;
    public float brakeDeceleration = 30f;
    public float coastDeceleration = 3f;

    [Header("Marcha atrás")]
    public float reverseSpeed = 8f;
    public float reverseAcceleration = 6f;

    [Header("Nitro")]
    public float boostDuration = 1.5f;
    public float boostCooldown = 4f;

    [Header("Aceleración")]
    public AnimationCurve accelerationBySpeed =
        new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(0.6f, 0.55f), new Keyframe(1f, 0.15f));

    [Header("Dirección (modelo bicicleta)")]
    public float wheelBase = 5.5f;                 // metros entre ejes: más = giro más lento y amplio
    public float maxSteerAngle = 28f;              // grados de las ruedas a baja velocidad
    public float maxSteerAngleAtTopSpeed = 7f;     // grados a velocidad máxima
    public float steerAngleRate = 60f;             // grados/s al girar el volante
    public float steerReturnRate = 90f;            // grados/s al soltarlo
    public float gripAcceleration = 40f;           // agarre: menos = derrapa más

    [Header("Giro")]
    public float yawResponse = 6f;               // rad/s²: lo rápido que alcanza la rotación pedida

    [Header("Choque")]
    public float crashImpactThreshold = 6f;      // m/s de impacto mínimo para contar como choque
    public float invulnerability = 1.2f;

    [Header("Cuerpo (visual)")]
    public float rollPerAccel = 0.6f;              // grados de alabeo por m/s² lateral
    public float pitchPerAccel = 0.4f;            // grados de cabeceo por m/s² longitudinal
}