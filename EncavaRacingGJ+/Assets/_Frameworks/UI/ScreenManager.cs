using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.UI
{
    /// <summary>
    /// Gestiona la pila (Stack) de pantallas activas en la interfaz.
    /// Permite navegación estructurada (Push / Pop / Clear) manteniendo historial de navegación.
    /// </summary> 
    public class ScreenManager : MonoBehaviour
    {
        [SerializeField] List<UIScreen> screens = new();

        readonly Dictionary<Type, UIScreen> byType = new();
        readonly Stack<UIScreen> stack = new();

        public UIScreen Current => stack.Count > 0 ? stack.Peek() : null;
        public int ActiveScreensCount => stack.Count;

        void Awake()
        {
            foreach (var screen in screens)
            {
                if (screen == null) continue;
                
                // Si estaba desactivada en el editor, esto dispara su Awake,
                // que la deja invisible con el CanvasGroup
                screen.gameObject.SetActive(true);
                screen.Hide();

                Type type = screen.GetType();
                if (!byType.ContainsKey(type))
                {
                    byType[type] = screen;
                }
            }
        }

        /// <summary>
        /// Muestra una pantalla por tipo genérico y la apila en el historial.
        /// </summary>
        public T Push<T>(bool hidePrevious = true) where T : UIScreen
        {
            if (!byType.TryGetValue(typeof(T), out var screen))
            {
                Debug.LogError($"ScreenManager: no hay una pantalla registrada de tipo {typeof(T).Name}");
                return null;
            }

            return (T)Push(screen, hidePrevious);
        }

        /// <summary>
        /// Muestra una pantalla por referencia directa y la apila en el historial.
        /// </summary>
        public UIScreen Push(UIScreen screen, bool hidePrevious = true)
        {
            if (screen == null) return null;

            if (stack.Count > 0 && hidePrevious)
            {
                stack.Peek().Hide(); // Oculta la pantalla anterior si no es un modal superpuesto
            }

            stack.Push(screen);
            screen.Show();
            return screen;
        }

        /// <summary>
        /// Cierra la pantalla superior y restaura la pantalla previa en la pila.
        /// </summary>
        public void Pop()
        {
            if (stack.Count == 0) return;
            var top = stack.Pop();
            top.Hide();

            if (stack.Count > 0)
            {
                stack.Peek().Show();
            }
        }

        /// <summary>
        /// Oculta y desapila todas las pantallas activas.
        /// </summary>
        public void Clear()
        {
            while (stack.Count > 0)
            {
                stack.Pop().Hide();
            }
        }
    }
}