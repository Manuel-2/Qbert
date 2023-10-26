using System.Collections;
using System.Collections.Generic;
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
    [HideInInspector] public Vector2 playerLogicalCoordinates;
    [HideInInspector] public float currentSpeedUpFactor;
    [HideInInspector] public float currentJumpSpeed;
    [HideInInspector] public float currentJumpDelay;

    private int currentLevelIndex;
    private Level currentLevel;
    private List<List<GameObject>> piramidMap;
    private int tilesCompleted;
    private List<Platform> currentPlatforms;

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
        // todo: move all of this to other place
        // todo: update the speed on every level


        piramidMap = Builder.BuildPiramidMap(_piramidSpawnPoint, _piramidLevels, _stepDistance, TilePrefab, CubePrefab);
        SetUpLevel(0);


        //SpawnEnemy(snakePrefab);
        //SpawnEnemy(ballPrefab);
    }

    private void SetUpLevel(int levelIndex)
    {
        currentLevelIndex = levelIndex;
        currentLevel = levelsConfig[currentLevelIndex];
        SetGameSpeed(currentLevelIndex);
        tilesCompleted = 0;
        SpawnPlayer();

        // spawn platforms
        currentPlatforms = new List<Platform>();
        if(currentLevel.platforms > _piramidLevels *2 - 2)
        {
            string errorMessage = $"Level: {currentLevelIndex} has platform overflow. the max platform posible is: {_piramidLevels * 2 - 2}";
            Debug.LogError(errorMessage);
            return;
        }
        for (int i = 0; i < currentLevel.platforms; i++)
        {
            Vector2 platformLogicalCoordinate = generatePlatformPosition();
            Vector2 platformPosition = Builder.sharedInstance.ConvertLogicalCoordinates2GlobalPosition(platformLogicalCoordinate);
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

    private Vector2 generatePlatformPosition()
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
                    return generatePlatformPosition();
                }
            }
        }
        return platformLogicalCoordinate;
    }

    private void SetGameSpeed(int levelIndex)
    {
        currentJumpSpeed = levelsConfig[levelIndex].lerpSpeed;
        currentSpeedUpFactor = currentJumpSpeed / levelsConfig[0].lerpSpeed;
        currentJumpDelay = baseJumpDelay / currentSpeedUpFactor;
    }

    private void SpawnPlayer()
    {
        Vector2 logicalSpawnPoint = Vector2.zero;
        playerLogicalCoordinates = SpawnJumper(playerPrefab, logicalSpawnPoint).currentLogicalCoordinates;
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

    public void StepOnTile(Vector2 logicalCoordinates, TileInteractions interactionType)
    {
        int rowMapIndex = (int)logicalCoordinates.x + (int)logicalCoordinates.y;
        GameObject currentTile = piramidMap[rowMapIndex][(int)logicalCoordinates.x];
        SpriteRenderer tileSpriteRenderer = currentTile.GetComponent<SpriteRenderer>();

        if (interactionType == TileInteractions.player)
        {
            Color currentTileColor = tileSpriteRenderer.color;
            int tileColorIndex = findColorIndexOnTile(currentTileColor);
            if (tileColorIndex == -1)
            {
                Debug.LogError("current tile color is outside of posible colors");
            }
            if (currentLevel.tileBehavior == TileBehavior.simpleBiColor || currentLevel.tileBehavior == TileBehavior.simpleTriColor)
            {
                if (tileColorIndex != currentLevel.tileColors.Length - 1)
                {
                    tileColorIndex++;
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
                }
            }

            tileSpriteRenderer.color = currentLevel.tileColors[tileColorIndex];
            if (tileColorIndex == currentLevel.tileColors.Length - 1)
            {
                tilesCompleted++;
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
}