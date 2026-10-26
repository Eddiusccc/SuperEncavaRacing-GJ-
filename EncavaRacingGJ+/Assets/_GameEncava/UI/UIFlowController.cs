using Framework.Core;
using Framework.UI;
using UnityEngine;

/// <summary>
/// Controla el flujo de la interfaz de usuario basándose en el estado global del juego.
/// Utiliza ScreenManager para apilar y desapilar pantallas ordenadamente.
/// </summary>
public class UIFlowController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] ScreenManager screens;

    [Header("Pantallas")]
    [SerializeField] UIScreen hudScreen;
    [SerializeField] UIScreen menuScreen;
    [SerializeField] UIScreen pauseScreen;
    [SerializeField] UIScreen gameOverScreen;
    [SerializeField] UIScreen shopScreen;
    [SerializeField] UIScreen perkSelectionScreen;

    void Awake()
    {
        if (screens == null) screens = GetComponent<ScreenManager>();

    }

    void OnEnable()  => EventBus.Subscribe<GameStateChanged>(OnGameStateChanged);
    void OnDisable() => EventBus.Unsubscribe<GameStateChanged>(OnGameStateChanged);

    void Start()
    {
        // Garantiza que al iniciar el juego se muestre la pantalla correcta según GameFlow,
        // incluso si el evento inicial se disparó antes de OnEnable.
        var flow = FindFirstObjectByType<GameFlow>();
        GameState initial = flow != null ? flow.CurrentState : GameState.Menu;
        ApplyState(initial, GameState.Menu);
    }

    void OnGameStateChanged(GameStateChanged e)
    {
        ApplyState(e.Current, e.Previous);
    }

    void ApplyState(GameState current, GameState previous)
    {
        if (screens == null) return;

        switch (current)
        {
            case GameState.Menu:
                screens.Clear();
                if (menuScreen != null) screens.Push(menuScreen);
                break;

            case GameState.Playing:
                // Si venimos de Pausa, basta con hacer Pop() de la pantalla de pausa
                if (previous == GameState.Paused && screens.ActiveScreensCount > 1)
                {
                    screens.Pop();
                }
                else
                {
                    screens.Clear();
                    if (hudScreen != null) screens.Push(hudScreen);
                }
                break;

            case GameState.Paused:
                // Apilar la pausa encima del HUD sin destruirlo
                if (pauseScreen != null)
                {
                    screens.Push(pauseScreen, hidePrevious: false);
                }
                break;

            case GameState.GameOver:
                screens.Clear();
                if (gameOverScreen != null) screens.Push(gameOverScreen);
                break;

            case GameState.Shop:
                screens.Clear();
                if (shopScreen != null) screens.Push(shopScreen);
                break;
            case GameState.PerkSelection:
                if (perkSelectionScreen != null) 
                    screens.Push(perkSelectionScreen, hidePrevious: false); // Sobre HUD

                break;
        }
    }
}
