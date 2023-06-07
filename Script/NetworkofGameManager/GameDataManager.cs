using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class Character
{
    public Dictionary<string, DataLIst> Name = new Dictionary<string, DataLIst>();
}
public class DataLIst
{
    public float HP;
    public float Speed;
    public float Damage;
    public List<float> Cooldown_time = new List<float> ();
    public List<float> Skill_Damage = new List<float> ();

    public DataLIst(float _HP, float _Speed, float _Damage, float[] _Time, float[] _Skill_Damage)
    {
        HP = _HP; Speed = _Speed; Damage = _Damage;

        for (int i = 0; i < _Time.Length; i++) Cooldown_time.Add(_Time[i]);
        for (int i = 0; i < _Skill_Damage.Length; i++) Skill_Damage.Add(_Skill_Damage[i]);
    }
}

public class GameDataManager : MonoBehaviourPunCallbacks
{
    public static GameDataManager Instance;
    [Header("타워 스크립트")]
    [SerializeField] TeamTower RedTower, BlueTower;

    [Header("화면 UI")]
    public GameObject SelectTool, SkillTool, StatTool;
    [SerializeField] Image[] Skill;
    public Image[] Cooldown_time_Image;
    public Image HP, LerpHP;
    public Text WinText;

    [Header("플레이어 생성 위치")]
    [SerializeField] GameObject MyTeam, Red, Blue;

    GameObject IsMineObj;
    public string SelectCharaterName;
    bool Aready = false;

    public Character CharacterData = new Character();

    private void InData()
    {
        CharacterData.Name.Add("Warrior",  new DataLIst(200, 4, 30, new float[] { 15, 10, 20, 20 }, new float[] { 1.5f, 2f, 2f, 0f }));
        CharacterData.Name.Add("Fighter",  new DataLIst(150, 5, 20, new float[] { 10, 15, 15, 20 }, new float[] { 0f, 2f, 2.5f, 1f }));
        CharacterData.Name.Add("Magic",    new DataLIst(100, 4, 25, new float[] { 20, 15, 15, 20 }, new float[] { 0f, 2f, 1.5f, 1f }));
        CharacterData.Name.Add("Assassin", new DataLIst(100, 6, 20, new float[] { 20, 10, 15, 10 }, new float[] { 0f, 1.5f, 0.1f, 2f }));
    }


    private void Awake()
    {
        Instance = this;

        InData();

        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "SceneReady", "true" } });

        StartCoroutine(AllPlayerReady_Check());

        MyTeam = PhotonNetwork.LocalPlayer.CustomProperties["IsTeam"].ToString() == "Red" ? Red : Blue;
    }

    private void Update()
    {
        if (NetworkManager.Instance.GameChat.activeSelf == false && Input.GetKeyDown(KeyCode.Return))
        {
            NetworkManager.Instance.GameChat.SetActive(true);
            NetworkManager.Instance.GameChatInput.ActivateInputField();
        }
        else if (NetworkManager.Instance.GameChat.activeSelf == true && Input.GetKeyDown(KeyCode.Return))
        {
            if (NetworkManager.Instance.GameChatInput.text != "")
                NetworkManager.Instance.GameSend();

                NetworkManager.Instance.GameChat.SetActive(false);
        }
    }

    public void CharacterInstanceButton()
    {
        if (SelectCharaterName != "")
        {
            IsMineObj = PhotonNetwork.Instantiate(SelectCharaterName, MyTeam.transform.position, MyTeam.transform.rotation);

            Camera.main.transform.parent = IsMineObj.transform.Find("CameraPoint");
            Camera.main.transform.localPosition = Vector3.zero;
            Camera.main.transform.localRotation = Quaternion.Euler(0, 0, 0);

            for (int i = 0; i < Skill.Length; i++)
            {
                Skill[i].sprite = IsMineObj.GetComponent<PlayerBace>().Skill[i];
                Cooldown_time_Image[i].fillAmount = 0;
            }

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            StatTool.SetActive(true);
            SkillTool.SetActive(true);
            SelectTool.SetActive(false);
        }
    }

    IEnumerator AllPlayerReady_Check()
    {
        WinText.text = "다른 플레이어 기다리는 중...";

        while(!Aready)
            yield return new WaitForSeconds(1f);

        if (PhotonNetwork.LocalPlayer.IsMasterClient) { RedTower._TowerInstance(); BlueTower._TowerInstance(); }

        WinText.text = "잠시후 게임이 시작됩니다.";
        yield return new WaitForSeconds(5f);

        WinText.text = "";
        SelectTool.SetActive(true);
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (changedProps.ContainsKey("SceneReady"))
        {
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                if (PhotonNetwork.PlayerList[i].CustomProperties["SceneReady"].ToString() == "false")
                    return;

            Aready = true;
        }
    }
}
