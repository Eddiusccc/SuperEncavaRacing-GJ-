using System;
using System.Collections.Generic;
using Framework.Core;
using Framework.Save;
using UnityEngine;

/// <summary>
/// Servicio que gestiona la reputación en las diferentes paradas de autobús.
/// La reputación aumenta con entregas a tiempo y disminuye con retrasos, afectando los multiplicadores de ganancia.
/// </summary>
public class ReputationService : MonoBehaviour, ISaveable
{
    /// <summary>
    /// Estructura de datos para persistir el progreso de reputación en cada parada.
    /// </summary>
    [Serializable]
    class SaveData
    {
        public int[] stopIndices;
        public int[] levels;
        public float[] points;
    }

    [Header("Configuración")]
    [Tooltip("Puntos requeridos para subir al siguiente nivel de parada.")]
    [SerializeField] float pointsPerLevel = 100f;
    [Tooltip("Puntos base ganados al entregar pasajeros a tiempo.")]
    [SerializeField] float onTimeReputationGain = 25f;
    [Tooltip("Puntos base perdidos al entregar pasajeros con retraso.")]
    [SerializeField] float lateReputationLoss = 15f;

    readonly Dictionary<int, float> reputationPoints = new();
    readonly Dictionary<int, int> stopLevels = new();

    /// <summary>Identificador para el sistema de guardado.</summary>
    public string SaveId => "game.reputation";

    void OnEnable()
    {
        SaveRegistry.Register(this);
        EventBus.Subscribe<FareDelivered>(OnDelivered);
    }

    void OnDisable()
    {
        SaveRegistry.Unregister(this);
        EventBus.Unsubscribe<FareDelivered>(OnDelivered);
    }

    /// <summary>
    /// Obtiene el nivel actual de reputación de una parada específica.
    /// </summary>
    /// <param name="stopIndex">Índice o ID de la parada.</param>
    /// <returns>Nivel de la parada (mínimo 1).</returns>
    public int GetStopLevel(int stopIndex)
    {
        return stopLevels.TryGetValue(stopIndex, out int lvl) ? lvl : 1;
    }

    /// <summary>
    /// Calcula el multiplicador de dinero basado en el nivel de reputación de la parada.
    /// </summary>
    /// <param name="stopIndex">Índice de la parada.</param>
    /// <returns>Multiplicador (ej: Nivel 2 = 1.15x).</returns>
    public float GetReputationMultiplier(int stopIndex)
    {
        int lvl = GetStopLevel(stopIndex);
        // Nivel 1 = 1.0x, Nivel 2 = 1.15x, Nivel 3 = 1.30x...
        return 1f + (lvl - 1) * 0.15f;
    }

    /// <summary>
    /// Actualiza la reputación basándose en el desempeño de la entrega actual.
    /// </summary>
    /// <param name="e">Datos del evento de entrega.</param>
    void OnDelivered(FareDelivered e)
    {
        int stop = e.DestinationStop;
        float current = reputationPoints.TryGetValue(stop, out float pts) ? pts : 0f;
        int lvl = GetStopLevel(stop);

        if (e.TimeRatio >= 1f)
        {
            // Entregado a tiempo o antes
            float bonus = onTimeReputationGain * Mathf.Clamp(e.TimeRatio, 1f, 1.5f);
            current += bonus;
        }
        else
        {
            // Entregado con retraso
            current -= lateReputationLoss * (1f - e.TimeRatio);
        }

        // Subir o bajar de nivel si cruza el umbral
        while (current >= pointsPerLevel)
        {
            current -= pointsPerLevel;
            lvl++;
        }
        while (current < 0f && lvl > 1)
        {
            lvl--;
            current += pointsPerLevel;
        }

        reputationPoints[stop] = current;
        stopLevels[stop] = lvl;

        EventBus.Publish(new StopReputationChanged(stop, lvl, current));
    }

    /// <summary>
    /// Reinicia los datos de reputación si fuera necesario para una nueva partida.
    /// </summary>
    public void ResetForNewRun()
    {
        // En una jam podemos optar por mantener los niveles o reiniciar según el diseño
    }

    /// <summary>
    /// Captura el estado de reputación de todas las paradas para su guardado.
    /// </summary>
    public string Capture()
    {
        var data = new SaveData
        {
            stopIndices = new int[stopLevels.Count],
            levels = new int[stopLevels.Count],
            points = new float[stopLevels.Count]
        };

        int i = 0;
        foreach (var kv in stopLevels)
        {
            data.stopIndices[i] = kv.Key;
            data.levels[i] = kv.Value;
            data.points[i] = reputationPoints.TryGetValue(kv.Key, out float p) ? p : 0f;
            i++;
        }
        return JsonUtility.ToJson(data);
    }

    /// <summary>
    /// Restaura los datos de reputación desde un JSON.
    /// </summary>
    public void Restore(string json)
    {
        if (string.IsNullOrEmpty(json)) return;
        var data = JsonUtility.FromJson<SaveData>(json);
        stopLevels.Clear();
        reputationPoints.Clear();

        if (data?.stopIndices != null)
        {
            for (int i = 0; i < data.stopIndices.Length; i++)
            {
                stopLevels[data.stopIndices[i]] = data.levels[i];
                reputationPoints[data.stopIndices[i]] = data.points[i];
            }
        }
    }
}
