/// <summary>
/// Evento publicado para notificar los cambios en el número de paradas con pasajeros esperando.
/// </summary>
public struct CityCallsUpdated
{
    /// <summary>Cantidad actual de llamadas o paradas con pasajeros esperando.</summary>
    public int ActiveCalls;

    public CityCallsUpdated(int activeCalls)
    {
        ActiveCalls = activeCalls;
    }
}