using System.Collections.Generic;
using UnityEngine;

public class Builder : MonoBehaviour
{
    public static Builder sharedInstance;

    [Header("PiramidContruction")]
    [SerializeField] [Min(2)] private int piramidLevels;
    [SerializeField] Vector2 stepDistance;
    [SerializeField] GameObject CubePrefab;
    [Header("SquareInEditorContruction")]
    [SerializeField] [Min(2)] private int squareWidth;
    [SerializeField] [Min(2)] private int squareHeight;

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

    public Vector2 ConvertLogicalCoordinates2GlobalPosition(Vector2 logicalCoordinates, Vector2 stepDistance)
    {
        Transform piramidTopGlobalPosition = this.transform;
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
                blockN++;
            }
        }
    }

    public void BuildSquareMapOnEditMode()
    {
        for (int row = 0; row < squareHeight; row++)
        {
            for (int x = 0; x < squareWidth; x++)
            {
                int y = row - x;
                string blockLogicalCoordinates = $"{x},{y}";
                Vector3 currentBlockGlobalPosition = ConvertLogicalCoordinates2GlobalPosition(new Vector2(x, y), stepDistance);
                GameObject currentBlock = Instantiate(CubePrefab, currentBlockGlobalPosition, Quaternion.identity, this.transform);
                currentBlock.name = blockLogicalCoordinates;
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

                int logicalX = posInRow;
                int logicalY = row - posInRow;
                currentBlock.name = $"{logicalX},{logicalY}";
            }
            rowsList.Add(rowBlocks);

            firstInLevel.x -= stepDistance.x / 2;
            firstInLevel.y -= stepDistance.y;
        }
        return rowsList;
    }

    static public List<List<GameObject>> BuildPiramidMap(Transform spawnPoint, int piramidLevels, Vector2 stepDistance, GameObject tilePrefab,GameObject cubePrefab)
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

                GameObject currentTile = Instantiate(tilePrefab, gloablPosition, Quaternion.identity, spawnPoint);
                rowBlocks.Add(currentTile);
                // background cube
                Instantiate(cubePrefab, gloablPosition, Quaternion.identity, currentTile.transform);

                int logicalX = posInRow;
                int logicalY = row - posInRow;
                currentTile.name = $"{logicalX},{logicalY}";
            }
            rowsList.Add(rowBlocks);

            firstInLevel.x -= stepDistance.x / 2;
            firstInLevel.y -= stepDistance.y;
        }
        return rowsList;
    }
}
