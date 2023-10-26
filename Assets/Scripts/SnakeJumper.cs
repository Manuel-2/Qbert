using System.Collections;
using UnityEngine;


public class SnakeJumper : Jumper
{
    enum TargetMode
    {
        random,
        following
    }

    [SerializeField] Sprite evolutionSprite;
    [SerializeField] SpriteRenderer characterRenderer;

    private TargetMode curretTargetMode;
    private Vector2 playerLoogicalCoordinates;

    public override void Update()
    {
        base.Update();
        if (this.lerping == false && canJump)
        {
            if (curretTargetMode == TargetMode.random)
            {
                int lastLevel = GameManager.sharedInstance.totalPiramidLevels - 1;
                if (currentLogicalCoordinates.x + currentLogicalCoordinates.y == lastLevel)
                {
                    curretTargetMode = TargetMode.following;
                    
                    characterRenderer.sprite = evolutionSprite;
                    this.facingDirections = 4;
                    return;
                }

                bool coinFlip = Random.Range(0, 2) > 0;
                Vector2 logicalTarget = this.currentLogicalCoordinates;
                if (coinFlip)
                {
                    logicalTarget.x++;
                }
                else
                {
                    logicalTarget.y++;
                }
                this.facingDirections = 0;
                this.Jump(logicalTarget);
            }
            else if (curretTargetMode == TargetMode.following)
            {
                bool coinFlip = Random.Range(0, 2) > 0;
                Vector2 logicalTarget = currentLogicalCoordinates;

                playerLoogicalCoordinates = GameManager.sharedInstance.playerLogicalCoordinates;
                var yDifference = (playerLoogicalCoordinates.y - currentLogicalCoordinates.y);
                if (yDifference != 0)
                {
                    yDifference = yDifference / Mathf.Abs(yDifference);
                }
                Vector2 testLogicalTargetY = new Vector2(currentLogicalCoordinates.x, currentLogicalCoordinates.y + yDifference);
                bool fallsOnYMove = CheckForFall(testLogicalTargetY);

                var xDifference = (playerLoogicalCoordinates.x - currentLogicalCoordinates.x);
                if (xDifference != 0)
                {
                    xDifference = xDifference / Mathf.Abs(xDifference);
                }
                Vector2 testLogicalTargetX = new Vector2(currentLogicalCoordinates.x + xDifference, currentLogicalCoordinates.y);
                bool fallsOnXMove = CheckForFall(testLogicalTargetX);

                if (fallsOnXMove)
                {
                    // move in Y
                    logicalTarget = testLogicalTargetY;
                }
                else if (fallsOnYMove)
                {
                    // move in X
                    logicalTarget = testLogicalTargetX;
                }
                else
                {
                    if (xDifference == 0)
                    {
                        logicalTarget = testLogicalTargetY;
                    }
                    else if (yDifference == 0)
                    {
                        logicalTarget = testLogicalTargetX;
                    }
                    else
                    {
                        if (coinFlip)
                        {
                            logicalTarget = testLogicalTargetX;
                        }
                        else
                        {
                            logicalTarget = testLogicalTargetY;
                        }
                    }
                }
                this.Jump(logicalTarget);
            }
        }
    }

    public override void Jump(Vector2 targetLogicalCoordinates)
    {
        if (!canJump) return;
        base.Jump(targetLogicalCoordinates);
    }
}