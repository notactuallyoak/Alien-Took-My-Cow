using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelLoader : MonoBehaviour
{
    public static LevelLoader instance;

    [Header("UI References")]
    public GameObject loadingScreen;
    public Image fadeImage; 
    public float fadeSpeed;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (loadingScreen != null) loadingScreen.SetActive(false);
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
        }
    }

    public void LoadLevel(int sceneIndex)
    {
        StartCoroutine(LoadAsynchronously(sceneIndex));
    }

    IEnumerator LoadAsynchronously(int sceneIndex)
    {
        loadingScreen.SetActive(true);

        yield return Fade(1f);

        SceneManager.LoadScene(sceneIndex);

        yield return new WaitForEndOfFrame();

        yield return Fade(0f);

        loadingScreen.SetActive(false);
    }

    IEnumerator Fade(float targetAlpha)
    {
        if (fadeImage == null) yield break;

        Color startColor = fadeImage.color;
        float timer = 0f;

        while (timer < (1f / fadeSpeed))
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(startColor.a, targetAlpha, timer * fadeSpeed);
            fadeImage.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        fadeImage.color = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);
    }
}