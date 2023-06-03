using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FighterScript : PlayerBace
{
    [Header("공격 콜라이더")]
    [SerializeField] BoxCollider AttackCollider, RCollider;

    [Header("공격 이펙트")]
    [SerializeField] ParticleSystem AttackEffect;

    [PunRPC]
    protected override IEnumerator Qrutin()
    {
        Rd.velocity = Vector3.zero;
        delay = true;
        Anime.SetTrigger("Q");
        Rd.AddForce(-Rd.transform.forward * 8, ForceMode.Impulse);

        yield return new WaitForSeconds(1f);

        delay = false;
    }
    [PunRPC]
    protected override IEnumerator Erutin()
    {
        Rd.velocity = Vector3.zero;
        delay = true;
        Anime.SetTrigger("E");
        Rd.AddForce(Rd.transform.forward * 10, ForceMode.Impulse);

        yield return new WaitForSeconds(1.4f);

        delay = false;
    }
    [PunRPC]
    protected override IEnumerator Rrutin()
    {
        Rd.velocity = Vector3.zero;
        delay = true;
        Anime.SetTrigger("R");

        yield return new WaitForSeconds(0.9f);

        if (!PV.IsMine)
        {
            Collider[] _Player = Physics.OverlapBox(RCollider.transform.position, RCollider.gameObject.GetComponent<BoxCollider>().size);

            for (int i = 0; i < _Player.Length; i++)
            {
                if (_Player[i].gameObject.tag == "Player" && _Player[i].gameObject.GetComponent<PlayerBace>().Rd != Rd)
                    if (_Player[i].gameObject.GetComponent<PlayerBace>().Team != Team)
                        _Player[i].gameObject.GetComponent<PlayerBace>().PV.RPC("PlayerStun", RpcTarget.All, 1f);
            }
        }

        yield return new WaitForSeconds(1f);

        delay = false;
    }
    [PunRPC]
    protected override IEnumerator Trutin()
    {
        Rd.velocity = Vector3.zero;
        delay = true;
        Anime.SetTrigger("T");


        yield return new WaitForSeconds(1.8f);

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

                Damage *= GameDataManager.Instance.CharacterData.Name["Fighter"].Skill_Damage[1];

                AttackEffect.Play();
                AttackCollider.enabled = true;

                yield return new WaitForSeconds(0.1f);

                AttackCollider.enabled = false;

                Damage /= GameDataManager.Instance.CharacterData.Name["Fighter"].Skill_Damage[1];

                break;

            case "R":

                Damage *= GameDataManager.Instance.CharacterData.Name["Fighter"].Skill_Damage[2];

                AttackEffect.Play();
                RCollider.enabled = true;

                yield return new WaitForSeconds(0.1f);

                RCollider.enabled = false;

                Damage /= GameDataManager.Instance.CharacterData.Name["Fighter"].Skill_Damage[2];

                break;

            case "T":

                Damage *= GameDataManager.Instance.CharacterData.Name["Fighter"].Skill_Damage[3];

                AttackEffect.Play();
                AttackCollider.enabled = true;

                yield return new WaitForSeconds(0.1f);

                AttackCollider.enabled = false;

                Damage /= GameDataManager.Instance.CharacterData.Name["Fighter"].Skill_Damage[3];

                break;
        }
    }
}
