using DG.Tweening;
using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    public Sound[] sounds;
    public Sound[] playlist;

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
}
