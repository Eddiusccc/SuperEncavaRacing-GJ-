using UnityEngine;

/// <summary>
/// Gestiona la representación visual en 2D/Sprite del autobús según la dirección del giro.
/// Cambia el sprite entre centro, izquierda y derecha según el ángulo de guiñada (YawAngle)
/// y aplica opcionalmente una inclinación en el eje Z (Roll).
/// </summary>
public class BusSpriteSteering : MonoBehaviour
{
    [Tooltip("Referencia al motor del autobús de donde se extraen los valores de guiñada y alabeo.")]
    [SerializeField] BusMotor bus;

    [Tooltip("SpriteRenderer donde se representará el bus.")]
    [SerializeField] SpriteRenderer sprite;

    [Tooltip("Sprite a mostrar cuando el bus va recto.")]
    [SerializeField] Sprite center;

    [Tooltip("Sprite a mostrar al girar a la izquierda.")]
    [SerializeField] Sprite left;

    [Tooltip("Sprite a mostrar al girar a la derecha.")]
    [SerializeField] Sprite right;

    [Tooltip("Umbral de ángulo de guiñada (YawAngle) a partir del cual se cambia el sprite a giro.")]
    [SerializeField] float threshold = 6f;

    [Tooltip("Límite máximo en grados para la inclinación visual en Z (Roll).")]
    [SerializeField] float maxRoll = 10f;

    [Tooltip("Activa o desactiva la inclinación en Z provocada por las fuerzas dinámicas.")]
    [SerializeField] bool enableTilt = true;

    /// <summary>
    /// Actualiza el sprite visual y la rotación en Z tras procesar las físicas del frame.
    /// </summary>
    void LateUpdate()
    {
        // Cambio de sprite según el ángulo relativo de guiñada
        float yaw = bus.YawAngle;
        sprite.sprite = yaw < -threshold ? left : yaw > threshold ? right : center;

        // Inclinación dinámica (alabeo) aplicada sobre el eje local Z del sprite
        if (enableTilt)
        {
            float roll = Mathf.Clamp(bus.BodyRoll, -maxRoll, maxRoll);
            transform.localRotation = Quaternion.Euler(0f, 0f, roll);
        }
    }
}
