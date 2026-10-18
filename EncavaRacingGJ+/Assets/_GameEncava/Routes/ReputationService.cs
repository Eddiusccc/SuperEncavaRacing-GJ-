using System;
using System.Collections.Generic;
using Framework.Core;
using Framework.Save;
using UnityEngine;

public class ReputationService : MonoBehaviour, ISaveable
{
    [Serializable]
    class SaveData
    {
        public int[] stopIndices;
        public int[] levels;
        public float[] points;
    }

    [SerializeField] float pointsPerLevel = 100f;
    [SerializeField] float onTimeReputationGain = 25f;
    [SerializeField] float lateReputationLoss = 15f;

    readonly Dictionary<int, float> reputationPoints = new();
    readonly Dictionary<int, int> stopLevels = new();

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

    public int GetStopLevel(int stopIndex)
    {
        return stopLevels.TryGetValue(stopIndex, out int lvl) ? lvl : 1;
    }

    public float GetReputationMultiplier(int stopIndex)
    {
        int lvl = GetStopLevel(stopIndex);
        // Nivel 1 = 1.0x, Nivel 2 = 1.15x, Nivel 3 = 1.30x...
        return 1f + (lvl - 1) * 0.15f;
    }

    void OnDelivered(FareDelivered e)
    {
        int stop = e.OriginStop;
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

    public void ResetForNewRun()
    {
        // En una jam podemos optar por mantener los niveles o reiniciar según el diseño
    }

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
