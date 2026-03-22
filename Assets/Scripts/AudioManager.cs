using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    // removed single sfxSource, will generate a pool below

    [Header("Pool Settings")]
    private int sfxPoolSize = 64; // how many sounds can play at the exact same time
    private List<AudioSource> sfxPool;

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

        // 1. Build Dictionaries
        foreach (var s in bgmSounds) bgmDict[s.name] = s;
        foreach (var s in sfxSounds) sfxDict[s.name] = s;

        // 2. Create the SFX Pool
        sfxPool = new List<AudioSource>();
        for (int i = 0; i < sfxPoolSize; i++)
        {
            GameObject go = new GameObject("SFX_Source_" + i);
            go.transform.SetParent(transform);
            AudioSource source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            sfxPool.Add(source);
        }

        LoadSettings();
    }

    public void PlayBGM(string name)
    {
        if (bgmDict.TryGetValue(name, out Sound s))
        {
            bgmSource.clip = s.clip;
            bgmSource.volume = s.volume * GetBGMVolume();
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

    // Updated PlaySFX to support multiple simultaneous sounds
    public void PlaySFX(string name, float pitchVariation = 0f)
    {
        if (sfxDict.TryGetValue(name, out Sound s))
        {
            // Find a free AudioSource in the pool
            AudioSource source = GetAvailableSource();

            if (source != null)
            {
                source.clip = s.clip;
                source.volume = s.volume * GetSFXVolume();

                // Pitch Variation Logic
                source.pitch = 1f;
                if (pitchVariation > 0)
                {
                    source.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
                }

                source.Play();
            }
        }
        else
        {
            Debug.LogWarning($"SFX: {name} not found.");
        }
    }

    // Helper to find an AudioSource that isn't currently playing
    private AudioSource GetAvailableSource()
    {
        foreach (AudioSource source in sfxPool)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }

        // Optional: If all sources are busy, return null (sound won't play)
        // Or return the first one to cut it off (aggressive)
        return null;
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

        // Update active sources immediately so UI sliders work
        foreach (AudioSource source in sfxPool)
        {
            if (source.isPlaying)
            {
                // Note: This doesn't update the specific sound's relative volume perfectly, 
                // but updates the master volume component.
                source.volume = GetSFXVolume();
            }
        }
    }

    public float GetBGMVolume() => PlayerPrefs.GetFloat(PREF_BGM_VOL, 1f);
    public float GetSFXVolume() => PlayerPrefs.GetFloat(PREF_SFX_VOL, 1f);

    private void LoadSettings()
    {
        bgmSource.volume = GetBGMVolume();
    }
}