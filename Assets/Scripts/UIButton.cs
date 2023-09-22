using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class UIButton : MonoBehaviour
{
    [SerializeField] RectTransform rectTransform;
    [SerializeField] Button button;

    private void OnMouseEnter()
    {
        rectTransform.localScale = new Vector2(10, 50);
        Debug.Log("entro");
    }
}