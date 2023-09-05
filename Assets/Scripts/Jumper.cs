using System.Collections;
using UnityEngine;

abstract public class Jumper : MonoBehaviour
{
    [SerializeField] bool interactsWithTiles;
    protected Vector2 currentLogicalCoordinates;

    [SerializeField] AnimationCurve jumpXCurve, jumpYCurve;
    private bool lerping;
    private float jumpLerpCurrent, jumpLerpTarget;
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
            jumpLerpCurrent = Mathf.MoveTowards(jumpLerpCurrent, jumpLerpTarget, currentJumpLerpSpeed * Time.deltaTime);

            float xCurrentPosition = Mathf.LerpUnclamped(startPosition.x, targetPosition.x, jumpXCurve.Evaluate(jumpLerpCurrent));
            float yCurrentPosition = Mathf.LerpUnclamped(startPosition.y, targetPosition.y, jumpYCurve.Evaluate(jumpLerpCurrent));
            this.transform.position = new Vector3(xCurrentPosition, yCurrentPosition, 0);

            if (jumpLerpCurrent == jumpLerpTarget)
            {
                lerping = false;
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
        if (currentPosition.y > targetPosition.y)
        {
            jumpLerpCurrent = 1;
            jumpLerpTarget = 0;
            startPosition = targetPosition;
            this.targetPosition = currentPosition;
        }
        else
        {
            // original
            jumpLerpCurrent = 0;
            jumpLerpTarget = 1;
            startPosition = currentPosition;
            this.targetPosition = targetPosition;
        }
    }

    public virtual void Jump(Vector2 targetLogicalCoordinates)
    {
        if (lerping) return;
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
        // todo: make and interpolation or something maybe physics
    }

}