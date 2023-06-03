using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SelectScript : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] Image[] Select_Image;
    [SerializeField] string ImageNumber;


    // ĳ���� ���� â���� Ŭ���� �ش� ĳ������ �̸��� ���� �Ŵ����� �ѱ�
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            for (int i = 0; i < Select_Image.Length; i++)
            {
                if (ImageNumber == Select_Image[i].name)
                    Select_Image[i].gameObject.SetActive(true);
                else
                    Select_Image[i].gameObject.SetActive(false);
            }
            switch (ImageNumber)
            {
                case "1":
                    GameDataManager.Instance.SelectCharaterName = "Warrior";
                    break;
                case "2":
                    GameDataManager.Instance.SelectCharaterName = "Fighter";
                    break;
                case "3":
                    GameDataManager.Instance.SelectCharaterName = "Magic";
                    break;
                case "4":
                    GameDataManager.Instance.SelectCharaterName = "Assassin";
                    break;
                default:
                    break;
            }
        }

    }
}
