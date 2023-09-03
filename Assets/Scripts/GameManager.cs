using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager sharedInstance;

    [Header("PiramidContruction")]
    [SerializeField] [Min(2)] private int piramidLevels;
    [SerializeField] Transform initialCube;
    [SerializeField] Vector2 stepDistance;
    [SerializeField] GameObject CubePrefab;

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

    // Start is called before the first frame update
    void Start()
    {
        BuildPiramidMap(initialCube,piramidLevels);
    }

    void BuildPiramidMap(Transform initialCube,int height)
    {
        Vector2 firstInLevel = initialCube.position;
        int blockN = 2;
        for (int row = 1; row < height; row++)
        {
            firstInLevel.x -= stepDistance.x / 2;
            firstInLevel.y -= stepDistance.y;
            int width = row + 1;
            for (int posInRow = 0;  posInRow < width; posInRow++)
            {
                float blockX = posInRow == 0? firstInLevel.x : firstInLevel.x + (posInRow * stepDistance.x);
                float blockY = firstInLevel.y;
                Vector3 blockPosition = new Vector3(blockX, blockY,0);
                var newBlock = Instantiate(CubePrefab, blockPosition,Quaternion.identity);
                newBlock.name = "block" + blockN;
                blockN++;
            }
        }
    }
}
