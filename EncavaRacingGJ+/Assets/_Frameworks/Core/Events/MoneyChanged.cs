namespace Framework.Core
{
    /// <summary>
    /// Evento publicado cuando cambia la cantidad de dinero del jugador.
    /// </summary>
    public readonly struct MoneyChanged
    {
        public readonly int Money;
        public MoneyChanged(int money) => Money = money;
    }
}
