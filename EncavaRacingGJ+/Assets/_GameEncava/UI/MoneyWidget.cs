using Framework.Core;
using Framework.UI;
using TMPro;
using UnityEngine;

public class MoneyWidget : EventWidget<MoneyChanged>
{
    [SerializeField] TMP_Text label;

    protected override void OnEvent(MoneyChanged e)
    {
        if (label != null) label.text = $"${e.Money:N0}";
    }
}
