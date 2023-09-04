using System.Collections;
using UnityEngine;

public class Jumper : MonoBehaviour
{
    [SerializeField] bool interactsWithTiles;
    [SerializeField] int jumpToX;
    [SerializeField] int jumpToY;

    Vector2 currentLogicalCoordinates;

    void initializedJumper(Vector2 logicalSpawnPoint)
    {
        currentLogicalCoordinates.x = logicalSpawnPoint.x;
        currentLogicalCoordinates.y = logicalSpawnPoint.y;
    }

    void Jump(Vector2 targetLogicalCoordinates)
    {
        /*
        piramid levels:
            # --- level 0
           ### --- level 1
          ##### --- level 2
         */
        float piramidLevel = targetLogicalCoordinates.x + targetLogicalCoordinates.y;

        if (targetLogicalCoordinates.x < 0 || targetLogicalCoordinates.y < 0 || piramidLevel > GameManager.sharedInstance.totalPiramidLevels)
        {
            FallInTheVoid(targetLogicalCoordinates);
            return;
        }
        Transform targetBlockTransform = GameManager.sharedInstance.piramidSpawnPoint.Find($"{targetLogicalCoordinates.x}-{targetLogicalCoordinates.y}");
        WalkableTile blockTileComponent = targetBlockTransform.GetComponent<WalkableTile>();

        //TODO: interpolate x and y position
        Vector2 newGlobalPosition = Builder.sharedInstance.ConvertLogicalCoordinates2GlobalPosition(targetLogicalCoordinates);
        this.transform.position = newGlobalPosition;
        currentLogicalCoordinates = targetLogicalCoordinates;

        if (interactsWithTiles)
            blockTileComponent.stepIn();
    }

    void FallInTheVoid(Vector2 logicalCoordinates)
    {
        // todo: make and interpolation or something maybe phisycss
    }

}