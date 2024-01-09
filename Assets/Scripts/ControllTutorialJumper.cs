using System.Collections;
using UnityEngine;

public class ControllTutorialJumper : MonoBehaviour
{
    [SerializeField] Animator jumperAnimator;
    [SerializeField] string airAnimationState;
    [SerializeField]
    [Tooltip("value between 0 and 1, defines where on the lerp the jumper should reproduce the land animation")]
    float originalWhenLands;
    [SerializeField]
    float delayBetweenJumps;
    [SerializeField] float currentJumpLerpSpeed;
    [SerializeField] AnimationCurve jumpXCurve, jumpYCurve;

    private Vector2 startPosition, targetPosition;
    private float jumpLerpCurrent, jumpLerpTarget;
    private bool lerping;
    private bool normalLerpDirection;
    float whenLands;


    [SerializeField] Transform[] targets = new Transform[4];
    [SerializeField] Sprite[] faces = new Sprite[4];
    [SerializeField] SpriteRenderer JumperSprite;
    private int positionIndex;
    [SerializeField] Animator[] keysAnimator = new Animator[4];
    
    private void OnEnable()
    {
        positionIndex = 0;
        Jump();
    }

    private void OnDisable()
    {
        this.transform.position = targets[0].position;
        positionIndex = 0;
    }

    private void Update()
    {
        Lerping();
    }

    private void Jump()
    {
        positionIndex++;
        if(positionIndex == targets.Length)
        {
            positionIndex = 0;
        }
        JumperSprite.sprite = faces[positionIndex];
        keysAnimator[positionIndex].SetTrigger("pressKey");
        jumperAnimator.SetBool(airAnimationState, true);
        LerpJump(transform.position,targets[positionIndex].position);
    }

    private void Lerping()
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
                }
            }

            if (jumpLerpCurrent == jumpLerpTarget)
            {
                lerping = false;
                StartCoroutine("Wait2JumpAgain", delayBetweenJumps);
            }
        }
    }

    private void LerpJump(Vector2 currentPosition, Vector2 targetPosition)
    {
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

    IEnumerator Wait2JumpAgain()
    {
        yield return new WaitForSeconds(delayBetweenJumps);
        Jump();
    }

}
