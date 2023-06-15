using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamTower : MonoBehaviour
{
    [SerializeField] Transform RedPos, BluePos;
    [SerializeField] bool IsRed;

    //��� �÷��̾ �� ���� ������ ������ Ŭ���̾�Ʈ�� �����Ʈ��ũ �������Ʈ�� Ÿ���� ����
    public void _TowerInstance()
    {
        if(PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            GameObject _Tower = IsRed? PhotonNetwork.InstantiateRoomObject("Tower/RedTower", RedPos.position, RedPos.rotation) 
                : PhotonNetwork.InstantiateRoomObject("Tower/BlueTower", BluePos.position, BluePos.rotation);
        }
    }
}
