using System.Collections;
using UnityEngine;

public class Builder : MonoBehaviour
{
    [Header("PiramidContruction")]
    [SerializeField] [Min(2)] private int piramidLevels;
    [SerializeField] Vector2 stepDistance;
    [SerializeField] GameObject CubePrefab;

    Cube initialCube;

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
        initialCube = this.gameObject.GetComponent<Cube>();
        initialCube.position = new int[] { 0, 0 };

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
                Cube currentCube = currentBlock.GetComponent<Cube>();
                currentCube.position = new int[] { row, posInRow };
                blockN++;
            }
        }
    }
}
