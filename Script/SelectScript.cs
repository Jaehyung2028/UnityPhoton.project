using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SelectScript : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] Image[] Select_Image;
    [SerializeField] string ImageNumber;


    // 캐릭터 선택 창에서 클릭시 해당 캐릭터의 이름을 게임 매니저에 넘김
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
