using Framework.Core;
using TMPro;
using UnityEngine;

namespace Framework.UI
{
    public class ScoreWidget : EventWidget<ScoreChanged>
    {
        [SerializeField] TMP_Text label;
        protected override void OnEvent(ScoreChanged evt)
        {
            label.text = $"Puntos: {evt.Score}";
        }
    }
}