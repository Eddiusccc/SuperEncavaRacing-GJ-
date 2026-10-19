using Framework.Core;
using Framework.Save;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Servicio que gestiona las mejoras permanentes del autobús y su persistencia.
/// Implementa <see cref="ISaveable"/> para guardar los niveles de las mejoras.
/// </summary>
public class UpgradeService : MonoBehaviour, ISaveable
{
    /// <summary>
    /// Estructura de datos interna para la serialización del estado de las mejoras.
    /// </summary>
    [Serializable]
    class SaveData
    {
        public string[] ids;
        public int[] levels;
    }

    [Header("Configuración")]
    [Tooltip("Lista de todas las mejoras disponibles en el juego.")]
    [SerializeField] List<UpgradeData> catalog = new();

    [Header("Referencias")]
    [SerializeField] BusMotor bus;
    [SerializeField] Wallet wallet;

    readonly Dictionary<string, int> levels = new();

    #region

    public UpgradeData[] ReturnRandomUpgradeDataArray(int quant = 3)
    {
        UpgradeData[] upgrades = new UpgradeData[quant];
        for (int i = 0; i < upgrades.Length; i++)
        {
            int trys = 10;
            upgrades[i] = ReturnRandomUpgradeData();
            while (trys > 0 && CheckDataCopy(upgrades, upgrades[i]))
            {
                upgrades[i] = ReturnRandomUpgradeData();
                trys--;
            }
        }
        return upgrades;
    }
    bool CheckDataCopy(UpgradeData[] arr, UpgradeData da)
    {
        int contador = 0;

        foreach (UpgradeData item in arr)
        {
            if (item.Equals(da))
            {
                contador++;

                if (contador > 1)
                {
                    return true;
                }
            }
        }
        return false;
    }
    UpgradeData ReturnRandomUpgradeData()
    {
        int rng = UnityEngine.Random.Range(0, catalog.Count);
        return catalog[rng];
    }
    #endregion

    /// <summary>Obtiene el catálogo de todas las mejoras definidas.</summary>
    public IReadOnlyList<UpgradeData> Catalog => catalog;

    /// <summary>Identificador único para el sistema de guardado.</summary>
    public string SaveId => "game.upgrades";

    void Awake()
    {
        if (bus == null) bus = FindFirstObjectByType<BusMotor>();
        if (wallet == null) wallet = FindFirstObjectByType<Wallet>();
    }

    void OnEnable() => SaveRegistry.Register(this);
    void OnDisable() => SaveRegistry.Unregister(this);
    void Start() => ApplyAll();

    /// <summary>
    /// Obtiene el nivel actual de una mejora específica.
    /// </summary>
    /// <param name="u">La mejora a consultar.</param>
    /// <returns>El nivel actual (0 si no se ha comprado nunca).</returns>
    public int Level(UpgradeData u)
    {
        if (u == null || string.IsNullOrEmpty(u.id)) return 0;
        return levels.TryGetValue(u.id, out int l) ? l : 0;
    }

    /// <summary>
    /// Intenta comprar el siguiente nivel de una mejora.
    /// Verifica si hay suficiente dinero y si no se ha alcanzado el nivel máximo.
    /// </summary>
    /// <param name="u">La mejora a intentar comprar.</param>
    /// <returns><c>true</c> si la compra fue exitosa; en caso contrario, <c>false</c>.</returns>
    public bool TryBuy(UpgradeData u)
    {
        if (u == null || wallet == null || bus == null) return false;
        int lvl = Level(u);
        if (lvl >= u.maxLevel) return false;

        int cost = u.CostFor(lvl);
        if (!wallet.TrySpend(cost)) return false;

        int nextLevel = lvl + 1;
        levels[u.id] = nextLevel;
        bus.Stats.Set(u, u.stat, u.MultiplierFor(nextLevel));
        EventBus.Publish(new UpgradeBought(u, nextLevel));
        return true;
    }

    /// <summary>
    /// Aplica los multiplicadores de todas las mejoras compradas a las estadísticas del autobús.
    /// </summary>
    public void ApplyAll()
    {
        if (bus == null) return;
        foreach (var u in catalog)
        {
            if (u == null) continue;
            int lvl = Level(u);
            bus.Stats.Set(u, u.stat, u.MultiplierFor(lvl));
        }
    }

    /// <summary>
    /// Captura el estado actual de los niveles de mejora para el sistema de guardado.
    /// </summary>
    /// <returns>JSON serializado con los IDs y niveles.</returns>
    public string Capture()
    {
        var data = new SaveData
        {
            ids = new string[levels.Count],
            levels = new int[levels.Count]
        };
        int i = 0;
        foreach (var kv in levels)
        {
            data.ids[i] = kv.Key;
            data.levels[i] = kv.Value;
            i++;
        }
        return JsonUtility.ToJson(data);
    }

    /// <summary>
    /// Restaura los niveles de mejora desde un JSON y los aplica al autobús.
    /// </summary>
    /// <param name="json">Datos serializados de los niveles.</param>
    public void Restore(string json)
    {
        if (string.IsNullOrEmpty(json)) return;
        var data = JsonUtility.FromJson<SaveData>(json);
        levels.Clear();
        if (data?.ids != null)
        {
            for (int i = 0; i < data.ids.Length; i++)
            {
                levels[data.ids[i]] = data.levels[i];
            }
        }
        ApplyAll();
    }
}
