using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.UI
{
    public class ScreenManager : MonoBehaviour
    {
        [SerializeField] List<UIScreen> screens = new();

        readonly Dictionary<Type, UIScreen> byType = new();
        readonly Stack<UIScreen> stack = new();

        public UIScreen Current => stack.Count > 0 ? stack.Peek() : null;

        void Awake()
        {
            foreach (var screen in screens){
                if (screen == null) continue;
                
                // Si estaba desactivada en el editor, esto dispara su Awake,
                // que la deja invisible con el CanvasGroup
                screen.gameObject.SetActive(true);

                byType[screen.GetType()] = screen;
            }
        }

        public T Push<T>() where T : UIScreen
        {
            if (!byType.TryGetValue(typeof(T), out var screen))
            {
                Debug.LogError($"ScreenManager: no hay una pantalla de tipo {typeof(T).Name}");
                return null;
            }

            if (stack.Count > 0) stack.Peek().Hide(); // La anterior se oculta
            stack.Push(screen);
            screen.Show();
            return (T)screen;
        }

        public void Pop()
        {
            if (stack.Count == 0) return;
            stack.Pop().Hide();
            if (stack.Count > 0) stack.Peek().Show();
        }

        public void Clear()
        {
            while (stack.Count > 0) stack.Pop().Hide();
        }
    }
}