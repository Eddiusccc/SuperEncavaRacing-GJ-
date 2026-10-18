using System;
using Framework.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Framework.UI
{
    public class InteractPromtWidget : EventWidget<InteractPromptChanged>
    {
        [SerializeField] TMP_Text label;
        [SerializeField] Image background;

        protected override void OnEvent(InteractPromptChanged evt)
        {
            bool hasPromt = !string.IsNullOrEmpty(evt.Prompt);
            label.enabled = hasPromt;
            background.enabled = hasPromt;
            if (hasPromt) label.text = $"[E] {evt.Prompt}";
        }
    }
}