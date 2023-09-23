using System.Collections.Generic;
using UnityEngine;

public class Builder : MonoBehaviour
{
    public static Builder sharedInstance;

    [Header("PiramidContruction")]
    [SerializeField] [Min(2)] private int piramidLevels;
    [SerializeField] Vector2 stepDistance;
    [SerializeField] GameObject CubePrefab;

    WalkableTile initialCube;

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

    public Vector2 ConvertLogicalCoordinates2GlobalPosition(Vector2 logicalCoordinates)
    {
        Transform piramidTopGlobalPosition = GameManager.sharedInstance.piramidSpawnPoint;
        Vector2 stepDistance = GameManager.sharedInstance.stepDistance;
        Vector3 targetGlobalPosition = Vector3.zero;
        targetGlobalPosition.x = (logicalCoordinates.x - logicalCoordinates.y) * stepDistance.x / 2;
        targetGlobalPosition.y = -1 * (logicalCoordinates.x + logicalCoordinates.y) * stepDistance.y;
        targetGlobalPosition = targetGlobalPosition + piramidTopGlobalPosition.position;
        return targetGlobalPosition;
    }

    public void BuildPiramidMap()
    {
        //if(this.transform.childCount > 0)
        //{
        //    foreach (Transform child in this.transform)
        //    {
        //        DestroyImmediate(child.gameObject);
        //    }
        //}

        Vector2 firstInLevel = this.transform.position;
        initialCube = this.gameObject.GetComponent<WalkableTile>();
        initialCube.logicalPosition = new int[] { 0, 0 };

        int blockN = 2;
        for (int row = 1; row < piramidLevels; row++)
        {
            firstInLevel.x -= stepDistance.x / 2;
            firstInLevel.y -= stepDistance.y;
            int width = row + 1;
            for (int posInRow = 0; posInRow < width; posInRow++)
            {
                float blockX = posInRow == 0 ? firstInLevel.x : firstInLevel.x + (posInRow * stepDistance.x);
                float blockY = firstInLevel.y;
                Vector3 blockPosition = new Vector3(blockX, blockY, 0);
                GameObject currentBlock = Instantiate(CubePrefab, blockPosition, Quaternion.identity, this.transform);
                currentBlock.name = "block" + blockN;
                WalkableTile currentCube = currentBlock.GetComponent<WalkableTile>();
                currentCube.logicalPosition = new int[] { row, posInRow };
                blockN++;
            }
        }
    }

    // build the whole piramid in execution time
    static public List<List<GameObject>> BuildPiramidMap(Transform spawnPoint, int piramidLevels, Vector2 stepDistance, GameObject CubePrefab)
    {
        Vector2 firstInLevel = spawnPoint.position;
        List<List<GameObject>> rowsList = new List<List<GameObject>>();

        for (int row = 0; row < piramidLevels; row++)
        {
            var rowBlocks = new List<GameObject>();
            for (int posInRow = 0; posInRow < row + 1; posInRow++)
            {
                float globalX = posInRow == 0 ? firstInLevel.x : firstInLevel.x + (posInRow * stepDistance.x);
                float globalY = firstInLevel.y;
                Vector3 gloablPosition = new Vector3(globalX, globalY, 0);

                GameObject currentBlock = Instantiate(CubePrefab, gloablPosition, Quaternion.identity, spawnPoint);
                rowBlocks.Add(currentBlock);
                WalkableTile walkableTile = currentBlock.GetComponent<WalkableTile>();

                int logicalX = posInRow;
                int logicalY = row - posInRow;
                currentBlock.name = $"{logicalX}-{logicalY}";
                walkableTile.logicalPosition = new int[] { logicalX, logicalY };
            }
            rowsList.Add(rowBlocks);

            firstInLevel.x -= stepDistance.x / 2;
            firstInLevel.y -= stepDistance.y;
        }
        return rowsList;
    }
}
