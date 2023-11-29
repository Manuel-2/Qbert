using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager sharedInstance;

    [Header("Game Configuration")]
    [SerializeField] float points2ExtraLife;
    public Level[] levelsConfig;

    [Header("Jumpers")]
    [Tooltip("Base jump delay, match with the first jump speed")]
    [SerializeField] float baseJumpDelay;
    public float spawnLerpSpeed;
    public float platformSpeed;
    [Tooltip("Delay in seconds the jumper have to wait to jump again")]
    [SerializeField] float spawnFallLenght;
    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject[] Jumpers;
    [SerializeField] int EnemiesSpawnLevel;
    [SerializeField] int trollSpawnChanse;

    [Header("PiramidContruction")]
    [SerializeField] [Min(2)] private int _piramidLevels;
    [SerializeField] Transform _piramidSpawnPoint;
    [SerializeField] Vector2 _stepDistance;
    [SerializeField] GameObject TilePrefab, CubePrefab;
    [SerializeField] GameObject platformPrefab;
    public float platformYOffset;
    [Header("Level Complete Animation")]
    [Tooltip("In seconds")]
    [SerializeField] float levelCompleteAnimationDuration;
    [Tooltip("Per second")]
    [SerializeField] float colorChangeSpeed;
    [SerializeField] Color[] levelCompleteAnimationColors;
    [SerializeField] AudioClip levelCompleteMusic;
    [SerializeField] AudioClip GameOverSong;
    [Space]
    [SerializeField] AudioSource backgroundAudioSource;

    public int totalPiramidLevels
    {
        get
        {
            return _piramidLevels;
        }
    }
    public Transform piramidSpawnPoint
    {
        get
        {
            return _piramidSpawnPoint;
        }
    }
    public Vector2 stepDistance
    {
        get
        {
            return _stepDistance;
        }
    }


    // Hide in the inspector
    [HideInInspector] public Jumper playerJumper;
    [HideInInspector] public float currentSpeedUpFactor;
    [HideInInspector] public float currentJumpSpeed;
    [HideInInspector] public float currentJumpDelay;
    [HideInInspector] public bool snakeOnGame;

    private int currentLevelIndex;
    private Level currentLevel;
    private List<List<GameObject>> piramidMap;
    private int tilesCompleted;
    private List<Platform> currentPlatforms;
    private Platform activePlatform;
    private bool enemiesSpawning;
    private List<Jumper> currentEnemies;

    private int score;
    private int highScore;
    [HideInInspector]
    public bool levelCompleted;
    private int lives;
    private int extraLivesScore;
    private int stage;
    private int level;

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
        level = 0;
    }

    private void Start()
    {
        currentLevel = levelsConfig[0];
        StartSpawnCicle();
        piramidMap = Builder.BuildPiramidMap(_piramidSpawnPoint, _piramidLevels, _stepDistance, TilePrefab, CubePrefab);
        score = 0;
        lives = 3;
        SetUpLevel(0);
        highScore = PlayerPrefs.GetInt("highScore", 0);
    }

    private void SetUpLevel(int levelIndex)
    {
        level++;
        if (level > 3)
        {
            level = 0;
        }
        stage = (int)System.Math.Ceiling((levelIndex + 1) / 3f);
        if (playerJumper != null)
        {
            Destroy(playerJumper.gameObject);

        }
        backgroundAudioSource.Play();

        StartCoroutine("ReactivateEnemysSpawn");
        snakeOnGame = false;
        levelCompleted = false;
        tilesCompleted = 0;

        currentEnemies = new List<Jumper>();
        SpawnPlayer();

        currentLevelIndex = levelIndex;
        currentLevel = levelsConfig[currentLevelIndex];
        SetGameSpeed(currentLevelIndex);

        // spawn platforms
        currentPlatforms = new List<Platform>();
        if (currentLevel.platforms > _piramidLevels * 2 - 2)
        {
            string errorMessage = $"Level: {currentLevelIndex} has platform overflow. the max platform posible is: {_piramidLevels * 2 - 2}";
            Debug.LogError(errorMessage);
            return;
        }
        for (int i = 0; i < currentLevel.platforms; i++)
        {
            Vector2 platformLogicalCoordinate = generatePlatformCoordinates();
            Vector2 platformPosition = Builder.sharedInstance.ConvertLogicalCoordinates2GlobalPosition(platformLogicalCoordinate);
            //platformPosition.y += platformYOffset;
            var newPlatform = Instantiate(platformPrefab, platformPosition, Quaternion.identity, _piramidSpawnPoint).GetComponent<Platform>();
            newPlatform.loogicalCoordinates = platformLogicalCoordinate;
            currentPlatforms.Add(newPlatform);
        }

        //paint the piramid
        piramidMap.ForEach(delegate (List<GameObject> row)
        {
            row.ForEach(delegate (GameObject piramidTile)
            {
                piramidTile.GetComponent<SpriteRenderer>().color = currentLevel.tileColors[0];
                SpriteRenderer piramidCubeRenderer = piramidTile.transform.GetChild(0).GetComponent<SpriteRenderer>();
                piramidCubeRenderer.color = currentLevel.blockColor;
            });
        });

        InGameUIController.sharedInstance.UpdateProgressFields(stage, level, currentLevel.tileColors[currentLevel.tileColors.Length - 1], currentLevel.blockColor);
        if (stage > 1)
        {
            trollSpawnChanse = 10 * 2;
        }
        else if (stage > 2)
        {
            trollSpawnChanse = 10 * 3;
        }
    }

    private void CleanSceneFromObjects()
    {
        if (currentEnemies != null)
        {
            UpdateScore(currentEnemies.Count * 500);
            foreach (Jumper enemy in currentEnemies)
            {
                Destroy(enemy.gameObject);
            }
            currentEnemies.Clear();
        }
        if (currentPlatforms != null)
        {
            UpdateScore(currentPlatforms.Count * 500);
            foreach (Platform platform in currentPlatforms)
            {
                Destroy(platform.gameObject);
            }
        }
    }

    private Vector2 generatePlatformCoordinates()
    {
        int pos = Random.Range(1, _piramidLevels);
        Vector2 platformLogicalCoordinate = new Vector2(-1, pos);
        bool coinFlip = Random.Range(0, 2) > 0;
        if (coinFlip) platformLogicalCoordinate = new Vector2(pos, -1);

        if (currentPlatforms.Count > 0)
        {
            foreach (var platform in currentPlatforms)
            {
                if (platformLogicalCoordinate == platform.loogicalCoordinates)
                {
                    return generatePlatformCoordinates();
                }
            }
        }
        return platformLogicalCoordinate;
    }

    public Platform Check4SavePlatform(Vector2 targetLogicalCoordinates)
    {
        foreach (Platform platform in currentPlatforms)
        {
            if (platform.loogicalCoordinates == targetLogicalCoordinates)
            {
                return platform;
            }
        }
        return null;
    }

    private void SetGameSpeed(int levelIndex)
    {
        currentJumpSpeed = levelsConfig[levelIndex].lerpSpeed;
        currentSpeedUpFactor = currentJumpSpeed / levelsConfig[0].lerpSpeed;
        currentJumpDelay = baseJumpDelay / currentSpeedUpFactor;
    }

    private void SpawnPlayer()
    {
        backgroundAudioSource.Play();
        Vector2 logicalSpawnPoint = Vector2.zero;
        playerJumper = SpawnJumper(playerPrefab, logicalSpawnPoint);
        StartCoroutine("ReactivateEnemysSpawn");
    }

    private Jumper SpawnEnemy(GameObject enemyPrefab)
    {
        bool coinFlip = Random.Range(0, 2) > 0;
        Vector2 logicalSpawnPoint = new Vector2(0, EnemiesSpawnLevel);
        if (coinFlip)
        {
            logicalSpawnPoint = new Vector2(EnemiesSpawnLevel, 0);
        }
        Jumper jumper = SpawnJumper(enemyPrefab, logicalSpawnPoint);
        if (enemyPrefab.name == "EnemySnake")
        {
            snakeOnGame = true;
        }
        currentEnemies.Add(jumper);
        return jumper;
    }

    private Jumper SpawnJumper(GameObject jumperPrefab, Vector2 logicalSpawnPoint)
    {
        // create a jumper, put it on top of its spawn point
        //todo: a new fuction to select a random jumper
        //todo: The gameManager interpolates the jumper position at spawn(the fall), don't use the jumper's jump interpolator

        GameObject newJumper = Instantiate(jumperPrefab);
        Vector3 globalSpawnPointTarget = Builder.sharedInstance.ConvertLogicalCoordinates2GlobalPosition(logicalSpawnPoint);
        newJumper.transform.position = new Vector3(globalSpawnPointTarget.x, globalSpawnPointTarget.y + spawnFallLenght);
        var jumperComponent = newJumper.GetComponent<Jumper>();
        jumperComponent.InitializeJumper(logicalSpawnPoint);

        return jumperComponent;
    }

    public void StartSpawnCicle()
    {
        StartCoroutine("SpawnCicle");
    }

    private void LevelComplete()
    {
        backgroundAudioSource.Stop();
        enemiesSpawning = false;
        levelCompleted = true;
        StartCoroutine("LevelCompleteTilesAnimation");
        backgroundAudioSource.PlayOneShot(levelCompleteMusic);
        CleanSceneFromObjects();
    }

    private void ChangeAllTilesColorEfect(Color color)
    {
        foreach (List<GameObject> row in piramidMap)
        {
            foreach (GameObject tile in row)
            {
                tile.GetComponent<SpriteRenderer>().color = color;
            }
        }
    }

    public void UpdateScore(int amount)
    {
        score += amount;
        extraLivesScore += amount;
        score = Mathf.Clamp(score, 0, int.MaxValue);

        if (extraLivesScore > points2ExtraLife)
        {
            extraLivesScore = 0;
            lives++;
            InGameUIController.sharedInstance.UpdateLivesField(lives);
        }
        InGameUIController.sharedInstance.UpdateScoreField(score);
    }

    public void EnemyDied(Jumper enemy)
    {
        currentEnemies.Remove(enemy);
    }

    public void StepOnTile(Vector2 logicalCoordinates, TileInteractions interactionType)
    {
        if (levelCompleted || interactionType == TileInteractions.snake) return;
        int rowMapIndex = (int)logicalCoordinates.x + (int)logicalCoordinates.y;
        GameObject currentTile = piramidMap[rowMapIndex][(int)logicalCoordinates.x];
        SpriteRenderer tileSpriteRenderer = currentTile.GetComponent<SpriteRenderer>();

        Color currentTileColor = tileSpriteRenderer.color;
        int tileColorIndex = findColorIndexOnTile(currentTileColor);
        int originalTileColorIndex = tileColorIndex;

        if (interactionType == TileInteractions.player)
        {
            if (tileColorIndex == -1)
            {
                Debug.LogError("current tile color is outside of posible colors");
            }
            if (currentLevel.tileBehavior == TileBehavior.simpleBiColor || currentLevel.tileBehavior == TileBehavior.simpleTriColor)
            {
                if (tileColorIndex != currentLevel.tileColors.Length - 1)
                {
                    tileColorIndex++;
                    UpdateScore(25);
                }

            }
            else if (currentLevel.tileBehavior == TileBehavior.reversibleBiColor || currentLevel.tileBehavior == TileBehavior.reversibleTriColor)
            {
                if (tileColorIndex == currentLevel.tileColors.Length - 1)
                {
                    tileColorIndex--;
                    tilesCompleted--;
                }
                else
                {
                    tileColorIndex++;
                    UpdateScore(25);
                }
            }

            tileSpriteRenderer.color = currentLevel.tileColors[tileColorIndex];
            if (tileColorIndex == currentLevel.tileColors.Length - 1 && originalTileColorIndex != tileColorIndex)
            {
                tilesCompleted++;
                if (tilesCompleted == 28)
                {
                    LevelComplete();
                }
            }
        }
        else if (interactionType == TileInteractions.reverse)
        {
            if (tileColorIndex > 0)
            {
                if (tileColorIndex == currentLevel.tileColors.Length - 1)
                {
                    tilesCompleted--;
                }
                tileColorIndex--;
                tileSpriteRenderer.color = currentLevel.tileColors[tileColorIndex];
            }
        }
    }

    private int findColorIndexOnTile(Color currentColor)
    {
        for (int i = 0; i < currentLevel.tileColors.Length; i++)
        {
            if (currentColor == currentLevel.tileColors[i])
            {
                return i;
            }
        }
        return -1;
    }

    public void ActivateRainbowPlatform(Platform currentPlatform)
    {
        playerJumper.transform.parent = currentPlatform.transform;
        enemiesSpawning = false;
        currentPlatform.ActivatePlatform();
        activePlatform = currentPlatform;
    }

    public void EndRainbowPlatform()
    {
        Platform platform2Delete = activePlatform;
        activePlatform = null;
        currentPlatforms.Remove(platform2Delete);
        playerJumper.jumpOffRainbowPlatform();
        //kill remaining enemies
        foreach (Jumper enemy in currentEnemies)
        {
            //TODO: kill the enemies one by one, not all in the same frame
            DestroyEntity(enemy.gameObject);
        }
        currentEnemies.Clear();
        StartCoroutine("ReactivateEnemysSpawn");
        snakeOnGame = false;
        DestroyEntity(platform2Delete.gameObject);
    }

    public void KillPlayer()
    {
        if (activePlatform != null) return;
        playerJumper.isAlive = false;
        playerJumper.RestoreScaleAnimation();
        UpdateScore(-300);
        playerJumper.StartBlinkAnimation();
        backgroundAudioSource.Pause();
        // disclamer! the level is not complete just a trick to stop the player for jumping
        levelCompleted = true;
        enemiesSpawning = false;
        if (currentEnemies != null)
        {
            foreach (Jumper enemy in currentEnemies)
            {
                Destroy(enemy.gameObject);
            }
            currentEnemies.Clear();
            snakeOnGame = false;
        }
        lives--;
        InGameUIController.sharedInstance.UpdateLivesField(lives);
        if (lives > 0)
        {
            StartCoroutine("RespawnPlayer");
        }
        else
        {
            GameOver();
        }
    }

    private void GameOver()
    {
        backgroundAudioSource.Stop();
        backgroundAudioSource.PlayOneShot(GameOverSong);
        enemiesSpawning = false;
        InGameUIController.sharedInstance.ShowGameoverScreen();
        if (score > highScore)
        {
            PlayerPrefs.SetInt("highScore", score);
        }
        // update gameOver ui
        InGameUIController.sharedInstance.UpdateGameOverScreen(stage, level, score, PlayerPrefs.GetInt("highScore", 0));
    }

    private void DestroyEntity(GameObject entity)
    {
        //TODO: add particles
        GameObject.Destroy(entity);
    }

    IEnumerator LevelCompleteTilesAnimation()
    {
        float currentTime = 0;
        int colorIndex = 0;
        while (currentTime <= levelCompleteAnimationDuration)
        {
            ChangeAllTilesColorEfect(levelCompleteAnimationColors[colorIndex]);
            yield return new WaitForSeconds(1 / colorChangeSpeed);
            currentTime += 1 / colorChangeSpeed;
            if (colorIndex == levelCompleteAnimationColors.Length - 1)
            {
                colorIndex = 0;
            }
            else
            {
                colorIndex++;
            }
        }
        ChangeAllTilesColorEfect(Color.white);
        SetUpLevel(currentLevelIndex + 1);
    }

    IEnumerator SpawnCicle()
    {
        while (true)
        {
            if (enemiesSpawning)
            {
                // change this if you add more enemies,generate a random index an select the prefab on an Array
                int jumper2SpawnIndex = -1;
                int Troll = Random.Range(0, 101);

                if (Troll < trollSpawnChanse)
                {
                    jumper2SpawnIndex = Random.Range(Jumpers.Length - 2, Jumpers.Length);
                }
                else if (snakeOnGame)
                {
                    jumper2SpawnIndex = 1;
                }
                else
                {
                    jumper2SpawnIndex = Random.Range(0, 1);
                }
                SpawnEnemy(Jumpers[jumper2SpawnIndex]);
            }
            yield return new WaitForSeconds(currentLevel.spawnDelay);
        }
    }

    IEnumerator RespawnPlayer()
    {
        //play a sound;
        yield return new WaitForSeconds(3f);
        if (playerJumper != null)
        {
            Destroy(playerJumper.gameObject);
        }
        // this revets the changes on PlayerDiedMetoh on LevelCompleted
        levelCompleted = false;
        SpawnPlayer();
    }

    IEnumerator ReactivateEnemysSpawn()
    {
        yield return new WaitForSeconds(3f);
        enemiesSpawning = true;
    }
}