using Framework.UI;
using TMPro;
using UnityEngine;

public class PassengersWidget : EventWidget<FaresChanged>
{
    [SerializeField] TMP_Text label;

    protected override void OnEvent(FaresChanged e)
    {
        if (label != null) label.text = $"{e.Onboard} / {e.Capacity}";
    }
}
