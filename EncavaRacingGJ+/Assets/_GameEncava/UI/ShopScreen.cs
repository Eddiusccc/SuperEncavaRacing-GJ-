using Framework.Core;
using Framework.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopScreen : UIScreen
{
    [SerializeField] UpgradeService upgradeService;
    [SerializeField] Wallet wallet;
    [SerializeField] GameFlow flow;
    [SerializeField] TMP_Text moneyLabel;
    [SerializeField] Button startRunButton;

    protected override void Awake()
    {
        if (upgradeService == null) upgradeService = FindFirstObjectByType<UpgradeService>();
        if (wallet == null) wallet = FindFirstObjectByType<Wallet>();
        if (flow == null) flow = FindFirstObjectByType<GameFlow>();

        if (startRunButton != null)
        {
            startRunButton.onClick.AddListener(OnStartRunClicked);
        }
    }

    public new void Show()
    {
        base.Show();
        RefreshUI();
        EventBus.Subscribe<MoneyChanged>(OnMoneyChanged);
        EventBus.Subscribe<UpgradeBought>(OnUpgradeBought);
    }

    public new void Hide()
    {
        base.Hide();
        EventBus.Unsubscribe<MoneyChanged>(OnMoneyChanged);
        EventBus.Unsubscribe<UpgradeBought>(OnUpgradeBought);
    }

    void OnMoneyChanged(MoneyChanged e) => RefreshMoney(e.Money);
    void OnUpgradeBought(UpgradeBought _) => RefreshUI();

    void RefreshUI()
    {
        if (wallet != null) RefreshMoney(wallet.Money);
    }

    void RefreshMoney(int amount)
    {
        if (moneyLabel != null) moneyLabel.text = $"${amount:N0}";
    }

    public void BuyUpgrade(UpgradeData upgrade)
    {
        if (upgradeService != null && upgrade != null)
        {
            upgradeService.TryBuy(upgrade);
        }
    }

    void OnStartRunClicked()
    {
        if (flow != null) flow.RequestStart();
    }
}
