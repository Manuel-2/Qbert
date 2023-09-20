using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager sharedInstance;

    [Header("Game Configuration")]
    [HideInInspector] public float currentJumpSpeed;
    private int dificulty;
    [Tooltip("Base jump delay, match with the first jump speed")]
    [SerializeField] float jumpDelay;
    [HideInInspector] public float currentJumpDelay;
    [Tooltip("1 is the original speed, this value can be 1.5, 2 etc")]
    [HideInInspector]
    public float currentSpeedUpFactor;
    [Tooltip("0 is the fall speed on spawn")]
    public float[] jumpSpeeds;

    [Header("Jumpers")]
    [Tooltip("Delay in seconds the jumper have to wait to jump again")]
    [SerializeField] float spawnFallLenght;
    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject ballPrefab;
    [SerializeField] int BallsSpawnLevel;

    [Header("PiramidContruction")]
    [SerializeField] [Min(2)] private int _piramidLevels;
    [SerializeField] Transform _piramidSpawnPoint;
    [SerializeField] Vector2 _stepDistance;
    [SerializeField] GameObject CubePrefab;


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
        // todo: update the speed on every level
        dificulty = 1;
        SetGameSpeed(dificulty);

        Builder.BuildPiramidMap(_piramidSpawnPoint, _piramidLevels, _stepDistance, CubePrefab);
        SpawnPlayer();
        SpawnBallEnemy();
    }

    private void SetGameSpeed(int dificultyIndex)
    {
        currentJumpSpeed = jumpSpeeds[dificulty++];
        currentSpeedUpFactor = currentJumpSpeed / jumpSpeeds[1];
        currentJumpDelay = jumpDelay / currentSpeedUpFactor;
    }

    private void SpawnPlayer()
    {
        // create a jumper, put it on top of its spawn point
        //todo: a new fuction to select a random jumper, and send it by argument
        Vector2 logicalSpawnPoint = Vector2.zero;
        SpawnJumper(playerPrefab, logicalSpawnPoint);
    }

    private void SpawnBallEnemy()
    {
        bool coinFlip = Random.Range(0, 2) > 0;
        Vector2 logicalSpawnPoint = new Vector2(0, BallsSpawnLevel);
        if (coinFlip)
        {
            logicalSpawnPoint = new Vector2(BallsSpawnLevel, 0);
        }
        SpawnJumper(ballPrefab, logicalSpawnPoint);
    }

    private void SpawnJumper(GameObject jumperPrefab, Vector2 logicalSpawnPoint)
    {
        GameObject newJumper = Instantiate(jumperPrefab);
        Vector3 globalSpawnPointTarget = Builder.sharedInstance.ConvertLogicalCoordinates2GlobalPosition(logicalSpawnPoint);
        newJumper.transform.position = new Vector3(globalSpawnPointTarget.x, globalSpawnPointTarget.y + spawnFallLenght);
        newJumper.GetComponent<Jumper>().InitializeJumper(logicalSpawnPoint);
    }
}