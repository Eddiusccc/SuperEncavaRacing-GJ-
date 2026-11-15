using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    public Sound[] sounds;
    public Sound[] playlist;
    public float fadeTransition = 1.5f;
    public int currentIndexPlaylist = 0;


    private void Awake()
    {
        instance = this;
        SetupAudios();
    }
    void SetupAudios()
    {
        foreach (var sound in sounds)
        {
            sound.source = gameObject.AddComponent<AudioSource>();
            sound.source.clip = sound.clip;
            sound.source.volume = sound.volume;
            sound.source.pitch = sound.pitch;
            sound.source.loop = sound.isLoop;
            sound.source.playOnAwake = sound.isPlayAwake;
        }
        foreach (var sound in playlist)
        {
            sound.source = gameObject.AddComponent<AudioSource>();
            sound.source.clip = sound.clip;
            sound.source.volume = sound.volume;
            sound.source.pitch = sound.pitch;
            sound.source.loop = sound.isLoop;
            sound.source.playOnAwake = sound.isPlayAwake;
        }
    }

    #region PLAYLIST METHODS
    public void FadePlaylist(int prevIndex, int nextIndex)
    {
        if (playlist == null || playlist.Length == 0) return;

        Sound prevSound = playlist[prevIndex];
        Sound nextSound = playlist[nextIndex];

        // Evitar tweens pendientes que puedan pisar el volumen
        if (prevSound?.source != null) DOTween.Kill(prevSound.source);
        if (nextSound?.source != null) DOTween.Kill(nextSound.source);

        nextSound.source.volume = 0;
        nextSound.source.Play();

        prevSound.source.DOFade(0, fadeTransition).OnComplete(() =>
        {
            prevSound.source.Stop();
        });

        nextSound.source.DOFade(1, fadeTransition);
    }

    public void StartPlaylist(int index = 0)
    {
        if (playlist == null || playlist.Length == 0) return;
        currentIndexPlaylist = Mathf.Clamp(index, 0, playlist.Length - 1);
        PlayCurrentPlaylist();
    }

    /// <summary>
    /// Elige una canción aleatoria distinta a la actual y la reproduce con crossfade.
    /// Pensado para iniciar cada partida con música diferente.
    /// </summary>
    public void StartPlaylistRandom()
    {
        if (playlist == null || playlist.Length == 0) return;

        int index = currentIndexPlaylist;
        if (playlist.Length > 1)
        {
            do { index = UnityEngine.Random.Range(0, playlist.Length); }
            while (index == currentIndexPlaylist);
        }
        else index = 0;

        int prev = currentIndexPlaylist;
        currentIndexPlaylist = index;

        // Si ya hay una canción sonando, hacemos crossfade para no superponer.
        if (prev != index && IsPlayingIndex(prev))
            FadePlaylist(prev, index);
        else
            PlayCurrentPlaylist();
    }

    public void PlayCurrentPlaylist()
    {
        if (playlist == null || playlist.Length == 0) return;
        Sound s = playlist[currentIndexPlaylist];
        if (s?.source == null) return;

        DOTween.Kill(s.source);
        s.source.volume = 0f;
        s.source.Play();
        s.source.DOFade(1, fadeTransition / 2f);
    }

    /// <summary>Indica si alguna canción de la playlist está sonando.</summary>
    public bool IsPlaylistPlaying()
    {
        if (playlist == null) return false;
        foreach (var s in playlist)
            if (s?.source != null && s.source.isPlaying) return true;
        return false;
    }

    bool IsPlayingIndex(int index)
    {
        return playlist != null
            && index >= 0 && index < playlist.Length
            && playlist[index]?.source != null
            && playlist[index].source.isPlaying;
    }
    public void StopCurrentPlaylist()
    {
        playlist[currentIndexPlaylist].source.DOFade(0, fadeTransition / 2f).OnComplete(() =>
        {
            playlist[currentIndexPlaylist].source.Stop();
        });
    }
    public void PauseCurrentPlaylist()
    {
        playlist[currentIndexPlaylist].source.DOFade(0, fadeTransition / 2f).OnComplete(() =>
        {
            playlist[currentIndexPlaylist].source.Pause();
        });
    }

    public void PlayPauseButton()
    {
        bool needPause = false;
        if (playlist[currentIndexPlaylist].source.isPlaying)
        {
            needPause = true;
        }

        switch (needPause)
        {
            case true:
                {
                    PauseCurrentPlaylist();
                    break;
                }
            case false:
                {
                    PlayCurrentPlaylist();
                    break;
                }
        }
    }

    public void NextSongButton()
    {
        if (playlist == null || playlist.Length < 2) return;
        int prev = currentIndexPlaylist;
        int next = (currentIndexPlaylist + 1) % playlist.Length;
        currentIndexPlaylist = next;
        FadePlaylist(prev, next);
    }
    public void PrevSongButton()
    {
        if (playlist == null || playlist.Length < 2) return;
        int prev = currentIndexPlaylist;
        int next = (currentIndexPlaylist - 1 + playlist.Length) % playlist.Length;
        currentIndexPlaylist = next;
        FadePlaylist(prev, next);
    }
    #endregion

    public void PlaySFX(string name)
    {
        //Debug.Log("Trying to play sound: " + name);
        Sound sound = Array.Find(sounds, s => s.name == name);
        if (sound != null)
        {
            sound.source.pitch = sound.pitch;
            sound.source.PlayOneShot(sound.clip);
            return;
        }
        else
        {
            Debug.LogWarning("Sound: " + name + " not found!");
        }
    }

    public void PlaySFXRandomPitch(string name, float pitch)
    {
        Sound sound = Array.Find(sounds, s => s.name == name);
        if (sound != null)
        {
            float newPitch = UnityEngine.Random.Range(sound.pitch - pitch, sound.pitch + pitch);
            sound.source.pitch = newPitch;
            sound.source.PlayOneShot(sound.clip);
            return;
        }
    }
    public void ChangeSoundVolume(string name, float newVolume, bool withFade)
    {
        newVolume = Mathf.Clamp01(newVolume);
        Sound sound = Array.Find(sounds, s => s.name == name);
        if (sound != null)
        {
            if (withFade)
            {
                //sound.source.volume = newVolume;
                sound.source.DOFade(newVolume, 1f);
            }
            else
            {
                sound.source.volume = newVolume;
            }
        }
        else
        {
            Debug.LogWarning("Sound: " + name + " not found!");
        }
    }
    public void ShutDownWithFade(string name, float fadeDuration)
    {
        float newVolume = 0f;
        Sound sound = Array.Find(sounds, s => s.name == name);
        if (sound != null)
        {
            sound.source.DOFade(newVolume, fadeDuration).OnComplete(() =>
            {
                sound.source.Stop();
            });
        }
        else
        {
            Debug.LogWarning("Sound: " + name + " not found!");
        }
    }
    public Sound ReturnSound(string name)
    {
        Sound sound = Array.Find(sounds, s => s.name == name);
        if (sound != null)
        {
            return sound;
        }
        return null;
    }
    public void Play(string name)
    {
        Sound sound = Array.Find(sounds, s => s.name == name);
        if (sound != null)
        {
            sound.source.Play();
        }
        else
        {
            Debug.LogWarning("Sound: " + name + " not found!");
        }
    }
    public void StopSFX(string name)
    {
        Sound sound = Array.Find(sounds, s => s.name == name);
        if (sound != null)
        {
            sound.source.Stop();
        }
    }
}

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    [HideInInspector] public AudioSource source;
    [Range(0f, 1f)] public float volume;
    [Range(0.01f, 3f)] public float pitch;
    public bool isLoop = false;
    public bool isPlayAwake = false;

    [Space(10f)]
    public string author;
    public string songName;
}
