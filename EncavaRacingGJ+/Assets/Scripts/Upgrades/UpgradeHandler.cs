using UnityEngine;

public class UpgradeHandler : MonoBehaviour
{
    public static UpgradeHandler instance;

    public BusStatSO busStats;


    #region UPGRADE STAT

    public Stat GetStat(BusStats type)
    {
        foreach (var item in busStats.stats)
        {
            if(item.stat == type)
            {
                return item;
            }
        }
        return new Stat();
    }

    public void UpgradeBusStat(BusStats type, float value, bool isMulti = false)
    {
        float valueToAdd = value;
        Stat s = GetStat(type);
        if (isMulti)
        {
           valueToAdd = s.value * (value - 1);
        }
        GetStat(type).AddValue(valueToAdd);
    }

    #endregion

    private void Awake()
    {
        if (instance == null) instance = this; else { Destroy(this); }
    }
}
