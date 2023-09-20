using System.Collections;
using UnityEngine;

abstract public class Jumper : MonoBehaviour
{
    [Header("Behavior")]
    [SerializeField] Vector2 logicalSpawnPoint;
    [SerializeField] float spawnFallLenght;
    [SerializeField] bool interactsWithTiles;
    [Header("Movement")]
    [SerializeField] Rigidbody2D jumperRigidbody2D;
    [SerializeField] AnimationCurve jumpXCurve, jumpYCurve;
    [SerializeField] float deathJumpForce;
    [Header("Animation")]
    [SerializeField] Animator jumperAnimator;
    [SerializeField] string airAnimationState;
    [SerializeField]
    [Tooltip("value between 0 and 1, defines where on the lerp the jumper should reproduce the land animation")]
    float originalWhenLands;
    [SerializeField] SpriteRenderer jumperSprite;
    [SerializeField] [Tooltip("-1 if is facing left, 1 to the right")] int facingDirection;

    protected Vector2 currentLogicalCoordinates;
    private Vector2 startPosition, targetPosition;
    private float jumpLerpCurrent, jumpLerpTarget;
    private float currentJumpLerpSpeed;
    // true when we lerp from 0 to 1, otherwise false
    private bool normalLerpDirection;

    private float whenLands;
    private bool lerping = false;
    private bool isAlive = false;
    private bool spawing = false;

    public virtual void Update()
    {
        if (lerping)
        {
            this.jumpLerpCurrent = Mathf.MoveTowards(this.jumpLerpCurrent, this.jumpLerpTarget, this.currentJumpLerpSpeed * Time.deltaTime);

            float xCurrentPosition = Mathf.LerpUnclamped(startPosition.x, targetPosition.x, jumpXCurve.Evaluate(jumpLerpCurrent));
            float yCurrentPosition = Mathf.LerpUnclamped(startPosition.y, targetPosition.y, jumpYCurve.Evaluate(jumpLerpCurrent));
            this.transform.position = new Vector3(xCurrentPosition, yCurrentPosition, 0);

            if (normalLerpDirection && this.jumpLerpCurrent >= whenLands || !normalLerpDirection && this.jumpLerpCurrent <= whenLands)
            {
                // Reproduce the Land Animation
                jumperAnimator.SetBool(airAnimationState, false);
            }

            if (jumpLerpCurrent == jumpLerpTarget)
            {
                lerping = false;
                if (spawing)
                {
                    // Discalimer this only works because the animation of the base speed was sincronised manually with the lerp speed factor of 2
                    // if you change the base speed, you have to re animate the character
                    currentJumpLerpSpeed = GameManager.sharedInstance.currentJumpSpeed;
                    jumperAnimator.speed = currentJumpLerpSpeed / GameManager.sharedInstance.jumpSpeeds[1];

                    spawing = false;
                }
            }
        }
    }

    public void InitializeJumper()
    {
        isAlive = true;
        spawing = true;

        // TODO: the gameManager desides where to put every jumper
        //put the jumper at the top of its spawn point by the fall variable and interpolate
        Vector3 globalSpawnPointTarget = Builder.sharedInstance.ConvertLogicalCoordinates2GlobalPosition(logicalSpawnPoint);
        this.transform.position = new Vector3(globalSpawnPointTarget.x, globalSpawnPointTarget.y + spawnFallLenght);
        currentLogicalCoordinates = logicalSpawnPoint;

        // the first speed is the spawn falling speed
        currentJumpLerpSpeed = GameManager.sharedInstance.jumpSpeeds[0];
        LerpJump(this.transform.position, globalSpawnPointTarget);
        // Reproduce the Jump Animation
        jumperAnimator.SetBool(airAnimationState, true);
        jumperSprite.sortingOrder = 5;
    }

    private void LerpJump(Vector2 currentPosition, Vector2 targetPosition)
    {
        if (!isAlive) return;
        lerping = true;
        if (currentPosition.y > targetPosition.y)
        {
            this.jumpLerpCurrent = 1;
            this.jumpLerpTarget = 0;
            this.startPosition = targetPosition;
            this.targetPosition = currentPosition;
            normalLerpDirection = false;
            whenLands = 1 - originalWhenLands;
        }
        else
        {
            // original
            this.jumpLerpCurrent = 0;
            this.jumpLerpTarget = 1;
            this.startPosition = currentPosition;
            this.targetPosition = targetPosition;
            normalLerpDirection = true;
            whenLands = originalWhenLands;
        }
        this.lerping = true;
    }

    public virtual void Jump(Vector2 targetLogicalCoordinates)
    {
        if (!isAlive) return;
        if (lerping) return;
        Vector2 targetGlobalPosition = Builder.sharedInstance.ConvertLogicalCoordinates2GlobalPosition(targetLogicalCoordinates);

        // facing direction
        if (targetGlobalPosition.x > this.transform.position.x)
        {
            facingDirection = 1;
        }
        else
        {
            facingDirection = -1;
        }
        jumperSprite.transform.localPosition = new Vector3(Mathf.Abs(jumperSprite.transform.localPosition.x) * facingDirection, jumperSprite.transform.localPosition.y, 0);
        jumperSprite.flipX = targetGlobalPosition.x > this.transform.position.x;
        // Reproduce the Jump Animation
        jumperAnimator.SetBool(airAnimationState, true);

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

        LerpJump(this.transform.position, targetGlobalPosition);

        if (interactsWithTiles)
        {
            Transform targetBlockTransform = GameManager.sharedInstance.piramidSpawnPoint.Find($"{targetLogicalCoordinates.x}-{targetLogicalCoordinates.y}");
            WalkableTile blockTileComponent = targetBlockTransform.GetComponent<WalkableTile>();
            blockTileComponent.stepIn();
        }
        //update the new logical position !leave at the end always
        currentLogicalCoordinates = targetLogicalCoordinates;
    }

    public virtual void FallInTheVoid(Vector2 targetLogicalCoordinates)
    {
        isAlive = false;
        Vector3 targetGlobalPosition = Builder.sharedInstance.ConvertLogicalCoordinates2GlobalPosition(targetLogicalCoordinates);
        Vector3 jumpDeadVector = ((targetGlobalPosition - this.transform.position).normalized + Vector3.up * 3).normalized;
        jumperRigidbody2D.simulated = true;
        jumperRigidbody2D.AddForceAtPosition(jumpDeadVector * deathJumpForce, this.transform.position + Vector3.up * 2, ForceMode2D.Impulse);
        if (targetLogicalCoordinates.y < 0 || targetLogicalCoordinates.x < 0)
            StartCoroutine(changeSortingOrderOverTime());
        //TODO: add a sound, make the sprite blink in white, aslo a little 
    }

    IEnumerator changeSortingOrderOverTime()
    {
        //todo: change that depending on the Y value
        yield return new WaitForSeconds(0.2f);
        jumperSprite.sortingOrder = -5;
        // Reproduce the Land Animation
        jumperAnimator.SetBool(airAnimationState, false);
    }

}