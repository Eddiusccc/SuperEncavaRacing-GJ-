using UnityEngine;

public class BusSpriteSteering : MonoBehaviour
{
    [SerializeField] BusMotor bus;
    [SerializeField] SpriteRenderer sprite;
    [SerializeField] Sprite center, left, right;
    [SerializeField] float threshold = 6f;
    [SerializeField] float maxRoll = 10f;
    [SerializeField] bool enableTilt = true;

    void LateUpdate()
    {
        // Cambio de sprite según YawAngle
        float yaw = bus.YawAngle;
        sprite.sprite = yaw < -threshold ? left : yaw > threshold ? right : center;

        // Alabeo (tilt) aplicado solo en Z
        if (enableTilt)
        {
            float roll = Mathf.Clamp(bus.BodyRoll, -maxRoll, maxRoll);
            transform.localRotation = Quaternion.Euler(0f, 0f, roll);
        }
    }
}
