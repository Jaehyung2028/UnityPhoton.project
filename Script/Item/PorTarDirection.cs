using System.Collections.Generic;
using UnityEngine;


public class PorTarDirection : MonoBehaviour
{
    [SerializeField] GameObject Open, Close;
    public string Direction;

    public void CanMove()
    {
        Close.SetActive(false);
        Open.SetActive(true);
    }

    public void DontMove()
    {
        Close.SetActive(true);
        Open.SetActive(false);
    }
}
