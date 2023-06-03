using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PhotionInstance : MonoBehaviour
{
    [SerializeField] GameObject _Potion , Pos;

    float CurTime = 0;


    // ¸Ê Æ÷¼Ç »ý¼º
    private void Update()
    {
        if (PhotonNetwork.LocalPlayer.IsMasterClient && _Potion == null)
        {
            CurTime += Time.deltaTime;

            if (CurTime >= 10)
            {
                string _PhotonName = null;
                int _Number = Random.Range(0, 3);

                switch (_Number)
                {
                    case 0:
                        _PhotonName = "_A";
                        break;
                    case 1:
                        _PhotonName = "_B";
                        break;
                    case 2:
                        _PhotonName = "_C";
                        break;
                    default:
                        break;
                }

                _Potion = PhotonNetwork.InstantiateRoomObject(_PhotonName, Pos.transform.position, Quaternion.identity);

                CurTime = 0;
            }
        }
    }
}
