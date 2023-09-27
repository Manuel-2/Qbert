using System.Collections;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class UIButton : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] int verticalIndex;
    [SerializeField] TextMeshProUGUI text;
    Button buttonComponent;
    RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = this.gameObject.GetComponent<RectTransform>();
        buttonComponent = this.gameObject.GetComponent<Button>();
    }

    public void Click()
    {
        buttonComponent.OnPointerClick(new PointerEventData(EventSystem.current));
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UIController.sharedInstance.MouseSelect(verticalIndex);
        rectTransform.DOScale(UIController.sharedInstance.selectedButtonScale, UIController.sharedInstance.selectedButtonAnimationTime).SetEase(UIController.sharedInstance.ButtonScaleEase);
        text.color = UIController.sharedInstance.selectedButtonColor;
    }

    public void UnSelect()
    {
        rectTransform.DOScale(1, UIController.sharedInstance.selectedButtonAnimationTime).SetEase(UIController.sharedInstance.ButtonScaleEase);
        text.color = UIController.sharedInstance.unSelectedButtonColor;
    }
}