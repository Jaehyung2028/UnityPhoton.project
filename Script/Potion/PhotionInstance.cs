using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PhotionInstance : MonoBehaviour
{
    [SerializeField] GameObject _Potion , Pos;

    float CurTime = 0;


    // 마스터 클라이언트에의해 맵의 포션이 룸오브젝트로 생성되고 이러한 오브젝트는 마스터 클라이언트에서 관리
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
