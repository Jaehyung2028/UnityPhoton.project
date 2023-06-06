using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Unity.VisualScripting;
using Unity.Collections.LowLevel.Unsafe;
using System;

public abstract class PlayerBace : MonoBehaviourPunCallbacks, IPunObservable
{
    [Header("텍스트")]
    [SerializeField] Text NickName;
    [SerializeField] Text DamageText;

    [Header("이펙트")]
    [SerializeField] ParticleSystem HitEffect, HealEffect;

    [Header("피격시 컬러 체인지")]
    [SerializeField] protected SkinnedMeshRenderer[] Render;
    Color[] OriginColor;

    public PhotonView PV;
    [SerializeField] protected Animator Anime;
    public Rigidbody Rd;
    [SerializeField] GameObject CaPos, TextPos;

    [Header("캐릭터 데이터")]
    [SerializeField] string _Name;
    [SerializeField] protected float Speed, HP;
    float NowHP;
    public float Damage;
    public string Team;
    public Sprite[] Skill;

    [Header("트랜스폼 동기화 변수")]
    Vector3 CurPos;
    Quaternion CurRot;

    [Header("이동 관련 변수")]
    float Move_X, Move_Z;
    int MoveValue;

    [Header("캐릭터 상태")]
    protected bool Stun = false, delay = false;

    [Header("공격 함수 모음")]
    Func<IEnumerator> AttackCoroutin;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(Rd.transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            CurPos = (Vector3)stream.ReceiveNext();
            CurRot = (Quaternion)stream.ReceiveNext();
        }
    }

    void DataReset()
    {

        HP = GameDataManager.Instance.CharacterData.Name[_Name].HP;
        Speed = GameDataManager.Instance.CharacterData.Name[_Name].Speed;
        Damage = GameDataManager.Instance.CharacterData.Name[_Name].Damage;
        Team = PV.IsMine ? PhotonNetwork.LocalPlayer.CustomProperties["IsTeam"].ToString() : PV.Owner.CustomProperties["IsTeam"].ToString();

        if (PV.IsMine)
        {
            NowHP = HP;

            AttackCoroutin = Attack;
            AttackCoroutin += Qrutin;
            AttackCoroutin += Erutin;
            AttackCoroutin += Rrutin;
            AttackCoroutin += Trutin;

        }
    }

    void Awake()
    {
        DataReset();

        NickName.text = PV.IsMine ? PhotonNetwork.NickName : PV.Owner.NickName;
        NickName.color = PhotonNetwork.LocalPlayer.CustomProperties["IsTeam"].ToString() == Team ? Color.green : Color.red;

        OriginColor = new Color[Render.Length];

        for (int i = 0; i < Render.Length; i++)
            OriginColor[i] = Render[i].material.color;
    }

    void MoveAnime()
    {
        Anime.SetInteger("Move", MoveValue);

        if (Input.GetKey(KeyCode.W)) MoveValue = 1;
        else if (Input.GetKey(KeyCode.S)) MoveValue = 2;
        else if (Input.GetKey(KeyCode.A)) MoveValue = 4;
        else if (Input.GetKey(KeyCode.D)) MoveValue = 3;
        else MoveValue = 0;
    }

    void PlayerAttack()
    {

        if (Input.GetMouseButtonDown(0)) StartCoroutine(Attack());

        if (Input.GetKeyDown(KeyCode.Q) && GameDataManager.Instance.Cooldown_time_Image[0].fillAmount == 0)
        {
            StartCoroutine(Skill_Cooldown_time(0, GameDataManager.Instance.CharacterData.Name[GameDataManager.Instance.SelectCharaterName].Cooldown_time[0]));
            PV.RPC("Qrutin", RpcTarget.All);
        }
        if (Input.GetKeyDown(KeyCode.E) && GameDataManager.Instance.Cooldown_time_Image[1].fillAmount == 0)
        {
            StartCoroutine(Skill_Cooldown_time(1, GameDataManager.Instance.CharacterData.Name[GameDataManager.Instance.SelectCharaterName].Cooldown_time[1]));
            PV.RPC("Erutin", RpcTarget.All);
        }
        if (Input.GetKeyDown(KeyCode.R) && GameDataManager.Instance.Cooldown_time_Image[2].fillAmount == 0)
        {
            StartCoroutine(Skill_Cooldown_time(2, GameDataManager.Instance.CharacterData.Name[GameDataManager.Instance.SelectCharaterName].Cooldown_time[2]));
            PV.RPC("Rrutin", RpcTarget.All);
        }
        if (Input.GetKeyDown(KeyCode.T) && GameDataManager.Instance.Cooldown_time_Image[3].fillAmount == 0)
        {
            StartCoroutine(Skill_Cooldown_time(3, GameDataManager.Instance.CharacterData.Name[GameDataManager.Instance.SelectCharaterName].Cooldown_time[3]));
            PV.RPC("Trutin", RpcTarget.All);
        }

        if (Input.GetMouseButtonDown(1)) StartCoroutine("Sheld");
    }

    IEnumerator Attack()
    {
        Anime.SetBool("Attack", true);

        while (!Input.GetMouseButtonUp(0))
        {
            yield return null;
        }

        Anime.SetBool("Attack", false);
    }

    IEnumerator Sheld()
    {
        Rd.velocity = Vector3.zero;

        Anime.SetBool("Rock", true);

        while (!Input.GetMouseButtonUp(1))
        {
            yield return null;
        }

        Anime.SetBool("Rock", false);
    }

    protected abstract IEnumerator Qrutin();
    protected abstract IEnumerator Erutin();
    protected abstract IEnumerator Rrutin();
    protected abstract IEnumerator Trutin();
    public abstract IEnumerator AttackColliderControl(string _Name);

    IEnumerator Skill_Cooldown_time(int _Number, float _Time)
    {
        float _CurrTime = _Time;
        GameDataManager.Instance.Cooldown_time_Image[_Number].fillAmount = 1;

        while (GameDataManager.Instance.Cooldown_time_Image[_Number].fillAmount > 0)
        {
            _CurrTime -= Time.deltaTime;
            GameDataManager.Instance.Cooldown_time_Image[_Number].fillAmount = _CurrTime / _Time;
            yield return null;
        }
    }

    [PunRPC]
    public IEnumerator DatageHit(float _Damage, string _Team)
    {
        if (_Team != Team)
        {
            StartCoroutine(ColorChage());

            HitEffect.Play();

            if (!Anime.GetBool("Rock"))
            {
                if (PV.IsMine) NowHP -= _Damage;

                StartCoroutine(TextInstance(_Damage));

                if (!delay) Anime.SetTrigger("Hit");
            }
            else { if (PV.IsMine) NowHP -= _Damage / 2; StartCoroutine(TextInstance(_Damage / 2)); }

            if (NowHP <= 0 && PV.IsMine)
            {
                gameObject.tag = "Death";

                Anime.SetBool("Death", true);

                StopCoroutine(AttackCoroutin());

                yield return new WaitForSeconds(5);

                Camera.main.transform.parent = null;

                Cursor.visible = true;

                Cursor.lockState = CursorLockMode.None;

                GameDataManager.Instance.SkillTool.SetActive(false);

                GameDataManager.Instance.StatTool.SetActive(false);

                GameDataManager.Instance.SelectTool.SetActive(true);

                PhotonNetwork.Destroy(gameObject);
            }

            delay = true;
            yield return new WaitForSeconds(1f);
            delay = false;
        }
    }

    [PunRPC]
    public IEnumerator PlayerStun(float _Time)
    {
        Stun = true;
        NickName.text = "기절";
        yield return new WaitForSeconds(_Time);
        Stun = false;
        NickName.text = PV.IsMine ? PhotonNetwork.NickName : PV.Owner.NickName;
    }

    [PunRPC]
    public IEnumerator PlayerBleeding(float _Damage)
    {
        int _Count = 0;
        NickName.text = "출혈";

        while (_Count < 10)
        {
            if (gameObject.tag == "Player" && PV.IsMine)
                PV.RPC("DatageHit", RpcTarget.All, Damage);

            _Count++;
            yield return new WaitForSeconds(1f);
        }

        NickName.text = PV.IsMine ? PhotonNetwork.NickName : PV.Owner.NickName;
    }

    [PunRPC]
    public void _HealEffect() => HealEffect.Play();

    IEnumerator TextInstance(float _Damage)
    {
        float MoveTime = 0;
        Text _Text = Instantiate(DamageText, TextPos.transform);
        _Text.text = _Damage.ToString();
        _Text.gameObject.GetComponent<Rigidbody2D>().AddForce(_Text.gameObject.transform.right, ForceMode2D.Impulse);

        while (MoveTime < 5)
        {
            MoveTime += Time.deltaTime;
            _Text.transform.gameObject.transform.rotation = Camera.main.transform.rotation;
            yield return null;
        }

        Destroy(_Text.transform.gameObject);
    }

    IEnumerator ColorChage()
    {

        for (int i = 0; i < Render.Length; i++)
            Render[i].material.color = Color.red;

        yield return new WaitForSeconds(1);

        for (int i = 0; i < Render.Length; i++)
            Render[i].material.color = OriginColor[i];
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && other.gameObject.GetComponent<PlayerBace>().PV.IsMine)
            other.gameObject.GetComponent<PlayerBace>().PV.RPC("DatageHit", RpcTarget.All, Damage, Team);
        else if (other.tag == "Tower" && PV.IsMine && other.gameObject.GetComponent<TowerStat>().HP > 0)
            other.gameObject.GetComponent<TowerStat>().PV.RPC("TowerHit", RpcTarget.All, Damage, Team);
    }

    public void Heel(int _HeelValue) { if (gameObject.tag == "Player") NowHP += (HP / 100) * _HeelValue; PV.RPC("_HealEffect", RpcTarget.All); }


    void Update()
    {
        NickName.transform.gameObject.transform.rotation = Camera.main.transform.rotation;

        if (PV.IsMine)
        {
            if (NetworkManager.Instance.GameChat.activeSelf == false)
            {
                if (!delay && !Stun && gameObject.tag == "Player")
                {
                    transform.Rotate(0, Input.GetAxis("Mouse X"), 0);

                    PlayerAttack();
                }

                MoveAnime();
            }

            NowHP = Math.Clamp(NowHP, 0, HP);
            GameDataManager.Instance.HP.fillAmount = NowHP / HP;
            GameDataManager.Instance.LerpHP.fillAmount = Mathf.Lerp(GameDataManager.Instance.LerpHP.fillAmount, GameDataManager.Instance.HP.fillAmount, Time.deltaTime);

            GameObject.Find("Canvas").transform.Find("Stat").Find("PlayerStat").GetComponent<Text>().text =
                "체력 : " + NowHP + " 스피드 : " + Speed + " 데미지 : " + Damage;
        }
        else
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, CurRot, 10f * Time.deltaTime);
        }
    }
    private void FixedUpdate()
    {
        if(PV.IsMine)
        {
            if (NetworkManager.Instance.GameChat.activeSelf == false)
            {
                if (!Anime.GetBool("Rock") && !delay && !Stun && gameObject.tag == "Player" && !Anime.GetBool("Attack"))
                {
                    Move_X = (Input.GetAxis("Horizontal") * Rd.transform.right.x + Input.GetAxis("Vertical") * Rd.transform.forward.x) * Speed;
                    Move_Z = (Input.GetAxis("Vertical") * Rd.transform.forward.z + Input.GetAxis("Horizontal") * Rd.transform.right.z) * Speed;
                    Rd.velocity = new Vector3(Move_X, Rd.velocity.y, Move_Z);
                }
            }
        }
        else Rd.transform.position = Vector3.Lerp(Rd.transform.position, CurPos, 10f * Time.deltaTime);
    }
}
