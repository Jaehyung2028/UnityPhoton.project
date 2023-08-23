using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SoulBox : MonoBehaviour
{
    [SerializeField] GameObject Idle, Open, Key;
    [SerializeField] ParticleSystem GetEffet;
    bool Close = false, Active = true, InPlayer = false;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.tag == "Player")
            InPlayer = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.tag == "Player")
            InPlayer = false;
    }

    IEnumerator KeyOpen()
    {
        if (Player.Instance.Scroll > 0)
        {
            if (!Close)
            {
                GetEffet.Play();
                Idle.SetActive(false);
                Open.SetActive(true);
                Key.SetActive(true);

                yield return new WaitForSeconds(1);

                Close = true;
            }
        }
        else
        {
            ButtonManager.instance.Alarm("<color=green>Scrolls</color> must be collected.");
        }
    }


    // 카메라 진동 효과 구현
    IEnumerator CameraShake()
    {
        Camera Ca = Camera.main;
        Vector3 Camera_Pos = Ca.transform.position;

        float CurTime = 3;


        while (CurTime > 0)
        {
            Ca.transform.position = new Vector3(Random.insideUnitCircle.x, Random.insideUnitCircle.y) * 0.5f + Camera_Pos;

            CurTime -= Time.deltaTime;

            yield return null;
        }

        Ca.transform.position = Camera_Pos;

    }

    private void Update()
    {
        if (InPlayer && Input.GetKeyDown(KeyCode.E) && Close && Player.Instance.Scroll > 0 && Active)
        {
            Key.SetActive(false);
            Active = false;
            Player.Instance.Soul++;
            Player.Instance.Scroll--;
            ButtonManager.instance.SoulText.text = "X " + Player.Instance.Soul;
            ButtonManager.instance.ItemText.text = "X " + Player.Instance.Scroll;

            if (Player.Instance.Soul == Map.Instance.HiddenCount)
            {
                StartCoroutine(CameraShake());
                ButtonManager.instance.Alarm("<color=red>The boss room has been opened.</color>");
            }
        }
        else if(InPlayer && Input.GetKeyDown(KeyCode.E) && !Close && Active)
        {
            StartCoroutine(KeyOpen());
        }
    }
}
