using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class InGameUIController : MonoBehaviour
{
    public static InGameUIController sharedInstance;

    [SerializeField] GameObject gameOverScreen;
    [SerializeField] int mainMenuSceneIndex;

    private void Awake()
    {
        if (sharedInstance == null)
        {
            sharedInstance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public void ShowGameoverScreen()
    {
        gameOverScreen.SetActive(true);
    }

    public void ReturnMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneIndex);
    }
}
