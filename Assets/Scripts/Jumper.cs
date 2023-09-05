using System.Collections;
using UnityEngine;

abstract public class Jumper : MonoBehaviour
{
    [SerializeField] bool interactsWithTiles;
    protected Vector2 currentLogicalCoordinates;

    private bool lerping;
    private float jumpLerpCurrent;
    private Vector2 startPosition, targetPosition;
    private float currentJumpLerpSpeed;

    private void Start()
    {
        InitializedJumper(Vector2.zero);
    }

    public virtual void Update()
    {
        if (lerping)
        {
            jumpLerpCurrent = Mathf.MoveTowards(jumpLerpCurrent, 1, currentJumpLerpSpeed * Time.deltaTime);
            this.transform.position = Vector3.Lerp(startPosition, targetPosition, jumpLerpCurrent);

            if(jumpLerpCurrent == 1)
            {
                //stop lerping
            }
        }
    }

    private void InitializedJumper(Vector2 logicalSpawnPoint)
    {
        currentLogicalCoordinates.x = logicalSpawnPoint.x;
        currentLogicalCoordinates.y = logicalSpawnPoint.y;
        // todo: select the speed based on dificulty
        currentJumpLerpSpeed = GameManager.sharedInstance.jumpSpeeds[0];
        lerping = false;
    }

    private void LerpJump(Vector2 currentPosition, Vector2 targetPosition)
    {
        lerping = true;
        jumpLerpCurrent = 0;
        startPosition = currentPosition;
        this.targetPosition = targetPosition;
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

        Vector2 targetGlobalPosition = Builder.sharedInstance.ConvertLogicalCoordinates2GlobalPosition(targetLogicalCoordinates);
        LerpJump(this.transform.position, targetGlobalPosition);

        if (interactsWithTiles)
        {
            WalkableTile blockTileComponent = targetBlockTransform.GetComponent<WalkableTile>();
            blockTileComponent.stepIn();
        }

        //update the new logical position
        currentLogicalCoordinates = targetLogicalCoordinates;
    }

    public virtual void FallInTheVoid(Vector2 logicalCoordinates)
    {
        // todo: make and interpolation or something maybe phisycss
        Debug.Log("you fall in to the void");
    }

}