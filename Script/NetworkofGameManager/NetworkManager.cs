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

    [Header("타이틀화면")]
    public GameObject TilteCanvas, TitleObj;
    public InputField PlayerNickName;

    [Header("로비 화면")]
    public GameObject SelectRoomObj, RoomListObj;
    public InputField CreatRoomName;
    public int TeamNumber;
    [SerializeField] Text InPlayerNickName, LobbyPlayerCountText, FaildText;
    [SerializeField] Text[] Team;
    [SerializeField] Button[] RoomListButton;
    [SerializeField] Button PreviousButton, NextButton;

    [Header("룸 화면")]
    public GameObject InRoomObj;
    [SerializeField] InputField ChatInput;
    [SerializeField] Text[] ChatTextList;
    [SerializeField] Text InRoomName, InRoomReadyorStart;

    [Header("게임 화면")]
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
        LobbyPlayerCountText.text = "접속자 : " + PhotonNetwork.CountOfPlayers.ToString() + " 대기실 : " + (PhotonNetwork.CountOfPlayers - PhotonNetwork.CountOfPlayersInRooms).ToString();

        if (PhotonNetwork.InRoom && ChatInput.text != "" && Input.GetKeyDown(KeyCode.Return)) { Send(); ChatInput.ActivateInputField(); }
    }

    #region 버튼
    
    //닉네임 입력 후 서버의 접속기능을 하는 버튼
    public void JoinButton()
    {
        if(PlayerNickName.text != "") PhotonNetwork.ConnectUsingSettings();
        else StartCoroutine("FailedText", "닉네임을 입력하세요.");
    }
    //룸 입장 상태에서 다시 타이틀 화면으로 돌아가는 기능을 하는 버튼
    public void ExitButton()
    {
        ButtonManager.instance.OptionTool.SetActive(false);
        PhotonNetwork.Disconnect();
    }

    // 룸 생성을 위한 기능을 하는 버튼
    public void CreatRoomButton()
    {
        if(CreatRoomName.text != "") PhotonNetwork.CreateRoom(CreatRoomName.text, new RoomOptions { MaxPlayers = 6 });
        else StartCoroutine("FailedText", "방이름을 입력하세요.");
    }
    //빠른 시작을 통해 랜덤한 룸에 입장하는 기능을 가진 버튼
    public void RandomJoinRoomButton() => PhotonNetwork.JoinRandomRoom();
    //룸을 떠나는 기능을 하는 버튼
    public void RoomExitButton() => PhotonNetwork.LeaveRoom();
    //마스터 클라이언트는 모든 플레이어의 커스텀 프로퍼티를 조사해 IsReady의 키 값이 True , 여러 조건을 만족시 모든 플레이어에게 씬 로딩 전달
    //마스터 클라이어트가 아닌 경우 커스텀 프로퍼티를 활용하여 Ready 상태를 변환
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
                        StartCoroutine("FailedText", "모든 플레이어가 준비되지 않았습니다.");
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
                    PV.RPC("ChatText_Rest", RpcTarget.All);
                    PhotonNetwork.CurrentRoom.IsOpen = false;
                    PhotonNetwork.LoadLevel("Loding");
                }
                else
                    StartCoroutine("FailedText", "팀별 인원이 맞지않습니다.");
            }
            else
                StartCoroutine("FailedText", "2인 이상 부터 시작가능합니다.");
        }
    }
    //팀 체인지 버튼은 커스텀 프로퍼티를 활용하여 Team의 키값을 변경 후 룸에 위치한 플레이어의 위치를 조정
    //PRC를 통해 자신의 상태를 전달하여 동기화를 시킴
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

    #region 실패 메세지
    public override void OnCreateRoomFailed(short returnCode, string message) => StartCoroutine("FailedText", "같은 이름의 방이 존재합니다.");
    public override void OnJoinRandomFailed(short returnCode, string message) => StartCoroutine("FailedText", "참가 가능한 방이 없습니다.");
    IEnumerator FailedText(string _Text)
    {
        FaildText.text = _Text;
        yield return new WaitForSeconds(1);
        FaildText.text = "";
    }
    #endregion

    #region 네트워크 연결 관리
    public override void OnDisconnected(DisconnectCause cause)
    {
        SelectRoomObj.SetActive(false);
        TitleObj.SetActive(true);
    }

    //서버 접속 완료시 콜백 함수를 통해 포톤 서버에 자신의 닉네임을 전달 후 로비에 접속
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.LocalPlayer.NickName = PlayerNickName.text;
        PhotonNetwork.JoinLobby();
    }

    #endregion

    #region 로비 관리
    public override void OnJoinedLobby()
    {
        TitleObj.SetActive(false);
        SelectRoomObj.SetActive(true);
        RoomListObj.SetActive(true);
        InPlayerNickName.text = PhotonNetwork.LocalPlayer.NickName;
    }
    #endregion

    #region 룸 관리

    //룸 접속시 룸안의 플레이어를 조사에 각 넘버에 따른 플레이어 배치 후 자신의 넘버를 찾아 표시
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
        ChatRPC("<color=yellow>방에 참가하셨습니다</color>");

        PV.RPC("RoomPlayerNumber", RpcTarget.Others, null);
    }

    //룸을 퇴장시 콜백함수를 통해 채팅 초기화
    public override void OnLeftRoom()
    {
        for (int i = 0; i < Team.Length; i++) { Team[i].text = ""; Team[i].transform.GetChild(0).gameObject.SetActive(false); }
        InRoomObj.SetActive(false);
    }

    // 룸에 새로운 플레이어 접속시 RPC를 통해 자신의 입장을 채팅을 통해 모든 플레이어에게 알림
    public override void OnPlayerEnteredRoom(Player newPlayer) => ChatRPC("<color=yellow>" + newPlayer.NickName + "님이 참가하셨습니다</color>");

    // 플레이어가 룸을 퇴장시 콜백함수를 통해 플레이어의 위치를 비우고 마스터 클라이언트였을 경우 그에 맞는 조치를 취함
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        ChatRPC("<color=yellow>" + otherPlayer.NickName + "님이 퇴장하셨습니다</color>");
        Team[otherPlayer.GetPlayerNumber()].text = "";
        Team[otherPlayer.GetPlayerNumber()].transform.GetChild(0).gameObject.SetActive(false);
        if(PhotonNetwork.LocalPlayer.IsMasterClient) Team[PhotonNetwork.LocalPlayer.GetPlayerNumber()].transform.GetChild(0).gameObject.SetActive(true);
        PV.RPC("RoomPlayerNumber", RpcTarget.All);
    }

    //룸의 플레이어의 넘버를 넘겨받아 자신 또는 플레이어의 조건에 맞게 상태를 변환
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

    // 플레이어 상태 변환
    [PunRPC]
    void TeamChage(int _Number)
    {
        Team[_Number].color = Color.white;
        Team[_Number].text = "";
        Team[_Number].transform.GetChild(0).gameObject.SetActive(false);
    }
    #endregion

    #region 플레이어 프로퍼티

    // 커스텀 프로퍼티의 콜백함수를 통해 Ready의 값에 따른 플레이어의 상태 변환
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (changedProps.ContainsKey("IsReady"))
        {
            Team[targetPlayer.GetPlayerNumber()].color = targetPlayer.CustomProperties["IsReady"].ToString() == "true" ?
                Color.green : Color.white;
        }
    }
    #endregion

    #region #방리스트 갱신
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

    #region #채팅 구현
    //RPC를 통해 자신의 InpuField의 텍스트를 룸의 플레이어에게 전달
    public void Send()
    {       
        PV.RPC("ChatRPC", RpcTarget.All, PhotonNetwork.NickName + " : " + ChatInput.text);
        ChatInput.text = "";
    }
    //RPC를 통해 자신의 인게임 안의 InpuField의 텍스트를 룸의 플레이어에게 전달
    public void GameSend()
    {
        PV.RPC("GameChatRPC", RpcTarget.All, PhotonNetwork.NickName + " : " + GameChatInput.text);
        GameChatInput.text = "";
    }
    
    //미리 할당되어 있는 Text에 RPC로 전달받은 매개변수를 받아와 할당
    //Text 사용 상태가 모든 완료 되었을 경우 Text의 내용을 한칸씩 끌어올려 재사용
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
    // 인게임의 채팅 Text
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
    //인게임 채팅 Text 초기화
    [PunRPC]
    void ChatText_Rest() { for (int i = 0; i < ChatTextList.Length; i++) ChatTextList[i].text = "";}

    [PunRPC]
    public void GameOver()
    {
        for (int i = 0; i < GameChatTextList.Length; i++) GameChatTextList[i].text = "";
        TilteCanvas.SetActive(true);
    }
}
