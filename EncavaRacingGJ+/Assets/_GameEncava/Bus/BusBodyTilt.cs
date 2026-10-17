using UnityEngine;

public class BusBodyTilt : MonoBehaviour
{
    [SerializeField] BusMotor bus;
    [SerializeField] float maxDynamicAngle = 6f;     // alabeo y cabeceo por aceleración y curvas
    [SerializeField] float maxTerrainPitch = 35f;    // cabeceo por rampas
    [SerializeField] float terrainSmoothing = 8f;    // más alto = sigue la rampa más rápido

    Quaternion baseRotation;
    float terrainPitch;

    void Awake() => baseRotation = transform.localRotation;

    void LateUpdate()
    {
        float roll = Mathf.Clamp(bus.BodyRoll, -maxDynamicAngle, maxDynamicAngle);
        float dynamicPitch = Mathf.Clamp(bus.BodyPitch, -maxDynamicAngle, maxDynamicAngle);

        // En el aire vuelve poco a poco a nivel; en suelo sigue la pendiente
        float target = bus.IsGrounded
            ? Mathf.Clamp(bus.GroundPitch, -maxTerrainPitch, maxTerrainPitch)
            : 0f;
        terrainPitch = Mathf.Lerp(terrainPitch, target, 1f - Mathf.Exp(-terrainSmoothing * Time.deltaTime));

        // En Unity un X positivo baja el morro, y GroundPitch positivo es morro arriba: de ahí el signo menos
        float pitch = -terrainPitch + dynamicPitch;
        transform.localRotation = Quaternion.Euler(pitch, 0f, roll) * baseRotation;
    }
}