using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // destroy duplicate
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // persist between scenes

        // SETTINGS
        Application.targetFrameRate = 60;
        Screen.SetResolution(640, 360, true);
    }
}