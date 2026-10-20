using UnityEngine;
using UnityEngine.UI;

public class MediaPlayer : MonoBehaviour
{
    public Button prev;
    public Button next;
    public Button playpause;


    private void Awake()
    {
        prev.onClick.AddListener(OnPrev);
        next.onClick.AddListener(OnNext);
        playpause.onClick.AddListener(OnPlayPause);
    }

    void OnPrev()
    {
        AudioManager.instance.PrevSongButton();
    }

    void OnNext()
    {
        AudioManager.instance.NextSongButton();
    }

    void OnPlayPause()
    {
        AudioManager.instance.PlayPauseButton();
    }
}
