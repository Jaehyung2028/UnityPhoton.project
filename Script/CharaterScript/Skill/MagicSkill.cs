using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class MagicSkill : MonoBehaviour
{
    [Header("스킬 종류")]
    [SerializeField] bool R, T;

    [SerializeField] PhotonView PV;
    [SerializeField] Collider Coll;
    string Team;
    float Damage;
    // Start is called before the first frame update
    void Start()
    {
        Team = PV.Owner.CustomProperties["IsTeam"].ToString();

        Damage = R? GameDataManager.Instance.CharacterData.Name["Magic"].Damage * GameDataManager.Instance.CharacterData.Name["Magic"].Skill_Damage[2]
            : GameDataManager.Instance.CharacterData.Name["Magic"].Damage * GameDataManager.Instance.CharacterData.Name["Magic"].Skill_Damage[3];

        StartCoroutine(_Skill());
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.tag == "Player" && other.gameObject.GetComponent<PlayerBace>().PV.IsMine && !PV.IsMine)
            other.gameObject.GetComponent<PlayerBace>().PV.RPC("DatageHit", RpcTarget.All, Damage, Team);
        else if (other.tag == "Tower" && PV.IsMine && other.gameObject.GetComponent<TowerStat>().HP > 0)
            other.gameObject.GetComponent<TowerStat>().PV.RPC("TowerHit", RpcTarget.All, Damage, Team);

    }

    private void Update() { if (R) transform.Translate(0, 0, 3 * Time.deltaTime); }

    IEnumerator _Skill()
    {
        int Count = 0;

        if (R)
        {
            while(Count != 10)
            {
                Coll.enabled = true;
                yield return new WaitForSeconds(0.1f);
                Coll.enabled = false;
                Count++;

                yield return new WaitForSeconds(0.5f);
            }
        }
        else
        {
            while (Count != 10)
            {
                Coll.enabled = true;
                yield return new WaitForSeconds(0.1f);
                Coll.enabled = false;
                Count++;

                yield return new WaitForSeconds(1f);
            }
        }

        if(PV.IsMine)
        PhotonNetwork.Destroy(gameObject);
    }
}
