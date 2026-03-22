using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class UIDebugger : MonoBehaviour
{
    public static UIDebugger Instance { get; private set; }

    private KeyCode toggleKey = KeyCode.F1;
    private int maxLevelCount = 12;

    private bool showDebug = false;
    // Larger default window size
    private Rect windowRect = new Rect(20, 20, 500, 600);
    private Vector2 scrollPosition;

    // GUI Styles for customization
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

        // Create a solid black texture for backgrounds
        bgTexture = new Texture2D(1, 1);
        bgTexture.SetPixel(0, 0, new Color(0, 0, 0, 0.95f)); // Black, almost fully opaque
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

        // Initialize Styles once
        if (windowStyle == null) InitStyles();

        // Draw the window
        windowRect = GUILayout.Window(0, windowRect, DrawDebugWindow, "DEBUG MENU", windowStyle);
    }

    private void InitStyles()
    {
        // 1. Window Style (Dark Background)
        windowStyle = new GUIStyle(GUI.skin.window);
        windowStyle.normal.background = bgTexture;
        windowStyle.onNormal.background = bgTexture;
        windowStyle.fontSize = 24;
        windowStyle.fontStyle = FontStyle.Bold;
        windowStyle.normal.textColor = Color.yellow;

        // 2. Label Style (Bigger Text)
        labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = 18;
        labelStyle.normal.textColor = Color.white;
        labelStyle.richText = true;

        // 3. Button Style (Bigger Buttons)
        buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 18;
        buttonStyle.fixedHeight = 35;

        // 4. Box Style
        boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.normal.background = bgTexture; // Solid box background
        boxStyle.normal.textColor = Color.white;
    }

    private void DrawDebugWindow(int windowID)
    {
        // Allow dragging
        GUI.DragWindow(new Rect(0, 0, 10000, 30));

        GUILayout.Space(10);

        // --- HEADER ---
        GUILayout.Label($"Current Scene: <b>{SceneManager.GetActiveScene().name}</b>", labelStyle);
        GUILayout.Label($"FPS: {(1f / Time.unscaledDeltaTime):F0}", labelStyle);

        GUILayout.Space(15);

        // --- GLOBAL CONTROLS ---
        GUILayout.Label("<b>SAVE CONTROL</b>", labelStyle);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Unlock All Levels", buttonStyle))
        {
            PlayerPrefs.SetInt("UnlockedLevel", 99);
            PlayerPrefs.Save();
            RefreshLevelSelector();
            Debug.Log("Cheats: All Levels Unlocked");
        }
        if (GUILayout.Button("Lock All", buttonStyle))
        {
            PlayerPrefs.SetInt("UnlockedLevel", 1);
            PlayerPrefs.Save();
            RefreshLevelSelector();
            Debug.Log("Cheats: Levels Reset to 1");
        }
        GUILayout.EndHorizontal();

        //if (GUILayout.Button("Delete All Save Data", buttonStyle))
        //{
        //    PlayerPrefs.DeleteAll();
        //    RefreshLevelSelector();
        //    Debug.Log("Cheats: All Data Deleted");
        //}

        GUILayout.Space(20);

        // --- LEVEL LIST ---
        GUILayout.Label("<b>LEVEL TELEPORT & TIMES</b>", labelStyle);

        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(350));

        for (int i = 1; i <= maxLevelCount; i++)
        {
            string levelName = "Level" + i;

            // Use custom box style for solid background
            GUILayout.BeginVertical(boxStyle);

            GUILayout.Space(5);

            // Row 1: Name and Status
            GUILayout.BeginHorizontal();
            GUILayout.Label($"<b>{levelName}</b>", labelStyle, GUILayout.Width(100));

            bool isUnlocked = (GameManager.Instance != null) && GameManager.Instance.IsLevelUnlocked(i);

            Color oldColor = GUI.color;
            GUI.color = isUnlocked ? Color.green : Color.red;
            GUILayout.Label(isUnlocked ? "UNLOCKED" : "LOCKED", labelStyle);
            GUI.color = oldColor;
            GUILayout.EndHorizontal();

            // Row 2: Times
            if (GameManager.Instance != null)
            {
                string best = GameManager.Instance.GetBestTime(levelName);
                string last = GameManager.Instance.GetLastTime(levelName);
                GUILayout.Label($"Best: {best}  |  Last: {last}", labelStyle);
            }

            // Row 3: Buttons
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

            GUILayout.Space(5); // Space between boxes
        }

        GUILayout.EndScrollView();
    }

    private void RefreshLevelSelector()
    {
        // Find every LevelNode currently in the scene and tell it to update
        LevelNode[] allNodes = FindObjectsByType<LevelNode>(FindObjectsSortMode.None);
        foreach (LevelNode node in allNodes)
        {
            node.UpdateButton();
        }
        Debug.Log("Level Selector UI Refreshed.");
    }
}