using UnityEngine;
using TMPro;

public class InGameTimer : MonoBehaviour
{
    [Header("References")]
    public TMP_Text timerText;

    public RectTransform stopWatchNeedle;
    private float fullRotationTime = 60f;

    private void Update()
    {
        // update timer text
        string timeString = GameManager.Instance.GetCurrentFormattedTime();

        if (timerText != null)
        {
            timerText.text = timeString;
        }

        // update needle
        float currentTime = GameManager.Instance.currentLevelTime;
        float angle = -((currentTime % fullRotationTime) * (360f / fullRotationTime));
        stopWatchNeedle.localEulerAngles = new Vector3(0, 0, angle);
    }
}