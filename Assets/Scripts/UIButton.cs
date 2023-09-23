using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class UIButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = this.gameObject.GetComponent<RectTransform>();
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        rectTransform.DOShakePosition(30);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
       
    }
}