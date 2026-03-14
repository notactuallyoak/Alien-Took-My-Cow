using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // persist between scenes

        // SETTINGS
        Application.targetFrameRate = 60;
        Screen.SetResolution(1920, 1080, true);
    }

    public void GameOver()
    {
        // reload current scene
        Debug.Log("Game Over! Reloading scene...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}