using Framework.Core;
using UnityEngine;

namespace Framework.Camerarig
{
    /// <summary>
    /// Cursor se libera cuando el GameFlow pausa, o cuando el dialogo desactiva el gameplay
    /// </summary>
    public class CursorStateSwitcher : MonoBehaviour
    {
        void OnEnable() => EventBus.Subscribe<GameplayInputChanged>(OnChanged);
        void OnDisable() => EventBus.Unsubscribe<GameplayInputChanged>(OnChanged);

        void OnChanged(GameplayInputChanged e)
        {
            Cursor.lockState = e.Enable ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !e.Enable;
        }
    }
}