using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelNode : MonoBehaviour
{
    [Header("Settings")]
    public int levelIndex = 1;
    public string levelSceneName = "Level1";

    [Header("UI References")]
    public Button button;
    public GameObject lockVisual;

    public TextMeshProUGUI bestTimeText;
    public TextMeshProUGUI lastTimeText;

    private void Start()
    {
        UpdateButton();
    }

    public void UpdateButton()
    {
        bool isUnlocked = GameManager.Instance.IsLevelUnlocked(levelIndex);

        if (isUnlocked)
        {
            // UNLOCK lvl
            button.interactable = true;
            if (lockVisual != null) lockVisual.SetActive(false);

            string best = GameManager.Instance.GetBestTime(levelSceneName);
            string last = GameManager.Instance.GetLastTime(levelSceneName);

            if (bestTimeText != null)
            {
                bestTimeText.text = "Best: " + best;
                bestTimeText.gameObject.SetActive(true);
            }

            if (lastTimeText != null)
            {
                lastTimeText.text = last;
                lastTimeText.gameObject.SetActive(true);
            }
        }
        else
        {
            // LOCK lvl
            button.interactable = false;
            if (lockVisual != null) lockVisual.SetActive(true);

            // hide text when locked
            if (bestTimeText != null) bestTimeText.gameObject.SetActive(false);
            if (lastTimeText != null) lastTimeText.gameObject.SetActive(false);
        }
    }

    public void OnClickLevel()
    {
        LevelLoader.instance.LoadLevel(levelSceneName);
    }
}