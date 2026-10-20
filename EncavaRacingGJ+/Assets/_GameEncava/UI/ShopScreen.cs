using Framework.Core;
using Framework.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pantalla de la tienda donde el jugador puede comprar mejoras y comenzar una nueva partida.
/// </summary>
public class ShopScreen : UIScreen
{
    [Header("Servicios")]
    [SerializeField] UpgradeService upgradeService;
    [SerializeField] Wallet wallet;
    [SerializeField] GameFlow flow;

    [Header("Elementos de UI")]
    [SerializeField] TMP_Text moneyLabel;

    #region 
    [SerializeField] UpgradeContainer[] containers;
    [SerializeField] Button refreshButton;
    [SerializeField] int refreshCost = 3;
    public void RefreshContainers(int cost = 1)
    {
        if (!wallet.TrySpend(refreshCost * cost))
        {
            return;
        }

        UpgradeData[] datas = upgradeService.ReturnRandomUpgradeDataArray();
        for (int i = 0; i < datas.Length; i++)
        {
            containers[i].SetContainer(datas[i], upgradeService.Level(datas[i]));
        }
    }
    void ChangeContainers()
    {
        RefreshContainers(0);
    }
    #endregion

    /// <summary>
    /// Inicializa las dependencias buscando los servicios necesarios en la escena si no están asignados.
    /// Importante: Llama a base.Awake() para registrar el CanvasGroup y arrancar con alpha = 0.
    /// </summary>
    protected override void Awake()
    {
        base.Awake();

        if (upgradeService == null) upgradeService = FindFirstObjectByType<UpgradeService>();
        if (wallet == null) wallet = FindFirstObjectByType<Wallet>();
        if (flow == null) flow = FindFirstObjectByType<GameFlow>();

        refreshButton.onClick.AddListener(ChangeContainers);
    }

    /// <summary>
    /// Muestra la pantalla de la tienda, refresca la UI y se suscribe a eventos relevantes.
    /// Se invoca automáticamente cuando la pantalla pasa a ser visible (Show).
    /// </summary>
    protected override void OnShown()
    {
        base.OnShown();
        RefreshUI();
        EventBus.Subscribe<MoneyChanged>(OnMoneyChanged);
        EventBus.Subscribe<UpgradeBought>(OnUpgradeBought);
    }

    /// <summary>
    /// Oculta la pantalla de la tienda y se desuscribe de los eventos.
    /// Se invoca automáticamente cuando la pantalla se oculta (Hide).
    /// </summary>
    protected override void OnHidden()
    {
        base.OnHidden();
        EventBus.Unsubscribe<MoneyChanged>(OnMoneyChanged);
        EventBus.Unsubscribe<UpgradeBought>(OnUpgradeBought);
    }

    /// <summary>
    /// Maneja el cambio de dinero del jugador.
    /// </summary>
    /// <param name="e">Datos del evento de cambio de dinero.</param>
    void OnMoneyChanged(MoneyChanged e) => RefreshMoney(e.Money);

    /// <summary>
    /// Maneja el evento de compra de una mejora para refrescar la interfaz.
    /// </summary>
    /// <param name="_">Datos del evento de mejora comprada.</param>
    void OnUpgradeBought(UpgradeBought _) => RefreshUI();

    /// <summary>
    /// Refresca los elementos visuales de la tienda (como el dinero actual).
    /// </summary>
    void RefreshUI()
    {
        if (wallet != null) RefreshMoney(wallet.Money);
    }

    /// <summary>
    /// Actualiza el texto que muestra el dinero disponible.
    /// </summary>
    /// <param name="amount">Cantidad de dinero a mostrar.</param>
    void RefreshMoney(int amount)
    {
        if (moneyLabel != null) moneyLabel.text = $"${amount:N0}";
    }

    /// <summary>
    /// Intenta comprar una mejora específica a través del servicio de mejoras.
    /// </summary>
    /// <param name="upgrade">Datos de la mejora a comprar.</param>
    public bool BuyUpgrade(UpgradeData upgrade)
    {
        bool b = false;
        if (upgradeService != null && upgrade != null)
        {
            b = upgradeService.TryBuy(upgrade);
        }
        return b;
    }
}
