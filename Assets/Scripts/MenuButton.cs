using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButton : MonoBehaviour
{
    public void StartGame()
    {
        LevelLoader.instance.LoadLevel(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void Option()
    {
        Debug.Log("WIP");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
