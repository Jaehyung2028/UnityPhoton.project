using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhoTonNetworkManager_singleton : MonoBehaviour
{
    [SerializeField] GameObject PhoTonNetworkManager_Obj;

    //����� ������Ʈ�� ���� ������Ʈ�� �Ϲ����� �̱��� ���� ����� �Ұ��� ������ ����
    //NetworkManager�� �θ� ������Ʈ�� Ȱ���Ͽ� �̱��� ���ϰ� �����ϰ� ����
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
