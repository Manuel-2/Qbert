using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using TMPro;
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
    [Space]
    [SerializeField] TextMeshProUGUI highScoreText;


    private int selectedButtonIndex;
    private bool control;

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
        selectedButtonIndex = 0;
        buttons[selectedButtonIndex].OnPointerEnter(new PointerEventData(EventSystem.current));

        control = GameManager.sharedInstance == null;

        int highScore = PlayerPrefs.GetInt("highScore", 0);
        highScoreText.text = $"HighScore: {highScore}";
    }

    private void Update()
    {

        if (control)
        {
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


    }

    public void EnableControl()
    {
        control = true;
    }

    public void MouseSelect(int buttonIndex)
    {
        foreach (UIButton button in buttons)
        {
            //button.OnPointerExit(new PointerEventData(EventSystem.current));
            buttons[selectedButtonIndex].UnSelect();
        }
        selectedButtonIndex = buttonIndex;
    }

    public void ResetHighScore()
    {
        PlayerPrefs.SetInt("highScore", 0);
        highScoreText.text = $"HighScore: {0}";
    }

    public void SetTutorialSkipPreference(bool skipTutorial)
    {
        // 0 is false, 1 is true
        int skipValue = 0;
        if (skipTutorial)
        {
            skipValue = 1;
        }
        PlayerPrefs.SetInt("skipTutorial", skipValue);
    }

    public void ClickPlay()
    {
        TransitionManager.sharedInstance.LoadScene(playScene);
    }

    public void ClickExitGame()
    {
        Application.Quit();
    }
}
