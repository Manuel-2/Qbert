using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager sharedInstance;

    [Header("PiramidContruction")]
    [SerializeField] [Min(2)] private int piramidLevels;
    [SerializeField] Vector2 stepDistance;
    [SerializeField] GameObject CubePrefab;
    [SerializeField] Transform priamidSpawnPoint;


    private void Awake()
    {
        if(sharedInstance == null)
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
        Builder.BuildPiramidMap(priamidSpawnPoint, piramidLevels, stepDistance, CubePrefab);
    }
}