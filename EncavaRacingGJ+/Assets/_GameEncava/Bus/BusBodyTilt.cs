using UnityEngine;

/// <summary>
/// Controla la inclinación visual (cabeceo y alabeo) del chasis del autobús.
/// Combina las fuerzas dinámicas de aceleración y giro con la pendiente del terreno detectada.
/// </summary>
public class BusBodyTilt : MonoBehaviour
{
    [Tooltip("Referencia al controlador del motor del bus.")]
    [SerializeField] BusMotor bus;

    [Tooltip("Ángulo máximo de alabeo (roll) y cabeceo (pitch) provocado por aceleración, frenado y curvas.")]
    [SerializeField] float maxDynamicAngle = 6f;

    [Tooltip("Ángulo máximo de cabeceo permitido por la inclinación de rampas o desniveles del terreno.")]
    [SerializeField] float maxTerrainPitch = 35f;

    [Tooltip("Velocidad de suavizado con la que el chasis se adapta a la pendiente del terreno. A mayor valor, más rápida la respuesta.")]
    [SerializeField] float terrainSmoothing = 8f;

    Quaternion baseRotation;
    float terrainPitch;

    void Awake() => baseRotation = transform.localRotation;

    /// <summary>
    /// Aplica el cálculo de inclinación en LateUpdate para garantizar que el motor ya actualizó sus físicas en este frame.
    /// </summary>
    void LateUpdate()
    {
        // Limita los ángulos dinámicos generados por fuerzas del motor
        float roll = Mathf.Clamp(bus.BodyRoll, -maxDynamicAngle, maxDynamicAngle);
        float dynamicPitch = Mathf.Clamp(bus.BodyPitch, -maxDynamicAngle, maxDynamicAngle);

        // Si está en el suelo sigue la pendiente del terreno; en el aire vuelve suavemente al nivel horizontal
        float target = bus.IsGrounded
            ? Mathf.Clamp(bus.GroundPitch, -maxTerrainPitch, maxTerrainPitch)
            : 0f;
        terrainPitch = Mathf.Lerp(terrainPitch, target, 1f - Mathf.Exp(-terrainSmoothing * Time.deltaTime));

        // En Unity, una rotación positiva en X inclina hacia abajo (baja el morro).
        // Como GroundPitch positivo representa morro arriba, se invierte el signo (-terrainPitch).
        float pitch = -terrainPitch + dynamicPitch;
        transform.localRotation = Quaternion.Euler(pitch, 0f, roll) * baseRotation;
    }
}