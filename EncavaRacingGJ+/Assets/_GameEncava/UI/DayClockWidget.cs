using Framework.Core;
using Framework.UI;
using TMPro;
using UnityEngine;

/// <summary>
/// Widget de HUD que muestra la hora del día de la run.
/// Se actualiza con <see cref="DayTimeChanged"/> y también al activarse,
/// así que funciona aunque se cree después de que empiece la partida.
/// </summary>
public class DayClockWidget : EventWidget<DayTimeChanged>
{
    [Tooltip("Etiqueta donde se muestra la hora, por ejemplo 5:30 AM.")]
    [SerializeField] TMP_Text label;

    [Tooltip("Usa formato 24 h (17:30) en lugar de 12 h (5:30 PM).")]
    [SerializeField] bool use24HourFormat;

    protected override void OnEnable()
    {
        base.OnEnable();

        // Valor inicial por si el widget aparece después del primer evento.
        var cycle = FindFirstObjectByType<DayNightCycle>();
        if (cycle != null) OnEvent(cycle.CurrentTime);
    }

    protected override void OnEvent(DayTimeChanged e)
    {
        if (label == null) return;
        label.text = e.ToClockString(use24HourFormat);
    }
}
