using Framework.Core;
using Framework.Input;
using UnityEngine;

/// <summary>
/// Conecta las teclas de navegación de música (Previous = Q, Next = E) del InputReader
/// con el AudioManager para cambiar de canción en la playlist.
/// </summary>
[RequireComponent(typeof(AudioManager))]
public class MusicInputHandler : MonoBehaviour
{
    [SerializeField] InputReader input;
    AudioManager audioManager;

    void Awake()
    {
        if (input == null) input = Resources.FindObjectsOfTypeAll<InputReader>()[0];
        audioManager = GetComponent<AudioManager>();
        if (audioManager == null) audioManager = AudioManager.instance;
    }

    void OnEnable()
    {
        if (input != null)
        {
            input.PreviousPressed += OnPrevious;
            input.NextPressed += OnNext;
        }
    }

    void OnDisable()
    {
        if (input != null)
        {
            input.PreviousPressed -= OnPrevious;
            input.NextPressed -= OnNext;
        }
    }

    void OnPrevious() => audioManager?.PrevSongButton();
    void OnNext() => audioManager?.NextSongButton();
}
