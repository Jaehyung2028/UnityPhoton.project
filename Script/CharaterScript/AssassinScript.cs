using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEditor;
using Photon.Pun.Demo.PunBasics;

public class AssassinScript : PlayerBace
{
    [Header("공격 콜라이더")]
    [SerializeField] BoxCollider AttackCollider;

    [Header("투명 효과 관련")]
    [SerializeField] protected GameObject NameText, OriginSkin, ChangeSkin;

    [Header("공격 이펙트")]
    [SerializeField] ParticleSystem AttackEffect, QEfect, TEfect;

    [PunRPC]
    protected override IEnumerator Qrutin()
    {
        Rd.velocity = Vector3.zero;
        delay = true;
        Anime.SetTrigger("Q");

        yield return new WaitForSeconds(1f);
        QEfect.Play();
        NameText.SetActive(false);
        ChangeSkin.SetActive(true);
        OriginSkin.SetActive(false);

        delay = false;

        yield return new WaitForSeconds(5f);
        NameText.SetActive(true);
        ChangeSkin.SetActive(false);
        OriginSkin.SetActive(true);
    }
    [PunRPC]
    protected override IEnumerator Erutin()
    {
        Rd.velocity = Vector3.zero;
        delay = true;
        Anime.SetTrigger("E");
        yield return new WaitForSeconds(0.2f);
        Rd.AddForce(Rd.transform.forward * 2, ForceMode.Impulse);
        yield return new WaitForSeconds(0.2f);
        Rd.AddForce(Rd.transform.forward * 2, ForceMode.Impulse);
        yield return new WaitForSeconds(0.5f);
        Rd.AddForce(Rd.transform.forward * 2, ForceMode.Impulse);
        yield return new WaitForSeconds(1.3f);
        delay = false;
    }
    [PunRPC]
    protected override IEnumerator Rrutin()
    {
        Rd.velocity = Vector3.zero;
        delay = true;
        Anime.SetTrigger("R");

        yield return new WaitForSeconds(1f);

        delay = false;
    }
    [PunRPC]
    protected override IEnumerator Trutin()
    {
        Rd.velocity = Vector3.zero;
        delay = true;
        Anime.SetTrigger("T");

        Rd.AddForce(Rd.transform.forward * 10, ForceMode.Impulse);
        yield return new WaitForSeconds(0.2f);
        TEfect.Play();
        yield return new WaitForSeconds(0.8f);

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

                Damage *= GameDataManager.Instance.CharacterData.Name["Assassin"].Skill_Damage[1];

                AttackEffect.Play();
                AttackCollider.enabled = true;

                yield return new WaitForSeconds(0.1f);

                AttackCollider.enabled = false;

                Damage /= GameDataManager.Instance.CharacterData.Name["Assassin"].Skill_Damage[1];

                break;

            case "R":

                Damage *= GameDataManager.Instance.CharacterData.Name["Assassin"].Skill_Damage[2];

                AttackEffect.Play();

                if (!PV.IsMine)
                {
                    Collider[] _Player = Physics.OverlapBox(AttackCollider.transform.position, AttackCollider.gameObject.GetComponent<BoxCollider>().size);

                    for (int i = 0; i < _Player.Length; i++)
                    {
                        if (_Player[i].gameObject.tag == "Player" && _Player[i].gameObject.GetComponent<PlayerBace>().Rd != Rd)
                            if (_Player[i].gameObject.GetComponent<PlayerBace>().Team != Team && _Player[i].gameObject.GetComponent<PlayerBace>().PV.IsMine)
                                _Player[i].gameObject.GetComponent<PlayerBace>().PV.RPC("PlayerBleeding", RpcTarget.All, Damage, Team);
                    }

                }
                yield return new WaitForSeconds(0.1f);

                Damage /= GameDataManager.Instance.CharacterData.Name["Assassin"].Skill_Damage[2];

                break;

            case "T":

                Damage *= GameDataManager.Instance.CharacterData.Name["Assassin"].Skill_Damage[3];

                AttackCollider.enabled = true;

                yield return new WaitForSeconds(0.1f);

                AttackCollider.enabled = false;

                Damage /= GameDataManager.Instance.CharacterData.Name["Assassin"].Skill_Damage[3];

                break;
        }
    }
}