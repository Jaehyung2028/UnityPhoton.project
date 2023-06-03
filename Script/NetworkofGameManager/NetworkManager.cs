using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Pun.UtilityScripts;
using Unity.VisualScripting;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance;

    [Header("Ÿ��Ʋȭ��")]
    public GameObject TilteCanvas, TitleObj;
    public InputField PlayerNickName;

    [Header("�κ� ȭ��")]
    public GameObject SelectRoomObj, RoomListObj;
    public InputField CreatRoomName;
    public int TeamNumber;
    [SerializeField] Text InPlayerNickName, LobbyPlayerCountText, FaildText;
    [SerializeField] Text[] Team;
    [SerializeField] Button[] RoomListButton;
    [SerializeField] Button PreviousButton, NextButton;

    [Header("�� ȭ��")]
    public GameObject InRoomObj;
    [SerializeField] InputField ChatInput;
    [SerializeField] Text[] ChatTextList;
    [SerializeField] Text InRoomName, InRoomReadyorStart;

    [Header("���� ȭ��")]
    public GameObject GameChat;
    public InputField GameChatInput;
    public Text[] GameChatTextList;

    List<RoomInfo> RoomList = new List<RoomInfo>();
    int currentPage = 1, maxPage, multiple;
    public PhotonView PV;

    private void Awake()
    {
        if (Instance == null)
        {
           Instance = this;
           DontDestroyOnLoad(this.gameObject);
        }

        Screen.SetResolution(640, 360, false);

        PhotonNetwork.AutomaticallySyncScene = true;
    }
    private void Update()
    {
        LobbyPlayerCountText.text = "������ : " + PhotonNetwork.CountOfPlayers.ToString() + " ���� : " + (PhotonNetwork.CountOfPlayers - PhotonNetwork.CountOfPlayersInRooms).ToString();

        if (PhotonNetwork.InRoom && ChatInput.text != "" && Input.GetKeyDown(KeyCode.Return)) { Send(); ChatInput.ActivateInputField(); }
    }

    #region ��ư
    public void JoinButton()
    {
        if(PlayerNickName.text != "") PhotonNetwork.ConnectUsingSettings();
        else StartCoroutine("FailedText", "�г����� �Է��ϼ���.");
    }
    public void ExitButton()
    {
        ButtonManager.instance.OptionTool.SetActive(false);
        PhotonNetwork.Disconnect();
    }

    public void CreatRoomButton()
    {
        if(CreatRoomName.text != "") PhotonNetwork.CreateRoom(CreatRoomName.text, new RoomOptions { MaxPlayers = 6 });
        else StartCoroutine("FailedText", "���̸��� �Է��ϼ���.");
    }
    public void RandomJoinRoomButton() => PhotonNetwork.JoinRandomRoom();

    public void RoomExitButton() => PhotonNetwork.LeaveRoom();

    public void ReadyButton()
    {
        if (!PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            string _PlayerReady;

            _PlayerReady = PhotonNetwork.LocalPlayer.CustomProperties["IsReady"].ToString() == "true" ?
                "false" : "true";

            PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable() { { "IsReady", _PlayerReady } });
        }
        else
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount > 1)
            {
                int _BlueTeam = 0, _RedTeam = 0;

                for (int i = 0; i < PhotonNetwork.PlayerListOthers.Length; i++)
                {
                    if (PhotonNetwork.PlayerListOthers[i].CustomProperties["IsReady"].ToString() == "false")
                    {
                        StartCoroutine("FailedText", "��� �÷��̾ �غ���� �ʾҽ��ϴ�.");
                        return;
                    }
                    else
                    {
                        if (PhotonNetwork.PlayerListOthers[i].CustomProperties["IsTeam"].ToString() == "Red") _RedTeam++;
                        else _BlueTeam++;
                    }
                }

                if (PhotonNetwork.LocalPlayer.CustomProperties["IsTeam"].ToString() == "Red") _RedTeam++;
                else _BlueTeam++;

                if (_BlueTeam == _RedTeam)
                {
                    for (int i = 0; i < ChatTextList.Length; i++) ChatTextList[i].text = "";
                    PhotonNetwork.CurrentRoom.IsOpen = false;
                    PhotonNetwork.LoadLevel("Loding");
                }
                else
                    StartCoroutine("FailedText", "���� �ο��� �����ʽ��ϴ�.");
            }
            else
                StartCoroutine("FailedText", "2�� �̻� ���� ���۰����մϴ�.");
        }
    }
    public void TeamChageButton()
    {
            if (PhotonNetwork.LocalPlayer.CustomProperties["IsTeam"].ToString() == "Blue")
            {
                for (int i = 0; i < 3; i++)
                {
                    if (Team[i].text == "")
                    {
                        PV.RPC("TeamChage", RpcTarget.All, PhotonNetwork.LocalPlayer.GetPlayerNumber());

                        Team[i].text = PhotonNetwork.LocalPlayer.NickName; PhotonNetwork.LocalPlayer.SetPlayerNumber(i);

                        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable() { { "IsTeam", "Red" } });

                        Team[i].color = PhotonNetwork.LocalPlayer.CustomProperties["IsReady"].ToString() == "true" ?
                        Color.green : Color.white;

                        if (PhotonNetwork.LocalPlayer.IsMasterClient)
                            Team[i].transform.GetChild(0).gameObject.SetActive(true);

                        PV.RPC("RoomPlayerNumber", RpcTarget.Others, null);

                        return;
                    }
                }
            }
            else if (PhotonNetwork.LocalPlayer.CustomProperties["IsTeam"].ToString() == "Red")
            {
                for (int i = 3; i < 6; i++)
                {
                    if (Team[i].text == "")
                    {
                        PV.RPC("TeamChage", RpcTarget.All, PhotonNetwork.LocalPlayer.GetPlayerNumber());

                        Team[i].text = PhotonNetwork.LocalPlayer.NickName; PhotonNetwork.LocalPlayer.SetPlayerNumber(i);

                        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable() { { "IsTeam", "Blue" } });

                        Team[i].color = PhotonNetwork.LocalPlayer.CustomProperties["IsReady"].ToString() == "true" ?
                        Color.green : Color.white;

                        if (PhotonNetwork.LocalPlayer.IsMasterClient) Team[i].transform.GetChild(0).gameObject.SetActive(true);

                        PV.RPC("RoomPlayerNumber", RpcTarget.Others, null);


                        return;
                    }
                }
            }
    }
    #endregion

    #region ���� �޼���
    public override void OnCreateRoomFailed(short returnCode, string message) => StartCoroutine("FailedText", "���� �̸��� ���� �����մϴ�.");
    public override void OnJoinRandomFailed(short returnCode, string message) => StartCoroutine("FailedText", "���� ������ ���� �����ϴ�.");
    IEnumerator FailedText(string _Text)
    {
        FaildText.text = _Text;
        yield return new WaitForSeconds(1);
        FaildText.text = "";
    }
    #endregion

    #region ��Ʈ��ũ ���� ����
    public override void OnDisconnected(DisconnectCause cause)
    {
        SelectRoomObj.SetActive(false);
        TitleObj.SetActive(true);
    }
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.LocalPlayer.NickName = PlayerNickName.text;
        PhotonNetwork.JoinLobby();
    }

    #endregion

    #region �κ� ����
    public override void OnJoinedLobby()
    {
        TitleObj.SetActive(false);
        SelectRoomObj.SetActive(true);
        RoomListObj.SetActive(true);
        InPlayerNickName.text = PhotonNetwork.LocalPlayer.NickName;
    }
    #endregion

    #region �� ����
    public override void OnJoinedRoom()
    {
        InRoomObj.SetActive(true);
        RoomListObj.SetActive(false);

        RoomPlayerNumber();

        for (int i = 0; i < Team.Length; i++)
        {
            if (Team[i].text == "")
            {
                Team[i].text = PhotonNetwork.LocalPlayer.NickName; PhotonNetwork.LocalPlayer.SetPlayerNumber(i);

                if (i < 3)
                    PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable() { { "IsTeam", "Red" } });
                else if (i >= 3)
                    PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable() { { "IsTeam", "Blue" } });

                PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable() { { "IsReady", "false" } });
                PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable() { { "SceneReady", "false" } });

                if (PhotonNetwork.LocalPlayer.IsMasterClient) Team[i].transform.GetChild(0).gameObject.SetActive(true);

                break;
            }
        }

        InRoomName.text = PhotonNetwork.CurrentRoom.Name;
        for (int i = 0; i < ChatTextList.Length; i++) ChatTextList[i].text = "";
        ChatRPC("<color=yellow>�濡 �����ϼ̽��ϴ�</color>");

        PV.RPC("RoomPlayerNumber", RpcTarget.Others, null);
    }
    public override void OnLeftRoom()
    {
        for (int i = 0; i < Team.Length; i++) { Team[i].text = ""; Team[i].transform.GetChild(0).gameObject.SetActive(false); }
        InRoomObj.SetActive(false);
    }
    public override void OnPlayerEnteredRoom(Player newPlayer) => ChatRPC("<color=yellow>" + newPlayer.NickName + "���� �����ϼ̽��ϴ�</color>");

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        ChatRPC("<color=yellow>" + otherPlayer.NickName + "���� �����ϼ̽��ϴ�</color>");
        Team[otherPlayer.GetPlayerNumber()].text = "";
        PV.RPC("RoomPlayerNumber", RpcTarget.All);
    }

    [PunRPC]
    void RoomPlayerNumber()
    {

        for (int i = 0; i < PhotonNetwork.PlayerListOthers.Length; i++)
        {
            Team[PhotonNetwork.PlayerListOthers[i].GetPlayerNumber()].text = PhotonNetwork.PlayerListOthers[i].NickName;

            Team[PhotonNetwork.PlayerListOthers[i].GetPlayerNumber()].color =
                PhotonNetwork.PlayerListOthers[i].CustomProperties["IsReady"].ToString() == "true" ?
                Color.green : Color.white;

            if (PhotonNetwork.PlayerListOthers[i].IsMasterClient)
                Team[PhotonNetwork.PlayerListOthers[i].GetPlayerNumber()].transform.GetChild(0).gameObject.SetActive(true);
            else
                Team[PhotonNetwork.PlayerListOthers[i].GetPlayerNumber()].transform.GetChild(0).gameObject.SetActive(false);
        }

        if (PhotonNetwork.LocalPlayer.IsMasterClient) InRoomReadyorStart.text = "Start";
        else InRoomReadyorStart.text = "Ready";

    }

    [PunRPC]
    void TeamChage(int _Number)
    {
        Team[_Number].color = Color.white;
        Team[_Number].text = "";
        Team[_Number].transform.GetChild(0).gameObject.SetActive(false);
    }
    #endregion

    #region �÷��̾� ������Ƽ
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (changedProps.ContainsKey("IsReady"))
        {
            Team[targetPlayer.GetPlayerNumber()].color = targetPlayer.CustomProperties["IsReady"].ToString() == "true" ?
                Color.green : Color.white;
        }
    }
    #endregion

    #region #�渮��Ʈ ����
    public void RoomListClick(int num)
    {
        if (num == -2) --currentPage;
        else if (num == -1) ++currentPage;
        else PhotonNetwork.JoinRoom(RoomList[multiple + num].Name);
        RoomListRenewal();
    }

    void RoomListRenewal()
    {
        
        maxPage = (RoomList.Count % RoomListButton.Length == 0) ? RoomList.Count / RoomListButton.Length : RoomList.Count / RoomListButton.Length + 1;

        PreviousButton.interactable = (currentPage <= 1) ? false : true;
        NextButton.interactable = (currentPage >= maxPage) ? false : true;
        
        multiple = (currentPage - 1) * RoomListButton.Length;

        for (int i = 0; i < RoomListButton.Length; i++)
        {
            RoomListButton[i].interactable = (multiple + i < RoomList.Count && RoomList[multiple + i].IsOpen) ? true : false;
            RoomListButton[i].transform.GetChild(0).GetComponent<Text>().text = (multiple + i < RoomList.Count) ? RoomList[multiple + i].Name : "";
            RoomListButton[i].transform.GetChild(1).GetComponent<Text>().text = (multiple + i < RoomList.Count) ? RoomList[multiple + i].PlayerCount + "/" + RoomList[multiple + i].MaxPlayers : "";
        }
    }
    
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
         for (int i = 0; i < roomList.Count; i++)
         {
            if (!roomList[i].RemovedFromList)
            {
                if (!RoomList.Contains(roomList[i])) RoomList.Add(roomList[i]);
                else
                    RoomList[RoomList.IndexOf(roomList[i])] = roomList[i];
            }
            else if (RoomList.Contains(roomList[i])) RoomList.RemoveAt(RoomList.IndexOf(roomList[i]));
         }
         RoomListRenewal();
    }
    #endregion

    #region #ä�� ����
    public void Send()
    {       
        PV.RPC("ChatRPC", RpcTarget.All, PhotonNetwork.NickName + " : " + ChatInput.text);
        ChatInput.text = "";
    }
    public void GameSend()
    {
        PV.RPC("GameChatRPC", RpcTarget.All, PhotonNetwork.NickName + " : " + GameChatInput.text);
        GameChatInput.text = "";
    }
    
    [PunRPC]
    void ChatRPC(string msg)
    {

        for (int i = 0; i < ChatTextList.Length; i++)
        {
            if (ChatTextList[i].text == "")
            {
                ChatTextList[i].text = msg;
                return;
            }
        }

        for (int j = 1; j < ChatTextList.Length; j++) ChatTextList[j - 1].text = ChatTextList[j].text;
        ChatTextList[ChatTextList.Length - 1].text = msg;
    }

    [PunRPC]
    void GameChatRPC(string msg)
    {

        for (int i = 0; i < GameChatTextList.Length; i++)
        {
            if (GameChatTextList[i].text == "")
            {
                GameChatTextList[i].text = msg;
                return;
            }
        }

        for (int j = 1; j < GameChatTextList.Length; j++) GameChatTextList[j - 1].text = GameChatTextList[j].text;
        GameChatTextList[ChatTextList.Length - 1].text = msg;
    }
    #endregion

    [PunRPC]
    public void GameOver()
    {
        for (int i = 0; i < GameChatTextList.Length; i++) GameChatTextList[i].text = "";
        TilteCanvas.SetActive(true);
    }
}
