using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
    public static ButtonManager instance;

    [Header("¿É¼Ç UI")]
    public GameObject OptionTool;
    [SerializeField] Dropdown ScreenOption;
    [SerializeField] Toggle FullScreen;

    private void Awake() => instance = this;

    public void OptionOpen() { if (OptionTool.activeSelf == true) OptionTool.SetActive(false); else OptionTool.SetActive(true); }
    public void GameExit() => Application.Quit();

    public void ScreenChage()
    {
        switch (ScreenOption.value)
        {
            case 0:
                Screen.SetResolution(1920, 1080, FullScreen.isOn);
                break;
            case 1:
                Screen.SetResolution(1120, 630, FullScreen.isOn);
                break;
            case 2:
                Screen.SetResolution(640, 360, FullScreen.isOn);
                break;
            default:
                break;
        }
    }

    public void _FullScreenChager() => Screen.fullScreen = FullScreen.isOn ? true : false;
}
