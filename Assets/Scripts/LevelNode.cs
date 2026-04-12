using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelNode : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Settings")]
    public int levelIndex = 1;
    public string levelSceneName = "Level1";

    [Header("UI References")]
    public Button button;
    public GameObject lockVisual;

    public TextMeshProUGUI bestTimeText;
    public TextMeshProUGUI lastTimeText;

    [Header("Hover Scale Settings")]
    private Vector3 normalScale = new Vector3(1f, 1f, 1f);
    private Vector3 hoverScale = new Vector3(1.1f, 1.1f, 1f); // 10% bigger
    private float scaleSpeed = 20f; // How fast it grows/shrinks

    private RectTransform rectTransform;
    private Vector3 targetScale;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        targetScale = normalScale;
        rectTransform.localScale = normalScale;

        UpdateButton();
    }

    private void Update()
    {
        if (rectTransform.localScale != targetScale)
        {
            rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, targetScale, scaleSpeed * Time.deltaTime);

            if (Vector3.Distance(rectTransform.localScale, targetScale) < 0.01f)
            {
                rectTransform.localScale = targetScale;
            }
        }
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

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button.interactable)
        {
            targetScale = hoverScale;
            AudioManager.Instance.PlaySFX("ButtonHover", 0.1f);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (button.interactable)
        {
            targetScale = normalScale;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (button.interactable)
        {
            AudioManager.Instance.PlaySFX("ButtonClick");
            OnClickLevel();
        }
    }

    public void OnClickLevel()
    {
        LevelLoader.instance.LoadLevel(levelSceneName);
    }
}