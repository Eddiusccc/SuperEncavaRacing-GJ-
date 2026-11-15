using Framework.Core;
using Framework.Save;
using UnityEngine;

public class SaveController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] SaveManager saveManager;
    [SerializeField] GameFlow flow;
    [SerializeField] int saveSlot = 0;

    void Awake()
    {
        if (saveManager == null) saveManager = SaveManager.Instance;
        if (flow == null) flow = FindFirstObjectByType<GameFlow>();
    }

    void Start()
    {
        // Cargar al inicio (despues de OnEnable)
        saveManager.Load(saveSlot);
    }

    void OnEnable()
    {
        EventBus.Subscribe<GameStateChanged>(OnStateChanged);
    }
    void OnDisable()
    {
        EventBus.Unsubscribe<GameStateChanged>(OnStateChanged);
    }

    void OnStateChanged(GameStateChanged e)
    {
        // Guardar al entrar en GameOver o Shop
        if (e.Current == GameState.GameOver || e.Current == GameState.Shop)
        {
            saveManager.Save(saveSlot);
        }
    }

    void OnApplicationQuit()
    {
        // Guardar al cerrar el juego
        saveManager.Save(saveSlot);
    }

    void OnApplicationPause(bool pause)
    {
        // Guardar al minimizar o perder foco
        if (pause) saveManager.Save(saveSlot);
    }

    // Para boton "Borrar progreso"
    public void ClearSave() => saveManager.ClearSave(saveSlot);
}