using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TowerStat : MonoBehaviour
{
    [Header("Å¸¿ö Ã¼·Â")]
    public float MaxHP, HP;
    [SerializeField] GameObject HPBace;
    [SerializeField] Text HPtext;

    [Header("ÆÄ±« ÀÌÆåÆ®")]
    [SerializeField] ParticleSystem OverEffect;
    [SerializeField] GameObject[] OffSkin;

    [Header("Å¸¿ö ÆÀ")]
    public string Team;

    public PhotonView PV;


    private void Awake() 
    { 
        HP = MaxHP; 
        HPtext.color = PhotonNetwork.LocalPlayer.CustomProperties["IsTeam"].ToString() == Team? Color.green : Color.red;
    }

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
                 "<color=blue><size=150>" + "Blue" + "</size></color>" + "ÆÀ ½Â¸®" : "<color=red><size=150>" + "Red" + "</size></color>" + "ÆÀ ½Â¸®";

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
