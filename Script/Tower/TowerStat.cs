using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TowerStat : MonoBehaviour
{
    [Header("타워 체력")]
    public float MaxHP, HP;
    [SerializeField] GameObject HPBace;
    [SerializeField] Text HPtext;

    [Header("파괴 이펙트")]
    [SerializeField] ParticleSystem OverEffect;
    [SerializeField] GameObject[] OffSkin;

    [Header("타워 팀")]
    public string Team;

    public PhotonView PV;


    private void Awake() 
    { 
        HP = MaxHP; 
        HPtext.color = PhotonNetwork.LocalPlayer.CustomProperties["IsTeam"].ToString() == Team? Color.green : Color.red;
    }

    //타워의 피격 판전은 클론이 아닌 자신의 클라이언트에서 룸의 모든 플레이어에게 RPC를 통해 전달
    [PunRPC]
    public IEnumerator TowerHit(float _Damage, string _Team)
    {
        if (_Team != Team)
        {
            HP -= _Damage;

            if (HP <= 0)
            {

                for (int i = 0; i < OffSkin.Length; i++)
                    OffSkin[i].SetActive(false);

                OverEffect.Play();

                GameDataManager.Instance.WinText.text = Team == "Red" ?
                 "<color=blue><size=150>" + "Blue" + "</size></color>" + "팀 승리" : "<color=red><size=150>" + "Red" + "</size></color>" + "팀 승리";

                yield return new WaitForSeconds(10f);

                if (PhotonNetwork.LocalPlayer.IsMasterClient)
                {
                    NetworkManager.Instance.PV.RPC("GameOver", RpcTarget.All);
                    PhotonNetwork.CurrentRoom.IsOpen = true;
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                    PhotonNetwork.LoadLevel("Title");
                }
            }
        }
    }

    private void Update()
    {
        HPBace.transform.rotation = Camera.main.transform.rotation;
        HP = Math.Clamp(HP, 0, MaxHP);
        HPtext.text = HP.ToString();
    }
}
