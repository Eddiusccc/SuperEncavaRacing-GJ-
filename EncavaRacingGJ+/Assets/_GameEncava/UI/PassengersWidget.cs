using Framework.UI;
using TMPro;
using UnityEngine;

/// <summary>
/// Widget de interfaz de usuario que muestra la ocupación de pasajeros en el autobús.
/// Se actualiza automáticamente cuando cambia la cantidad de pasajeros o la capacidad (<see cref="FaresChanged"/>).
/// </summary>
public class PassengersWidget : EventWidget<FaresChanged>
{
    [Tooltip("Etiqueta de texto donde se mostrará el conteo de pasajeros (ej: 5 / 20).")]
    [SerializeField] TMP_Text label;

    /// <summary>
    /// Se llama cuando se recibe un evento <see cref="FaresChanged"/>.
    /// Actualiza la etiqueta con la relación actual de pasajeros a bordo frente a la capacidad total.
    /// </summary>
    /// <param name="e">Datos del evento con la ocupación y capacidad actual.</param>
    protected override void OnEvent(FaresChanged e)
    {
        if (label != null) label.text = $"{e.Onboard} / {e.Capacity}";
    }
}
