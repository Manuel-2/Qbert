using System.Collections;
using UnityEngine;

public class Builder : MonoBehaviour
{
    [Header("PiramidContruction")]
    [SerializeField] [Min(2)] private int piramidLevels;
    [SerializeField] Vector2 stepDistance;
    [SerializeField] GameObject CubePrefab;

    WalkableTile initialCube;

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


    // make the code build the whole piramid
    static public void BuildPiramidMap(Transform spawnPoint,int piramidLevels,Vector2 stepDistance, GameObject CubePrefab)
    {
        Vector2 firstInLevel = spawnPoint.position;

        for (int row = 0; row < piramidLevels; row++)
        {
            for (int posInRow = 0; posInRow < row + 1; posInRow++)
            {
                float globalX = posInRow == 0 ? firstInLevel.x : firstInLevel.x + (posInRow * stepDistance.x);
                float globalY = firstInLevel.y;
                Vector3 gloablPosition = new Vector3(globalX, globalY, 0);

                GameObject currentBlock = Instantiate(CubePrefab, gloablPosition, Quaternion.identity, spawnPoint);
                WalkableTile walkableTile = currentBlock.GetComponent<WalkableTile>();

                int logicalX = posInRow;
                int logicalY = row - posInRow;
                currentBlock.name = $"{logicalX}-{logicalY}";
                walkableTile.logicalPosition = new int[] { logicalX, logicalY };
            }

            firstInLevel.x -= stepDistance.x / 2;
            firstInLevel.y -= stepDistance.y;
        }
    }
}
