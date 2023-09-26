using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public static UIController sharedInstance;

    [SerializeField] UIButton[] buttons;
    public float selectedButtonScale;
    public float selectedButtonAnimationTime;
    public Ease ButtonScaleEase;
    public Color selectedButtonColor,unSelectedButtonColor;

    private void Awake()
    {
        if (sharedInstance == null)
        {
            sharedInstance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void ClickPlay()
    {

    }

    public void ClickSettings()
    {

    }

    public void ClickHowToPlay()
    {

    }

    public void ClickCreddits()
    {

    }

    public void ClickExitGame()
    {

    }
}
