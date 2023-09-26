using System.Collections;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class UIButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] TextMeshProUGUI text;
    Button buttonComponent;
    RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = this.gameObject.GetComponent<RectTransform>();
        buttonComponent = this.gameObject.GetComponent<Button>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        rectTransform.DOScale(UIController.sharedInstance.selectedButtonScale, UIController.sharedInstance.selectedButtonAnimationTime).SetEase(UIController.sharedInstance.ButtonScaleEase);
        text.color = UIController.sharedInstance.selectedButtonColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        rectTransform.DOScale(1, UIController.sharedInstance.selectedButtonAnimationTime).SetEase(UIController.sharedInstance.ButtonScaleEase);
        text.color = UIController.sharedInstance.unSelectedButtonColor;
    }
}