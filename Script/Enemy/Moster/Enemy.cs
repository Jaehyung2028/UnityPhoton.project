using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

// 몬스터의 베이스 스크립트이기 때문에 추상클래스로 작성
public abstract class Enemy : MonoBehaviour
{
    [Header("이동 알고리즘")][SerializeField] NodeControl MoveNode;

    [Space][Header("컴포넌트")][SerializeField] protected Rigidbody2D Rd, Player;
    [SerializeField] protected Animator Ani;
    [SerializeField] protected ParticleSystem DeathEffect, HItEffect;
    [SerializeField] protected Collider2D AttackCollider;
    [SerializeField] protected GameObject AllBody;
    [SerializeField] protected SpriteRenderer[] EnemySprite;

    [Space][Header("몬스터 수치")][SerializeField] protected int Hp, Speed;
    public Vector3Int LeftPos, RightPos;
    [SerializeField] protected float Move_Area, Attack_Area, AttackDelay;

    protected bool Follow_Player = false, Attack_Player = false, IsDeath = false, Delay = false, Lock = false;
    public Dungeon CurDungeon;

    protected enum EnemyStat { Idle, Move, Attack, Skill, Die };
    [Space][Header("몬스터 상태")][SerializeField] protected EnemyStat enemyStat;


    protected abstract void OnDrawGizmos();

    protected virtual void Idle()
    {
        Rd.velocity = Vector3.zero;
        Ani.SetBool("Walk", false);
        Ani.SetBool("Idle", true);
    }

    protected IEnumerator Move()
    {
        Lock = true;

        Stack<Node> MovePos = new Stack<Node>();
        Node Destination;
        Node Point = null;

        float CurTime = 0;

        Ani.SetBool("Walk", true);
        Ani.SetBool("Idle", false);

        // 몬스터 이동의 경우 매개변수를 넘겨주어 이동 스크립트에 있는 함수를 싱행
        MoveNode.PathFind(LeftPos, RightPos, Rd.transform.position, Player.transform.position, out MovePos, out Destination);

        // 넘겨 받은 경로 노드가 존재하는 경우 계속해서 실행 되고 아닌 경우 함수 종료
        if (MovePos.Count != 0)
        {
            if (Destination != null)
            {
                Point = MovePos.Pop();
            }
        }
        else
        {
            Lock = false;
            yield break;
        }

        while (Lock && !IsDeath)
        {
            if (CurTime < 0.5f)
                CurTime += Time.deltaTime;

            // 자신의 위치가 현재 이동하여야 하는 위치값에 도착하였을 경우 실행
            // 실시간으로 플레이어를 추적하여야 하기 때문에 0.5초가 지난 시점에 타겟의 반올림 위치가 도착점과 다를 경우 재탐색 하도록 설정
            if (Rd.transform.position == new Vector3(Point.x, Point.y, 0))
            {
                if (Vector3Int.RoundToInt(Player.transform.position) != new Vector3Int(Destination.x, Destination.y, 0) && CurTime >= 0.5f)
                {
                    MoveNode.PathFind(LeftPos, RightPos, Rd.transform.position, Player.transform.position, out MovePos, out Destination);

                    CurTime = 0;

                    if (MovePos.Count != 0)
                    {
                        if (Destination != null)
                        {
                            Point = MovePos.Pop();
                        }
                    }
                    else
                    {
                        Lock = false;
                        yield break;
                    }
                }
                else
                {
                    if (MovePos.Count != 0)
                    {
                        if (Destination != null)
                        {
                            Point = MovePos.Pop();
                        }
                    }
                }

            }

            // 위치 이동은 스택으로 넘겨 받은 노드의 각 위치를 차례대로 이동하도록 설정
            Rd.transform.position = Vector3.MoveTowards(Rd.transform.position, new Vector3(Point.x, Point.y, 0), Speed * Time.deltaTime);

            // 플레이어와 몬스터의 거리 값을 이용하여 플레이어를 바라보도록 설정
            if ((Player.transform.position.x - Rd.transform.position.x) < 0 && AllBody.transform.rotation != Quaternion.Euler(0, 0, 0))
                AllBody.transform.rotation = Quaternion.Euler(0, 0, 0);
            else if ((Player.transform.position.x - Rd.transform.position.x) > 0 && AllBody.transform.rotation != Quaternion.Euler(0, 180, 0))
                AllBody.transform.rotation = Quaternion.Euler(0, 180, 0);

            yield return null;

        }
    }

    protected virtual void Attack() { if (!Delay) StartCoroutine(AttackControl()); }

    protected virtual IEnumerator AttackControl()
    {
        Delay = true;

        while (enemyStat == EnemyStat.Attack)
        {
            Ani.SetTrigger("Attack");
            yield return new WaitForSeconds(AttackDelay);
        }

        Delay = false;
    }

    public IEnumerator AttackCollider_Control()
    {
        AttackCollider.enabled = true;
        yield return new WaitForSeconds(0.1f);
        AttackCollider.enabled = false;
    }

    abstract protected IEnumerator Die();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "PlayerAttack")
            Hit();
    }

    protected virtual void Hit()
    {
        if (enemyStat != EnemyStat.Die)
        {

            HItEffect.Play();

            Hp--;

            if (Hp <= 0)
            {
                enemyStat = EnemyStat.Die;
            }
        }
    }

    // Phisics를 이용하여 일반 공격 범위 및 추적 범위 감지
    protected virtual void PlayerCheck()
    {
        Follow_Player = Physics2D.CircleCast(Rd.transform.position, Move_Area, Vector2.zero, 0, LayerMask.GetMask("Player"));
        Attack_Player = Physics2D.BoxCast(Rd.transform.position, new Vector2(Attack_Area, Attack_Area / 2), 0, Vector2.zero, 0, LayerMask.GetMask("Player"));
    }

    protected virtual void EnemyControl()
    {
        // 플레이어가 자신의 방 범위에 위치할 경우 실행
        if (Player.transform.position.x >= LeftPos.x && Player.transform.position.x <= RightPos.x &&
            Player.transform.position.y >= LeftPos.y && Player.transform.position.y <= RightPos.y)
        {
            // 몬스터의 상태를 열겨형으로 관리
            if (enemyStat != EnemyStat.Die)
            {
                PlayerCheck();

                if (Follow_Player)
                {
                    if (!Attack_Player)
                        enemyStat = EnemyStat.Move;
                    else
                    {
                        enemyStat = EnemyStat.Attack;
                        Lock = false;
                    }
                }
                else
                {
                    enemyStat = EnemyStat.Idle;
                    Lock = false;
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

                    case EnemyStat.Die:

                        StartCoroutine(Die());

                        break;

                    default:
                        break;
                }
            }

        }
        else
        {
            if (Lock)
                Lock = false;
        }
    }

    public void CurPosDungeon()
    {
        for (int i = 0; i < Map.Instance._DungeonList.Count; i++)
        {
            if (Map.Instance._DungeonList[i].LeftBenchmark == new Vector2Int(LeftPos.x, LeftPos.y))
                CurDungeon = Map.Instance._DungeonList[i];
        }
    }

    protected virtual void Awake()
    {
        if (GameObject.Find("Player(Clone)") != null)
            Player = GameObject.Find("Player(Clone)").GetComponent<Rigidbody2D>();

        Hp = Hp * ButtonManager.instance.HpLevel;
    }

    private void FixedUpdate() => EnemyControl();
}
