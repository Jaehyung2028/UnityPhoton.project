using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhoTonNetworkManager_singleton : MonoBehaviour
{
    [SerializeField] GameObject PhoTonNetworkManager_Obj;

    //포톤뷰 컴포넌트를 가진 오브젝트는 일반적인 싱글톤 패턴 사용이 불가한 문제로 인해
    //NetworkManager의 부모 오브젝트를 활용하여 싱글톤 패턴과 유사하게 구현
    private void Awake()
    {
        if(GameObject.Find("PhoTonNetworkManager") != null)
            Destroy(gameObject);
        else
        {
            gameObject.transform.GetChild(0).gameObject.transform.parent = null;
            PhoTonNetworkManager_Obj.SetActive(true);
            Destroy(gameObject);
        }
    }
}
