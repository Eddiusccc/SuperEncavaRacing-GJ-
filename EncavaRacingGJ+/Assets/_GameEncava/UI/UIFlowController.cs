using Framework.Core;
using Framework.UI;
using UnityEngine;

public class UIFlowController : MonoBehaviour
{
    [SerializeField] ScreenManager screens;
    [SerializeField] UIScreen hudScreen;
    [SerializeField] UIScreen menuScreen;
    [SerializeField] UIScreen pauseScreen;
    [SerializeField] UIScreen gameOverScreen;
    [SerializeField] UIScreen shopScreen;

    void Awake()
    {
        if (screens == null) screens = GetComponent<ScreenManager>();
    }

    void OnEnable()  => EventBus.Subscribe<GameStateChanged>(OnGameStateChanged);
    void OnDisable() => EventBus.Unsubscribe<GameStateChanged>(OnGameStateChanged);

    void OnGameStateChanged(GameStateChanged e)
    {
        if (screens == null) return;

        screens.Clear();

        switch (e.Current)
        {
            case GameState.Menu:
                if (menuScreen != null) menuScreen.Show();
                break;

            case GameState.Playing:
                if (hudScreen != null) hudScreen.Show();
                break;

            case GameState.Paused:
                if (pauseScreen != null) pauseScreen.Show();
                break;

            case GameState.GameOver:
                if (gameOverScreen != null) gameOverScreen.Show();
                break;

            case GameState.Shop:
                if (shopScreen != null) shopScreen.Show();
                break;
        }
    }
}
