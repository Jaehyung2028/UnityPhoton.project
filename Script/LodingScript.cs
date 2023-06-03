using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LodingScript : MonoBehaviourPunCallbacks
{
    AsyncOperation MainSceneLoad;
    [SerializeField] GameObject[] Red, Blue;
    [SerializeField] Text CountText;
    bool AllReady = false;

    private void Awake() { PlayerList(); StartCoroutine(MainScene()); }

    // �ε� ȭ�鿡 �÷��̾��� �̸��� ǥ��
    void PlayerList()
    {
        NetworkManager.Instance.TilteCanvas.SetActive(false);

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if(PhotonNetwork.PlayerList[i].CustomProperties["IsTeam"].ToString() == "Red")
            {
                for (int j = 0; j < Red.Length; j++)
                {
                    if(Red[j].transform.GetChild(0).GetComponent<Text>().text == "")
                    {
                        Red[j].transform.GetChild(0).GetComponent<Text>().text = PhotonNetwork.PlayerList[i].NickName;
                        Red[j].SetActive(true);
                        break;
                    }
                }
            }
            else
            {
                for (int j = 0; j < Blue.Length; j++)
                {
                    if (Blue[j].transform.GetChild(0).GetComponent<Text>().text == "")
                    {
                        Blue[j].transform.GetChild(0).GetComponent<Text>().text = PhotonNetwork.PlayerList[i].NickName;
                        Blue[j].SetActive(true);
                        break;
                    }
                }
            }
        }
    }

    // �� �÷��̾��� ���ξ� �ε� ���¿� ���� Ŀ���� ������Ƽ ����
    IEnumerator MainScene()
    {
        MainSceneLoad = SceneManager.LoadSceneAsync("Main");
        MainSceneLoad.allowSceneActivation = false;

        while(MainSceneLoad.progress < 0.9f)
            yield return null;

        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "SceneReady", "true" } });

        while (!AllReady)
            yield return new WaitForSeconds(1f);

        int Count = 5;
        while(Count >= 0)
        {
            CountText.text = Count + "�� �� �����մϴ�.";
            Count--;
            yield return new WaitForSeconds(1f);
        }

        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "SceneReady", "false" } });
        yield return new WaitForSeconds(1f);
        MainSceneLoad.allowSceneActivation = true;

    }

    // ��� �÷��̾��� �� �ε� ���°� �Ϸ� �Ǿ����� �� ��ȯ
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (changedProps.ContainsKey("SceneReady") && changedProps["SceneReady"].ToString() == "true")
        {
            for (int i = 0; i < Red.Length; i++)
            {
                if (targetPlayer.NickName == Red[i].transform.GetChild(0).GetComponent<Text>().text)
                {
                    Red[i].transform.GetChild(1).gameObject.SetActive(true);
                    break;
                }
                else if(targetPlayer.NickName == Blue[i].transform.GetChild(0).GetComponent<Text>().text)
                {
                    Blue[i].transform.GetChild(1).gameObject.SetActive(true);
                    break;
                }
            }

            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                if (PhotonNetwork.PlayerList[i].CustomProperties["SceneReady"].ToString() == "false")
                    return;

                    AllReady = true;
        }
    }
}
