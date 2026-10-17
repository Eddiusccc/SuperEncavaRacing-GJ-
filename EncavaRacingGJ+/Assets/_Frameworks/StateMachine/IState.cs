namespace Framework.States
{
    public interface IState
    {
        void Enter(); // al entrar al estado
        void Tick(float deltaTime); // cada frame mientras está activo
        void Exit(); // al salir del estado
    }
}