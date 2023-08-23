using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEnemy : Enemy
{
    [Space]
    [Header("보스 HP 이미지")]
    [SerializeField] GameObject[] Hp_obj;

    Stack<GameObject> HP_Image = new Stack<GameObject>();

    [Space]
    [Header("총알 발사 위치")]
    [SerializeField] GameObject[] BulletPos;

    [Space]
    [Header("총알 오브젝트")]
    [SerializeField] GameObject[] Bullet;

    public Queue<GameObject> Bullet_List = new Queue<GameObject>();

    float CurTime = 0;
    bool IsSkill = false;


    protected override void Awake()
    {
        base.Awake();

        // 난이도에 따른 보스 몬스터 HP이미지 설정 후 스택에 대입
        for (int i = 0; i < Hp; i++)
        { Hp_obj[i].SetActive(true); HP_Image.Push(Hp_obj[i]); }

        // 몬스터 총알을 큐로 관리
        for (int i = 0; i < Bullet.Length; i++)
            Bullet_List.Enqueue(Bullet[i]);
    }

    protected override void Hit()
    {
        // 보스 몬스터의 경우 HP를 스택으로 관리
        if (enemyStat != EnemyStat.Die)
        {
            HItEffect.Play();

            Destroy(HP_Image.Pop());

            if (HP_Image.Count == 0)
                enemyStat = EnemyStat.Die;
        }
    }

    protected override IEnumerator AttackControl()
    {
        Delay = true;

        Ani.SetTrigger("Attack");
        yield return new WaitForSeconds(AttackDelay);

        Delay = false;
    }

    IEnumerator SkillControl()
    {
        IsSkill = true;

        // 보스의 패턴을 랜덤을 이용하여 설정
        int Pattern = UnityEngine.Random.Range(0, 2);

        switch (Pattern)
        {
            case 0:

                // 총알 발사 패턴은 총알의 부모를 해제 시키면서 미리 설정된 위치에서 발사 되도록 설정
                GameObject _Bullet;

                for (int i = 0; i < 4; i++)
                {
                    _Bullet = Bullet_List.Dequeue();
                    _Bullet.transform.SetParent(null);
                    _Bullet.transform.position = BulletPos[i].transform.position;
                    _Bullet.transform.rotation = BulletPos[i].transform.rotation;
                    _Bullet.SetActive(true);
                }

                yield return new WaitForSeconds(1f);

                for (int i = 4; i < BulletPos.Length; i++)
                {
                    _Bullet = Bullet_List.Dequeue();
                    _Bullet.transform.SetParent(null);
                    _Bullet.transform.position = BulletPos[i].transform.position;
                    _Bullet.transform.rotation = BulletPos[i].transform.rotation;
                    _Bullet.SetActive(true);
                }

                yield return new WaitForSeconds(1f);

                break;


            case 1:

                // 패턴 실행시 플레이어의 위치를 받아와 돌진하도록 구현
                float _time = 0;

                Vector3 PlayerPos = Player.transform.position;

                AttackCollider.enabled = true;


                while (_time < 3 || Rd.transform.position != PlayerPos)
                {
                    _time += Time.deltaTime;
                    Rd.transform.position = Vector3.MoveTowards(Rd.transform.position, PlayerPos, Speed * 3 * Time.deltaTime);

                    yield return null;
                }

                AttackCollider.enabled = false;

                break;

            default:
                break;
        }

        IsSkill = false;
        CurTime = 0;
    }

    protected override IEnumerator Die()
    {
        IsDeath = true;

        DeathEffect.Play();

        Color[] color = new Color[EnemySprite.Length];
        float CurTime = 0;

        for (int i = 0; i < EnemySprite.Length; i++)
            color[i] = EnemySprite[i].color;

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

        ButtonManager.instance.Alarm("Game Clear");

        yield return new WaitForSeconds(2f);

        Destroy(Player.transform.gameObject);

        // 보스 몬스터 처치시 모든 맵과 오브젝트를 삭제하는 함수 실행
        Map.Instance.TileReset();
    }

    // Phisics를 이용하여 일반 공격 범위 감지
    protected override void PlayerCheck()
    {
        Attack_Player = Physics2D.BoxCast(Rd.transform.position, new Vector2(Attack_Area, Attack_Area / 2), 0, Vector2.zero, 0, LayerMask.GetMask("Player"));

        if (CurTime < 3)
            CurTime += Time.deltaTime;
    }

    protected override void EnemyControl()
    {
        // 플레이어가 자신의 방 범위에 위치할 경우 실행
        if (Player.transform.position.x >= LeftPos.x && Player.transform.position.x <= RightPos.x)
        {
            if (Player.transform.position.y >= LeftPos.y && Player.transform.position.y <= RightPos.y)
            {
                // 몬스터의 상태를 열겨형으로 관리
                if (enemyStat != EnemyStat.Die)
                {
                    PlayerCheck();

                    if (Attack_Player)
                    {
                        enemyStat = EnemyStat.Attack;
                        Lock = false;
                    }
                    else
                    {
                        if (CurTime >= 3)
                        {
                            enemyStat = EnemyStat.Skill;
                            Lock = false;
                        }
                        else
                            enemyStat = EnemyStat.Move;
                    }
                }

                if (!IsDeath)
                {
                    switch (enemyStat)
                    {
                        case EnemyStat.Idle:

                            Idle();

                            break;

                        case EnemyStat.Move:

                            if (!Lock)
                                StartCoroutine(Move());

                            break;

                        case EnemyStat.Attack:

                            Attack();

                            break;

                        case EnemyStat.Skill:

                            if (!IsSkill)
                                StartCoroutine(SkillControl());

                            break;

                        case EnemyStat.Die:


                            StartCoroutine(Die());

                            break;

                        default:
                            break;
                    }
                }
            }
        }
    }

    protected override void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(Rd.transform.position, Attack_Area);
    }
}
