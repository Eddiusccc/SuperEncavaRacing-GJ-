using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Framework.UI
{
    /// <summary>
    /// Por que ocultamos con `CanvasGroup` y no con `SetActive(false)`:
    /// un objeto desactivado no recibe eventos del `EventBus`. Con CanvasGroup
    /// la pantalla sigue "viva" (activa), solo invisible y sin interaccion.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIScreen : MonoBehaviour
    {
        // boton que se selecciona al mostrar (mando/teclado)
        [Tooltip("Objeto o botón que se seleccionará automáticamente al abrir esta pantalla (ideal para teclado/mando).")]
        [SerializeField] GameObject firstSelected;
        
        CanvasGroup group;

        public bool IsVisible { get; private set; }
        protected virtual void Awake()
        {
            group = GetComponent<CanvasGroup>();
            SetVisible(false);
        }

        public void Show()
        {
            SetVisible(true);
            if (firstSelected != null && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(firstSelected);
            OnShown();
        }

        public void Hide()
        {
            SetVisible(false);
            OnHidden();
        }

        void SetVisible(bool visible)
        {
            IsVisible = visible;
            group.alpha = visible ? 1f : 0f;
            group.interactable = visible;
            group.blocksRaycasts = visible;
        }

        protected virtual void OnShown() { }
        protected virtual void OnHidden() { }
    }
}