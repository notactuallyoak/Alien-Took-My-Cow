using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class UIDebugger : MonoBehaviour
{
    public static UIDebugger Instance { get; private set; }

    private KeyCode toggleKey = KeyCode.F1;
    private int maxLevelCount = 12;

    private bool showDebug = false;
    private Rect windowRect = new Rect(20, 20, 500, 600);
    private Vector2 scrollPosition;

    private GUIStyle windowStyle;
    private GUIStyle labelStyle;
    private GUIStyle buttonStyle;
    private GUIStyle boxStyle;
    private Texture2D bgTexture;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        bgTexture = new Texture2D(1, 1);
        bgTexture.SetPixel(0, 0, new Color(0, 0, 0, 0.90f));
        bgTexture.Apply();
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            showDebug = !showDebug;
        }
    }

    private void OnGUI()
    {
        if (!showDebug) return;
        if (windowStyle == null) InitStyles();
        windowRect = GUILayout.Window(0, windowRect, DrawDebugWindow, "DEBUG MENU", windowStyle);
    }

    private void InitStyles()
    {
        windowStyle = new GUIStyle(GUI.skin.window);
        windowStyle.normal.background = bgTexture;
        windowStyle.onNormal.background = bgTexture;
        windowStyle.fontSize = 24;
        windowStyle.fontStyle = FontStyle.Bold;
        windowStyle.normal.textColor = Color.yellow;

        labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = 18;
        labelStyle.normal.textColor = Color.white;
        labelStyle.richText = true;

        buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 18;
        buttonStyle.fixedHeight = 35;

        boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.normal.background = bgTexture;
        boxStyle.normal.textColor = Color.white;
    }

    private void DrawDebugWindow(int windowID)
    {
        GUI.DragWindow(new Rect(0, 0, 10000, 30));
        GUILayout.Space(10);

        GUILayout.Label($"Current Scene: <b>{SceneManager.GetActiveScene().name}</b>", labelStyle);
        GUILayout.Label($"FPS: {(1f / Time.unscaledDeltaTime):F0}", labelStyle);

        GUILayout.Space(15);

        // --- PLAYER CHEATS (NEW) ---
        GUILayout.Label("<b>PLAYER CHEATS</b>", labelStyle);
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Toggle Invincible", buttonStyle))
        {
            PlayerHealth player = FindFirstObjectByType<PlayerHealth>();
            if (player != null)
            {
                // Assuming PlayerHealth has a public bool isInvincible
                // If not, you need to add 'public bool isInvincible;' to PlayerHealth.cs
                player.isInvincible = !player.isInvincible;
                Debug.Log($"Invincible: {player.isInvincible}");
            }
        }

        if (GUILayout.Button("Heal +10 HP", buttonStyle))
        {
            PlayerHealth player = FindFirstObjectByType<PlayerHealth>();
            if (player != null)
            {
                // Assuming PlayerHealth has a Heal function or public currentHealth
                player.Heal(10);
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        // --- SAVE CONTROL ---
        GUILayout.Label("<b>SAVE CONTROL</b>", labelStyle);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Unlock All Levels", buttonStyle))
        {
            PlayerPrefs.SetInt("UnlockedLevel", 99);
            PlayerPrefs.Save();
            RefreshLevelSelector();
        }
        if (GUILayout.Button("Lock All", buttonStyle))
        {
            PlayerPrefs.SetInt("UnlockedLevel", 1);
            PlayerPrefs.Save();
            RefreshLevelSelector();
        }
        GUILayout.EndHorizontal();

        //if (GUILayout.Button("Delete All Save Data", buttonStyle))
        //{
        //    PlayerPrefs.DeleteAll();
        //    RefreshLevelSelector();
        //}

        GUILayout.Space(20);
        GUILayout.Label("<b>LEVEL TELEPORT & TIMES</b>", labelStyle);

        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));

        for (int i = 1; i <= maxLevelCount; i++)
        {
            string levelName = "Level" + i;
            GUILayout.BeginVertical(boxStyle);
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.Label($"<b>{levelName}</b>", labelStyle, GUILayout.Width(100));
            bool isUnlocked = (GameManager.Instance != null) && GameManager.Instance.IsLevelUnlocked(i);
            Color oldColor = GUI.color;
            GUI.color = isUnlocked ? Color.green : Color.red;
            GUILayout.Label(isUnlocked ? "UNLOCKED" : "LOCKED", labelStyle);
            GUI.color = oldColor;
            GUILayout.EndHorizontal();

            if (GameManager.Instance != null)
            {
                GUILayout.Label($"Best: {GameManager.Instance.GetBestTime(levelName)}  |  Last: {GameManager.Instance.GetLastTime(levelName)}", labelStyle);
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("LOAD LEVEL", buttonStyle))
            {
                LevelLoader.instance.LoadLevel(levelName);
            }
            if (GUILayout.Button("CLEAR TIME", buttonStyle, GUILayout.Width(120)))
            {
                PlayerPrefs.DeleteKey("BestTime_" + levelName);
                PlayerPrefs.DeleteKey("LastTime_" + levelName);
                PlayerPrefs.Save();
                RefreshLevelSelector();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
            GUILayout.EndVertical();
            GUILayout.Space(5);
        }

        GUILayout.EndScrollView();
    }

    private void RefreshLevelSelector()
    {
        LevelNode[] allNodes = FindObjectsByType<LevelNode>(FindObjectsSortMode.None);
        foreach (LevelNode node in allNodes)
        {
            node.UpdateButton();
        }
    }
}