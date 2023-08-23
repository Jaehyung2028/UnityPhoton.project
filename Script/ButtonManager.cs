using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
    static public ButtonManager instance;

    [Header("컴포넌트")]
    [SerializeField] TMP_Text LevelText, AlarmText;
    [SerializeField] Image[] IdleImage;
    public TMP_Text ItemText, SoulText;
    [SerializeField] GameObject LodingObj, _Player, Hpprefab, ItemObj, HpObj, UiObj;
    [SerializeField] ParticleSystem Spawn;

    [Space]
    public bool Success = false;
    bool Delay = false;
    int HpCount = 0;
    public int HpLevel = 1;

    private void Awake() => instance = this;

    // 난이도 조절 버튼을 이용하여 텍스트 변경
    public void LevelButton(string _Name)
    {
        switch (_Name)
        {
            case "Up":
                LevelText.text = LevelText.text == "Easy" ? LevelText.text = "Nomar" : LevelText.text = "Hard";
                break;

            case "Down":
                LevelText.text = LevelText.text == "Hard" ? LevelText.text = "Nomar" : LevelText.text = "Easy";
                break;

            default:
                break;
        }
    }

    // 시작 버튼을 누를 경우 난이도의 텍스트에 따라 맵 및 몬스터의 수치가 변경
    public void StartButton()
    {
        switch (LevelText.text)
        {
            case "Easy":
                Map.Instance.RoomSize_X = 40;
                Map.Instance.RoomSize_Y = 20;
                Map.Instance.RoomCount = 20;
                Map.Instance.HiddenCount = 3;
                Map.Instance.MaxMonster = 5;
                HpLevel = 1;
                HpCount = 5;
                break;

            case "Nomar":
                Map.Instance.RoomSize_X = 60;
                Map.Instance.RoomSize_Y = 30;
                Map.Instance.RoomCount = 20;
                Map.Instance.HiddenCount = 5;
                Map.Instance.MaxMonster = 6;
                HpLevel = 2;
                HpCount = 3;
                break;

            case "Hard":
                Map.Instance.RoomSize_X = 60;
                Map.Instance.RoomSize_Y = 30;
                Map.Instance.RoomCount = 30;
                Map.Instance.HiddenCount = 8;
                Map.Instance.MaxMonster = 7;
                HpLevel = 3;
                HpCount = 1;
                break;

            default:
                break;
        }

        StartCoroutine(ImageFadeOut());

    }

    // 선형보간을 이용하여 페이드인, 페이드 아웃 효과를 적용
    // 동시에 맵 스크립트의 맵생성 함수를 실행
    IEnumerator ImageFadeOut()
    {
        Map.Instance.Reset = false;

        UiObj.SetActive(false);
        LodingObj.SetActive(true);

        GameObject _OBJ = Instantiate(_Player, new Vector3(Map.Instance.RoomSize_X / 2 - 0.5f, Map.Instance.RoomSize_Y / 2 - 0.5f, 0), Quaternion.identity);

        Map.Instance.StartTile();

        while (!Success)
            yield return null;

        yield return new WaitForSeconds(3f);

        LodingObj.SetActive(false);

        Color[] color = new Color[IdleImage.Length];

        for (int i = 0; i < IdleImage.Length; i++)
            color[i] = IdleImage[i].color;

        float CurTime = 0;
        float _alpha = 1;

        while (_alpha > 0)
        {
            CurTime += 0.5f * Time.deltaTime;

            _alpha = Mathf.Lerp(1, 0, CurTime);

            for (int i = 0; i < IdleImage.Length; i++)
            {
                color[i].a = _alpha;
                IdleImage[i].color = color[i];
            }

            yield return null;
        }

        ItemObj.SetActive(true);
        HpObj.SetActive(true);

        GameObject _HP = null;

        for (int i = 0; i < HpCount; i++)
        {
            _HP = Instantiate(Hpprefab, Vector2.zero, Quaternion.identity);
            _HP.transform.SetParent(HpObj.transform);
            _HP.transform.localScale = new Vector3(1, 1, 1);
            _OBJ.transform.GetChild(0).GetComponent<Player>().HP_IMAGE.Push(_HP);
        }

        Spawn.gameObject.transform.position = new Vector3(Map.Instance.RoomSize_X / 2 - 0.5f, Map.Instance.RoomSize_Y / 2, 0);

        Spawn.Play();
        StartCoroutine(_OBJ.transform.GetChild(0).GetComponent<Player>().Spawn());
    }

    public IEnumerator ImageFadeIn()
    {
        HpObj.SetActive(false);
        ItemObj.SetActive(false);

        Success = false;

        SoulText.text = "X " + 0;
        ItemText.text = "X " + 0;

        Color[] color = new Color[IdleImage.Length];

        for (int i = 0; i < IdleImage.Length; i++)
        {
            color[i] = IdleImage[i].color;
        }

        float CurTime = 0;
        float _alpha = 0;

        while (_alpha < 1)
        {
            CurTime += 0.25f * Time.deltaTime;

            _alpha = Mathf.Lerp(0, 1, CurTime);

            for (int i = 0; i < IdleImage.Length; i++)
            {
                color[i].a = _alpha;
                IdleImage[i].color = color[i];
            }

            yield return null;
        }

        UiObj.SetActive(true);
    }

    public void Alarm(string Alarm)
    {
        StartCoroutine(AlarmString(Alarm));
    }

    // 상황에 맞는 텍스트를 화면에 띄우기 위한 함수
    IEnumerator AlarmString(string Alarm)
    {
        if (!Delay)
        {
            Delay = true;

            AlarmText.text = Alarm;

            Color TextColor = AlarmText.color;

            float _alpha = 1, CurTime = 0;

            TextColor.a = 1;
            AlarmText.color = TextColor;

            yield return new WaitForSeconds(0.5f);

            while (_alpha > 0)
            {
                CurTime += 0.5f * Time.deltaTime;

                _alpha = Mathf.Lerp(1, 0, CurTime);

                TextColor.a = _alpha;
                AlarmText.color = TextColor;

                yield return null;
            }

            AlarmText.text = "";

            Delay = false;
        }
    }

    // 종료 함수
    public void ExitButton() => Application.Quit();

}
