using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.AI;

public class MagicScript : PlayerBace
{
    [Header("공격 콜라이더")]
    [SerializeField] BoxCollider AttackCollider;
    [SerializeField] SphereCollider ECollider;

    [Header("공격 오브젝트 생성위치")]
    [SerializeField] GameObject Rpos;

    [Header("공격 이펙트")]
    [SerializeField] ParticleSystem AttackEffect, QEfect, EEfect;

    [PunRPC]
    void Teleport(Vector3 Point)
    {
        gameObject.transform.position = Point;
    }

    [PunRPC]
    protected override IEnumerator Qrutin()
    {
        Vector3 Point;

        Rd.velocity = Vector3.zero;
        delay = true;
        Anime.SetTrigger("Q");

        yield return new WaitForSeconds(0.3f);
        QEfect.Play();

        if (RandomPoint(gameObject.transform.position, 20, out Point) && PV.IsMine)
        {
            PV.RPC("Teleport", RpcTarget.All, Point);
        }

        yield return new WaitForSeconds(1.5f);

        delay = false;
    }
    [PunRPC]
    protected override IEnumerator Erutin()
    {
        Rd.velocity = Vector3.zero;
        delay = true;
        Anime.SetTrigger("E");

        yield return new WaitForSeconds(1.3f);
        EEfect.Play();

        if (!PV.IsMine)
        {
            Collider[] _Player = Physics.OverlapSphere(ECollider.transform.position, ECollider.gameObject.GetComponent<SphereCollider>().radius);

            for (int i = 0; i < _Player.Length; i++)
            {
                if (_Player[i].gameObject.tag == "Player" && _Player[i].gameObject.GetComponent<PlayerBace>().Rd != Rd)
                    if (_Player[i].gameObject.GetComponent<PlayerBace>().Team != Team)
                        _Player[i].gameObject.GetComponent<PlayerBace>().PV.RPC("PlayerStun", RpcTarget.All, 1f);
            }
        }
        yield return new WaitForSeconds(0.9f);

        delay = false;
    }
    [PunRPC]
    protected override IEnumerator Rrutin()
    {
        Rd.velocity = Vector3.zero;
        delay = true;
        Anime.SetTrigger("R");

        yield return new WaitForSeconds(1.5f);

        if (PV.IsMine)
            PhotonNetwork.Instantiate("MagicParticle/MagicR", Rpos.transform.position, Rpos.transform.rotation);

        yield return new WaitForSeconds(1.2f);

        delay = false;
    }
    [PunRPC]
    protected override IEnumerator Trutin()
    {
        Rd.velocity = Vector3.zero;
        delay = true;
        Anime.SetTrigger("T");

        yield return new WaitForSeconds(0.5f);

        if (PV.IsMine)
            PhotonNetwork.Instantiate("MagicParticle/MagicT", ECollider.gameObject.transform.position, ECollider.gameObject.transform.rotation);

        yield return new WaitForSeconds(1.5f);

        delay = false;

    }

    public override IEnumerator AttackColliderControl(string _Name)
    {
        switch (_Name)
        {
            case "N":

                AttackEffect.Play();
                AttackCollider.enabled = true;

                yield return new WaitForSeconds(0.1f);

                AttackCollider.enabled = false;

                break;

            case "E":

                Damage *= GameDataManager.Instance.CharacterData.Name["Magic"].Skill_Damage[1];

                ECollider.enabled = true;

                yield return new WaitForSeconds(0.1f);

                ECollider.enabled = false;

                Damage /= GameDataManager.Instance.CharacterData.Name["Magic"].Skill_Damage[1];

                break;
        }
    }

    protected bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 randomPoint = center + Random.insideUnitSphere * range;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 5.0f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }
        result = Vector3.zero;
        return false;
    }
}

