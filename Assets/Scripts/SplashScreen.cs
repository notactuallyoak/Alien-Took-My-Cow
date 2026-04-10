using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashScreen : MonoBehaviour
{
    public void IntroSFX()
    {
        AudioManager.Instance.PlaySFX("Intro");
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
