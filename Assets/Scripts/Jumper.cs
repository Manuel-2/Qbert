using System.Collections;
using UnityEngine;

abstract public class Jumper : MonoBehaviour
{
    [SerializeField] bool interactsWithTiles;
    protected Vector2 currentLogicalCoordinates;

    void initializedJumper(Vector2 logicalSpawnPoint)
    {
        currentLogicalCoordinates.x = logicalSpawnPoint.x;
        currentLogicalCoordinates.y = logicalSpawnPoint.y;
    }

    public virtual void Jump(Vector2 targetLogicalCoordinates)
    {
        /*
        piramid levels:
            # --- level 0
           ### --- level 1
          ##### --- level 2
         */
        float piramidLevel = targetLogicalCoordinates.x + targetLogicalCoordinates.y;

        if (targetLogicalCoordinates.x < 0 || targetLogicalCoordinates.y < 0 || piramidLevel > GameManager.sharedInstance.totalPiramidLevels - 1)
        {
            FallInTheVoid(targetLogicalCoordinates);
            return;
        }
        Transform targetBlockTransform = GameManager.sharedInstance.piramidSpawnPoint.Find($"{targetLogicalCoordinates.x}-{targetLogicalCoordinates.y}");
        //if (targetBlockTransform == null)
        //{
        //    FallInTheVoid(targetLogicalCoordinates);
        //    return;
        //}
        //TODO: interpolate x and y position
        Vector2 newGlobalPosition = Builder.sharedInstance.ConvertLogicalCoordinates2GlobalPosition(targetLogicalCoordinates);
        this.transform.position = newGlobalPosition;
        currentLogicalCoordinates = targetLogicalCoordinates;

        if (interactsWithTiles)
        {
            WalkableTile blockTileComponent = targetBlockTransform.GetComponent<WalkableTile>();
            blockTileComponent.stepIn();
        }
    }

    public virtual void FallInTheVoid(Vector2 logicalCoordinates)
    {
        // todo: make and interpolation or something maybe phisycss
        Debug.Log("you fall in to the void");
    }

}