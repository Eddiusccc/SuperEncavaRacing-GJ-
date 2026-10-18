using Framework.Core;
using Framework.UI;
using TMPro;
using UnityEngine;

/// <summary>
/// Widget de interfaz de usuario que muestra el tiempo restante de la partida.
/// Se actualiza automáticamente cuando cambia el tiempo del juego (<see cref="TimeChanged"/>).
/// </summary>
public class TimerWidget : EventWidget<TimeChanged>
{
    [Tooltip("Etiqueta de texto donde se mostrará el cronómetro formateado (MM:SS).")]
    [SerializeField] TMP_Text label;

    /// <summary>
    /// Se llama cuando se recibe un evento <see cref="TimeChanged"/>.
    /// Formatea el tiempo restante en minutos y segundos y lo muestra en la etiqueta.
    /// </summary>
    /// <param name="e">Datos del evento con el tiempo restante en segundos.</param>
    protected override void OnEvent(TimeChanged e)
    {
        if (label == null) return;
        int totalSec = Mathf.CeilToInt(e.Remaining);
        int min = totalSec / 60;
        int sec = totalSec % 60;
        label.text = $"{min:D2}:{sec:D2}";
    }
}
