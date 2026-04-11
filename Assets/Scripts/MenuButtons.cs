using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuButtons : MonoBehaviour
{
    [Header("UI References")]
    public Slider bgmSlider;
    public Slider sfxSlider;

    public GameObject optionsPanel;
    // these keys MUST match exactly what is inside your AudioManager script
    private const string PREF_BGM_VOL = "BGMVolume";
    private const string PREF_SFX_VOL = "SFXVolume";

    private const string MAIN_MENU_SCENE_NAME = "MainMenu";

    private bool isPause = false;
    private bool isInMainMenu = false;

    private void Start()
    {
        isInMainMenu = (SceneManager.GetActiveScene().name == MAIN_MENU_SCENE_NAME);

        // load saved values from PlayerPrefs
        float savedBGM = PlayerPrefs.GetFloat(PREF_BGM_VOL, 0.5f);
        float savedSFX = PlayerPrefs.GetFloat(PREF_SFX_VOL, 0.75f);

        bgmSlider.value = savedBGM;
        sfxSlider.value = savedSFX;

        // apply values to the AudioManager
        AudioManager.Instance.SetBGMVolume(ConvertToExponential(savedBGM));
        AudioManager.Instance.SetSFXVolume(ConvertToExponential(savedSFX));
    }

    private void Update()
    {
        if (!isInMainMenu)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (!isPause) OpenOptions();
                else CloseOptions();
            }
        }
    }

    private void OnEnable()
    {
        if (bgmSlider != null) bgmSlider.onValueChanged.AddListener(OnBGMSliderChanged);
        if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(OnSFXSliderChanged);
    }

    private void OnDisable()
    {
        if (bgmSlider != null) bgmSlider.onValueChanged.RemoveListener(OnBGMSliderChanged);
        if (sfxSlider != null) sfxSlider.onValueChanged.RemoveListener(OnSFXSliderChanged);
    }

    public void PlayGame()
    {
        LevelLoader.instance.LoadLevel("LevelSelector");
    }

    public void OnBGMSliderChanged(float value)
    {
        // save to PlayerPrefs
        PlayerPrefs.SetFloat(PREF_BGM_VOL, value);

        AudioManager.Instance.SetBGMVolume(ConvertToExponential(value));
    }

    public void OnSFXSliderChanged(float value)
    {
        // save to PlayerPrefs
        PlayerPrefs.SetFloat(PREF_SFX_VOL, value);

        AudioManager.Instance.SetSFXVolume(ConvertToExponential(value));
    }

    public void OpenOptions()
    {
        optionsPanel.SetActive(true);
        isPause = true;

        if (!isInMainMenu) Time.timeScale = 0f;
    }
    public void CloseOptions()
    {
        optionsPanel.SetActive(false);
        isPause = false;

        if (!isInMainMenu) Time.timeScale = 1f;

        PlayerPrefs.Save(); 
    }

    public void Leave()
    {
        Time.timeScale = 1f;
        LevelLoader.instance.LoadLevel("LevelSelector");
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        LevelLoader.instance.LoadLevel("MainMenu");
    }

    public void QuitGame()
    {
        PlayerPrefs.Save();
        Application.Quit();
    }

    private float ConvertToExponential(float linearValue)
    {
        // clamp to prevent NaN errors (just in case)
        linearValue = Mathf.Clamp01(linearValue);

        // The 'exponent' controls how aggressive the curve is.
        // 1.0f = Linear (no change)
        // 1.5f = Slight curve
        // 2.0f = Standard exponential (Recommended)
        // 3.0f = Extreme curve (quiet for a long time, then gets loud very fast at the end)
        float exponent = 1.5f;

        return Mathf.Pow(linearValue, exponent);
    }
}