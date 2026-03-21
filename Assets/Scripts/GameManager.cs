using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public List<string> nonGameScenes = new List<string>() { "LevelSelector", "MainMenu", "SplashScreen" };

    // timer data
    public float currentLevelTime { get; private set; }
    private bool isTimerRunning = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // SETTINGS
        Application.targetFrameRate = 60;
        Screen.SetResolution(1920, 1080, true);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        if (isTimerRunning)
        {
            currentLevelTime += Time.deltaTime;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (nonGameScenes.Contains(scene.name))
        {
            isTimerRunning = false;
        }
        else
        {
            // entered game level, timer started
            StartLevelTimer();
        }
    }

    // timer control
    public void StartLevelTimer()
    {
        currentLevelTime = 0f;
        isTimerRunning = true;
    }

    public void StopLevelTimer()
    {
        isTimerRunning = false;
    }

    public void FinishLevel()
    {
        StopLevelTimer();

        string levelKey = "BestTime_" + SceneManager.GetActiveScene().name;

        // load saved best time (huge number is bad)
        float savedBest = PlayerPrefs.GetFloat(levelKey, 99999f);

        // Check if current time is better (lower)
        if (currentLevelTime < savedBest)
        {
            PlayerPrefs.SetFloat(levelKey, currentLevelTime);
            PlayerPrefs.Save();
            Debug.Log($"New Record! Time: {GetFormattedTime(currentLevelTime)}");
        }
        else
        {
            Debug.Log($"Level Finished. Time: {GetFormattedTime(currentLevelTime)}. Best: {GetFormattedTime(savedBest)}");
        }

        LevelLoader.instance.LoadLevel("LevelSelector");
    }


    public string GetFormattedTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        int milliseconds = Mathf.FloorToInt((time * 100f) % 100f);
        return string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
    }

    public string GetCurrentFormattedTime()
    {
        return GetFormattedTime(currentLevelTime);
    }


    public void GameOver()
    {
        StopLevelTimer();
        LevelLoader.instance.LoadLevel(SceneManager.GetActiveScene().name);
    }
}