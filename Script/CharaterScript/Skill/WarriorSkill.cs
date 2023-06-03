using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarriorSkill : MonoBehaviour
{
    [SerializeField] PhotonView PV;
    [SerializeField] BoxCollider AttackColl;
    string Team;
    float Damage;
    // Start is called before the first frame update
    void Start()
    {
        Team = PV.Owner.CustomProperties["IsTeam"].ToString();

        Damage = GameDataManager.Instance.CharacterData.Name["Warrior"].Damage * GameDataManager.Instance.CharacterData.Name["Warrior"].Skill_Damage[2];

        StartCoroutine(_Skill());
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.tag == "Player" && other.gameObject.GetComponent<PlayerBace>().PV.IsMine && !PV.IsMine)
            other.gameObject.GetComponent<PlayerBace>().PV.RPC("DatageHit", RpcTarget.All, Damage, Team);
        else if (other.tag == "Tower" && PV.IsMine && other.gameObject.GetComponent<TowerStat>().HP > 0)
            other.gameObject.GetComponent<TowerStat>().PV.RPC("TowerHit", RpcTarget.All, Damage, Team);

    }

    IEnumerator _Skill()
    {
        yield return new WaitForSeconds(0.3f);

        AttackColl.enabled = true;
        yield return new WaitForSeconds(0.1f);
        AttackColl.enabled = false;

        yield return new WaitForSeconds(1.7f);

        AttackColl.enabled = true;
        yield return new WaitForSeconds(0.1f);
        AttackColl.enabled = false;

        yield return new WaitForSeconds(0.8f);

        if (PV.IsMine)
            PhotonNetwork.Destroy(gameObject);
    }
}
