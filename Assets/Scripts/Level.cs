using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "ScriptableObjects/Level", order = 1)]
public class Level : ScriptableObject
{
    public TileBehavior tileBehavior;
    public Color[] tileColors;
    public Color blockColor;
    public int platforms;
    public float lerpSpeed;
    public float spawnBallMinDelay, spawnBallMaxDelay;
    public float spawnSnakeMinDelay, spawnSnakeMaxDelay;
}
