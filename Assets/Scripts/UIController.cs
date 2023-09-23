using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public static UIController sharedInstance;

    public float buttonShakeDuration, buttonShakeStreng, buttonShakeVibrato;

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
