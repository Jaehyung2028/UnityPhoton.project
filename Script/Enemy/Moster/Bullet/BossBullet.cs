using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossBullet : MonoBehaviour
{
    [SerializeField] BossEnemy Boss_Script;

    [SerializeField] Transform Bullet_Parent;

    [SerializeField] GameObject[] Body;

    [SerializeField] CircleCollider2D Hit_Coll;

    [SerializeField] ParticleSystem DestroyEffect;
    bool Hit = false;


    // 몬스터에서 총알 오브젝트를 큐로 관리 하여 풀링 기법을 사용
    private void OnEnable() { Hit_Coll.enabled = true; Hit = false; }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player" || collision.tag == "Ground")
            StartCoroutine(DestroyControl());
    }

    // 충돌 감지 후에 오브젝트의 컬러의 알파 값을 선형보간을 이용하여 천천히 투명해지도록 설정
    IEnumerator DestroyControl()
    {
        Hit = true;

        Hit_Coll.enabled = false;

        for (int i = 0; i < Body.Length; i++)
        {
            Body[i].SetActive(false);
        }

        DestroyEffect.Play();

        yield return new WaitForSeconds(1.5f);

        // 오브젝트에 부모를 대입 시키고 다시 큐에 대입
        Boss_Script.Bullet_List.Enqueue(gameObject);
        gameObject.transform.SetParent(Bullet_Parent);

        gameObject.transform.localPosition = Vector3.zero;
        gameObject.transform.localRotation = Quaternion.Euler(0, 0, 0);

        for (int i = 0; i < Body.Length; i++)
        {
            Body[i].SetActive(true);
        }

        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!Hit)
            transform.Translate(-transform.right * 10 * Time.deltaTime, Space.World);

        // 큐에 대입되기 전 맵 초기화시 오브젝트 삭제
        if (Map.Instance.Reset) Destroy(gameObject);
    }
}
