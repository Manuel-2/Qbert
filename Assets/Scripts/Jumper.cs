using System.Collections;
using UnityEngine;

public enum TileInteractions
{
    none,
    player,
    reverse,
    snake
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
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip jumpAudioClip;
    [SerializeField] AudioClip jumperDeath;
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
    protected Platform currentPlatform;

    private void Awake()
    {
        facingDirections = facingDirectionSprites.Length;
        jumperRigidbody2D.gravityScale = 0;
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
                    if (tileInteraction != TileInteractions.none && currentPlatform == null)
                    {
                        GameManager.sharedInstance.StepOnTile(currentLogicalCoordinates, tileInteraction);
                    }
                }
            }

            if (jumpLerpCurrent == jumpLerpTarget)
            {
                lerping = false;
                if (currentPlatform != null)
                {
                    GameManager.sharedInstance.ActivateRainbowPlatform(this.currentPlatform);
                    if (currentPlatform.loogicalCoordinates.x == -1)
                    {
                        jumperSprite.sprite = facingDirectionSprites[0];
                    }
                    else if (currentPlatform.loogicalCoordinates.y == -1)
                    {
                        jumperSprite.sprite = facingDirectionSprites[0];
                    }
                }
                else
                {
                    StartCoroutine("makeJumpAbailable", GameManager.sharedInstance.currentJumpDelay);
                }
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
    }

    public void InitializeJumper(Vector2 logicalSpawnPoint)
    {
        isAlive = true;
        spawing = true;
        currentPlatform = null;
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
        if (!isAlive || lerping || GameManager.sharedInstance.levelCompleted) return;
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
            if (tileInteraction == TileInteractions.player)
            {
                currentPlatform = GameManager.sharedInstance.Check4SavePlatform(targetLogicalCoordinates);
                if (currentPlatform != null)
                {
                    targetGlobalPosition = currentPlatform.floorPivot.position;
                }
            }

            if (currentPlatform == null)
            {
                FallInTheVoid(targetLogicalCoordinates);
                return;
            }
        }

        LerpJump(this.transform.position, targetGlobalPosition);

        //update the new logical position !leave at the end always
        audioSource.PlayOneShot(jumpAudioClip);
        currentLogicalCoordinates = targetLogicalCoordinates;
        canJump = false;
    }

    public void jumpOffRainbowPlatform()
    {
        transform.parent = null;
        currentPlatform = null;
        Jump(Vector2.zero);
    }

    public bool CheckForFall(Vector2 targetLogicalCoordinates)
    {
        float piramidLevel = targetLogicalCoordinates.x + targetLogicalCoordinates.y;
        return targetLogicalCoordinates.x < 0
            || targetLogicalCoordinates.y < 0
            || piramidLevel > GameManager.sharedInstance.totalPiramidLevels - 1;
    }
    public virtual void FallInTheVoid(Vector2 targetLogicalCoordinates)
    {
        isAlive = false;
        Vector3 targetGlobalPosition = Builder.sharedInstance.ConvertLogicalCoordinates2GlobalPosition(targetLogicalCoordinates);
        Vector3 jumpDeadVector = ((targetGlobalPosition - this.transform.position).normalized + Vector3.up * 3).normalized;
        // the rigidbody is activated by default but it has 0 gravity, when the player falls of it gets a gravity scale of 1
        jumperRigidbody2D.gravityScale = 1;

        jumperRigidbody2D.AddForceAtPosition(jumpDeadVector * deathJumpForce, this.transform.position + Vector3.up * 2, ForceMode2D.Impulse);
        if (tileInteraction == TileInteractions.snake)
        {
            GameManager.sharedInstance.UpdateScore(500);
            GameManager.sharedInstance.snakeOnGame = false;
        }
        else if (tileInteraction == TileInteractions.player)
        {
            GameManager.sharedInstance.KillPlayer();
        }

        if (tileInteraction != TileInteractions.player)
        {
            GameManager.sharedInstance.EnemyDied(this);
        }
        audioSource.PlayOneShot(jumperDeath);
        Destroy(this.gameObject, 4f);
        StartCoroutine(changeSortingOrderOverTime());
        StartBlinkAnimation();
    }

    IEnumerator changeSortingOrderOverTime()
    {
        yield return new WaitForSeconds(0.2f);
        if (currentLogicalCoordinates.x == 0 || currentLogicalCoordinates.y == 0)
        {
            jumperSprite.sortingOrder = -5;
        }
        // Reproduce the Land Animation
        jumperAnimator.SetBool(airAnimationState, false);
    }

    IEnumerator makeJumpAbailable(float delay)
    {
        yield return new WaitForSeconds(delay);
        canJump = true;
    }

    public void StartBlinkAnimation()
    {
        StartCoroutine(blinkSprite(.2f));
    }

    IEnumerator blinkSprite(float blinkDuration)
    {
        yield return new WaitForSeconds(0.25f);
        var spriteMaterial = jumperSprite.material;
        spriteMaterial.SetFloat("_FlashAmount", 1);
        yield return new WaitForSeconds(blinkDuration);
        spriteMaterial.SetFloat("_FlashAmount", 0);
    }

}