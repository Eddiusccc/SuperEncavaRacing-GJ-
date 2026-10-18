using Framework.Core;
using Framework.UI;
using UnityEngine;

/// <summary>
/// Controla el flujo de la interfaz de usuario basándose en el estado global del juego.
/// Escucha los cambios de estado del juego y activa las pantallas correspondientes.
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

    /// <summary>
    /// Inicializa las referencias necesarias si no han sido asignadas en el inspector.
    /// </summary>
    void Awake()
    {
        if (screens == null) screens = GetComponent<ScreenManager>();
    }

    /// <summary>
    /// Se suscribe a los eventos de cambio de estado del juego al activarse el componente.
    /// </summary>
    void OnEnable()  => EventBus.Subscribe<GameStateChanged>(OnGameStateChanged);

    /// <summary>
    /// Se desuscribe de los eventos de cambio de estado del juego al desactivarse el componente.
    /// </summary>
    void OnDisable() => EventBus.Unsubscribe<GameStateChanged>(OnGameStateChanged);

    /// <summary>
    /// Maneja el cambio de estado del juego y activa la pantalla correspondiente.
    /// </summary>
    /// <param name="e">Datos del evento de cambio de estado.</param>
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
