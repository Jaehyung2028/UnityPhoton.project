using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollScript : MonoBehaviour
{
    bool InPlayer = false;

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

    private void Update()
    {
        if (InPlayer && Input.GetKeyDown(KeyCode.E))
        {
            Player.Instance.Scroll += 1;
            ButtonManager.instance.ItemText.text = "X " + Player.Instance.Scroll;
            Destroy(gameObject);
        }
    }
}
