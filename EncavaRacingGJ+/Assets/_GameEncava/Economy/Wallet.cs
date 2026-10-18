using System;
using Framework.Core;
using Framework.Save;
using UnityEngine;

public class Wallet : MonoBehaviour, ISaveable
{
    [Serializable]
    class SaveData
    {
        public int money;
    }

    [SerializeField] int startingMoney = 0;

    public int Money { get; private set; }
    public string SaveId => "game.wallet";

    void Awake()
    {
        Money = startingMoney;
    }

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

    void Start() => Publish();

    void OnDelivered(FareDelivered e)
    {
        Money += Mathf.RoundToInt(e.Reward);
        Publish();
    }

    public bool TrySpend(int amount)
    {
        if (amount <= 0) return true;
        if (amount > Money) return false;

        Money -= amount;
        Publish();
        return true;
    }

    public void AddMoney(int amount)
    {
        if (amount <= 0) return;
        Money += amount;
        Publish();
    }

    void Publish() => EventBus.Publish(new MoneyChanged(Money));

    public string Capture() => JsonUtility.ToJson(new SaveData { money = Money });

    public void Restore(string json)
    {
        if (string.IsNullOrEmpty(json)) return;
        var data = JsonUtility.FromJson<SaveData>(json);
        if (data != null)
        {
            Money = data.money;
            Publish();
        }
    }
}
