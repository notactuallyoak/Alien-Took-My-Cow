using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButton : MonoBehaviour
{
    public void StartGame()
    {
        LevelLoader.instance.LoadLevel("LevelSelector");
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
