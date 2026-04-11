using UnityEngine;

public class EndCredit : MonoBehaviour
{
    public void GoBackToMainMenu()
    {
        LevelLoader.instance.LoadLevel("MainMenu");
    }
}
