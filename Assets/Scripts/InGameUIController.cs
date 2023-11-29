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
    [SerializeField] int gameSceneIndex;
    [SerializeField] Animator animator;

    [Header("In Game UI")]
    [SerializeField] TextMeshProUGUI progressField;
    [SerializeField] Image targetTile;
    [SerializeField] Image targetBlock;
    [Space]
    [SerializeField] TextMeshProUGUI scoreTextField;
    [SerializeField] TextMeshProUGUI highScoreTextField;
    [SerializeField] GameObject livesContainer;
    [SerializeField] GameObject liveUiSpritePrefab;
    [SerializeField] List<GameObject> liveSprites;

    [Header("GameOver Screen")]
    [SerializeField] TextMeshProUGUI gameOverLevelField;
    [SerializeField] TextMeshProUGUI gameOverScoreField;
    [SerializeField] TextMeshProUGUI gameOverHighScoreField;

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

    private void Start()
    {
        highScoreTextField.text = $"High Score: {PlayerPrefs.GetInt("highScore", 0)}";
    }

    public void ShowGameoverScreen()
    {
        animator.SetTrigger("GameOver");
    }

    public void UpdateGameOverScreen(int stage, int level, int score, int highScore)
    {
        gameOverLevelField.text = $"Stage: {stage} Level: {level}";
        gameOverScoreField.text = $"Score: {score}";
        gameOverHighScoreField.text = $"HighScore: {highScore}";
    }

    public void ReturnMainMenu()
    {
        TransitionManager.sharedInstance.LoadScene(mainMenuSceneIndex);
    }

    public void RetryGame()
    {
        TransitionManager.sharedInstance.LoadScene(gameSceneIndex);
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

    public void UpdateProgressFields(int stage, int level, Color tileColor, Color blockColor)
    {
        progressField.text = $"Stage: {stage} Level: {level}";
        targetTile.color = tileColor;
        targetBlock.color = blockColor;
    }
}
