using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerBulletl : MonoBehaviour
{
    [SerializeField] CircleCollider2D Coll;

    [SerializeField] TowerMonster TowerEnemy;

    [SerializeField] Transform Pos;

    [SerializeField] ParticleSystem DestroyEffect;

    Rigidbody2D PlayerPos;
    Vector2 Direction;
    bool Hit = false;

    private void Awake() => PlayerPos = GameObject.Find("Player(Clone)").GetComponent<Rigidbody2D>();

    // 몬스터에서 총알 오브젝트를 큐로 관리 하여 풀링 기법을 사용
    private void OnEnable()
    {
        Coll.enabled = true;
        Direction = (PlayerPos.transform.position - transform.position).normalized;
        Hit = false;
        gameObject.transform.SetParent(null);
    }

    void Update()
    {
        if (!Hit) transform.Translate(Direction * 5 * Time.deltaTime, Space.World);

        // 큐에 대입되기 전 맵 초기화시 오브젝트 삭제
        if (Map.Instance.Reset) Destroy(gameObject);
    }

    // 충돌 감지 후에 오브젝트의 컬러의 알파 값을 선형보간을 이용하여 천천히 투명해지도록 설정
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player" || collision.tag == "Ground")
            StartCoroutine(DestroyControl());
    }

    IEnumerator DestroyControl()
    {
        Hit = true;

        Coll.enabled = false;
        gameObject.transform.GetChild(0).gameObject.SetActive(false);

        DestroyEffect.Play();

        yield return new WaitForSeconds(1.5f);

        // 오브젝트에 부모를 대입 시키고 다시 큐에 대입
        TowerEnemy.Bullet_List.Enqueue(gameObject);
        gameObject.transform.SetParent(Pos);

        gameObject.transform.localPosition = Vector3.zero;
        gameObject.transform.localRotation = Quaternion.Euler(0, 0, 0);

        gameObject.transform.GetChild(0).gameObject.SetActive(true);
        gameObject.SetActive(false);
    }
}
