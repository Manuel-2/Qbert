using System.Collections;
using UnityEngine;

abstract public class Jumper : MonoBehaviour
{
    [SerializeField] SpriteRenderer jumperSprite;
    [SerializeField][Tooltip("0 if is facing left, 1 to the right")] int spriteFacingDirection;

    [SerializeField] bool interactsWithTiles;
    protected Vector2 currentLogicalCoordinates;

    [SerializeField] AnimationCurve jumpXCurve, jumpYCurve;
    private bool lerping;
    private float jumpLerpCurrent, jumpLerpTarget;
    private Vector2 startPosition, targetPosition;
    private float currentJumpLerpSpeed;

    [SerializeField] Rigidbody2D jumperRigidbody2D;
    [SerializeField] float deatJumpForce;

    bool isAlive = true;

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
        isAlive = true;
        jumperSprite.sortingOrder = 5;
        currentLogicalCoordinates.x = logicalSpawnPoint.x;
        currentLogicalCoordinates.y = logicalSpawnPoint.y;
        // todo: select the speed based on dificulty
        currentJumpLerpSpeed = GameManager.sharedInstance.jumpSpeeds[0];
        lerping = false;
    }

    private void LerpJump(Vector2 currentPosition, Vector2 targetPosition)
    {
        if (!isAlive) return;
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
        if (!isAlive) return;
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
        
        Vector2 targetGlobalPosition = Builder.sharedInstance.ConvertLogicalCoordinates2GlobalPosition(targetLogicalCoordinates);
        LerpJump(this.transform.position, targetGlobalPosition);

        if (interactsWithTiles)
        {
            Transform targetBlockTransform = GameManager.sharedInstance.piramidSpawnPoint.Find($"{targetLogicalCoordinates.x}-{targetLogicalCoordinates.y}");
            WalkableTile blockTileComponent = targetBlockTransform.GetComponent<WalkableTile>();
            blockTileComponent.stepIn();
        }

        //feedback and polish
        // 0 && t.y > c.y = dont
        // 1 && t.y > c.y = flip
        jumperSprite.flipX = System.Convert.ToBoolean(spriteFacingDirection) || targetGlobalPosition.x > this.transform.position.x;

        //update the new logical position !leave at the end always
        currentLogicalCoordinates = targetLogicalCoordinates;
    }

    public virtual void FallInTheVoid(Vector2 targetLogicalCoordinates)
    {
        isAlive = false;
        Vector3 targetGlobalPosition = Builder.sharedInstance.ConvertLogicalCoordinates2GlobalPosition(targetLogicalCoordinates);
        Vector3 jumpDeadVector = ((targetGlobalPosition - this.transform.position).normalized + Vector3.up * 3).normalized;
        jumperRigidbody2D.simulated = true;
        jumperRigidbody2D.AddForceAtPosition(jumpDeadVector * deatJumpForce, this.transform.position + Vector3.up * 2, ForceMode2D.Impulse);
        if (targetLogicalCoordinates.y < 0 || targetLogicalCoordinates.x < 0)
            StartCoroutine(changeSortingOrderOverTime());
        //TODO: add a sound, make the sprite blink in white, aslo a little 
    }

    IEnumerator changeSortingOrderOverTime()
    {
        yield return new WaitForSeconds(0.2f);
        jumperSprite.sortingOrder = -5;
    }

}