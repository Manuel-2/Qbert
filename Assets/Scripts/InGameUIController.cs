using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine;

public class InGameUIController : MonoBehaviour
{
    public static InGameUIController sharedInstance;

    [SerializeField] GameObject gameOverScreen;
    [SerializeField] int mainMenuSceneIndex;

    [Header("In Game UI")]
    [SerializeField] TextMeshProUGUI progressField;
    [SerializeField] Image targetTile;
    [SerializeField] Image targetBlock;
    [Space]
    [SerializeField] TextMeshProUGUI scoreTextField;
    [SerializeField] GameObject livesContainer;
    [SerializeField] GameObject liveUiSpritePrefab;
    [SerializeField] List<GameObject> liveSprites;

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
        //Todo: call an animation
        gameOverScreen.SetActive(true);
    }

    public void ReturnMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneIndex);
    }

    public void UpdateScoreField(int score)
    {
        scoreTextField.text = $"Score: {score}";

    }

    public void UpdateLivesField(int lives)
    {
        if (lives < liveSprites.Count)
        {
            int spriteIndex = liveSprites.Count - 1;
            Destroy(liveSprites[spriteIndex]);
            liveSprites.RemoveAt(spriteIndex);
        }
        else
        {
            GameObject liveSprite = Instantiate(liveUiSpritePrefab, livesContainer.transform);
            liveSprites.Add(liveSprite);
        }
    }

    public void UpdateProgressFields(int stage,int level, Color tileColor, Color blockColor)
    {
        progressField.text = $"Stage: {stage} Level: {level}";
        targetTile.color = tileColor;
        targetBlock.color = blockColor;
    }
}
