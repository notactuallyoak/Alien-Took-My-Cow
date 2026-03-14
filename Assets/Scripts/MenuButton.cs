using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButton : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("lvl-0-0");
    }

    public void Option()
    {
        Debug.Log("Option button clicked");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
