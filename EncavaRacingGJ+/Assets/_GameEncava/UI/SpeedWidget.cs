using Framework.UI;
using TMPro;
using UnityEngine;

public class SpeedWidget : EventWidget<BusSpeedChanged>
{
    [SerializeField] TMP_Text label;

    protected override void OnEvent(BusSpeedChanged e)
    {
        if (label != null) label.text = $"{e.Speed * 3.6f:F0} km/h";
    }
}
