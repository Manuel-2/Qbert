using System.Collections;
using UnityEngine;

public enum TileInteractions
{
    none,
    player,
    reverse
}

abstract public class Jumper : MonoBehaviour
{
    [Header("Behavior")]
    [SerializeField] TileInteractions tileInteraction;
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
    [SerializeField] Sprite[] facingDirectionSprites;
    //[SerializeField] [Tooltip("-1 if is facing left, 1 to the right")] int facingDirection;

    public Vector2 currentLogicalCoordinates;
    private Vector2 startPosition, targetPosition;
    private float jumpLerpCurrent, jumpLerpTarget;
    private float currentJumpLerpSpeed;
    // true when we lerp from 0 to 1, otherwise false
    private bool normalLerpDirection;

    private float whenLands;
    protected bool lerping = false;
    protected bool canJump;
    private bool isAlive = false;
    private bool spawing = false;
    protected int facingDirections;

    private void Awake()
    {
        facingDirections = facingDirectionSprites.Length;
    }

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
                if (jumperAnimator.GetBool(airAnimationState))
                {
                    // Reproduce the Land Animation
                    jumperAnimator.SetBool(airAnimationState, false);
                    if (tileInteraction != TileInteractions.none)
                    {
                        GameManager.sharedInstance.StepOnTile(currentLogicalCoordinates, tileInteraction);
                    }
                }
            }

            if (jumpLerpCurrent == jumpLerpTarget)
            {
                lerping = false;
                StartCoroutine("makeJumpAbailable", GameManager.sharedInstance.currentJumpDelay);
                // just at the start
                if (spawing)
                {
                    // Discalimer this only works because the animation of the base speed was sincronised manually with the lerp speed factor of 2
                    // if you change the base speed, you have to re animate the character
                    currentJumpLerpSpeed = GameManager.sharedInstance.currentJumpSpeed;
                    jumperAnimator.speed = GameManager.sharedInstance.currentSpeedUpFactor;
                    spawing = false;
                }
            }
        }
        else if (!isAlive && jumperRigidbody2D.simulated == true)
        {
            Vector3 lastFloorPosition = Builder.sharedInstance.ConvertLogicalCoordinates2GlobalPosition(currentLogicalCoordinates);
            if (this.transform.position.y < lastFloorPosition.y + 0.25)
            {
                // Reproduce the Land Animation
                jumperAnimator.SetBool(airAnimationState, false);
            }
        }
    }

    public void InitializeJumper(Vector2 logicalSpawnPoint)
    {
        isAlive = true;
        spawing = true;
        Vector3 globalSpawnPointTarget = Builder.sharedInstance.ConvertLogicalCoordinates2GlobalPosition(logicalSpawnPoint);
        currentLogicalCoordinates = logicalSpawnPoint;

        // the first speed is the spawn falling speed
        currentJumpLerpSpeed = GameManager.sharedInstance.spawnLerpSpeed;
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
    }

    public virtual void Jump(Vector2 targetLogicalCoordinates)
    {
        if (!isAlive || lerping) return;
        Vector2 targetGlobalPosition = Builder.sharedInstance.ConvertLogicalCoordinates2GlobalPosition(targetLogicalCoordinates);
        Vector2 deltaCoordinates = targetLogicalCoordinates - currentLogicalCoordinates;
        // | i | direction |
        //   0 = left down
        //   1 = right down
        //   2 = left up
        //   3 = right up
        if (facingDirections == 2)
        {
            if (deltaCoordinates.x == -1 || deltaCoordinates.y == 1)
            {
                jumperSprite.sprite = facingDirectionSprites[0];
            }
            else if (deltaCoordinates.x == 1 || deltaCoordinates.y == -1)
            {
                jumperSprite.sprite = facingDirectionSprites[1];
            }
        }
        else if (facingDirections == 4)
        {
            if (deltaCoordinates.x == 0 && deltaCoordinates.y == 1)
            {
                jumperSprite.sprite = facingDirectionSprites[0];
            }
            else if (deltaCoordinates.x == 1 && deltaCoordinates.y == 0)
            {
                jumperSprite.sprite = facingDirectionSprites[1];
            }
            else if (deltaCoordinates.x == 0 && deltaCoordinates.y == -1)
            {
                jumperSprite.sprite = facingDirectionSprites[2];
            }
            else if (deltaCoordinates.x == -1 && deltaCoordinates.y == 0)
            {
                jumperSprite.sprite = facingDirectionSprites[3];
            }
        }

        // Reproduce the Jump Animation
        jumperAnimator.SetBool(airAnimationState, true);

        /*
        piramid levels:
            # --- level 0
           ### --- level 1
          ##### --- level 2
         */
        if (CheckForFall(targetLogicalCoordinates))
        {
            FallInTheVoid(targetLogicalCoordinates);
            return;
        }

        LerpJump(this.transform.position, targetGlobalPosition);

        //update the new logical position !leave at the end always
        currentLogicalCoordinates = targetLogicalCoordinates;
        canJump = false;
    }

    public bool CheckForFall(Vector2 targetLogicalCoordinates)
    {
        float piramidLevel = targetLogicalCoordinates.x + targetLogicalCoordinates.y;
        if (targetLogicalCoordinates.x < 0 || targetLogicalCoordinates.y < 0 || piramidLevel > GameManager.sharedInstance.totalPiramidLevels - 1)
        {
            return true;
        }
        return false;
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

    IEnumerator makeJumpAbailable(float delay)
    {
        yield return new WaitForSeconds(delay);
        canJump = true;
    }

}