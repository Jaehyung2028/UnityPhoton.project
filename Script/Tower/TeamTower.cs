using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamTower : MonoBehaviour
{
    [SerializeField] Transform RedPos, BluePos;
    [SerializeField] bool IsRed;

    //모든 플레이어가 씬 진입 성공시 마스터 클라이언트가 포톤네트워크 룸오브젝트로 타워를 생성
    public void _TowerInstance()
    {
        if(PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            GameObject _Tower = IsRed? PhotonNetwork.InstantiateRoomObject("Tower/RedTower", RedPos.position, RedPos.rotation) 
                : PhotonNetwork.InstantiateRoomObject("Tower/BlueTower", BluePos.position, BluePos.rotation);
        }
    }
}
