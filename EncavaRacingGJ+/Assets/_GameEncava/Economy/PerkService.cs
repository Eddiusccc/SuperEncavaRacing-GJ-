using System;
using System.Collections.Generic;
using Framework.Core;
using UnityEngine;

/// <summary>
/// Evento que se publica cuando se ofrecen mejoras temporales (perks) al jugador.
/// </summary>
public readonly struct PerkOffered
{
    /// <summary>Las opciones de perks disponibles para elegir.</summary>
    public readonly PerkData[] Choices;
    /// <summary>
    /// Constructor para <see cref="PerkOffered"/>.
    /// </summary>
    /// <param name="choices">Array de opciones.</param>
    public PerkOffered(PerkData[] choices) => Choices = choices;
}

/// <summary>
/// Servicio que gestiona la lógica de obtención y aplicación de beneficios temporales (perks) durante una partida.
/// </summary>
public class PerkService : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Lista de todos los perks posibles que pueden aparecer.")]
    [SerializeField] List<PerkData> perkPool = new();
    [Tooltip("Número de entregas requeridas para que se ofrezca un nuevo grupo de perks.")]
    [SerializeField] int deliveriesPerPerk = 3;

    [Header("Referencias")]
    [SerializeField] BusMotor bus;

    readonly List<PerkData> activePerks = new();
    int deliveriesSinceLastPerk;

    /// <summary>Lista de perks que el jugador ha seleccionado durante la partida actual.</summary>
    public IReadOnlyList<PerkData> ActivePerks => activePerks;

    void Awake()
    {
        if (bus == null) bus = FindFirstObjectByType<BusMotor>();
    }

    void OnEnable()  => EventBus.Subscribe<FareDelivered>(OnDelivered);
    void OnDisable() => EventBus.Unsubscribe<FareDelivered>(OnDelivered);

    /// <summary>
    /// Maneja el evento de entrega de pasajeros para contar el progreso hacia la siguiente oferta de perks.
    /// </summary>
    void OnDelivered(FareDelivered _)
    {
        deliveriesSinceLastPerk++;
        if (deliveriesSinceLastPerk >= deliveriesPerPerk)
        {
            deliveriesSinceLastPerk = 0;
            OfferPerks();
        }
    }

    /// <summary>
    /// Selecciona aleatoriamente un grupo de perks del pool y publica la oferta.
    /// </summary>
    void OfferPerks()
    {
        if (perkPool.Count == 0) return;

        // Seleccionar hasta 3 perks al azar
        int count = Mathf.Min(3, perkPool.Count);
        var shuffled = new List<PerkData>(perkPool);
        for (int i = 0; i < shuffled.Count; i++)
        {
            int r = UnityEngine.Random.Range(i, shuffled.Count);
            (shuffled[i], shuffled[r]) = (shuffled[r], shuffled[i]);
        }

        var choices = shuffled.GetRange(0, count).ToArray();
        EventBus.Publish(new PerkOffered(choices));
    }

    /// <summary>
    /// Aplica un perk seleccionado al autobús y lo añade a la lista de activos.
    /// </summary>
    /// <param name="perk">El perk seleccionado por el jugador.</param>
    public void SelectPerk(PerkData perk)
    {
        if (perk == null || bus == null) return;
        activePerks.Add(perk);
        bus.Stats.Set(perk, perk.stat, perk.multiplier);
    }

    /// <summary>
    /// Reinicia los perks acumulados y limpia sus efectos en las estadísticas del autobús.
    /// Debe llamarse al terminar una partida.
    /// </summary>
    public void ResetRunPerks()
    {
        deliveriesSinceLastPerk = 0;
        if (bus != null)
        {
            foreach (var p in activePerks)
            {
                bus.Stats.Clear(p);
            }
        }
        activePerks.Clear();
    }
}
