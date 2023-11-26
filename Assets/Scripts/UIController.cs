using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIController : MonoBehaviour
{
    public static UIController sharedInstance;

    [SerializeField] UIButton[] buttons;
    public float selectedButtonScale;
    public float selectedButtonAnimationTime;
    public Ease ButtonScaleEase;
    public Color selectedButtonColor, unSelectedButtonColor;
    [Header("Scene indexs")]
    [SerializeField] int playScene;

    private int selectedButtonIndex;

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

    private void Start()
    {
        Application.targetFrameRate = 60;
        selectedButtonIndex = 2;
        buttons[selectedButtonIndex].OnPointerEnter(new PointerEventData(EventSystem.current));
    }

    private void Update()
    {
        //todo: define the controls

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            //buttons[selectedButtonIndex].OnPointerExit(new PointerEventData(EventSystem.current));
            buttons[selectedButtonIndex].UnSelect();
            selectedButtonIndex--;
            if (selectedButtonIndex < 0)
            {
                selectedButtonIndex = buttons.Length - 1;
            }
            buttons[selectedButtonIndex].OnPointerEnter(new PointerEventData(EventSystem.current));
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            //buttons[selectedButtonIndex].OnPointerExit(new PointerEventData(EventSystem.current));
            buttons[selectedButtonIndex].UnSelect();
            selectedButtonIndex++;
            if (selectedButtonIndex > buttons.Length - 1)
            {
                selectedButtonIndex = 0;
            }
            buttons[selectedButtonIndex].OnPointerEnter(new PointerEventData(EventSystem.current));
        }
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            buttons[selectedButtonIndex].Click();
        }
    }

    public void MouseSelect(int buttonIndex)
    {
        foreach(UIButton button in buttons)
        {
            //button.OnPointerExit(new PointerEventData(EventSystem.current));
            buttons[selectedButtonIndex].UnSelect();
        }
        selectedButtonIndex = buttonIndex;
    }

    public void ClickPlay()
    {
        TransitionManager.sharedInstance.LoadScene(playScene);
    }

    public void ClickSettings()
    {
        Debug.Log("Settings");
    }

    public void ClickCreddits()
    {
        Debug.Log("Creddits");
    }

    public void ClickExitGame()
    {
        Debug.Log("Exit");
    }
}
