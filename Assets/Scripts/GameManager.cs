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
        for (int level = 1; level < height; level++)
        {
            firstInLevel.x -= stepDistance.x / 2;
            firstInLevel.y -= stepDistance.y;
            int width = level + 1;
            for (int block = 0;  block < width; block++)
            {
                float blockX = block == 0? firstInLevel.x : firstInLevel.x + (block * stepDistance.x);
                float blockY = firstInLevel.y;
                Vector3 blockPosition = new Vector3(blockX, blockY,0);
                var newBlock = Instantiate(CubePrefab, blockPosition,Quaternion.identity);
                newBlock.name = "block" + blockN;
                blockN++;
            }
        }
    }
}
