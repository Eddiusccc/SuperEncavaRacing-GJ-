namespace Framework.Core
{
    public interface IDamageable
    {
        // cualquier cosa que se pueda dañar tiene un método TakeDamage que recibe un número
        void TakeDamage(int amout);
    }
}