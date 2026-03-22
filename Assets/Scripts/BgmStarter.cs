using System;
using UnityEngine;

public class BgmStarter : MonoBehaviour
{
    public enum MusicEnum { MainMenu, LevelSelector, Level1, Level2, Level3, Level4, Level5, Level6, Level7, Level8, Level9, Level10, Level11, Level12, Credit }
    public MusicEnum music;

    private void Start()
    {
        switch (music)
        {
            case MusicEnum.MainMenu:
                AudioManager.Instance.PlayBGM("MainMenu");
                break;

            case MusicEnum.LevelSelector:
                AudioManager.Instance.PlayBGM("LevelSelector");
                break;

            case MusicEnum.Level1:
                AudioManager.Instance.PlayBGM("Level1");
                break;

            case MusicEnum.Level2:
                AudioManager.Instance.PlayBGM("Level2");
                break;

            case MusicEnum.Level3:
                AudioManager.Instance.PlayBGM("Level3");
                break;

            case MusicEnum.Level4:
                AudioManager.Instance.PlayBGM("Level4");
                break;

            case MusicEnum.Level5:
                AudioManager.Instance.PlayBGM("Level5");
                break;

            case MusicEnum.Level6:
                AudioManager.Instance.PlayBGM("Level6");
                break;

            case MusicEnum.Level7:
                AudioManager.Instance.PlayBGM("Level7");
                break;

            case MusicEnum.Level8:
                AudioManager.Instance.PlayBGM("Level8");
                break;

            case MusicEnum.Level9:
                AudioManager.Instance.PlayBGM("Level9");
                break;

            case MusicEnum.Level10:
                AudioManager.Instance.PlayBGM("Level10");
                break;

            case MusicEnum.Level11:
                AudioManager.Instance.PlayBGM("Level11");
                break;

            case MusicEnum.Level12:
                AudioManager.Instance.PlayBGM("Level12");
                break;

            case MusicEnum.Credit:
                AudioManager.Instance.PlayBGM("Credit");
                break;
        }
    }
}
