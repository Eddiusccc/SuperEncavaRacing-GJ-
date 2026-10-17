using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Framework.States
{
    /// <summary>
    /// Gestiona transiciones entre estados (Menú, Jugando, Pausa, Game Over).
    /// </summary>
    public class StateMachine
    {
        class Transition
        {
            public readonly IState To;
            // funcion que no recibe nada y devuelve bool. Es la "condicion de la transicion"
            public readonly Func<bool> Condition; 
            public Transition(IState to, Func<bool> condition)
            {
                To = to;
                Condition = condition;
            }
        }

        readonly Dictionary<IState, List<Transition>> transitions = new();
        readonly List<Transition> anyTransition = new();

        public IState Current { get; private set; }

        // (estado anterior, estado nuevo) 
        public event Action<IState, IState> StateChanged;

        /// <summary>
        /// Agrega una transición condicional.
        /// </summary>
        /// <param name="from">Estado base</param>
        /// <param name="to">Estado objetivo</param>
        /// <param name="condition">condicion de transicion</param>
        public void AddTransition(IState from, IState to, Func<bool> condition)
        {
            if (!transitions.TryGetValue(from, out var list))
                transitions[from] = list = new List<Transition>();
                list.Add(new Transition(to, condition));
        }

        /// <summary>
        /// Transición que se evalúa desde CUALQUIER estado (ej: morir)
        /// </summary>
        /// <param name="to">estado objetivo</param>
        /// <param name="condition">condicion de transición</param>
        public void AddAnyTransition(IState to, Func<bool> condition) 
            => anyTransition.Add(new Transition(to, condition));
        
        /// <summary>
        /// Cambia de estado
        /// </summary>
        /// <param name="next"></param>
        public void SetState(IState next)
        {
            if (next == null || next == Current) return;
            
            var previous = Current;
            previous?.Exit();
            Current = next;
            Current.Enter();
            StateChanged?.Invoke(previous, next);
        }
        public void Tick(float deltaTime)
        {
            var transition = FindTransition();
            if (transition != null) SetState(transition.To);
            Current?.Tick(deltaTime);
        }
        
        Transition FindTransition()
        {
            // Las transiciones "desde cualquiera" tienen prioridad
            foreach (var t in anyTransition) if (t.Condition()) return t;

            if (Current != null && transitions.TryGetValue(Current, out var list))
                foreach (var t in list) if (t.Condition()) return t;
                
            return null;
        }
    }
}