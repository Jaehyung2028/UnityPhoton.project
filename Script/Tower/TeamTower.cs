using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamTower : MonoBehaviour
{
    [SerializeField] Transform RedPos, BluePos;
    [SerializeField] bool IsRed;

    public void _TowerInstance()
    {
        if(PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            GameObject _Tower = new GameObject();
            if (IsRed)
                _Tower = PhotonNetwork.InstantiateRoomObject("Tower/RedTower", RedPos.position, RedPos.rotation);
            else
                _Tower = PhotonNetwork.InstantiateRoomObject("Tower/BlueTower", BluePos.position, BluePos.rotation);
        }
    }
}
