using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager sharedInstance;

    [Header("Game Configuration")]
    [SerializeField] Transform jumpersTopSpawn;
    [Tooltip("0 is the fall speed on spawn")]
    public float[] jumpSpeeds;
    public float currentJumpSpeed;

    [Header("PiramidContruction")]
    [SerializeField] [Min(2)] private int _piramidLevels;
    [SerializeField] Transform _piramidSpawnPoint;
    [SerializeField] Vector2 _stepDistance;
    [SerializeField] GameObject CubePrefab;

    [SerializeField] GameObject[] jumpersPrefabs;

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
        currentJumpSpeed = jumpSpeeds[1];
        Builder.BuildPiramidMap(_piramidSpawnPoint, _piramidLevels, _stepDistance, CubePrefab);
        SpawnJumper();
    }

    private void SpawnJumper()
    {
        // create a jumper, put it on top of its spawn point

        //todo: a new fuction to select a random jumper, and send it by argument
        GameObject newJumper = Instantiate(jumpersPrefabs[0]);
        newJumper.GetComponent<Jumper>().InitializeJumper();
    }
}