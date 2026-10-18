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
    [SerializeField] Button startRunButton;

    /// <summary>
    /// Inicializa las dependencias buscando los servicios necesarios en la escena si no están asignados.
    /// </summary>
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

    /// <summary>
    /// Muestra la pantalla de la tienda, refresca la UI y se suscribe a eventos relevantes.
    /// </summary>
    public new void Show()
    {
        base.Show();
        RefreshUI();
        EventBus.Subscribe<MoneyChanged>(OnMoneyChanged);
        EventBus.Subscribe<UpgradeBought>(OnUpgradeBought);
    }

    /// <summary>
    /// Oculta la pantalla de la tienda y se desuscribe de los eventos.
    /// </summary>
    public new void Hide()
    {
        base.Hide();
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
    public void BuyUpgrade(UpgradeData upgrade)
    {
        if (upgradeService != null && upgrade != null)
        {
            upgradeService.TryBuy(upgrade);
        }
    }

    /// <summary>
    /// Solicita el inicio de una nueva partida al sistema de flujo de juego.
    /// </summary>
    void OnStartRunClicked()
    {
        if (flow != null) flow.RequestStart();
    }
}
