using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaterpillarMonster : Enemy
{
    [SerializeField] GameObject[] MiniEnem;

    // 몬스터의 컨셉에 맞게 자식오브젝트로 등록되어 있던 오브젝트들을 활성화 시켜 분열하는 기능을 구현
    protected override IEnumerator Die()
    {
        IsDeath = true;

        gameObject.tag = "Untagged";

        CurDungeon.Monster--;

        if (CurDungeon.Monster <= 0)
        {
            ButtonManager.instance.Alarm("Clear.");

            for (int i = 0; i < Map.Instance.PortalArray.Count; i++)
            {
                Map.Instance.PortalArray[i].GetComponent<PorTarDirection>().CanMove();
            }
        }

        for (int i = 0; i < MiniEnem.Length; i++)
        {
            CurDungeon.Monster++;

            Map.Instance.AllMonster.Add(MiniEnem[i]);
            MiniEnem[i].transform.GetChild(0).GetComponent<Enemy>().LeftPos = LeftPos;
            MiniEnem[i].transform.GetChild(0).GetComponent<Enemy>().RightPos = RightPos;
            MiniEnem[i].transform.GetChild(0).GetComponent<Enemy>().CurDungeon = CurDungeon;
            MiniEnem[i].SetActive(true);
            MiniEnem[i].transform.parent = null;

        }

        yield return null;

        DeathEffect.Play();

        Color[] color = new Color[EnemySprite.Length];
        float CurTime = 0;

        for (int i = 0; i < EnemySprite.Length; i++)
        {
            color[i] = EnemySprite[i].color;
        }

        while (color[0].a > 0)
        {
            CurTime += 0.4f * Time.deltaTime;

            for (int i = 0; i < color.Length; i++)
            {
                color[i].a = Mathf.Lerp(1, 0, CurTime);
                EnemySprite[i].color = color[i];
            }

            yield return null;
        }

        Map.Instance.AllMonster.Remove(AllBody.transform.parent.gameObject);
        Destroy(AllBody.transform.parent.gameObject);

    }

    protected override void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(Rd.transform.position, Move_Area);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(Rd.transform.position, new Vector3(Attack_Area, Attack_Area / 2, Attack_Area));
    }
}
