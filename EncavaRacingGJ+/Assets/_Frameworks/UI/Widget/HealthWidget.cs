using Framework.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Framework.UI
{
    public class HealthWidget : EventWidget<HealthChanged>
    {
        [SerializeField] Slider slider;
        [SerializeField] TMP_Text label;

        protected override void OnEvent(HealthChanged evt)
        {
            if (slider) slider.value = (float)evt.Current / evt.Max;
            if (label) label.text = $"{evt.Current} / {evt.Max}";
        }
    }
}