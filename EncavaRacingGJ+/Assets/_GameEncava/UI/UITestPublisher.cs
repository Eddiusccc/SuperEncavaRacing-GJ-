using Framework.Core;
using UnityEngine;

public class UITestPublisher : MonoBehaviour
{
    int score;

    [ContextMenu("Test: +10 puntos")]
    void AddScore()
    {
        score += 10;
        EventBus.Publish(new ScoreChanged(score));
    }
    
    [ContextMenu("Test: mostrar promt")]
    void ShowPromt() => EventBus.Publish(new InteractPromptChanged("Abrir puerta"));

    [ContextMenu("Test: ocultar promt")]
    void HidePromt() => EventBus.Publish(new InteractPromptChanged(null));
}