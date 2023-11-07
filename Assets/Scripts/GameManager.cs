using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager sharedInstance;

    [Header("Game Configuration")]
    public Level[] levelsConfig;

    [Header("Jumpers")]
    [Tooltip("Base jump delay, match with the first jump speed")]
    [SerializeField] float baseJumpDelay;
    public float spawnLerpSpeed;
    public float platformSpeed;
    [Tooltip("Delay in seconds the jumper have to wait to jump again")]
    [SerializeField] float spawnFallLenght;
    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject ballPrefab;
    [SerializeField] GameObject snakePrefab;
    [SerializeField] int EnemiesSpawnLevel;

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

    [Header("UI")]
    [SerializeField] TextMeshProUGUI scoreTextField;

    private AudioSource audioSource;

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
    [HideInInspector]
    public bool levelCompleted;
    private int lives;

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
        audioSource = this.gameObject.GetComponent<AudioSource>();
    }

    private void Start()
    {
        // todo: move all of this to other place
        // todo: update the speed on every level
        piramidMap = Builder.BuildPiramidMap(_piramidSpawnPoint, _piramidLevels, _stepDistance, TilePrefab, CubePrefab);
        score = 0;
        lives = 4;
        StartCoroutine("BallsSpawnCicle");
        SetUpLevel(0);
    }

    private void SetUpLevel(int levelIndex)
    {
        if (playerJumper != null)
        {
            Destroy(playerJumper.gameObject);

        }

        enemiesSpawning = true;
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
    }

    private void CleanSceneFromObjects()
    {
        //if (playerJumper != null)
        //{
        //    Destroy(playerJumper.gameObject);
        //}
        if (currentEnemies != null)
        {
            AddScore(currentEnemies.Count * 500);
            foreach (Jumper enemy in currentEnemies)
            {
                //Debug.Log(enemy.gameObject.name);
                Destroy(enemy.gameObject);
            }
            currentEnemies.Clear();
        }
        if (currentPlatforms != null)
        {
            AddScore(currentPlatforms.Count * 500);
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
        lives--;
        Vector2 logicalSpawnPoint = Vector2.zero;
        playerJumper = SpawnJumper(playerPrefab, logicalSpawnPoint);
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

    private void LevelComplete()
    {
        enemiesSpawning = false;
        levelCompleted = true;
        //TODO: animation change color of all tiles, play win sound, wait and set up the next level
        StartCoroutine("LevelCompleteTilesAnimation");
        audioSource.PlayOneShot(levelCompleteMusic);
        CleanSceneFromObjects();

        //todo: count platforms and add points to them
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

    public void AddScore(int amount)
    {
        score += amount;
        scoreTextField.text = $"Score: {score}";
        //TODO: update the UI
    }

    public void EnemyDied(Jumper enemy)
    {
        currentEnemies.Remove(enemy);
    }

    public void StepOnTile(Vector2 logicalCoordinates, TileInteractions interactionType)
    {
        if (levelCompleted) return;
        int rowMapIndex = (int)logicalCoordinates.x + (int)logicalCoordinates.y;
        GameObject currentTile = piramidMap[rowMapIndex][(int)logicalCoordinates.x];
        SpriteRenderer tileSpriteRenderer = currentTile.GetComponent<SpriteRenderer>();

        if (interactionType == TileInteractions.player)
        {
            Color currentTileColor = tileSpriteRenderer.color;
            int tileColorIndex = findColorIndexOnTile(currentTileColor);
            int originalTileColorIndex = tileColorIndex;
            if (tileColorIndex == -1)
            {
                Debug.LogError("current tile color is outside of posible colors");
            }
            if (currentLevel.tileBehavior == TileBehavior.simpleBiColor || currentLevel.tileBehavior == TileBehavior.simpleTriColor)
            {
                if (tileColorIndex != currentLevel.tileColors.Length - 1)
                {
                    tileColorIndex++;
                    AddScore(25);
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
                    AddScore(25);
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
                else if (tilesCompleted == 3)
                {
                    SpawnSnake();
                }
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
        enemiesSpawning = true;
        snakeOnGame = false;
        DestroyEntity(platform2Delete.gameObject);
    }

    public void PlayerDied()
    {
        enemiesSpawning = false;
        if (currentEnemies != null)
        {
            foreach (Jumper enemy in currentEnemies)
            {
                //Debug.Log(enemy.gameObject.name);
                Destroy(enemy.gameObject);
            }
            currentEnemies.Clear();
            snakeOnGame = false;
        }
        lives--;
        if (lives > 0)
        {
            StartCoroutine("RespawnPlayer");
        }
        else
        {
            //game over
        }

    }

    private void DestroyEntity(GameObject entity)
    {
        //TODO: add particles
        GameObject.Destroy(entity);
    }

    IEnumerator BallsSpawnCicle()
    {
        yield return new WaitForSeconds(1f);
        while (true)
        {
            float spawnDelay = Random.Range(currentLevel.spawnBallMinDelay, currentLevel.spawnBallMaxDelay + 1);
            yield return new WaitForSeconds(spawnDelay);
            if (enemiesSpawning)
            {
                SpawnEnemy(ballPrefab);
            }
        }
    }

    public void SpawnSnake()
    {
        StartCoroutine("SnakeSpawnCicle");
    }

    IEnumerator SnakeSpawnCicle()
    {
        while (true)
        {
            float spawnDelay = Random.Range(currentLevel.spawnSnakeMinDelay, currentLevel.spawnSnakeMaxDelay + 1);
            yield return new WaitForSeconds(spawnDelay);
            if (snakeOnGame == false && enemiesSpawning)
            {
                SpawnEnemy(snakePrefab);
                snakeOnGame = true;
            }
        }
    }

    IEnumerator RespawnPlayer()
    {
        //play a sound;
        yield return new WaitForSeconds(3f);
        SpawnPlayer();
        enemiesSpawning = true;
    }
}