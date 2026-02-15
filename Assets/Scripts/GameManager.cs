using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    // The "Instance" that other scripts will look for
    public static GameManager Instance { get; private set; }

    [Header("References")]
    public Camera mainCamera;
    public GameObject player;

    [Header("Game State")]
    public int score;
    public bool isPaused;

    private void Awake()
    {
        // Singleton Pattern Logic
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keeps manager alive between levels
        }
    }

    public void CamShake(float duration, float magnitude)
    {
        if (mainCamera != null)
        {
            StartCoroutine(Shake(duration, magnitude));
        }
    }

    IEnumerator Shake(float duration, float magnitude)
    {
        if (mainCamera != null)
        {
            Vector3 originalPos = transform.localPosition;
            float elapsed = 0.0f;

            while (elapsed < duration)
            {
                float x = Random.Range(-1f, 1f) * magnitude;
                float y = Random.Range(-1f, 1f) * magnitude;

                transform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);
                elapsed += Time.deltaTime;
                yield return null;
            }
            transform.localPosition = originalPos;
        }
    }

    private bool isFreezing = false;

    public void Freeze(float duration)
    {
        if (isFreezing) return;
        StartCoroutine(DoFreeze(duration));
    }

    private IEnumerator DoFreeze(float duration)
    {
        isFreezing = true;
        float originalDeltaTime = Time.fixedDeltaTime;

        // Stop time
        Time.timeScale = 0f;

        // Wait for real-world seconds (because timeScale is 0)
        yield return new WaitForSecondsRealtime(duration);

        // Restore time
        Time.timeScale = 1f;
        Time.fixedDeltaTime = originalDeltaTime;
        isFreezing = false;
    }

    public void AddScore(int amount)
    {
        score += amount;
    }

    public void ResetScore(int amount)
    {
        score = 0;
    }
}