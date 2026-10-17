using System;
namespace Framework.States
{
    public class DelegateState : IState
    {
        readonly Action onEnter;
        readonly Action<float> onTick;
        readonly Action onExit;

        public DelegateState(Action enter = null, Action<float> tick = null, 
        Action exit = null)
        {
            onEnter = enter;
            onTick = tick;
            onExit = exit;
        }
        public void Enter() => onEnter?.Invoke();
        public void Tick(float deltaTime) => onTick?.Invoke(deltaTime);
        public void Exit() => onExit?.Invoke();
    }
}