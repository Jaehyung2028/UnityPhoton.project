using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Potion : MonoBehaviour
{
    [SerializeField] int HealValue;
    [SerializeField] PhotonView PV;

    //포션 흭득시 플레이어의 힐 함수를 실행하고 _Destroy RPC를 통해 마스터 클라이언트가 포션을 포톤네트워크에서 삭제
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && other.gameObject.GetComponent<PlayerBace>().PV.IsMine)
        {
            other.gameObject.GetComponent<PlayerBace>().Heel(HealValue);
            PV.RPC("_Destroy", RpcTarget.All);
        }
    }

    private void Update() => gameObject.transform.Rotate(0, 10 * Time.deltaTime, 0, Space.World);

    [PunRPC]
    void _Destroy() { if (PhotonNetwork.LocalPlayer.IsMasterClient) PhotonNetwork.Destroy(gameObject); }

}
