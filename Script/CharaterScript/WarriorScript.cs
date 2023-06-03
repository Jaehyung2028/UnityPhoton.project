using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class WarriorScript : PlayerBace
{
    [Header("공격 콜라이더")]
    [SerializeField] BoxCollider AttackCollider;
    [SerializeField] SphereCollider QCollider;

    [Header("공격 오브젝트 생성위치")]
    [SerializeField] GameObject RPos;

    [Header("공격 이펙트")]
    [SerializeField] ParticleSystem QEffect, EEffect, TEffect;

    [PunRPC]
    protected override IEnumerator Qrutin()
    {
        Rd.velocity = Vector3.zero;
        delay = true;
        Anime.SetTrigger("Q");

        yield return new WaitForSeconds(0.5f);

        QEffect.Play();

        if (!PV.IsMine)
        {
            Collider[] _Player = Physics.OverlapSphere(QCollider.gameObject.transform.position, QCollider.GetComponent<SphereCollider>().radius);

            for (int i = 0; i < _Player.Length; i++)
            {
                if (_Player[i].gameObject.tag == "Player" && _Player[i].gameObject.GetComponent<PlayerBace>().Rd != Rd)
                {
                    if (_Player[i].gameObject.GetComponent<PlayerBace>().Team != Team)
                    {
                        Vector3 _Distance = _Player[i].gameObject.GetComponent<PlayerBace>().Rd.transform.position - Rd.transform.position;
                        _Player[i].gameObject.GetComponent<PlayerBace>().Rd.AddForce(_Distance * 50, ForceMode.Impulse);
                    }
                }
            }
        }

        yield return new WaitForSeconds(2.5f);

        delay = false;
    }
    [PunRPC]
    protected override IEnumerator Erutin()
    {
        Rd.velocity = Vector3.zero;
        delay = true;
        Anime.SetTrigger("E");

        yield return new WaitForSeconds(0.8f);
        EEffect.Play();

        Collider[] _Player = Physics.OverlapBox(AttackCollider.transform.position, AttackCollider.gameObject.GetComponent<BoxCollider>().size);

        if (!PV.IsMine)
        {
            for (int i = 0; i < _Player.Length; i++)
            {
                if (_Player[i].gameObject.tag == "Player" && _Player[i].gameObject.GetComponent<PlayerBace>().Rd != Rd)
                {
                    if (_Player[i].gameObject.GetComponent<PlayerBace>().Team != Team)
                    {
                        _Player[i].gameObject.GetComponent<PlayerBace>().Rd.AddForce(_Player[i].gameObject.GetComponent<PlayerBace>().Rd.transform.up * 10, ForceMode.Impulse);
                        _Player[i].gameObject.GetComponent<PlayerBace>().PV.RPC("PlayerStun", RpcTarget.All, 1.5f);
                    }
                }
            }
        }

        yield return new WaitForSeconds(1.4f);

        delay = false;
    }
    [PunRPC]
    protected override IEnumerator Rrutin()
    {
        Rd.velocity = Vector3.zero;
        delay = true;
        Anime.SetTrigger("R");

        Rd.AddForce(Rd.transform.forward * 10, ForceMode.Impulse);

        yield return new WaitForSeconds(0.4f);
        Rd.AddForce(Rd.transform.up * 8, ForceMode.Impulse);

        yield return new WaitForSeconds(1f);

        if(PV.IsMine)
        PhotonNetwork.Instantiate("WarriorParticle/WarriorR", RPos.transform.position, RPos.transform.rotation);

        yield return new WaitForSeconds(1.3f);

        delay = false;
    }

    [PunRPC]
    protected override IEnumerator Trutin()
    {
        Rd.velocity = Vector3.zero;
        delay = true;
        Anime.SetTrigger("T");


        yield return new WaitForSeconds(2f);

        delay = false;
    }


    public override IEnumerator AttackColliderControl(string _Name)
    {
        Collider _Coll = AttackCollider;

        switch (_Name)
        {
            case "N":

                _Coll.enabled = true;
                yield return new WaitForSeconds(0.1f);
                _Coll.enabled = false;

                break;

            case "Q":
                _Coll = QCollider;

                Damage *= GameDataManager.Instance.CharacterData.Name["Warrior"].Skill_Damage[0];

                _Coll.enabled = true;
                yield return new WaitForSeconds(0.1f);
                _Coll.enabled = false;

                Damage /= GameDataManager.Instance.CharacterData.Name["Warrior"].Skill_Damage[0];

                break;

            case "E":

                Damage *= GameDataManager.Instance.CharacterData.Name["Warrior"].Skill_Damage[1];

                _Coll.enabled = true;

                yield return new WaitForSeconds(0.1f);
                _Coll.enabled = false;

                Damage /=  GameDataManager.Instance.CharacterData.Name["Warrior"].Skill_Damage[1];

                break;

            case "T":

                Damage *= 1.5f;
                Speed *= 1.5f;
                TEffect.Play();

                yield return new WaitForSeconds(10);

                TEffect.Stop();
                Damage /= 1.5f;
                Speed /= 1.5f;


                break;
        }
    }
}
