using System;
using Framework.Core;
using Framework.Input;
using Framework.States;
using UnityEngine;

/// <summary>
/// Gestiona el flujo principal del juego utilizando una máquina de estados (FSM).
/// Se encarga de coordinar las transiciones entre estados como el menú, la partida,
/// la pausa, el Game Over y la tienda, controlando el tiempo y el acceso a los inputs.
/// </summary>
public class GameFlow : MonoBehaviour
{
    [Tooltip("Referencia al lector de input para habilitar/deshabilitar controles según el estado.")]
    [SerializeField] InputReader input;
    [Tooltip("Estado inicial en el que arrancará el juego.")]
    [SerializeField] GameState startingState = GameState.Menu;

    StateMachine fsm;
    GameState currentState;

    // Flags utilizados para disparar las transiciones de la máquina de estados
    bool startRequested;
    bool pauseRequested;
    bool resumeRequested;
    bool gameOverRequested;
    bool shopRequested;
    bool menuRequested;
    bool perkSelectionRequested;

    /// <summary>
    /// Obtiene el estado actual del juego.
    /// </summary>
    public GameState CurrentState => currentState;

    void Awake()
    {
        fsm = new StateMachine();

        var menuState     = new DelegateState(enter: () => Enter(GameState.Menu, 0f, false));
        var playingState  = new DelegateState(enter: () => Enter(GameState.Playing, 1f, true));
        var pausedState   = new DelegateState(enter: () => Enter(GameState.Paused, 0f, false));
        var gameOverState = new DelegateState(enter: () => Enter(GameState.GameOver, 0f, false));
        var shopState     = new DelegateState(enter: () => Enter(GameState.Shop, 0f, false));
        var perkSeleccionState = new DelegateState(enter: () => Enter(GameState.PerkSelection, 0f, false));

        // Transiciones desde Menú
        fsm.AddTransition(menuState, playingState, () => startRequested);

        // Transiciones desde Playing
        fsm.AddTransition(playingState, pausedState,   () => pauseRequested);
        fsm.AddTransition(playingState, gameOverState, () => gameOverRequested);
        fsm.AddTransition(playingState, perkSeleccionState, () => perkSelectionRequested);

        fsm.AddTransition(perkSeleccionState, playingState, () => resumeRequested);

        // Transiciones desde Paused
        fsm.AddTransition(pausedState, playingState, () => resumeRequested);
        fsm.AddTransition(pausedState, menuState,    () => menuRequested);

        // Transiciones desde GameOver
        fsm.AddTransition(gameOverState, shopState,    () => shopRequested);
        fsm.AddTransition(gameOverState, playingState, () => startRequested);
        fsm.AddTransition(gameOverState, menuState,    () => menuRequested);

        // Transiciones desde Shop
        fsm.AddTransition(shopState, playingState, () => startRequested);
        fsm.AddTransition(shopState, menuState,    () => menuRequested);

        IState initial = startingState switch
        {
            GameState.Playing => playingState,
            GameState.Shop => shopState,
            GameState.GameOver => gameOverState,
            _ => menuState
        };

        fsm.SetState(initial);
        AudioManager.instance.StartPlaylist();
    }

    void OnEnable()
    {
        if (input != null) input.PausePressed += OnPauseInput;
    }

    void OnDisable()
    {
        if (input != null) input.PausePressed -= OnPauseInput;
    }

    /// <summary>
    /// Actualiza la máquina de estados cada frame, ignorando el timeScale del juego.
    /// </summary>
    void Update()
    {
        fsm.Tick(Time.unscaledDeltaTime);
    }

    /// <summary>
    /// Método centralizado para procesar la entrada a un nuevo estado.
    /// Ajusta el timeScale, habilita/deshabilita el input y notifica el cambio mediante el EventBus.
    /// </summary>
    /// <param name="state">El nuevo estado al que se accede.</param>
    /// <param name="timeScale">Escala de tiempo deseada (ej. 0 para pausa, 1 para juego normal).</param>
    /// <param name="enableInput">Si se deben habilitar los controles de gameplay.</param>
    void Enter(GameState state, float timeScale, bool enableInput)
    {
        GameState prev = currentState;
        currentState = state;
        Time.timeScale = timeScale;

        if (input != null)
        {
            if (enableInput) input.EnableGameplay();
            else input.DisableGameplay();
        }

        ResetFlags();
        EventBus.Publish(new GameStateChanged(prev, state));
    }

    /// <summary>
    /// Reinicia todas las solicitudes de transición para evitar saltos de estado no deseados tras entrar en uno nuevo.
    /// </summary>
    void ResetFlags()
    {
        startRequested = false;
        pauseRequested = false;
        resumeRequested = false;
        gameOverRequested = false;
        shopRequested = false;
        menuRequested = false;
    }

    void OnPauseInput()
    {
        if (currentState == GameState.Playing) RequestPause();
        else if (currentState == GameState.Paused) RequestResume();
    }

    /// <summary>Solicita iniciar o reiniciar la partida.</summary>
    public void RequestStart()    => startRequested = true;
    /// <summary>Solicita pausar el juego.</summary>
    public void RequestPause()    => pauseRequested = true;
    /// <summary>Solicita reanudar el juego tras una pausa.</summary>
    public void RequestResume()   => resumeRequested = true;
    /// <summary>Solicita transicionar al estado de fin de juego (GameOver).</summary>
    public void RequestGameOver() => gameOverRequested = true;
    /// <summary>Solicita abrir el estado de la tienda.</summary>
    public void RequestShop()
    {
        shopRequested = true;
        FindAnyObjectByType<ShopScreen>().ChangeContainersFirstTime();
    }  
    /// <summary>Solicita volver al menú principal.</summary>
    public void RequestMenu()     => menuRequested = true;
    public void RequestPerkSelection() => perkSelectionRequested = true;
}
