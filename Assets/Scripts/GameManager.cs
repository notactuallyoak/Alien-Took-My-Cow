using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public List<string> nonGameScenes = new List<string>() { "LevelSelector", "MainMenu", "SplashScreen" };

    // for heart animation in game level
    public Animator heartAnimator;

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

        // SETTINGS

        // get the native resolution of the monitor
        int screenWidth = Screen.currentResolution.width;
        int screenHeight = Screen.currentResolution.height;

        // set resolution to native, fullscreen
        Screen.SetResolution(screenWidth, screenHeight, FullScreenMode.FullScreenWindow);

        Application.runInBackground = true;
        Application.targetFrameRate = 90;
        QualitySettings.vSyncCount = 0;
        QualitySettings.antiAliasing = 0;

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
            StartLevelTimer();
            UpdateHealthUI();
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
        string currentSceneName = SceneManager.GetActiveScene().name;

        string bestTimeKey = "BestTime_" + currentSceneName;
        string lastTimeKey = "LastTime_" + currentSceneName;

        // save last time
        PlayerPrefs.SetFloat(lastTimeKey, currentLevelTime);

        // save best timme
        float savedBest = PlayerPrefs.GetFloat(bestTimeKey, 99999f);
        if (currentLevelTime < savedBest)
        {
            PlayerPrefs.SetFloat(bestTimeKey, currentLevelTime);
            PlayerPrefs.Save();
            //Debug.Log($"New Record! Time: {GetFormattedTime(currentLevelTime)}");
            AudioManager.Instance.PlaySFX("PlayerBestTimer");
        }
        else
        {
            PlayerPrefs.Save(); // make sure lasttime is saved
            //Debug.Log($"Level Finished. Time: {GetFormattedTime(currentLevelTime)}.");
        }

        // unlock next level
        int currentLevelNum = GetLevelNumber(currentSceneName);
        if (currentLevelNum > 0)
        {
            int nextLevelNum = currentLevelNum + 1;
            int highestUnlocked = PlayerPrefs.GetInt("UnlockedLevel", 1);
            if (nextLevelNum > highestUnlocked)
            {
                PlayerPrefs.SetInt("UnlockedLevel", nextLevelNum);
                PlayerPrefs.Save();
            }
        }
    }


    // get number from scene name (e.g., "Level1" -> 1)
    private int GetLevelNumber(string sceneName)
    {
        if (sceneName.StartsWith("Level"))
        {
            string numPart = sceneName.Substring(5); // Removes "Level"
            int num;
            if (int.TryParse(numPart, out num))
            {
                return num;
            }
        }
        return 0;
    }

    // returns true if the level index is unlocked
    public bool IsLevelUnlocked(int levelIndex)
    {
        int highestUnlocked = PlayerPrefs.GetInt("UnlockedLevel", 1);
        return levelIndex <= highestUnlocked;
    }

    public string GetLastTime(string sceneName)
    {
        string key = "LastTime_" + sceneName;
        float time = PlayerPrefs.GetFloat(key, 99999f);

        // If time is still default, return dashes
        if (time >= 99999f) return "--:--:--";
        return GetFormattedTime(time);
    }

    // Returns formatted string "00:00:00" or "--:--:--" if not beaten
    public string GetBestTime(string sceneName)
    {
        string key = "BestTime_" + sceneName;
        float time = PlayerPrefs.GetFloat(key, 99999f);

        if (time < 99999f)
        {
            return GetFormattedTime(time);
        }
        else
        {
            return "--:--:--";
        }
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

    public void UpdateHealthUI(int currentHealth = 8)
    {
        if(heartAnimator != null)
        {
            heartAnimator.SetInteger("Health", currentHealth);
        }
    }

    public void GameOver()
    {
        StopLevelTimer();
        LevelLoader.instance.LoadLevel(SceneManager.GetActiveScene().name);
    }
}