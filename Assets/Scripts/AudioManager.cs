using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("Sound Libraries")]
    public Sound[] bgmSounds;
    public Sound[] sfxSounds;

    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        public bool loop;
    }

    // dictionaries for fast lookup
    private Dictionary<string, Sound> bgmDict = new Dictionary<string, Sound>();
    private Dictionary<string, Sound> sfxDict = new Dictionary<string, Sound>();

    private const string PREF_BGM_VOL = "BGMVolume";
    private const string PREF_SFX_VOL = "SFXVolume";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Build Dictionaries
        foreach (var s in bgmSounds) bgmDict[s.name] = s;
        foreach (var s in sfxSounds) sfxDict[s.name] = s;

        LoadSettings();
    }

    public void PlayBGM(string name)
    {
        if (bgmDict.TryGetValue(name, out Sound s))
        {
            bgmSource.clip = s.clip;
            bgmSource.volume = s.volume * GetBGMVolume(); // multiply individual volume by global volume
            bgmSource.loop = s.loop;
            bgmSource.Play();
        }
        else
        {
            Debug.LogWarning($"BGM: {name} not found.");
        }
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    public void PlaySFX(string name)
    {
        if (sfxDict.TryGetValue(name, out Sound s))
        {
            // allows overlapping sounds
            sfxSource.PlayOneShot(s.clip, s.volume * GetSFXVolume());
        }
        else
        {
            Debug.LogWarning($"SFX: {name} not found.");
        }
    }


    public void SetBGMVolume(float volume)
    {
        bgmSource.volume = volume;
        PlayerPrefs.SetFloat(PREF_BGM_VOL, volume);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float volume)
    {
        PlayerPrefs.SetFloat(PREF_SFX_VOL, volume);
        PlayerPrefs.Save();
    }

    public float GetBGMVolume()
    {
        return PlayerPrefs.GetFloat(PREF_BGM_VOL, 1f);
    }

    public float GetSFXVolume()
    {
        return PlayerPrefs.GetFloat(PREF_SFX_VOL, 1f);
    }

    private void LoadSettings()
    {
        // apply saved volumes to AudioSources
        bgmSource.volume = GetBGMVolume();
    }
}