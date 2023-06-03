using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhoTonNetworkManager_singleton : MonoBehaviour
{
    [SerializeField] GameObject PhoTonNetworkManager_Obj;

    //NetworkManager ΩÃ±€≈Ê ∆–≈œ ¥Î√º
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
