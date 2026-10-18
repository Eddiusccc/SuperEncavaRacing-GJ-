using Framework.Core;
using Framework.UI;
using TMPro;
using UnityEngine;

public class TimerWidget : EventWidget<TimeChanged>
{
    [SerializeField] TMP_Text label;

    protected override void OnEvent(TimeChanged e)
    {
        if (label == null) return;
        int totalSec = Mathf.CeilToInt(e.Remaining);
        int min = totalSec / 60;
        int sec = totalSec % 60;
        label.text = $"{min:D2}:{sec:D2}";
    }
}
