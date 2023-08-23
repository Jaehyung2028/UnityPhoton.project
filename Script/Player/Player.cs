using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using static Enemy;

public class Player : MonoBehaviour
{
    public static Player Instance;

    [Header("오브젝트")]
    [SerializeField] GameObject MiniMap, AllBody;
    [SerializeField] GameObject[] HPObj, Bullet;

    [Space]
    [Header("컴포넌트")]
    [SerializeField] Rigidbody2D Rd;
    [SerializeField] Animator Ani;
    [SerializeField] Transform Main_Camera;
    [SerializeField] SpriteRenderer[] PlayerSprite;

    [Space]
    [Header("이펙트")]
    [SerializeField] ParticleSystem HitEffect, DeathEffect;

    [Space]
    [Header("수치")]
    [SerializeField] float AttackSpeed = 0.2f;
    public int Scroll, Soul;

    public Queue<GameObject> Bullet_List = new Queue<GameObject>();
    public Stack<GameObject> HP_IMAGE = new Stack<GameObject>();

    Dungeon CurRoom;

    public Vector2Int PlayerLocalPos;

    Vector3 Center;

    public bool MapCreate = false;
    bool Delay = false, IsDeath = false;

    void Idle()
    {
        Rd.velocity = Vector3.zero;

        Ani.SetBool("Walk", false);
        Ani.SetBool("Idle", true);
    }

    void Move()
    {
        Ani.SetBool("Walk", true);
        Ani.SetBool("Idle", false);

        if (Input.GetAxisRaw("Horizontal") > 0)
            AllBody.transform.rotation = Quaternion.Euler(0, 180, 0);
        else if (Input.GetAxisRaw("Horizontal") < 0)
            AllBody.transform.rotation = Quaternion.Euler(0, 0, 0);

        Rd.velocity = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")) * 5;
    }

    void Attack() { if (!Delay) StartCoroutine(AttackCollider_Control()); }

    IEnumerator AttackCollider_Control()
    {
        Delay = true;

        Ani.SetTrigger("Attack");
        Bullet_List.Dequeue().SetActive(true);

        yield return new WaitForSeconds(AttackSpeed);

        Delay = false;
    }

    public IEnumerator Spawn()
    {

        Color[] color = new Color[PlayerSprite.Length];
        float CurTime = 0;

        for (int i = 0; i < PlayerSprite.Length; i++)
        {
            color[i] = PlayerSprite[i].color;
        }

        while (color[0].a < 1)
        {
            CurTime += 0.25f * Time.deltaTime;

            for (int i = 0; i < color.Length; i++)
            {
                color[i].a = Mathf.Lerp(0, 1, CurTime);
                PlayerSprite[i].color = color[i];
            }
            yield return null;
        }

        CurTime = 0;

        int CaSize = Mathf.RoundToInt(Map.Instance.RoomSize_Y / 1.5f);

        while (Camera.main.orthographicSize != CaSize)
        {
            CurTime += 0.2f * Time.deltaTime;
            Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, CaSize, CurTime);
            yield return null;
        }

        CurRoom = Map.Instance._DungeonList[0];

        MapCreate = true;
    }

    IEnumerator Die()
    {
        IsDeath = true;

        gameObject.layer = 1;

        DeathEffect.Play();

        Color[] color = new Color[PlayerSprite.Length];
        float CurTime = 0;

        for (int i = 0; i < PlayerSprite.Length; i++)
        {
            color[i] = PlayerSprite[i].color;
        }

        while (color[0].a > 0)
        {
            CurTime += 0.25f * Time.deltaTime;

            for (int i = 0; i < color.Length; i++)
            {
                color[i].a = Mathf.Lerp(1, 0, CurTime);
                PlayerSprite[i].color = color[i];
            }
            yield return null;
        }

        Map.Instance.TileReset();

        Destroy(gameObject.transform.parent.gameObject);
    }

    private void KeyControl()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (MiniMap.activeSelf == true) MiniMap.SetActive(false);
            else MiniMap.SetActive(true);
        }

        if (Input.GetMouseButtonDown(0))
            Attack();
    }


    private void Awake()
    {
        Instance = this;
        PlayerLocalPos = Vector2Int.zero;
        Main_Camera = Camera.main.transform;
        MiniMap = GameObject.Find("Canvas").transform.Find("MiniMap").gameObject;
        Center = new Vector3(Map.Instance.RoomSize_X / 2 - 0.5f, Map.Instance.RoomSize_Y / 2 - 0.5f, 0);

        for (int i = 0; i < Bullet.Length; i++)
            Bullet_List.Enqueue(Bullet[i]);
    }

    private void Update() { if (MapCreate && !IsDeath) KeyControl(); }

    private void FixedUpdate()
    {
        if (MapCreate && !IsDeath)
        {
            if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
                Move();
            else
                Idle();
        }
    }

    // 포탈의 방향에 따라 이동되는 위치 대입
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "PorTal")
        {
            // 현재 방에 몬스터가 존재 하지 않을 경우 이동 가능
            if (CurRoom.Monster == 0)
            {
                switch (other.GetComponent<PorTarDirection>().Direction)
                {
                    // 이동 하려고 하는 방의 좌측하단 좌표와 플레이어의 위치 변동 값을 매개변수로 넘김
                    case "Up":
                        PortalMove(new Vector2Int(0, Map.Instance.RoomSize_Y + 10), -new Vector3(0, Map.Instance.RoomSize_Y / 2 - 2.5f));
                        break;
                    case "Down":
                        PortalMove(-new Vector2Int(0, Map.Instance.RoomSize_Y + 10), new Vector3(0, Map.Instance.RoomSize_Y / 2 - 3.5f));
                        break;
                    case "Right":
                        PortalMove(new Vector2Int(Map.Instance.RoomSize_X + 10, 0), -new Vector3(Map.Instance.RoomSize_X / 2 - 3, 0));
                        break;
                    case "Left":
                        PortalMove(-new Vector2Int(Map.Instance.RoomSize_X + 10, 0), new Vector3(Map.Instance.RoomSize_X / 2 - 3, 0));
                        break;
                    default:
                        break;
                }
            }
            else
                ButtonManager.instance.Alarm("Kill all monsters in the room.");
        }
        else if (other.tag == "EnemyAttack")
        {
            if (HP_IMAGE.Count != 0)
                Hit();
        }
    }

    private void Hit()
    {
        HitEffect.Play();

        Destroy(HP_IMAGE.Pop());

        if (HP_IMAGE.Count <= 0)
            StartCoroutine(Die());
    }

    private void PortalMove(Vector2Int _LeftPos, Vector3 MovePos)
    {
        // 받아온 매개 변수에 따라 플레이어 이동 위치 및 카메라 위치 설정
        foreach (Dungeon Pos in Map.Instance._DungeonList)
        {
            // 이동하려고 하는 방의 좌표가 존재 할 경우
            if (Pos.LeftBenchmark == PlayerLocalPos + _LeftPos)
            {

                // 히든룸과 보스룸의 경우 플레이어의 아이템의 조건에 따라 입장가능
                if (Pos.BossRoom == false)
                {
                    PlayerLocalPos = PlayerLocalPos + _LeftPos;
                    Main_Camera.position = new Vector3(PlayerLocalPos.x, PlayerLocalPos.y, 0) + new Vector3(Map.Instance.RoomSize_X / 2, Map.Instance.RoomSize_Y / 2, -10);
                    Rd.transform.position = new Vector3(PlayerLocalPos.x, PlayerLocalPos.y, 0) + new Vector3(Map.Instance.RoomSize_X / 2, Map.Instance.RoomSize_Y / 2, 0) + MovePos;
                }
                else
                {
                    if (Soul == Map.Instance.HiddenCount)
                    {
                        PlayerLocalPos = PlayerLocalPos + _LeftPos;
                        Main_Camera.position = new Vector3(PlayerLocalPos.x, PlayerLocalPos.y, 0) + new Vector3(Map.Instance.RoomSize_X / 2, Map.Instance.RoomSize_Y / 2, -10);
                        Rd.transform.position = new Vector3(PlayerLocalPos.x, PlayerLocalPos.y, 0) + new Vector3(Map.Instance.RoomSize_X / 2, Map.Instance.RoomSize_Y / 2, 0) + MovePos;

                        ButtonManager.instance.SoulText.text = "X " + 0;
                    }
                    else
                    {
                        ButtonManager.instance.Alarm("You must collect <color=blue>souls</color>.");
                        return;
                    }
                }

                if (Pos.MiniMap_Obj != null)
                    Destroy(Pos.MiniMap_Obj);

                CurRoom = Pos;

                break;
            }
        }

        // 방 입장시 이전 방의 포탈을 제거
        for (int i = 0; i < Map.Instance.PortalArray.Count; i++)
            Destroy(Map.Instance.PortalArray[i]);

        Map.Instance.PortalArray.Clear();

        GameObject PorTal;
        GameObject PorTal_Instance;

        // 현재 방의 기준으로 4방향에 방 존재 유무를 확인 후 방의 조건에 맞는 포탈을 생성
        foreach (Dungeon Pos in Map.Instance._DungeonList)
        {
            if (Pos.LeftBenchmark == PlayerLocalPos + new Vector2Int(0, Map.Instance.RoomSize_Y + 10))
            {
                if (Pos.BossRoom == false)
                {
                    PorTal_Instance = Pos.HidenRoom ? Map.Instance.HiddenPortal : Map.Instance._Portal;

                    PorTal = Instantiate(PorTal_Instance, new Vector3(PlayerLocalPos.x, PlayerLocalPos.y, 0) + Center + new Vector3(0, Map.Instance.RoomSize_Y / 2 - 1.5f), Quaternion.identity);
                    Map.Instance.PortalArray.Add(PorTal);

                    PorTal.GetComponent<PorTarDirection>().Direction = "Up";
                }
                else
                {
                    PorTal = Instantiate(Map.Instance.BossPortal, new Vector3(PlayerLocalPos.x, PlayerLocalPos.y, 0) + Center + new Vector3(0, Map.Instance.RoomSize_Y / 2 - 1.5f), Quaternion.identity);
                    Map.Instance.PortalArray.Add(PorTal);
                    PorTal.GetComponent<PorTarDirection>().Direction = "Up";
                }
            }
            else if (Pos.LeftBenchmark == PlayerLocalPos - new Vector2Int(0, Map.Instance.RoomSize_Y + 10))
            {
                if (Pos.BossRoom == false)
                {
                    PorTal_Instance = Pos.HidenRoom ? Map.Instance.HiddenPortal : Map.Instance._Portal;

                    PorTal = Instantiate(PorTal_Instance, new Vector3(PlayerLocalPos.x, PlayerLocalPos.y, 0) + Center - new Vector3(0, Map.Instance.RoomSize_Y / 2 - 1.5f), Quaternion.identity);
                    Map.Instance.PortalArray.Add(PorTal);

                    PorTal.GetComponent<PorTarDirection>().Direction = "Down";
                }
                else
                {
                    PorTal = Instantiate(Map.Instance.BossPortal, new Vector3(PlayerLocalPos.x, PlayerLocalPos.y, 0) + Center - new Vector3(0, Map.Instance.RoomSize_Y / 2 - 1.5f), Quaternion.identity);
                    Map.Instance.PortalArray.Add(PorTal);
                    PorTal.GetComponent<PorTarDirection>().Direction = "Down";
                }
            }
            else if (Pos.LeftBenchmark == PlayerLocalPos + new Vector2Int(Map.Instance.RoomSize_X + 10, 0))
            {
                if (Pos.BossRoom == false)
                {
                    PorTal_Instance = Pos.HidenRoom ? Map.Instance.HiddenPortal : Map.Instance._Portal;

                    PorTal = Instantiate(PorTal_Instance, new Vector3(PlayerLocalPos.x, PlayerLocalPos.y, 0) + Center + new Vector3(Map.Instance.RoomSize_X / 2 - 1.5f, 0), Quaternion.identity);
                    Map.Instance.PortalArray.Add(PorTal);

                    PorTal.GetComponent<PorTarDirection>().Direction = "Right";
                }
                else
                {
                    PorTal = Instantiate(Map.Instance.BossPortal, new Vector3(PlayerLocalPos.x, PlayerLocalPos.y, 0) + Center + new Vector3(Map.Instance.RoomSize_X / 2 - 1.5f, 0), Quaternion.identity);
                    Map.Instance.PortalArray.Add(PorTal);
                    PorTal.GetComponent<PorTarDirection>().Direction = "Right";
                }
            }
            else if (Pos.LeftBenchmark == PlayerLocalPos - new Vector2Int(Map.Instance.RoomSize_X + 10, 0))
            {
                if (Pos.BossRoom == false)
                {
                    PorTal_Instance = Pos.HidenRoom ? Map.Instance.HiddenPortal : Map.Instance._Portal;

                    PorTal = Instantiate(PorTal_Instance, new Vector3(PlayerLocalPos.x, PlayerLocalPos.y, 0) + Center - new Vector3(Map.Instance.RoomSize_X / 2 - 1.5f, 0), Quaternion.identity);
                    Map.Instance.PortalArray.Add(PorTal);

                    PorTal.GetComponent<PorTarDirection>().Direction = "Left";
                }
                else
                {
                    PorTal = Instantiate(Map.Instance.BossPortal, new Vector3(PlayerLocalPos.x, PlayerLocalPos.y, 0) + Center - new Vector3(Map.Instance.RoomSize_X / 2 - 1.5f, 0), Quaternion.identity);
                    Map.Instance.PortalArray.Add(PorTal);
                    PorTal.GetComponent<PorTarDirection>().Direction = "Left";
                }
            }
        }

        // 현재 방의 몬스터 존재 여부에 따라 포탈 구분
        for (int i = 0; i < Map.Instance.PortalArray.Count; i++)
        {
            if(CurRoom.Monster == 0)
            {
                Map.Instance.PortalArray[i].GetComponent<PorTarDirection>().CanMove();
            }
            else
                Map.Instance.PortalArray[i].GetComponent<PorTarDirection>().DontMove();
        }
    }
}
