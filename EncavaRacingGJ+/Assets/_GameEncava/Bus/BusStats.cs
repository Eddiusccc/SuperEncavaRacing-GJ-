using System.Collections.Generic;

/// <summary>
/// Define los diferentes tipos de estadísticas que puede tener un autobús.
/// </summary>
public enum StatType
{
    /// <summary>Velocidad máxima del autobús.</summary>
    MaxSpeed,
    /// <summary>Aceleración del autobús.</summary>
    Acceleration,
    /// <summary>Fuerza de frenado del autobús.</summary>
    BrakeForce,
    /// <summary>Manejo/control del autobús.</summary>
    Handling,
    /// <summary>Velocidad adicional al usar el boost.</summary>
    BoostSpeed,
    /// <summary>Tiempo de enfriamiento del boost.</summary>
    BoostCooldown,
    /// <summary>Capacidad de pasajeros del autobús.</summary>
    Capacity,
    /// <summary>Multiplicador de tarifa por pasajero.</summary>
    FareMultiplier,
    /// <summary>Bonificación de tiempo en el juego.</summary>
    TimeBonus,
    /// <summary>Resistencia del autobús a los impactos.</summary>
    Armor
}

/// <summary>
/// Gestiona las estadísticas de un autobús, permitiendo aplicar y combinar multiplicadores de diferentes fuentes.
/// </summary>
public class BusStats
{
    readonly Dictionary<StatType, Dictionary<object, float>> mods = new();

    /// <summary>
    /// Establece o actualiza un multiplicador para una estadística específica desde una fuente dada.
    /// </summary>
    /// <param name="source">El objeto que aplica el multiplicador (por ejemplo, una mejora, un efecto, etc.).</param>
    /// <param name="stat">El tipo de estadística a modificar.</param>
    /// <param name="multiplier">El valor del multiplicador a aplicar (por ejemplo, 1.2f para un aumento del 20%).</param>
    public void Set(object source, StatType stat, float multiplier)
    {
        if (!mods.TryGetValue(stat, out var bySource))
            mods[stat] = bySource = new Dictionary<object, float>();
        bySource[source] = multiplier;
    }

    /// <summary>
    /// Elimina todos los multiplicadores aplicados por una fuente específica en todas las estadísticas.
    /// </summary>
    /// <param name="source">El objeto cuya influencia en las estadísticas debe eliminarse.</param>
    public void Clear(object source)
    {
        foreach (var d in mods.Values) d.Remove(source);
    }

    /// <summary>
    /// Obtiene el valor final de una estadística después de aplicar todos los multiplicadores registrados.
    /// </summary>
    /// <param name="stat">El tipo de estadística a consultar.</param>
    /// <param name="baseValue">El valor base de la estadística antes de aplicar cualquier multiplicador.</param>
    /// <returns>El valor de la estadística con todos los multiplicadores aplicados.</returns>
    public float Get(StatType stat, float baseValue)
    {
        float total = baseValue;
        if (mods.TryGetValue(stat, out var bySource))
            foreach (float m in bySource.Values) total *= m;
        return total;
    }
}