using Framework.Core;
using Framework.Input;
using Framework.States;
using UnityEngine;

public class GameFlow : MonoBehaviour
{
    [SerializeField] InputReader input;
    [SerializeField] GameState startingState = GameState.Menu;

    StateMachine fsm;
    GameState currentState;

    bool startRequested;
    bool pauseRequested;
    bool resumeRequested;
    bool gameOverRequested;
    bool shopRequested;
    bool menuRequested;

    public GameState CurrentState => currentState;

    void Awake()
    {
        fsm = new StateMachine();

        var menuState     = new DelegateState(enter: () => Enter(GameState.Menu, 0f, false));
        var playingState  = new DelegateState(enter: () => Enter(GameState.Playing, 1f, true));
        var pausedState   = new DelegateState(enter: () => Enter(GameState.Paused, 0f, false));
        var gameOverState = new DelegateState(enter: () => Enter(GameState.GameOver, 0f, false));
        var shopState     = new DelegateState(enter: () => Enter(GameState.Shop, 0f, false));

        // Transiciones desde Menú
        fsm.AddTransition(menuState, playingState, () => startRequested);

        // Transiciones desde Playing
        fsm.AddTransition(playingState, pausedState,   () => pauseRequested);
        fsm.AddTransition(playingState, gameOverState, () => gameOverRequested);

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
    }

    void OnEnable()
    {
        if (input != null) input.PausePressed += OnPauseInput;
    }

    void OnDisable()
    {
        if (input != null) input.PausePressed -= OnPauseInput;
    }

    void Update()
    {
        fsm.Tick(Time.unscaledDeltaTime);
    }

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

    public void RequestStart()    => startRequested = true;
    public void RequestPause()    => pauseRequested = true;
    public void RequestResume()   => resumeRequested = true;
    public void RequestGameOver() => gameOverRequested = true;
    public void RequestShop()     => shopRequested = true;
    public void RequestMenu()     => menuRequested = true;
}
