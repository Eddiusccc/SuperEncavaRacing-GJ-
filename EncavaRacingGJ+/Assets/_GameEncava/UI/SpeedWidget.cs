using Framework.UI;
using TMPro;
using UnityEngine;

/// <summary>
/// Widget de interfaz de usuario que muestra la velocidad actual del autobús.
/// Se actualiza automáticamente mediante el evento <see cref="BusSpeedChanged"/>.
/// </summary>
public class SpeedWidget : EventWidget<BusSpeedChanged>
{
    [Tooltip("Etiqueta de texto donde se mostrará la velocidad.")]
    [SerializeField] TMP_Text label;

    /// <summary>
    /// Se llama cuando se recibe un evento <see cref="BusSpeedChanged"/>.
    /// Convierte la velocidad de m/s a km/h y actualiza la etiqueta de texto.
    /// </summary>
    /// <param name="e">Datos del evento con la velocidad actual.</param>
    protected override void OnEvent(BusSpeedChanged e)
    {
        if (label != null) label.text = $"{e.Speed * 3.6f:F0} km/h";
    }
}
